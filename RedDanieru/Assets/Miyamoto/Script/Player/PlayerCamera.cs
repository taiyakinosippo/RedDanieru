using UnityEngine;

namespace Player
{
    /// <summary>
    /// プレイヤーのカメラを制御するためのスクリプト
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        [Header("カメラの機能")]

        [Tooltip("3人称視点のカメラ")]
        public GameObject ThirdPersonPerspective;

        [Tooltip("1人称視点のカメラ")]
        public GameObject FirstPersonPerspective;


        [Header("カメラ角度")]

        [Tooltip("カメラを上に移動できる最大角度")]
        public float TopClamp = 70.0f;

        [Tooltip("カメラを下に移動できる最大角度")]
        public float BottomClamp = -30.0f;

        [Tooltip("カメラの角度")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("カメラを動かせるかどうか")]
        public bool LockCameraPosition = false;


        [Header("壁判定")]

        [Tooltip("壁として扱うレイヤー")]
        public LayerMask _wallLayer;

        [Tooltip("SphereCastで使用する球の半径")]
        public float _sphereSize = 0.5f;

        [Tooltip("壁から離す距離")]
        public float _wallOffset = 0.1f;

        [Tooltip("壁に接触したときのカメラの最低距離")]
        public float _minimumCameraDistance = 0.5f;


        [Header("カメラ追従")]

        [Tooltip("壁に当たったときのカメラの移動速度")]
        public float _followSpeed = 10.0f;

        [Tooltip("プレイヤーからのカメラの高さ")]
        public float _cameraHeight = 1.0f;


        // 左右のカメラ角度
        private float _cinemachineTargetYaw;

        // 上下のカメラ角度
        private float _cinemachineTargetPitch;

        private const float _threshold = 0.01f;


        // プレイヤーとカメラの初期位置関係
        private Vector3 diff;

        // プレイヤーのカメラ中心位置
        private Vector3 _playerPosition;


        // 現在使用しているカメラ
        public GameObject currentCamera { get; private set; }

        // 一人称視点かどうか
        public bool isFirstPerson { get; private set; } = false;


        // 入力優先度管理
        private PlayerInputPriority _actionPriority;


        // プレイヤー自身が壁に接触しているか
        private bool _isPlayerTouchingWall;


        private void Awake()
        {
            // 現在使用するカメラを取得
            currentCamera =
                ThirdPersonPerspective.activeSelf
                ? ThirdPersonPerspective
                : FirstPersonPerspective;

            // カメラの初期角度を保存
            _cinemachineTargetYaw =
                currentCamera.transform.rotation.eulerAngles.y;

            _cinemachineTargetPitch =
                currentCamera.transform.rotation.eulerAngles.x;

            // InputPriority取得
            _actionPriority = GetComponent<PlayerInputPriority>();

            // プレイヤーのカメラ中心位置
            _playerPosition =
                transform.position +
                Vector3.up * _cameraHeight;

            // プレイヤーから見たカメラの相対位置を保存
            diff =
                currentCamera.transform.position -
                _playerPosition;
        }


        /// <summary>
        /// カメラを無効化する
        /// </summary>
        public void DisableCamera()
        {
            if (ThirdPersonPerspective != null)
                ThirdPersonPerspective.SetActive(false);

            if (FirstPersonPerspective != null)
                FirstPersonPerspective.SetActive(false);
        }


        /// <summary>
        /// 1人称 / 3人称カメラを切り替える
        /// </summary>
        public void CameraChange(StarterAssetsInputs _input)
        {
            _input.cameraChange = false;

            isFirstPerson = !isFirstPerson;

            FirstPersonPerspective.SetActive(isFirstPerson);
            ThirdPersonPerspective.SetActive(!isFirstPerson);

            currentCamera =
                isFirstPerson
                ? FirstPersonPerspective
                : ThirdPersonPerspective;

            _cinemachineTargetYaw =
                currentCamera.transform.rotation.eulerAngles.y;

            _cinemachineTargetPitch =
                currentCamera.transform.rotation.eulerAngles.x;

            _actionPriority.EndAction();
        }


        /// <summary>
        /// カメラの回転・位置を更新する
        /// </summary>
        public void CameraLateUpdate(
            bool _IsCurrentDeviceMouse,
            StarterAssetsInputs _input)
        {
            // ========================================
            // ① カメラ入力
            // ========================================

            if (_input.look.sqrMagnitude >= _threshold &&
                !LockCameraPosition)
            {
                float deltaTimeMultiplier =
                    _IsCurrentDeviceMouse
                    ? 1.0f
                    : Time.deltaTime;

                _cinemachineTargetYaw +=
                    _input.look.x * deltaTimeMultiplier;

                _cinemachineTargetPitch +=
                    _input.look.y * deltaTimeMultiplier;
            }


            // ========================================
            // ② カメラ角度を制限
            // ========================================

            _cinemachineTargetYaw =
                ClampAngle(
                    _cinemachineTargetYaw,
                    float.MinValue,
                    float.MaxValue);

            _cinemachineTargetPitch =
                ClampAngle(
                    _cinemachineTargetPitch,
                    BottomClamp,
                    TopClamp);


            // ========================================
            // ③ カメラの回転を作る
            // ========================================

            Quaternion cameraRotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch +
                    CameraAngleOverride,

                    _cinemachineTargetYaw,

                    0.0f);


