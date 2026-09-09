using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class DungeonButtonUI : MonoBehaviour
{
    public TMP_Text stageNameText;
    public TMP_Text creatorNameText;
    public TMP_Text tagText;

    public TMP_Text likeCountText;
    public TMP_Text clearCountText;

    public Image thumbnailImage;

    public GameObject extendArea;

    private static DungeonButtonUI currentOpen;

    public Button soloButton;
    public Button multiButton;

    public void Toggle()
    {
        // 別のボタンが開いていたら閉じる
        if (currentOpen != null &&
            currentOpen != this)
        {
            currentOpen.Close();
        }

        bool open = !extendArea.activeSelf;

        extendArea.SetActive(open);

        LayoutElement layout =
            GetComponent<LayoutElement>();

        layout.preferredHeight =
            open ? 350 : 250;

        if (open)
        {
            currentOpen = this;
        }
        else if (currentOpen == this)
        {
            currentOpen = null;
        }

        Canvas.ForceUpdateCanvases();
    }

    public void Close()
    {
        extendArea.SetActive(false);

        LayoutElement layout =
            GetComponent<LayoutElement>();

        layout.preferredHeight = 250;

        Canvas.ForceUpdateCanvases();
    }
}