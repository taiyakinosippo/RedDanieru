using UnityEngine.UI;
using UnityEngine;

public class DungeonItemUI : MonoBehaviour
{
    public Text nameText;
    public Text creatorText;
    public Text dateText;

    public void Setup(
        string dungeonName,
        string creatorName,
        string createDate)
    {
        nameText.text = dungeonName;
        creatorText.text =
            "作者 : " + creatorName;

        dateText.text = createDate;
    }
}