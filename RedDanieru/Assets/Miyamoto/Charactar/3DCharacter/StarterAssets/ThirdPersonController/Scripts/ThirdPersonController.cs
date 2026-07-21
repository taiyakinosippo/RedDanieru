using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
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

        [Tooltip("足音")]
        public AudioSource AudioFootsteps;

        [Tooltip("着地音")]
        public AudioSource LandingAudio;

        [Tooltip("服がこすれる音")]
        public AudioSource AudioFoley;

        [Tooltip("着地したときの音データ")]
        public AudioClip LandingAudioClip;

        [Tooltip("足音の音データ")]
        public AudioClip[] FootstepAudioClips;

        [Tooltip("足音の音量")]
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

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

        [Header("Cinemachine")]
        [Tooltip("このGameObjectはカメラで使用されます")]

        public GameObject ThirdPersonPerspective;
        public GameObject FirstPersonPerspective;


        [Tooltip("プレイヤーがカメラを上に移動できる最大角度")]
        public float TopClamp = 70.0f;

        [Tooltip("プレイヤーがカメラを下に移動できる最大角度")]
        public float BottomClamp = -30.0f;

        [Tooltip("カメラの角度")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("カメラを動かせるかどうか")]
        public bool LockCameraPosition = false;

        // 左右の角度
        private float _cinemachineTargetYaw;
        // 上下の角度
        private float _cinemachineTargetPitch;

        // プレイヤーの基礎設定を格納する変数
        private float _speed;　　　　　　　　　　　　　　// プレイヤーの現在の速度
        private float _animationBlend;                   // アニメーションへのブレンド値
        private float _targetRotation = 0.0f;            // プレイヤーの目標回転角度
        private float _rotationVelocity;                 // プレイヤーの左右速度
        private float _verticalVelocity;                 // プレイヤーの上下速度
        private float _terminalVelocity = 53.0f;　　　　 // 落下速度の上限値

        // 次のジャンプが出来る時間
        private float _jumpTimeoutDelta;
        // 落下アニメーションに入るまでの時間
        private float _fallTimeoutDelta;

        // アニメーションのIDを格納する変数
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDAttack;
        private int _animIDStickerPaste;
        private int _animIDStickerPeelOff;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;　　　　　　　　　　　// アニメーションを制御するためのAnimatorコンポーネント
        private CharacterController _controller;         // プレイヤーの移動を制御するためのCharacterControllerコンポーネント
        //private StarterAssetsInputs _input;              // プレイヤーの入力を制御するためのStarterAssetsInputsコンポーネント
       
        private const float _threshold = 0.01f;          // 入力の大きさを判定するための定数

        private bool _hasAnimator;                       // Animatorコンポーネントが存在するかどうかを判定するための変数

        private GameObject currentCamera;　　　　　　　　// 現在のカメラを格納する変数
        private bool isFirstPerson = false;              // 現在のカメラが一人称視点かどうかを判定する変数

        //行動管理
        private StarterAssetsInputs _input;
        // 現在実行中のアクション
        private ActionType currentActionType = ActionType.None;

        // 新しく押されたアクション
        private ActionType nextActionType = ActionType.None;

        // 待機中のアクション
        private Queue<ActionType> actionQueue = new Queue<ActionType>();

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

        //カメラが追従するオブジェクトを取得するためのAwakeメソッド
        private void Awake()
        {
            // get a reference to our main camera
            if (currentCamera == null)
            {
                currentCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        //初期化
        private void Start()
        {
            // 初期化時にカメラの角度を取得
            currentCamera = ThirdPersonPerspective.activeSelf
            ? ThirdPersonPerspective: FirstPersonPerspective;

            _cinemachineTargetYaw = currentCamera.transform.rotation.eulerAngles.y;
            // アニメーションの有無を判定し、Animatorコンポーネントを取得
            _hasAnimator = TryGetComponent(out _animator);

            // キャラクターの動きを制御するためのCharacterControllerコンポーネント
            _controller = GetComponent<CharacterController>();

            // プレイヤーの入力を制御するためのStarterAssetsInputsコンポーネント
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            // プレイヤーの入力を制御するためのPlayerInputコンポーネント
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
            //アニメーションをIDに変換
            AssignAnimationIDs();


            _jumpTimeoutDelta = JumpTimeout;　　　　　// ジャンプできるようになるまでの時間を初期化
            _fallTimeoutDelta = FallTimeout;          // 落下アニメーションに入るまでの時間を初期化
        }

        private void Update()
        {
            //Animatorコンポーネントを取得しているかの判定
            _hasAnimator = TryGetComponent(out _animator);

            // 入力を取得
            CheckInput();

            // 地面にいるかどうかの判定
            GroundedCheck();

            if (currentActionType == ActionType.Move || currentActionType == ActionType.None || currentActionType == ActionType.Jump)
            {
                // プレイヤーの移動処理
                Move();

                // ジャンプと重力の処理
                JumpAndGravity();
            }

            if (currentActionType == ActionType.Sticker)
            {
                // プレイヤーのスティッカー使用処理
                StickerPaste();
            }

            if (currentActionType == ActionType.Attack)
            {
                // プレイヤーの攻撃処理
                Attack();
            }

            if (currentActionType == ActionType.CameraChange)
            {
                // カメラ変更処理
                CameraChange();
            }
        }

       

        private void LateUpdate()
        {
            //カメラの追従や回転の処理
            CameraRotation();
        }

        //-----------------------------------------------------------
        //アニメーションのIDを取得する関数
        //-----------------------------------------------------------
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDAttack = Animator.StringToHash("Attack");
            _animIDStickerPaste = Animator.StringToHash("StickerPaste");
            _animIDStickerPeelOff = Animator.StringToHash("StickerPeelOff");

        }

        //-----------------------------------------------------------
        //入力を取得する関数
        //-----------------------------------------------------------
        private void CheckInput()
        {
            if (_input.attack && Grounded)
            {
                AddAction(ActionType.Attack);
            }

            if (_input.jump)
            {
                AddAction(ActionType.Jump);
            }

            if (_input.move != Vector2.zero)
            {
                AddAction(ActionType.Move);
            }

            if (_input.cameraChange)
            {
                Debug.Log("CameraChange");
                AddAction(ActionType.CameraChange);
            }
            
            if(_input.sticker)
            {
                AddAction(ActionType.Sticker);
            }

        }

        //-----------------------------------------------------------
        //アクションの優先度を取得する関数
        //-----------------------------------------------------------
        private int GetPriority(ActionType action)
        {
            switch (action)
            {
                case ActionType.Sticker:
                    return 100;

                case ActionType.Attack:
                    return 80;

                case ActionType.Jump:
                    return 70;
                    
                case ActionType.CameraChange:
                    return 30;

                case ActionType.Move:
                    return 10;

                case ActionType.None:
                    return 0;
            }

            return 0;
        }

 
        private ActionType AddAction(ActionType action)
        {
            // 今押されたアクション
            nextActionType = action;

            // 現在の行動より優先度が高いなら
            if (GetPriority(nextActionType) > GetPriority(currentActionType))
            {
                // 待機中を全部捨てる
                actionQueue.Clear();

                // 現在の行動を切り替える
                currentActionType = nextActionType;
            }
            else
            {
                // 優先度が低いので待機
                actionQueue.Enqueue(nextActionType);
            }

            return currentActionType;
        }
        private void EndAction()
        {
            currentActionType = ActionType.None;
        }
        //-----------------------------------------------------------
        //地面にいるかどうかの判定
        //-----------------------------------------------------------
        private void GroundedCheck()
        {
            // プレイヤーの下にある球体を使って地面にいるかどうかを判定
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

            // 地面にいるかどうかの判定を行う
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // アニメーションのパラメータを更新するfalseならアニメーションのパラメータを更新しない
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        //-----------------------------------------------------------
        //カメラの追従や回転の処理
        //-----------------------------------------------------------
        private void CameraRotation()
        {
            // カメラが動かせれていないかつロックされていないかどうか
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //マウスで操作している場合は1.0f、コントローラーで操作している場合はTime.deltaTimeを使用する
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                // カメラの左右角度を更新する
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;

                // カメラの上下角度を更新する
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // 横は無制限
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

            // 縦は制限あり
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // 画面を回転させる
            currentCamera.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        //-----------------------------------------------------------
        //プレイヤーの移動処理
        //-----------------------------------------------------------
        private void Move()
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
                                  currentCamera.transform.eulerAngles.y;

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

            // アニメーションのブレンド値を更新する
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        //-----------------------------------------------------------
        // ジャンプと重力の処理
        //-----------------------------------------------------------
        private void JumpAndGravity()
        {
            // 地面にいる場合の処理
            if (Grounded)
            {
                // 落ちてくる速度をリセットする
                _fallTimeoutDelta = FallTimeout;

                // ここでアニメーションをジャンプと落下のフラグをfalseにする
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

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

                    // アニメーションのジャンプのフラグをtrueにする
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
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
                    // 落下アニメーションを再生する
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // ジャンプの入力をリセットする
                _input.jump = false;
            }

            // 重力を適用する(重力が終端速度に達するまで)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
            EndAction();
        }

        //-----------------------------------------------------------
        // 角度を制限する関数
        //-----------------------------------------------------------
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            //
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        //-----------------------------------------------------------
        // プレイヤーの攻撃処理
        //-----------------------------------------------------------
        private void Attack()
        {
            if (_input.attack && _hasAnimator)
            {
                _animator.SetTrigger(_animIDAttack);
                _input.attack = false;
            }

        }

        //-----------------------------------------------------------
        // 攻撃アニメーションが終了したら呼ばれる関数
        //-----------------------------------------------------------
        public void OnAttackAnimationEnd()
        {
            _animator.ResetTrigger(_animIDAttack);
            Debug.Log("Attack animation end");
            EndAction();
        }

        //-----------------------------------------------------------
        // ステッカーを貼りつける処理
        //-----------------------------------------------------------
        private void StickerPaste()
        {
            if (_input.sticker && _hasAnimator)
            {
                _animator.SetTrigger(_animIDStickerPaste);
                _input.sticker = false;
            }
        }

        //-----------------------------------------------------------
        // ステッカーを貼り付ける処理が終了したら呼ばれる関数
        //-----------------------------------------------------------
        public void OnStickerAnimationEnd()
        {
            _animator.ResetTrigger(_animIDStickerPaste);
            Debug.Log("Sticker animation end");
            EndAction();
        }

        //-----------------------------------------------------------
        // カメラの切り替え処理
        //-----------------------------------------------------------
        private void CameraChange()
        {
            _input.cameraChange = false;
            isFirstPerson = !isFirstPerson;

            FirstPersonPerspective.SetActive(isFirstPerson);
            ThirdPersonPerspective.SetActive(!isFirstPerson);

            currentCamera = isFirstPerson ? FirstPersonPerspective : ThirdPersonPerspective;

            _cinemachineTargetYaw = currentCamera.transform.rotation.eulerAngles.y;

            _cinemachineTargetPitch = currentCamera.transform.rotation.eulerAngles.x;

            EndAction();
        }

        //debug用に、地面にいるかどうかの判定を可視化するためのGizmosを描画する関数
        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {

                if (AudioFootsteps != null)
                    AudioFootsteps.Play();
                if (AudioFoley != null)
                    AudioFoley.Play();
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudio != null)
                    LandingAudio.Play();

            }
        }
    }
}