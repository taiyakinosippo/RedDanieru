using UnityEngine;

namespace StarterAssets
{
    public class PlayerDebug : MonoBehaviour
    {
        private PlayerMovement _playerMovement;

        private StickerCheck   _stickerCheck;

        private PlayerCamera _playerCamera;
       
      
        //debug用に、地面にいるかどうかの判定を可視化するためのGizmosを描画する関数
        private void OnDrawGizmosSelected()
        {
            _playerMovement = GetComponent<PlayerMovement>();

            _stickerCheck = GetComponent<StickerCheck>();

            _playerCamera = GetComponent<PlayerCamera>();

            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (_playerMovement.Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            //地面用のデバッグ機能
            Gizmos.DrawSphere( new Vector3(transform.position.x, 
                               transform.position.y - _playerMovement.GroundedOffset, 
                               transform.position.z), _playerMovement.GroundedRadius
                             );

            if (_playerCamera == null || _playerCamera.currentCamera == null ||
                _stickerCheck == null || _playerMovement == null)
            {
                return;
            }

            //1人称用のステッカーや壁のデバッグ機能
            if (_playerCamera.isFirstPerson)
            {
                if (_stickerCheck.isStickerDected) Gizmos.color = transparentRed;
                else Gizmos.color = transparentGreen;

                Vector3 rayOrigin = 
                _playerCamera.currentCamera.transform.position + Vector3.up * _stickerCheck.firstPersonWallCheckHeight;

                Gizmos.DrawLine(rayOrigin,
                                rayOrigin + _playerCamera.currentCamera.transform.forward *
                                _stickerCheck.firstPersonWallDistance
                               );
            }

            //3人称用のステッカーや壁のデバッグ機能
            else
            {
                if (_stickerCheck.isStickerDected) Gizmos.color = transparentRed;
                else Gizmos.color = transparentGreen;
                // プレイヤーの前にある球体を使って地面にいるかどうかを判定
                Vector3 spherePosition =
                transform.position + transform.forward * _stickerCheck.thirdPersonWallDistance + Vector3.up * _stickerCheck.thirdPersonWallCheckHeight;
                // ステッカーあるかの判定を行う
                Gizmos.DrawSphere(spherePosition, _stickerCheck.checkRadius);
            }
        }
        
    }
}
