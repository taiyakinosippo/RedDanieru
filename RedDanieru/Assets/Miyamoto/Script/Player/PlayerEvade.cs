using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerEvade : MonoBehaviour
    {
        [Tooltip("回避の時間")]  
        [SerializeField] private float _evadeTime = 0.3f;

        [Tooltip("回避後の減速時間")]
        [SerializeField] private float _decelerationTime = 0.2f;

        [Tooltip("回避の時の回転の補間時間")]
        [SerializeField] private float _rotationSmoothTime = 0.1f;

        private PlayerAnimation     _playerAnimation;    // プレイヤーのアニメーションを管理しているスクリプト
        private PlayerInputPriority _actionPriority;     // プレイヤーのアクションの優先度を管理しているスクリプト
        private PlayerStatus        _playerStatus;　　　 // プレイヤーのステータスを管理しているスクリプト
        private CharacterController _controller;         // プレイヤーの移動を制御するためのCharacterControllerコンポーネント
        private float _evadeTimer;
        private float _decelerationTimer;
        private float _animationBlend;
        private Vector3 _evadeDirection;
        private float _evadeSpeed;
        private float _inputMagnitude;

        private float _targetRotation;

        private float _rotationVelocity;
        private PlayerCamera _playerCamera;



        private void Start()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
            _actionPriority = GetComponent<PlayerInputPriority>();
            _controller = GetComponent<CharacterController>();
            _playerStatus = GetComponent<PlayerStatus>();
            _playerCamera = GetComponent<PlayerCamera>();
            _evadeSpeed = _playerStatus._playerEvadeSpeed;
        }

        //----------------------------------------------------------
        //プレイヤーの回避処理を時間に応じて変更する
        //----------------------------------------------------------
        public void Evade(StarterAssetsInputs _input)
        {　
            // 回避を開始する条件を満たしている場合、回避をするための準備をする
            if (_evadeTimer <= 0.0f && _decelerationTimer <= 0.0f)
            {
                StartEvade(_input);
            }

            // 回避できる時間が残っている場合は回避をする
            if (_evadeTimer > 0.0f)
            {
                EvadeMove();
            }
            // 減速時間が残っている場合は回避した後に減速をする
            else if (_decelerationTimer > 0.0f)
            {
                Deceleration(_input);
            }
        }
        
        private void StartEvade(StarterAssetsInputs _input)
        {
            // 回避時間をセット
            _evadeTimer = _evadeTime;

            _playerStatus.PlayerDefenseUp(1000);

            // プレイヤーの入力の大きさを取得する(ゲームパッドなら入力の大きさをPCなら1f)
            _inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            // ボタンの入力方向を取得する
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized; 

            //プレイヤーの入力方向がある場合はプレイヤーが現在移動している方向に回避する
            if (inputDirection != Vector3.zero) 
            {

                // 入力方向を元にプレイヤーの回転角度を計算する
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                _playerCamera.currentCamera.transform.eulerAngles.y;

                // プレイヤーの回転角度を補間して更新したのを変数に格納する
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                 _rotationSmoothTime);

                // プレイヤーの回転角度を更新する
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                //指定した回転角度を元に、プレイヤーの移動方向を計算する
                _evadeDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            } 
            // ないから回避を行わない
            else 
            {
                // 入力がない場合は現在向いている方向に回避
                _evadeDirection = transform.forward;
            }
            // プレイヤーの回避アニメーションを再生する(今は仮実装のなので走るモーションで代用する)
            _playerAnimation.PlayerEvadeAnimation();

        }

        //----------------------------------------------------------
        // 回避の処理
        //----------------------------------------------------------
        private void EvadeMove()
        {
           
            // 一定速度で回避
            _controller.Move( _evadeDirection.normalized * _evadeSpeed * Time.deltaTime);

            // 回避出来る時間を減らす
            _evadeTimer -= Time.deltaTime;

            // 回避終了
            if (_evadeTimer <= 0.0f)
            {
                _evadeTimer = 0.0f;

                // 減速時間を開始
                _decelerationTimer = _decelerationTime;

                Debug.Log("回避終了 → 減速開始");
            }
        }

        //----------------------------------------------------------
        // 減速の処理
        //----------------------------------------------------------
        private void Deceleration(StarterAssetsInputs _input)
        {
           
            // 残り時間の割合
            float t =_decelerationTimer / _decelerationTime;

            // 速度を徐々に下げる
            float currentSpeed = _evadeSpeed * t;

            _controller.Move( _evadeDirection.normalized * currentSpeed * Time.deltaTime);

            // タイマーを減らす
            _decelerationTimer -= Time.deltaTime;

            // 減速終了
            if (_decelerationTimer <= 0.0f)
            {
                _playerStatus.PlayerDefenseDown();
                _decelerationTimer = 0.0f;
                _playerAnimation.PlayerEvadeAnimationEnd();
                _input.evade = false;
                _actionPriority.EndAction();
                Debug.Log("回避終了");
            }
        }
    }

}


    
