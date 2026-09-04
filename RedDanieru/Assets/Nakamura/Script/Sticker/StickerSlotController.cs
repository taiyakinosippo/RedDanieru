using UnityEngine;
using UnityEngine.UI;

public class StickerSlotController : MonoBehaviour
{
    [SerializeField] private Image icon;  //そのスロットのステッカーアイコン
    [SerializeField] private GameObject selectedFrame;  //選択中のフレーム

    //スロットのアイコンを設定する関数
    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    //スロットの選択状態を設定する関数
    public void SetSelected(bool selected)
    {
        selectedFrame.SetActive(selected);
    }
}
