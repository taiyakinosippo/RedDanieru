using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

///<summry>
///
///</summry>
namespace Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("カメラの機能")]
        [Tooltip("このGameObjectはカメラで使用されます")]

        public GameObject ThirdPersonPerspective;           //3人称視点
        public GameObject FirstPersonPerspective;　　　　　 //1人称視点


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

        private float _targetRotation = 0.0f;            // プレイヤーの目標回転角度
        private float _rotationVelocity;                 // プレイヤーの左右速度
        private float _verticalVelocity;                 // プレイヤーの上下速度


#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private StarterAssetsInputs _input;              // プレイヤーの入力を制御するためのStarterAssetsInputsコンポーネント

        private const float _threshold = 0.01f;          // 入力の大きさを判定するための定数

        private GameObject currentCamera;        　　　　// 現在のカメラを格納する変数
        private bool isFirstPerson = false;              // 現在のカメラが一人称視点かどうかを判定する変数

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
            ? ThirdPersonPerspective : FirstPersonPerspective;

            _cinemachineTargetYaw = currentCamera.transform.rotation.eulerAngles.y;

            // プレイヤーの入力を制御するためのStarterAssetsInputsコンポーネント
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            // プレイヤーの入力を制御するためのPlayerInputコンポーネント
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
        }

      　void CameraChange()
        {
            _input.cameraChange = false;
            isFirstPerson = !isFirstPerson;

            FirstPersonPerspective.SetActive(isFirstPerson);
            ThirdPersonPerspective.SetActive(!isFirstPerson);

            currentCamera = isFirstPerson ? FirstPersonPerspective : ThirdPersonPerspective;

            _cinemachineTargetYaw = currentCamera.transform.rotation.eulerAngles.y;

            _cinemachineTargetPitch = currentCamera.transform.rotation.eulerAngles.x;

            //EndAction();
        }
        void CameraLateUpdate()
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
        // 角度を制限する関数
        //-----------------------------------------------------------
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            //
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}
