using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
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
        public GameObject _deadCamera;
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
        public float _wallDistance = 0.1f;
        [Tooltip("カメラの追従速度")]
        public float _followSpeed = 10.0f;
        [Tooltip("プレイヤーのRayを受け取る高さ")]
        public Vector3 _playerRayOffset = new Vector3(0, 1.0f, 0);


        // 左右の角度
        private float _cinemachineTargetYaw;
        // 上下の角度
        private float _cinemachineTargetPitch;
        private const float _threshold = 0.01f;          // 入力の大きさを判定するための定数
        private Vector3 diff;
        private Vector3 _playerPosition;
        private Vector3 _rayPosition;
        public GameObject currentCamera { get; private set; }        　　　　// 現在のカメラを格納する変数
        public bool isFirstPerson { get; private set; } = false;              // 現在のカメラが一人称視点かどうかを判定する変数
        private PlayerInputPriority _actionPriority;
        private PlayerStatus _playerStatus;
        // 現在のカメラ距離
        private float _cameraDistance;
        // 壁がないときの通常距離
        private float _defaultCameraDistance;



        //初期化
        private void Awake()
        {
            // 死亡時カメラ
            if (_deadCamera != null)
            {
                _deadCamera.SetActive(false);
            }
            // 初期化時にカメラを取得
            currentCamera = ThirdPersonPerspective.activeSelf
            ? ThirdPersonPerspective : FirstPersonPerspective;
            // 初期化時にカメラの角度を取得
            _cinemachineTargetYaw = currentCamera.transform.rotation.eulerAngles.y;
            _actionPriority = GetComponent<PlayerInputPriority>();
            _playerStatus = GetComponent<PlayerStatus>();
            _playerPosition = transform.position + Vector3.up * _playerRayOffset.y;
            _rayPosition = transform.position + _playerRayOffset;
            Vector3 cameraOffset = currentCamera.transform.position - _playerPosition;
            _defaultCameraDistance = cameraOffset.magnitude;
            // 最初は通常距離
            _cameraDistance = _defaultCameraDistance;
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


            // カメラの回転中心
            _playerPosition = transform.position + Vector3.up * _playerRayOffset.y;

            // 壁判定の開始位置
            _rayPosition = transform.position + _playerRayOffset;

            Vector3 cameraDirection = cameraRotation * Vector3.back;

            Vector3 rotatedDiff = cameraRotation * diff;

            Debug.DrawRay(
             _rayPosition,
             cameraDirection * _defaultCameraDistance,
             Color.red
            );
            //==================================================
            // 壁判定
            //==================================================
            if (Physics.SphereCast(_rayPosition, _sphereSize, cameraDirection, out RaycastHit hit, _defaultCameraDistance, _wallLayer, QueryTriggerInteraction.Ignore))
            {

                // 壁に接触する位置より少し手前
                float safeDistance = hit.distance - _wallDistance;
                safeDistance = Mathf.Max(safeDistance, 0.1f);

                // 現在のカメラ位置から安全位置へゆっくり移動
                _cameraDistance = Mathf.Lerp(_cameraDistance, safeDistance, _followSpeed * Time.deltaTime);
            }

            else
            {
                // 本来の位置に一瞬で戻す
                _cameraDistance = Mathf.Lerp(_cameraDistance, _defaultCameraDistance, _followSpeed * Time.deltaTime);
            }

            Vector3 targetPosition = _playerPosition + cameraDirection * _cameraDistance;


            currentCamera.transform.position = targetPosition;
            currentCamera.transform.rotation = cameraRotation;


        }
        public void DeadPlayerCameraMove(bool _IsCurrentDeviceMouse, StarterAssetsInputs _input)
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

            Quaternion cameraRotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);

            currentCamera.transform.rotation = cameraRotation;
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


        public void PlayerDiedCamera()
        {
            if (_playerStatus != null && _playerStatus._isDead && !GameModeManager.IsMultiplayer)
            {
                Debug.Log("死亡カメラに切り替え");
                currentCamera.SetActive(false);

                _deadCamera.SetActive(true);
            }
            else if (_playerStatus != null && !_playerStatus._isDead && !GameModeManager.IsMultiplayer)
            {
                Debug.Log("通常カメラに切り替え");
                _deadCamera.SetActive(false);
            }
        }
    }
}


    


