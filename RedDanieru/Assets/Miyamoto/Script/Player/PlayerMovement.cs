using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

///<summry>
///プレイヤーの基本的な動きを処理するスクリプト(歩く走るジャンプする等)
///</summry>>
namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("プレイヤーの設定")]
        [Tooltip("プレイヤーの歩く速度")]
        public float MoveSpeed = 2.0f;

        [Tooltip("プレイヤーの走る速度")]
        public float SprintSpeed = 5.335f;

        [Tooltip("キャラクターが移動方向を向く速度")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("加速減速の速さ")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("ジャンプの高さ")]
        public float JumpHeight = 1.2f;

        [Tooltip("キャラクターが使用する重力値。エンジンのデフォルトは -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("ジャンプできるようになるまでに必要な時間。0fに設定すると即座にジャンプできる")]
        public float JumpTimeout = 0.50f;

        [Tooltip("落下アニメーションに入るまでの時間")]
        public float FallTimeout = 0.15f;

        [Tooltip("プレイヤーが地面にいるかどうか")]
        public bool Grounded = true;

        [Tooltip("プレイヤーが地面にいるかどうかの判定場所を少し下に下げる")]
        public float GroundedOffset = -0.14f;

        [Tooltip("プレイヤーが地面にいるかどうかの判定半径")]
        public float GroundedRadius = 0.28f;

        [Tooltip("どのレイヤーを地面として使用するか")]
        public LayerMask GroundLayers;

        // プレイヤーの基礎設定を格納する変数
        private float _speed;              // プレイヤーの現在の速度
        private float _animationBlend;                   // アニメーションへのブレンド値
        private float _targetRotation = 0.0f;            // プレイヤーの目標回転角度
        private float _rotationVelocity;                 // プレイヤーの左右速度
        private float _verticalVelocity;                 // プレイヤーの上下速度
        private float _terminalVelocity = 53.0f;     // 落下速度の上限値

        // 次のジャンプが出来る時間
        private float _jumpTimeoutDelta;
        // 落下アニメーションに入るまでの時間
        private float _fallTimeoutDelta;

        private CharacterController _controller;         // プレイヤーの移動を制御するためのCharacterControllerコンポーネント
        private PlayerCamera _playerCamera;              // プレイヤーのカメラを制御するためのコンポーネント
        private PlayerAnimation _playerAnimation; 
        private PlayerInputPriority _actionPriority;

        private void Start()
        {
            //プレイヤーのカメラを制御するためのコンポーネント
            _playerCamera = GetComponent<PlayerCamera>();

            // キャラクターの動きを制御するためのCharacterControllerコンポーネント
            _controller = GetComponent<CharacterController>();

            _playerAnimation = GetComponent<PlayerAnimation>();

            _actionPriority = GetComponent<PlayerInputPriority>();

            _jumpTimeoutDelta = JumpTimeout;　　　　　// ジャンプできるようになるまでの時間を初期化
            _fallTimeoutDelta = FallTimeout;          // 落下アニメーションに入るまでの時間を初期化
        }

        public void GroundedCheck()
        {
            // プレイヤーの下にある球体を使って地面にいるかどうかを判定
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

            // 地面にいるかどうかの判定を行う
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            _playerAnimation.GroundedCheckAnimator(Grounded);
        }

        //-----------------------------------------------------
        //プレイヤーの基本的な動きを処理する
        //-----------------------------------------------------
        public void PlayerMove(StarterAssetsInputs _input) 
        {
            // Shiftキーを押している場合は歩き、押していない場合は走る
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;


            // 何も入力されていない場合は速度を0にする
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // プレイヤーの現在の速度を取得する
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            // プレイヤーの速度の誤差を設定する
            float speedOffset = 0.1f;

            // プレイヤーの入力の大きさを取得する(ゲームパッドなら入力の大きさをPCなら1f)
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // 現在の速度と目標速度の差が誤差より大きい場合は、速度を補間して更新する
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {

                // 速度を補間して更新する
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // 細かい数字の誤差をなくすために、小数点以下3桁で丸める
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }

            //同じならそのままの速度
            else
            {
                _speed = targetSpeed;
            }

            // アニメーションのブレンド値を補間して更新する
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

            // 小数点以下3桁で待機モーションに入る
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // ボタンの入力方向を取得する
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;


            // 入力がある場合は、プレイヤーの回転角度を更新する
            if (_input.move != Vector2.zero)
            {
                // 入力方向を元にプレイヤーの回転角度を計算する
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _playerCamera.currentCamera.transform.eulerAngles.y;

                // プレイヤーの回転角度を補間して更新したのを変数に格納する
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // プレイヤーの回転角度を更新する
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            //指定した回転角度を元に、プレイヤーの移動方向を計算する
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // プレイヤーを移動させる＋プレイヤーのジャンプ(落下)も考慮する
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            _playerAnimation.PlayerMoveAnimatior(_animationBlend,inputMagnitude);
        }


        //-----------------------------------------------------
        //プレイヤーのジャンプや重力、落下などの処理
        //-----------------------------------------------------
        public void PlayerJumpAndGravity(StarterAssetsInputs _input)
        {
            // 地面にいる場合の処理
            if (Grounded)
            {
                // 落ちてくる速度をリセットする
                _fallTimeoutDelta = FallTimeout;

                //アニメーションのリセット
                _playerAnimation.PlayerJumpAnimatorFalse();
        
                    // 今までの落下速度が0より小さい場合は、落下速度を-2fにする
                    if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // ジャンプの入力があり、ジャンプのクールタイムが0以下の場合はジャンプする
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // 飛ぶ力を計算する
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    _playerAnimation.PlayerJumpAnimtor();
                }

                // 一応、ジャンプのクールタイムを減らす
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // ジャンプのクールタイムをリセットする
                _jumpTimeoutDelta = JumpTimeout;

                // 落ちるアニメーションまでの時間を減らす
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    _playerAnimation.PlayerfallAnimatorFall();
                }

                // ジャンプの入力をリセットする
                _input.jump = false;
            }

            // 重力を適用する(重力が終端速度に達するまで)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
    }
}
