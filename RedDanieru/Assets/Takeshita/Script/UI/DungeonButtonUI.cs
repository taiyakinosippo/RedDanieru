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

    public GameObject selectArea;

    private bool isOpen;

    public void ToggleOpen()
    {
        isOpen = !isOpen;
        selectArea.SetActive(isOpen);
    }
}