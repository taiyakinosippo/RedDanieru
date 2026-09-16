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
        private float RotationSmoothTime;


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
            

            // ボタンの入力方向を取得する
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;


            // 入力がある場合は、プレイヤーの回転角度を更新する
            if (_input.move != Vector2.zero)
            {
                // 入力方向を元にプレイヤーの回転角度を計算する
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _playerCamera.currentCamera.transform.eulerAngles.y;

                // プレイヤーの回転角度を補間して更新したのを変数に格納する
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // プレイヤーの回転角度を更新する
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            //指定した回転角度を元に、プレイヤーの移動方向を計算する
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            transform.position += targetDirection * _cameraSpeed * Time.deltaTime;
        }

        private void LateUpdate()
        {
            if (_playerActor != null && _playerStatus._isDead && GameModeManager.IsMultiplayer)
            {
                _playerActor.CameraContoller();
            }
        }
    }
}