            currentCamera.transform.rotation =
                cameraRotation;


            // ========================================
            // ④ プレイヤーのカメラ中心位置
            // ========================================

            _playerPosition =
                transform.position +
                Vector3.up * _cameraHeight;


            // ========================================
            // ⑤ カメラの理想位置を計算
            // ========================================

            // 初期のプレイヤー→カメラ距離を
            // 現在のカメラ回転に合わせて回転させる
            Vector3 rotatedDiff =
                cameraRotation * diff;

            // プレイヤー位置 + 回転後の距離
            Vector3 targetPosition =
                _playerPosition + rotatedDiff;


            // ========================================
            // ⑥ プレイヤー → 理想カメラの方向
            // ========================================

            Vector3 direction =
                targetPosition - _playerPosition;

            float distance =
                direction.magnitude;

            Vector3 normalizedDirection =
                direction.normalized;


            // ========================================
            // デバッグ用Ray
            // ========================================

            Debug.DrawRay(
                _playerPosition,
                normalizedDirection * distance,
                Color.red);


            // ========================================
            // ⑦ カメラ経路に壁があるか調べる
            // ========================================

            bool isWallDetected =
                Physics.SphereCast(
                    _playerPosition,
                    _sphereSize,
                    normalizedDirection,
                    out RaycastHit hit,
                    distance,
                    _wallLayer,
                    QueryTriggerInteraction.Ignore);


            // ========================================
            // ⑧ プレイヤー自身が壁に接触しているか
            // ========================================

            bool isPlayerTouchingWall =
            Physics.CheckSphere(
           _playerPosition,
           _sphereSize,
           _wallLayer,
           QueryTriggerInteraction.Ignore);


            // ========================================
            // ⑨ カメラ位置を決定
            // ========================================

            // ========================================
            // ⑨ カメラ位置を決定
            // ========================================

            if (isPlayerTouchingWall)
            {
                // ------------------------------------
                // プレイヤー自身が壁に接触
                // ------------------------------------

                Vector3 safePosition =
                    _playerPosition +
                    normalizedDirection *
                    _minimumCameraDistance;

                // 壁に接触しているときはゆっくり寄せる
                currentCamera.transform.position =
                    Vector3.Lerp(
                        currentCamera.transform.position,
                        safePosition,
                        _followSpeed * Time.deltaTime);

                Debug.DrawLine(
                    _playerPosition,
                    safePosition,
                    Color.blue);
            }
            else if (isWallDetected)
            {
                // ------------------------------------
                // カメラとプレイヤーの間に壁がある
                // ------------------------------------

                float safeDistance =
                    hit.distance - _wallOffset;

                safeDistance =
                    Mathf.Max(
                        safeDistance,
                        _minimumCameraDistance);

                Vector3 safePosition =
                    _playerPosition +
                    normalizedDirection *
                    safeDistance;

                // 壁に当たったときはゆっくり寄せる
                currentCamera.transform.position =
                    Vector3.Lerp(
                        currentCamera.transform.position,
                        safePosition,
                        _followSpeed * Time.deltaTime);

                Debug.DrawLine(
                    _playerPosition,
                    safePosition,
                    Color.yellow);
            }
            else
            {
                // ------------------------------------
                // 壁がない
                // ------------------------------------

                // 理想位置へゆっくり戻す
                currentCamera.transform.position =
                    Vector3.Lerp(
                        currentCamera.transform.position,
                        targetPosition,
                        _followSpeed * Time.deltaTime);

                Debug.DrawLine(
                    _playerPosition,
                    targetPosition,
                    Color.green);
            }


            // ========================================
            // ⑩ カメラをプレイヤーへ向ける
            // ========================================

            currentCamera.transform.LookAt(
                _playerPosition);
        }

        /// <summary>
        /// 角度を制限する
        /// </summary>
        private static float ClampAngle(
            float lfAngle,
            float lfMin,
            float lfMax)
        {
            if (lfAngle < -360f)
                lfAngle += 360f;

            if (lfAngle > 360f)
                lfAngle -= 360f;

            return Mathf.Clamp(
                lfAngle,
                lfMin,
                lfMax);
        }
    }
}