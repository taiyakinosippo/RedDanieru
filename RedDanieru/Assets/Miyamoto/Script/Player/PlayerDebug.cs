using NUnit.Framework;
using System.Collections.Generic;
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

        private PlayerAttack _playerAttack;

        // 攻撃データのリスト
        [SerializeField] private List<PlayerAttackData> attackData = new();

        // 攻撃の範囲などを見たいときに指定の名前を入力することで、Gizmosで攻撃範囲を表示することができる
        [SerializeField] private string debugAttackName;

        private void Start()
        {
            _playerStatus = GetComponent<PlayerStatus>();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerAttack = GetComponent<PlayerAttack>();
        }

        // デバッグ用の攻撃データを取得する関数
        public PlayerAttackData GetDebugAttackData()
        {
            if (attackData == null)
                return null;

            foreach (PlayerAttackData data in attackData)
            {
                if (data == null)
                    continue;

                if (data.attackName == debugAttackName)
                    return data;
            }

            return null;
        }
        private void Update()
        {
            hpText.text = $"HP : {_playerStatus.CurrentHP}/{_playerStatus._playerHP}";
            attackText.text = $"ATK : {_playerStatus.CurrentAttack}";
            defenseText.text = $"DEF : {_playerStatus.CurrentDefense}";
            speedText.text = $"Speed : {_playerMovement._speed}";
        }



        //debug用の範囲などをGizmosで表示するための関数
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

            if (attackData == null)
                return;

            PlayerAttackData debugData = null;

            // 入力された名前と一致する攻撃を探す
            foreach (PlayerAttackData data in attackData)
            {
                if (data == null)
                    continue;

                if (data.attackName == debugAttackName)
                {
                    debugData = data;
                    break;
                }
            }

            // 見つからなかった
            if (debugData == null)
                return;

            // 攻撃判定の中心位置
            Vector3 center =
            _playerAttack._attackPoint.position +
            _playerAttack._attackPoint.rotation * debugData.attackOffset;

            // 攻撃判定の向きを計算する
            Quaternion attackRotation =
    　　　　_playerAttack._attackPoint.rotation * Quaternion.Euler(debugData.attackRotation);

            // 攻撃判定の向き
            Gizmos.matrix = Matrix4x4.TRS(
                center,
                attackRotation,
                Vector3.one
            );

            // 攻撃範囲を表示
            Gizmos.DrawWireCube(
                Vector3.zero,
                debugData.playerAttackRadius
            );

            // Gizmosの設定を元に戻す
            Gizmos.matrix = Matrix4x4.identity;
        }
        
    }
}
