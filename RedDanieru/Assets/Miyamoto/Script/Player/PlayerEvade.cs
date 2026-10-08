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



        private void Start()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
            _actionPriority = GetComponent<PlayerInputPriority>();
            _controller = GetComponent<CharacterController>();
            _playerStatus = GetComponent<PlayerStatus>();
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

            // プレイヤーの入力の大きさを取得する(ゲームパッドなら入力の大きさをPCなら1f)
            _inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            // ボタンの入力方向を取得する
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized; 
            //プレイヤーの入力方向がある場合はプレイヤーが現在移動している方向に回避する
            if (inputDirection != Vector3.zero) 
            {
                _evadeDirection = new Vector3(_controller.velocity.x, 0.0f,_controller.velocity.z).normalized;
            } 
            // ないからしゃーなしプレイヤーの正面方向に回避する
            else 
            {
                _evadeDirection = transform.forward;
            }
            _animationBlend = _playerStatus._playerEvadeSpeed;

            // プレイヤーの回避アニメーションを再生する(今は仮実装のなので走るモーションで代用する)
            _playerAnimation.PlayerEvadeAnimation();

        }

        //----------------------------------------------------------
        // 回避の処理
        //----------------------------------------------------------
        private void EvadeMove()
        {
           
            // 一定速度で回避
            _controller.Move( _evadeDirection * _evadeSpeed * Time.deltaTime);

            // 回避出来る時間を減らす
            _evadeTimer -= Time.deltaTime;

            _playerStatus.PlayerDefenseUp(1000);
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

            _controller.Move( _evadeDirection * currentSpeed * Time.deltaTime);

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


    
