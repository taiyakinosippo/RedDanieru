using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

///<summry>
///プレイヤーのカメラを制御するためのスクリプト
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

        [Tooltip("壁判定をするレイヤー")]
        public LayerMask _wallLayer;

        [Tooltip("飛ばすRayの大きさ")]
        public float _sphereSize = 0.5f;

        [Tooltip("壁からどれだけ離すか")]
        public float _wallOffset = 0.1f;

        [Tooltip("カメラの追従速度")]
        public float _followSpeed = 10.0f;

        [Tooltip("カメラの高さ")]
        public float _cameraHeight = 1.0f;

        // 左右の角度
        private float _cinemachineTargetYaw;
        // 上下の角度
        private float _cinemachineTargetPitch;

        private const float _threshold = 0.01f;          // 入力の大きさを判定するための定数

        private Vector3 diff;

        private Vector3 _playerPosition;

        public GameObject currentCamera { get; private set; }        　　　　// 現在のカメラを格納する変数
        public bool isFirstPerson { get; private set; } = false;              // 現在のカメラが一人称視点かどうかを判定する変数
        private PlayerInputPriority _actionPriority;
        //初期化
        private void Awake()
        {
            // 初期化時にカメラを取得
            currentCamera = ThirdPersonPerspective.activeSelf
            ? ThirdPersonPerspective : FirstPersonPerspective;

            // 初期化時にカメラの角度を取得
            _cinemachineTargetYaw = currentCamera.transform.rotation.eulerAngles.y;

            _actionPriority = GetComponent<PlayerInputPriority>();

            _playerPosition = transform.position + Vector3.up * _cameraHeight;
          
            diff =currentCamera.transform.position - _playerPosition;
        }

        //-------------------------------------------------
        // 相手プレイヤーのカメラを無効にする
        //-------------------------------------------------
        public void DisableCamera()
        {
            if (ThirdPersonPerspective != null)
                ThirdPersonPerspective.SetActive(false);

            if (FirstPersonPerspective != null)
                FirstPersonPerspective.SetActive(false);
        }
        //-------------------------------------------------
        //カメラを一人称か3人称に切り替える
        //-------------------------------------------------
        public void CameraChange(StarterAssetsInputs _input)
        {
            _input.cameraChange = false;
            isFirstPerson = !isFirstPerson;

            FirstPersonPerspective.SetActive(isFirstPerson);
            ThirdPersonPerspective.SetActive(!isFirstPerson);

            currentCamera = isFirstPerson ? FirstPersonPerspective : ThirdPersonPerspective;

            _cinemachineTargetYaw = currentCamera.transform.rotation.eulerAngles.y;

            _cinemachineTargetPitch = currentCamera.transform.rotation.eulerAngles.x;

            _actionPriority.EndAction();
        }

        //------------------------------------------------
        //カメラの向きを変更する
        //------------------------------------------------
        public void CameraLateUpdate(bool _IsCurrentDeviceMouse, StarterAssetsInputs _input)
        {
            // カメラが動かせれていないかつロックされていないかどうか
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //マウスで操作している場合は1.0f、コントローラーで操作している場合はTime.deltaTimeを使用する
                float deltaTimeMultiplier = _IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                // カメラの左右角度を更新する
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;

                // カメラの上下角度を更新する
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // 横は無制限
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);

            // 縦は制限あり
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            //カメラの回転を作る
            Quaternion cameraRotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);

            currentCamera.transform.rotation = cameraRotation;

            _playerPosition = transform.position + Vector3.up * _cameraHeight;

            Vector3 rotatedDiff = cameraRotation * diff;

            //==================================================
            // カメラの理想位置を計算
            //==================================================

            Vector3 targetPosition =
                _playerPosition + rotatedDiff;


            //==================================================
            // プレイヤーから理想カメラ位置への方向
            //==================================================

            Vector3 direction =
                targetPosition - _playerPosition;

            float distance =
                direction.magnitude;

            Vector3 normalizedDirection =
                direction.normalized;
            // SphereCastの方向をSceneビューに表示
            Debug.DrawRay(
                _playerPosition,
                normalizedDirection * distance,
                Color.red
            );
           
            //==================================================
            // 壁判定
            //==================================================

            if (Physics.SphereCast(
                _playerPosition,
                _sphereSize,
                normalizedDirection,
                out RaycastHit hit,
                distance,
                _wallLayer))
            {
                //==============================================
                // 壁がある
                //==============================================

                // 壁に接触する位置より少し手前
                float safeDistance =
                    hit.distance - _wallOffset;

                safeDistance =
                    Mathf.Max(safeDistance, 0.0f);

                // 壁の手前のカメラ位置
                Vector3 safePosition =
                    _playerPosition +
                    normalizedDirection * safeDistance;

                // 現在のカメラ位置から安全位置へゆっくり移動
                currentCamera.transform.position =
                    Vector3.Lerp(
                        currentCamera.transform.position,
                        safePosition,
                        _followSpeed * Time.deltaTime);
            }
            else
            {
                //==============================================
                // 壁がない
                //==============================================

                // 本来の位置に一瞬で戻す
                currentCamera.transform.position =
                    targetPosition;
            }
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
