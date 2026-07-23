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

        //壁があればtrueになければflase
        private bool isWallDetected = false;

        //ステッカーがあればtrueになければfalse
        public bool isStickerDected = false;

        private PlayerAnimation _playerAnimation;

        private PlayerInputPriority _actionPriority;

        private PlayerCamera _playerCamera;

        private PlayerMovement _playerMovement;

        private void Start()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();

            _actionPriority = GetComponent<PlayerInputPriority>();

            _playerCamera = GetComponent<PlayerCamera>();

            _playerMovement = GetComponent<PlayerMovement>();
        }

        //-----------------------------------------
        //ステッカーがあるかどうか、もしなければ壁があるかどうかを判定する関数
        //-----------------------------------------
        public void StickerAndWallCheck(StarterAssetsInputs input)
        {
            //押されていないなら処理を動かさない
            if (!input.sticker) return;

            //押された判定を元に戻す
            input.sticker = false;

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
                    _playerAnimation.StickerPeelOffAnimation();
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
                        //ステッカーを持っているかの処理をここに書く
                        _playerAnimation.PlayerStickerPasteAnimator();
                        Debug.Log("目の前に壁がありまーす");
                        isWallDetected = false;
                    }
                    else
                    {
                        Debug.Log("目の前に壁がありませーん");
                        _actionPriority.EndAction();
                        isWallDetected = false;
                    }
                }
            }

            //3人称だった場合
            else
            {
                // プレイヤーの前にある球体を使って地面にいるかどうかを判定
                Vector3 spherePosition =
                transform.position + transform.forward * thirdPersonWallDistance + Vector3.up * thirdPersonWallCheckHeight;
                // ステッカーあるかの判定を行う
                isStickerDected = 
                Physics.CheckSphere(spherePosition, checkRadius, stickerLayer,
                                    QueryTriggerInteraction.Ignore);

                //あればはがすアニメーションを再生させる
                if (isStickerDected)
                {
                    _playerAnimation.StickerPeelOffAnimation();
                }

                //ないなら壁があるかどうかを判定する
                else
                {

                    // 壁があるのかどうかの判定を行う
                    isWallDetected =
                    Physics.CheckSphere(spherePosition, checkRadius, wallLayer,
                                        QueryTriggerInteraction.Ignore);

                    //もし壁があった場合次にステッカーがないのかを確認する
                    if (isWallDetected)
                    {
                        //ステッカーを持っているかの処理をここに書く
                        _playerAnimation.PlayerStickerPasteAnimator();
                        Debug.Log("目の前に壁がありまーす");
                        isWallDetected = false;
                    }
                    else
                    {
                        Debug.Log("目の前に壁がありませーん");
                        isWallDetected = false;
                        _actionPriority.EndAction();
                    }
                }
            }
           
        }
    }
}

