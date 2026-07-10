using UnityEngine;
using System.IO;

public class DungeonListViewer : MonoBehaviour
{
    public Transform content;

    public GameObject dungeonItemPrefab;

    public void RefreshList()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        string[] files =
            Directory.GetFiles(
                Application.persistentDataPath,
                "*.json"
            );

        foreach(string file in files)
        {
            string json =
                File.ReadAllText(file);

            DungeonData data =
                JsonUtility.FromJson<DungeonData>(
                    json
                );

            GameObject item =
                Instantiate(
                    dungeonItemPrefab,
                    content
                );

            DungeonItemUI ui =
                item.GetComponent<DungeonItemUI>();

            ui.Setup(
                data.dungeonName,
                data.creatorName,
                data.createDate
            );
        }
    }
}