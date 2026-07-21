using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;
#endif
namespace StarterAssets
{
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class PlayerActor : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private StarterAssetsInputs _input;              // プレイヤーの入力を制御するためのフィールド
        private PlayerInputPriority _actionPriority;      // プレイヤーのアクションの優先度を制御するためのフィールド
        private PlayerAnimation     _animation;　　　　　// プレイヤーのアニメーションを制御するためのフィールド
        private PlayerAttack        _playerAttack;
        private PlayerCamera        _playerCamera;
        private PlayerMovement      _playerMovement;
        private StickerCheck        _stickerCheck;

        private bool _debugMode = false;

        // 現在の入力デバイスがマウスかどうかを判定するプロパティ
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }
        private void Start()
        {    
            // プレイヤーの入力を制御するためのStarterAssetsInputsコンポーネント
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            // プレイヤーの入力を制御するためのPlayerInputコンポーネント
            _playerInput = GetComponent<PlayerInput>();
#else

#endif
            //プレイヤーのアクションの優先度を決めるためのコンポーネント
            _actionPriority = GetComponent<PlayerInputPriority>();
            //プレイヤーのアニメーションを管理するためのコンポーネント
            _animation     = GetComponent<PlayerAnimation>();
            //プレイヤーの攻撃を管理するためのコンポーネント
            _playerAttack  = GetComponent<PlayerAttack>();
            //プレイヤーのカメラを管理するためのコンポーネント
            _playerCamera  = GetComponent<PlayerCamera>();
            //プレイヤーの基本的な動きを管理するためのコンポーネント
            _playerMovement= GetComponent<PlayerMovement>();
            //プレイヤーのステッカーの基本的な処理を行うコンポーネント
            _stickerCheck　= GetComponent<StickerCheck>();
        }

        private void FixedUpdate()
        {
            //コンポーネントを取得できているか
            _animation.AnimatorComPonent();

            // 地面にいるかどうかの判定
            _playerMovement.GroundedCheck();

            // 入力を取得
            _actionPriority.CheckInput(_input, _playerMovement.Grounded);


            Debug.Log(_actionPriority.currentActionType);

            if (_actionPriority.currentActionType == ActionType.Move ||
                _actionPriority.currentActionType == ActionType.None || 
                _actionPriority.currentActionType == ActionType.Jump)
            {
                // プレイヤーの移動処理
                _playerMovement.PlayerMove(_input);

                // ジャンプと重力の処理
                _playerMovement.PlayerJumpAndGravity(_input);
            }

            if (_actionPriority.currentActionType == ActionType.Sticker)
            {
                // プレイヤーのスティッカー使用処理
                _stickerCheck.StickerAndWallCheck(_input);
            }

            if (_actionPriority.currentActionType == ActionType.Attack)
            {
                // プレイヤーの攻撃処理
                _playerAttack.Attack(_input);
            }

            if (_actionPriority.currentActionType == ActionType.CameraChange)
            {
                // カメラ変更処理
                _playerCamera.CameraChange(_input);
            }
        }

        private void LateUpdate()
        {
            //カメラの動き
            _playerCamera.CameraLateUpdate(IsCurrentDeviceMouse, _input);
        }
    }
}
