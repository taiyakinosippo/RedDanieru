using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Player
{
    public class StickerCheck : MonoBehaviour
    {
        [Tooltip("壁との判定距離")]
        public float thirdPersonWallDistance = 1.0f;
        public float firstPersonWallDistance = 1.0f;

        [Tooltip("壁判定を開始する高さ")]
        public float thirdPersonWallCheckHeight = 1.0f;
        public float firstPersonWallCheckHeight = 1.0f;

        [Tooltip("3人称の時のステッカーや壁があるかの判定の範囲")]
        public float checkRadius = 1.0f;

        [Tooltip("壁として判定するレイヤー")]
        public LayerMask wallLayer;

        [Tooltip("ステッカーがあるかどうかを判定するレイヤー")]
        public LayerMask stickerLayer;

        private bool isWallDetected = false;

        //ステッカーがあればtrueになければfalse
        public bool isStickerDected = false;

        private bool isStickerStateFound = false;

        private bool _playerHasSticker = false;

        private PlayerAnimation _playerAnimation;

        private PlayerInputPriority _actionPriority;

        private PlayerCamera _playerCamera;

        private PlayerMovement _playerMovement;

        private StickerInteractor _stickerInteractor;

        private GameObject _interactObject = null;  //インタラクトトリガー内にあるStickerState持ちのオブジェクトのリスト

        private void Start()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();

            _actionPriority = GetComponent<PlayerInputPriority>();

            _playerCamera = GetComponent<PlayerCamera>();

            _playerMovement = GetComponent<PlayerMovement>();

            _stickerInteractor = GetComponent<StickerInteractor>();
        }

        //-----------------------------------------
        //ステッカーがあるかどうか、もしなければ壁があるかどうかを判定する関数
        //-----------------------------------------
        public void StickerAndWallCheck(StarterAssetsInputs input)
        {
            //押されていないなら処理を動かさない
            if (!input.sticker) return;

            //前回の対象が残っていると、離れた物に貼ったり剥がしたりしてしまう
            _interactObject = null;

            //1人称の時のRayの判定
            if (_playerCamera.isFirstPerson)
            {
                // レイを飛ばす開始位置（胸あたり）
                Vector3 rayOrigin = _playerCamera.currentCamera.transform.position + Vector3.up * firstPersonWallCheckHeight;

                //光線をだいしてあるどうかを判定する（出す方向はカメラの向き）
                isStickerDected =
                Physics.Raycast(rayOrigin, _playerCamera.currentCamera.transform.forward,
                               firstPersonWallDistance, stickerLayer,
                               QueryTriggerInteraction.Ignore
                               );

                //あればはがすアニメーションを再生させる
                if (isStickerDected)
                {
                    // ステッカー入力を消費
                    input.sticker = false;

                    //_playerAnimation.StickerPeelOffAnimation();
                }

                //ないなら壁があるかどうかを判定する
                else
                {
                    // 前方に壁があるか判定（出す方向はカメラの向き）
                    isWallDetected =
                    Physics.Raycast(rayOrigin, _playerCamera.currentCamera.transform.forward,
                                    firstPersonWallDistance, wallLayer,
                                    QueryTriggerInteraction.Ignore
                                    );

                    //もし壁があった場合次にステッカーがないのかを確認する
                    if (isWallDetected)
                    {
                        // ステッカー入力を消費
                        input.sticker = false;
                        //ステッカーを持っているかの処理をここに書く
                        _playerAnimation.PlayerStickerPasteAnimator();
                        Debug.Log("目の前に壁がありまーす");
                        isWallDetected = false;
                    }
                    else
                    {
                        // ステッカー入力を消費
                        input.sticker = false;
                        Debug.Log("目の前に壁がありませーん");
                        _actionPriority.EndAction();
                        isWallDetected = false;
                    }
                }
            }

            //3人称だった場合
            else
            {
                // 現在一番高いDot値
                float maxDot = -1.0f;

                isStickerDected = false;

                isStickerStateFound = false;

                // 一番カメラの真正面に近いステッカー
                StickerState targetSticker = null;

                // プレイヤーの前にある球体を使って地面にいるかどうかを判定
                Vector3 spherePosition =
                transform.position + transform.forward * thirdPersonWallDistance + Vector3.up * thirdPersonWallCheckHeight;

                // ステッカーあるかの判定を行う
                Collider[] isCollider =
                Physics.OverlapSphere(spherePosition, checkRadius, ~0, QueryTriggerInteraction.Ignore);

                foreach (Collider collider in isCollider)
                {
                    StickerState sticker = collider.GetComponent<StickerState>();

                    if (sticker == null) continue;

                    Debug.Log("StickerState発見 / currentSticker = " + sticker.currentSticker);

                    if (sticker.currentSticker == Sticker.None)
                        continue;

                    // カメラ → ステッカーの方向
                    Vector3 direction =
                        (sticker.transform.position -
                         _playerCamera.currentCamera.transform.position)
                         .normalized;

                    // カメラ真正面との一致度
                    float dot =
                        Vector3.Dot(
                            _playerCamera.currentCamera.transform.forward,
                            direction
                        );

                    Debug.Log(
                        "Sticker = "
                        + sticker.currentSticker
                        + " / Dot = "
                        + dot
                    );

                    // 一番真正面に近いものを記録
                    if (dot > maxDot)
                    {
                        maxDot = dot;
                        targetSticker = sticker;
                    }

                }

                // ステッカーが見つかった
                if (targetSticker != null)
                {
                    _interactObject = targetSticker.gameObject;

                    isStickerDected = true;

                    Debug.Log(
                        "一番正面のステッカー = "
                        + targetSticker.currentSticker
                    );

                    input.sticker = false;

                    _playerAnimation.StickerPeelOffAnimation();
                }

                // ステッカーがなかった
                else
                {
                    _playerHasSticker = false;

                    // ステッカーあるかの判定を行う
                    isCollider =
                    Physics.OverlapSphere(spherePosition, checkRadius, ~0, QueryTriggerInteraction.Ignore);

                    foreach (Collider collider in isCollider)
                    {
                        StickerState sticker = collider.GetComponent<StickerState>();

                        if (sticker == null) continue;

                        isStickerStateFound = true;

                        Debug.Log("StickerState発見 / currentSticker = " + sticker.currentSticker);

                        if (sticker.currentSticker != Sticker.None)
                            continue;

                        // カメラ → ステッカーの方向
                        Vector3 direction =
                            (sticker.transform.position -
                             _playerCamera.currentCamera.transform.position)
                             .normalized;

                        // カメラ真正面との一致度
                        float dot =
                            Vector3.Dot(
                                _playerCamera.currentCamera.transform.forward,
                                direction
                            );

                        Debug.Log(
                            "Sticker = "
                            + sticker.currentSticker
                            + " / Dot = "
                            + dot
                        );

                        // 一番真正面に近いものを記録
                        if (dot > maxDot)
                        {
                            maxDot = dot;
                            targetSticker = sticker;
                        }

                        if (targetSticker != null)
                        {
                            _interactObject = targetSticker.gameObject;

                            foreach (Sticker stickerhold in _stickerInteractor.holdSticker)
                            {
                                if(stickerhold != Sticker.None)
                                {
                                    _playerHasSticker = true;
                                    break;
                                }
                            }
                            // ステッカー入力を消費
                            input.sticker = false;
                            if (_playerHasSticker)
                            {
                                _playerAnimation.PlayerStickerPasteAnimator();
                            }

                            else _actionPriority.EndAction();
                        }
                        else
                        {
                            // ステッカー入力を消費
                            input.sticker = false;
                            _actionPriority.EndAction();
                        }


                    }

                    // foreachが全部終わった後に判定する
                    if (!isStickerStateFound)
                    {
                        // StickerStateが1個もなかった
                        input.sticker = false;
                        _actionPriority.EndAction();

                        Debug.Log("StickerStateが1個もありません");
                    }
                }
            }
        }


     

        public void StickerAnimationEnd()
        {
            if (isStickerDected)
            {
                _stickerInteractor.ReceiptPickup(_interactObject);
                _actionPriority.EndAction();
            }
            else
            {
                _stickerInteractor.ReceiptPickup(_interactObject);
                _actionPriority.EndAction();
            }
           
        }
    }
}

