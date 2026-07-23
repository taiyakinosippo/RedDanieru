using TMPro;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// プレイヤー用のデバッグスクリプト
    /// </summary>
    public class PlayerDebug : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text speedText;

        private PlayerMovement _playerMovement;

        private StickerCheck   _stickerCheck;

        private PlayerCamera _playerCamera;

        private PlayerStatus _playerStatus;

        private void Start()
        {
            _playerStatus = GetComponent<PlayerStatus>();
            _playerMovement = GetComponent<PlayerMovement>();

        }

        private void Update()
        {
            hpText.text = $"HP : {_playerStatus.CurrentHP}/{_playerStatus._playerHP}";
            attackText.text = $"ATK : {_playerStatus.CurrentAttack}";
            defenseText.text = $"DEF : {_playerStatus.CurrentDefense}";
            speedText.text = $"Speed : {_playerMovement._speed}";
        }



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
