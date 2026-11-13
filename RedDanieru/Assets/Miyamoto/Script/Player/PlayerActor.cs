using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using Fusion;
#endif
namespace Player
{
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    /// <summary>
    /// プレイヤーに関係する各スクリプトをまとめて管理するためのスクリプト
    ///</summary>
    public class PlayerActor : /*MonoBehaviour*/NetworkBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private StarterAssetsInputs _input;              // プレイヤーの入力を制御するためのシステム
        private PlayerInputPriority _actionPriority;     // プレイヤーのアクションの優先度を制御するためのもの
        private PlayerAnimation     _animation;　　　　　// プレイヤーのアニメーションを再生させたり、制御するためのもの
        private PlayerAttack        _playerAttack;　　　 // プレイヤーの攻撃の当たり判定やダメージを与えたりするためのもの
        private PlayerCamera        _playerCamera;       // プレイヤーのカメ
        private PlayerMovement      _playerMovement;
        private StickerCheck        _stickerCheck;
        private NetworkMecanimAnimator _networkAnimator;
        private PlayerStatus        _playerStatus;
        private DeadCameraMulti     _deadMultiCamera;
        private PlayerEvade         _playerEvade;


        private bool _debugMode = false;

        private bool _deathProcessed;
        private bool _clearProcessed;

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


        //--------------------------------------------------------
        // 初期化
        //--------------------------------------------------------
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

            _playerStatus = GetComponent<PlayerStatus>();

            _deadMultiCamera = GetComponent<DeadCameraMulti>();

            _playerEvade = GetComponent<PlayerEvade>();
        }

        //public override void Spawned()
        //{
        //    _networkAnimator = GetComponent<NetworkMecanimAnimator>();

        //    if (!HasInputAuthority)
        //    {
        //        // 相手キャラは入力禁止
        //        if (_playerInput != null)
        //            _playerInput.enabled = false;

        //        _playerCamera.DisableCamera();
        //    }

        //    Debug.Log(
        //        $"{gameObject.name} Authority={HasInputAuthority}"
        //    );
        //}

        //public override void FixedUpdateNetwork()
        //{
        //    //コンポーネントを取得できているか
        //    _animation.AnimatorComPonent();

        //    // 地面にいるかどうかの判定
        //    _playerMovement.GroundedCheck();

        //    // 入力を取得
        //    _actionPriority.CheckInput(_input, _playerMovement.Grounded);


        //    Debug.Log(_actionPriority.currentActionType);

        //    if (_actionPriority.currentActionType == ActionType.Move ||
        //        _actionPriority.currentActionType == ActionType.None ||
        //        _actionPriority.currentActionType == ActionType.Jump)
        //    {
        //        // プレイヤーの移動処理
        //        _playerMovement.PlayerMove(_input);

        //        // ジャンプと重力の処理
        //        _playerMovement.PlayerJumpAndGravity(_input);
        //    }

        //    if (_actionPriority.currentActionType == ActionType.Sticker)
        //    {
        //        // プレイヤーのスティッカー使用処理
        //        _stickerCheck.StickerAndWallCheck(_input);
        //    }

        //    if (_actionPriority.currentActionType == ActionType.Attack)
        //    {
        //        // プレイヤーの攻撃処理
        //        _playerAttack.Attack(_input);
        //    }

        //    if (_actionPriority.currentActionType == ActionType.CameraChange)
        //    {
        //        // カメラ変更処理
        //        _playerCamera.CameraChange(_input);
        //    }
        //}

        // ソロ・マルチ共通のUpdate
        // マルチでは自分が操作するキャラだけ動かす（他の人のキャラはNetworkTransformで同期される）
        // ※以前はFixedUpdateNetworkでも呼んでいたため、マルチだと1フレームに2回動いていた
        private void FixedUpdate()
        {
            if (Object != null && Object.IsValid && !HasStateAuthority)
                return;

            UpdatePlayer();
        }

        //--------------------------------------------------------
        // プレイヤーの更新処理
        //--------------------------------------------------------
        private void UpdatePlayer()
        {
            GameOverManager gameOver = GameOverManager.Instance;

            if (gameOver != null && gameOver.IsGameOver)
            {
                return;
            }

            if (_playerStatus._isDead && !_deathProcessed)
            {
                _deathProcessed = true;

                HidePlayer();
            }

            if (!_playerStatus._isDead)
            {
                //コンポーネントを取得できているか
                _animation.AnimatorComPonent();

                // 地面にいるかどうかの判定
                _playerMovement.GroundedCheck();

                // プレイヤーの入力を取得して、何の動きをするかを決定する
                _actionPriority.CheckInput(_input, _playerMovement.Grounded);

                if (_actionPriority.currentActionType == ActionType.Move ||
                    _actionPriority.currentActionType == ActionType.None ||
                    _actionPriority.currentActionType == ActionType.Jump)
                {
                    // プレイヤーの移動処理
                    _playerMovement.PlayerMove(_input);

                    // ジャンプと重力の処理
                    _playerMovement.PlayerJumpAndGravity(_input);
                }

                if (_actionPriority.currentActionType == ActionType.Evade)
                {
                    // 攻撃中に回避した場合だけ攻撃を中断する
                    if (_actionPriority.IsCancelAttackByEvade)
                    {
                        _playerAttack.CancelAttack();
                    }

                    // 回避処理
                    _playerEvade.Evade(_input);
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
            else if (GameModeManager.IsMultiplayer && _playerStatus._isDead)
            {
                _actionPriority.CheckInput(_input, _playerMovement.Grounded);   

                // カメラの移動処理
                _deadMultiCamera.CameraMoveFiexdUpdate(_input);
                  
            }
        }

        //--------------------------------------------------------
        // カメラの更新処理をLateUpdateで呼ぶプレイヤーの動きに追従させるため
        //--------------------------------------------------------
        private void LateUpdate()
        {
            CameraContoller();
        }


        //--------------------------------------------------------
        // カメラの更新処理
        //--------------------------------------------------------
        public void CameraContoller()
        {
            // プレイヤーが死んでいない場合は通常のカメラ操作
            if (!_playerStatus._isDead)
            {
                //カメラの動き
                _playerCamera.CameraLateUpdate(IsCurrentDeviceMouse, _input);
            }

            // プレイヤーが死んでいる場合は死んだ後のカメラ操作
            else if (GameModeManager.IsMultiplayer && _playerStatus._isDead)
            {
                //カメラの動き
                _playerCamera.DeadPlayerCameraMove(IsCurrentDeviceMouse, _input);
            }
        }

        //--------------------------------------------------------
        // プレイヤーが死んだ時の処理
        //--------------------------------------------------------
        private void HidePlayer()
        {
            // ソロでは通信していないのでRPCは使えない
            if (Object == null || !Object.IsValid)
            {
                HidePlayerLocal();
                return;
            }

            RPC_HidePlayer();

            NetworkAuthorityController authority = GetComponent<NetworkAuthorityController>();

            if (authority != null)
            {
                authority.MarkDead();
            }

            if (NetworkGameState.Instance != null)
            {
                NetworkGameState.Instance.RPC_PlayerDied();
            }
        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_HidePlayer()
        {
            HidePlayerLocal();
        }

        private void HidePlayerLocal()
        {
            CharacterController cc =
                GetComponent<CharacterController>();

            if (cc != null)
            {
                cc.enabled = false;
            }

            Renderer[] renderers =
                GetComponentsInChildren<Renderer>();

            Debug.Log($"RendererCount={renderers.Length}");

            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }

            Debug.Log($"{name} 非表示");
        }
    }
}
