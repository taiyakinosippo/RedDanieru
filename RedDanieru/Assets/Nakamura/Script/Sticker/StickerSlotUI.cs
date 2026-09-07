using UnityEngine;
using UnityEngine.UI;

public class StickerSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject stickerSlotPrefab;  //ステッカーのスロットのPrefab

    //ステッカーの種類とアイコンの対応を設定するクラス
    [System.Serializable] private class StickerIcon
    {
        public Sticker stickerType;
        public Sprite icon;
    }

    [SerializeField] private StickerIcon[] stickerIcons;  //ステッカーの種類とアイコンの対応を設定する配列

    private StickerSlotController[] slots;  //生成したステッカーのスロットの配列

    //ステッカーのスロットを生成する関数
    public void CreateStickerSlots(int slotCount)
    {
        //配列にスロットの数を設定
        slots = new StickerSlotController[slotCount];

        //スロットの数だけPrefabを生成して配列に格納
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObject = Instantiate(stickerSlotPrefab, transform);
            slots[i] = slotObject.GetComponent<StickerSlotController>();
        }
    }

    public void SetStickerUI(int slotIndex, Sticker sticker)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return;

        //スロットのアイコンを取得
        Image icon = slots[slotIndex].transform.GetChild(0).GetComponent<Image>();

        //Noneならアイコンを消す
        if (sticker == Sticker.None)
        {
            slots[slotIndex].SetIcon(null);
            return;
        }

        //Stickerに対応するアイコンを探す
        foreach (StickerIcon stickerIcon in stickerIcons)
        {
            if (stickerIcon.stickerType == sticker)
            {
                //スロットのアイコンを設定
                slots[slotIndex].SetIcon(stickerIcon.icon);
                return;
            }
        }

        // 対応するアイコンがなかった場合
        slots[slotIndex].SetIcon(null);
        Debug.LogWarning("ステッカーに対応するアイコンが見つかりません: " + sticker);
    }

    //スロットの選択状態を設定する関数
    public void SetSelectedSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetSelected(i == slotIndex);
        }
    }
}
