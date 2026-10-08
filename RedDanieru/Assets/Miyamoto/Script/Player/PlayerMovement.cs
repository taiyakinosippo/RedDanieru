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

        [Tooltip("敵のレイヤー（未設定ならEnemy）。敵とは物理的にぶつからず、重なった分だけ横に押し出す")]
        public LayerMask EnemyLayers;

        [Tooltip("敵から押し出される最大の速さ")]
        public float EnemyPushOutSpeed = 6.0f;

        // プレイヤーの基礎設定を格納する変数
        public float _speed { get; private set; }        // プレイヤーの現在の速度
        private float _animationBlend;                   // アニメーションへのブレンド値
        private float _targetRotation = 0.0f;            // プレイヤーの目標回転角度
        private float _rotationVelocity;                 // プレイヤーの左右速度
        private float _verticalVelocity;                 // プレイヤーの上下速度
        private float _terminalVelocity = 53.0f;         // 落下速度の上限値

        // 次のジャンプが出来る時間
        private float _jumpTimeoutDelta;
        // 落下アニメーションに入るまでの時間
        private float _fallTimeoutDelta;
        private bool _wasGrounded;                                 // 前回のフレームで地面にいたかどうかを判定する変数

        private CharacterController _controller;         // プレイヤーの移動を制御するためのCharacterControllerコンポーネント
        private PlayerCamera        _playerCamera;       // プレイヤーのカメラを制御するためのコンポーネント
        private PlayerAnimation     _playerAnimation;    
        private PlayerInputPriority _actionPriority;
        private PlayerStatus        _playerStatus;

        // 押し出し判定用（毎回配列を作らないように使い回す）
        private readonly Collider[] _enemyHits = new Collider[16];

        private void Start()
        {
            //プレイヤーのカメラを制御するためのコンポーネント
            _playerCamera = GetComponent<PlayerCamera>();

            // キャラクターの動きを制御するためのCharacterControllerコンポーネント
            _controller = GetComponent<CharacterController>();

            _playerAnimation = GetComponent<PlayerAnimation>();

            _actionPriority = GetComponent<PlayerInputPriority>();

            _playerStatus = GetComponent<PlayerStatus>();

            _jumpTimeoutDelta = JumpTimeout;　　　　　// ジャンプできるようになるまでの時間を初期化
            _fallTimeoutDelta = FallTimeout;          // 落下アニメーションに入るまでの時間を初期化
            _wasGrounded = Grounded;

            if (EnemyLayers.value == 0)
            {
                EnemyLayers = LayerMask.GetMask("Enemy");
            }

            // 敵と物理的にぶつかると、敵に囲まれたときに上へ押し出されて敵の上に乗ってしまう
            // そのため敵とはぶつからないようにし、重なった分はGetEnemyPushOutで横方向にだけ押し出す
            _controller.excludeLayers |= EnemyLayers;
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
            if (GameStopManager.IsPaused)
                return;

            // Shiftキーを押している場合は走り、押していない場合は歩く
            float targetSpeed =  _playerStatus._playerMoveSpeed;

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

            // プレイヤーを移動させる＋プレイヤーのジャンプ(落下)も考慮する＋敵と重なっていたら横に押し出す
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime +GetEnemyPushOut());

            _playerAnimation.PlayerMoveAnimatior(_animationBlend, inputMagnitude);
        }


        //-----------------------------------------------------
        //敵と重なっている分だけ、横方向（水平）にだけ押し出す
        //上には押し出さないので、敵に囲まれても地面に立ったままになる
        //-----------------------------------------------------
        private Vector3 GetEnemyPushOut()
        {
            float radius = _controller.radius + _controller.skinWidth;

            Vector3 center = transform.TransformPoint(_controller.center);

            float halfHeight = Mathf.Max(0.0f, _controller.height * 0.5f - _controller.radius);

            int count = Physics.OverlapCapsuleNonAlloc(
                center + Vector3.up * halfHeight,
                center - Vector3.up * halfHeight,
                radius,
                _enemyHits,
                EnemyLayers,
                QueryTriggerInteraction.Ignore);

            Vector3 push = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                Collider enemy = _enemyHits[i];

                // 敵の表面のうち、プレイヤーの中心に一番近い点から離れる向き
                Vector3 away = center - enemy.ClosestPoint(center);
                away.y = 0.0f;

                float distance = away.magnitude;

                // 中心が敵の中に入っているときは、敵の中心から離れる向き
                if (distance < 0.001f)
                {
                    away = center - enemy.bounds.center;
                    away.y = 0.0f;
                    distance = 0.0f;

                    if (away.sqrMagnitude < 0.0001f)
                    {
                        away = -transform.forward;
                    }
                }

                push += away.normalized * Mathf.Max(0.0f, radius - distance);
            }

            push.y = 0.0f;

            return Vector3.ClampMagnitude(push, EnemyPushOutSpeed * Time.deltaTime);
        }

        //-----------------------------------------------------
        //プレイヤーのジャンプや重力、落下などの処理
        //-----------------------------------------------------
        public void PlayerJumpAndGravity(StarterAssetsInputs _input)
        {
            if (GameStopManager.IsPaused)
                return;

            // 空中から地面に戻った瞬間
            if (Grounded && !_wasGrounded)
            {
                Debug.Log("着地しました");

                if (_actionPriority.currentActionType == ActionType.Jump)
                {
                    _actionPriority.EndAction();
                }

                _input.jump = false;
            }

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
            }

            // 重力を適用する(重力が終端速度に達するまで)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            // 今回の地面状態を保存
            _wasGrounded = Grounded;
        }
    }
}
