using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEditor.PlayerSettings;

namespace Player
{
    public class DeadCameraMulti : MonoBehaviour
    {
        private PlayerActor _playerActor;
        private PlayerStatus _playerStatus;
        private PlayerCamera _playerCamera;
        
        [Tooltip("最大移動速度")]
        public float _maxMoveSpeed = 5.0f;
        [Tooltip("最小移動速度")]
        public float _minMoveSpeed = 5.0f;
        [Tooltip("速度の変化率")]
        public float SpeedChangeRate = 10.0f;
        private float _cameraSpeed; 
        private float _targetRotation;
        private float _rotationVelocity;
        [Tooltip("回転の補間時間")]
        public float RotationSmoothTime = 10.0f;


        void Start()
        {
            _playerStatus = GetComponent<PlayerStatus>();
            _playerActor = GetComponent<PlayerActor>();
            _playerCamera = GetComponent<PlayerCamera>();
        }

        public void CameraMoveFiexdUpdate(StarterAssetsInputs _input)
        {
            if (!_playerStatus._isDead) return;
            // Shiftキーを押している場合は早く移動し、押していない場合はゆっくり移動する
            float targetSpeed = _input.sprint ? _maxMoveSpeed : _minMoveSpeed;

            // 何も入力されていない場合は速度を0にする
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // プレイヤーの入力の大きさを取得する(ゲームパッドなら入力の大きさをPCなら1f)
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            
            // 速度を補間して更新する
            _cameraSpeed = Mathf.Lerp(_cameraSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // 細かい数字の誤差をなくすために、小数点以下3桁で丸める
            _cameraSpeed = Mathf.Round(_cameraSpeed * 1000f) / 1000f;

            // カメラの前方向
            Vector3 cameraForward = _playerCamera.currentCamera.transform.forward;

            // カメラの右方向
            Vector3 cameraRight = _playerCamera.currentCamera.transform.right;

            // WASDの入力をカメラ基準の方向に変換
            Vector3 targetDirection = cameraForward * _input.move.y + cameraRight * _input.move.x;

            // 斜め移動の速度を一定にする
            if (targetDirection.sqrMagnitude > 1.0f)
            {
                targetDirection.Normalize();
            }

            // カメラを移動
            _playerCamera.currentCamera.transform.position += targetDirection * _cameraSpeed * Time.deltaTime;
        }

     
    }
}
