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

        ScrollRect scrollRect =
            GetComponentInParent<ScrollRect>();

        if (open && scrollRect != null)
        {
            RectTransform viewport =
                scrollRect.viewport;

            RectTransform item =
                transform as RectTransform;

            Canvas.ForceUpdateCanvases();

            Vector3[] corners = new Vector3[4];
            item.GetWorldCorners(corners);

            Vector3[] viewportCorners = new Vector3[4];
            viewport.GetWorldCorners(viewportCorners);

            float itemBottom = corners[0].y;
            float viewportBottom = viewportCorners[0].y;

            if (itemBottom < viewportBottom)
            {
                scrollRect.content.anchoredPosition +=
                    new Vector2(0, viewportBottom - itemBottom + 50);
            }
        }
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