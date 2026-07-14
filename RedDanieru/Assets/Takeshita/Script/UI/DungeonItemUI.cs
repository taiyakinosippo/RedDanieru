using TMPro;
using UnityEngine;

public class DungeonItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text creatorText;
    public TMP_Text dateText;

    public void Setup(
        string dungeonName,
        string creatorName,
        string createDate)
    {
        nameText.text = dungeonName;

        if (string.IsNullOrEmpty(creatorName))
        {
            creatorText.gameObject.SetActive(false);
        }
        else
        {
            creatorText.text = "Creator : " + creatorName;
        }

        dateText.text = createDate;
    }
}