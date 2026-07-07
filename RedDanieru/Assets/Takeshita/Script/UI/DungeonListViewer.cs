using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class DungeonListViewer: MonoBehaviour
{
    public TMP_Text listText;

    public void RefreshList()
    {
        string[] files = Directory.GetFiles(
            Application.persistentDataPath,
            "*.json"
        );

        string result = "Dungeon List\n\n";

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);

            DungeonData data =
                JsonUtility.FromJson<DungeonData>(json);

            result +=
                "====================\n" +
                "Name : " + data.DungeonName + "\n" +
                "Creator : " + data.CreatorName + "\n" +
                "Date : " + data.CreateDate + "\n" +
                "ID : " + data.DungeonID + "\n" +
                "====================\n\n";
        }

        listText.text = result;

        LayoutRebuilder.ForceRebuildLayoutImmediate(
    listText.rectTransform
);
    }
}

