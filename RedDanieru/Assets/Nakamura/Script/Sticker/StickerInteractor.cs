using System.Collections.Generic;
using UnityEngine;

public class StickerInteractor : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private StickerSlotUI stickerSlotUI;

    private List<GameObject> interactObjects = new List<GameObject>();  //インタラクトトリガー内にあるStickerState持ちのオブジェクトのリスト
    public Sticker[] holdSticker { get; private  set; }  //保持してるステッカーのタイプ
    [SerializeField] private int maxHoldCount = 3;  //保持できるステッカーの数

    private float wallCheckDistance = 1.5f;
    private float wallCheckHeight = 1.5f;
    private float checkRadius = 1.5f;
    private int holdIndex = 0;  //保持しているステッカーのインデックス

    void Start()
    {
        //保持できるステッカーの数を設定
        holdSticker = new Sticker[maxHoldCount];

        //ステッカーのスロットを生成
        stickerSlotUI.CreateStickerSlots(maxHoldCount);

        //プレイヤーのTransformを取得
        player = gameObject.transform;
    }

    void Update()
    {
        //// インタラクト対象を更新
        //UpdateInteractObjects();

        //保持中のステッカーのインデックスを変更
        HoldIndex();

        ////右クリックでステッカーを貼る、剥がす
        //if (interactObjects.Count > 0 && Input.GetMouseButtonDown(1))
        //{
        //    ReceiptPickup();
        //}
    }

    //インタラクト対象のオブジェクトを更新
    private void UpdateInteractObjects()
    {
        interactObjects.Clear();

        //プレイヤーの前方にある壁の位置を計算
        Vector3 spherePosition = player.position + player.forward * wallCheckDistance + Vector3.up * wallCheckHeight;

        //プレイヤーの前方にあるオブジェクトを取得
        Collider[] objects = Physics.OverlapSphere(spherePosition, checkRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        foreach (Collider sticker in objects)
        {
            StickerState stickerState = sticker.GetComponent<StickerState>();

            //StickerStateを持っているものだけインタラクト対象に追加
            if (stickerState != null)
            {
                interactObjects.Add(sticker.gameObject);
            }
        }
    }

    private void HoldIndex()
    {
        //数字キーで保持中のステッカーのインデックスを変更
        for (int i = 0; i < maxHoldCount && i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i + 1))
            {
                holdIndex = i;
                Debug.Log("HoldIndex: " + holdIndex);
                break;
            }
        }

        //マウスホイールで保持中のステッカーのインデックスを変更
        float scroll = Input.mouseScrollDelta.y;

        //スクロールの方向に応じてインデックスを変更
        if (scroll != 0)
        {
            if (scroll > 0)  //上方向にスクロールした場合
            {
                holdIndex = (holdIndex - 1 + maxHoldCount) % maxHoldCount;
            }
            else if (scroll < 0)  //下方向にスクロールした場合
            {
                holdIndex = (holdIndex + 1) % maxHoldCount;
            }
        }

        //ステッカーUIの選択中のスロットを更新
        stickerSlotUI.SetSelectedSlot(holdIndex);
    }

    public void ReceiptPickup(GameObject interactObj)
    {
        if (interactObj == null)
            return;

        StickerState target = interactObj.GetComponent<StickerState>();  //インタラクトしているオブジェクトのStickerStateを取得
        if (target == null)
            return;

        //既に貼られているなら剥がして保持
        if (target.currentSticker != Sticker.None)
        {
            //選択中のスロットが空ならそこ、埋まっていたら他の空いているスロット、全部埋まっていたら選択中のスロットに上書き
            int slot = holdIndex;

            if (holdSticker[holdIndex] != Sticker.None)
            {
                int emptySlot = System.Array.IndexOf(holdSticker, Sticker.None);

                if (emptySlot >= 0)
                {
                    slot = emptySlot;
                }
            }

            //マルチでは他の人と取り合いになるのでホストに剥がしてもらう（結果はReceiveStickerで届く）
            if (NetworkGameState.RequestPeelSticker(target, slot))
                return;

            //ステッカーUIの更新
            stickerSlotUI.SetStickerUI(slot, target.currentSticker);

            holdSticker[slot] = target.Remove();
        }
        //何も貼られていないなら保持中のステッカーを貼り、保持中のステッカーを空にする
        else if (holdSticker[holdIndex] != Sticker.None)
        {
            Sticker sticker = holdSticker[holdIndex];

            holdSticker[holdIndex] = Sticker.None;
            //ステッカーUIのスロットを空にする
            stickerSlotUI.SetStickerUI(holdIndex, Sticker.None);

            //マルチではホストに貼ってもらう（先に他の人が貼っていたらReceiveStickerで戻ってくる）
            if (NetworkGameState.RequestApplySticker(target, sticker, holdIndex))
                return;

            target.Apply(sticker);
        }
        //ステッカーを持っていない
        else
        {
            Debug.Log("ステッカーを持ってないよ");
        }
    }

    //マルチでホストから届いたステッカーを保持する
    public void ReceiveSticker(int slot, Sticker sticker)
    {
        if (sticker == Sticker.None)
        {
            Debug.Log("先に他の人がステッカーを剥がしました");
            return;
        }

        //届くまでの間にスロットが埋まっていたら空いているスロットへ
        if (slot < 0 || slot >= holdSticker.Length || holdSticker[slot] != Sticker.None)
        {
            int emptySlot = System.Array.IndexOf(holdSticker, Sticker.None);

            slot = emptySlot >= 0 ? emptySlot : Mathf.Clamp(slot, 0, holdSticker.Length - 1);
        }

        holdSticker[slot] = sticker;

        //ステッカーUIの更新
        stickerSlotUI.SetStickerUI(slot, sticker);
    }

    //一番カメラの中央にあるオブジェクトを取得
    //private GameObject GetInteractObject()
    //{
    //    GameObject targetObject = null;  //一番カメラの中央にあるオブジェクト
    //    float maxDot = -1.0f;  //カメラの中央にあるオブジェクトを取得するためのドット積の最大値

    //    //インタラクト対象のオブジェクトの中で一番カメラの中央にあるオブジェクトを取得
    //    foreach (GameObject target in interactObjects)
    //    {
    //        //プレイヤーの前方ベクトルと対象オブジェクトへの方向ベクトルのドット積を計算
    //        Vector3 dir = (target.transform.position - player.position).normalized;
    //        float dot = Vector3.Dot(player.forward, dir);

    //        //ドット積が最大のオブジェクトを取得
    //        if (dot > maxDot)
    //        {
    //            maxDot = dot;
    //            targetObject = target;
    //        }
    //    }

    //    return targetObject;
    //}
}
