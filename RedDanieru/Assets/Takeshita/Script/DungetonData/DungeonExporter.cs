using UnityEngine;
using System.IO;

public class DungeonExporter : MonoBehaviour
{
    public static string LastExportedJson;

    public Transform dungeonRoot;

    public void ExportDungeon(string dungeonName,string creatorName)
    {
        DungeonData data = new DungeonData();

        data.DungeonID =System.Guid.NewGuid().ToString();

        data.DungeonName = dungeonName;

        data.CreatorName = creatorName;

        data.CreateDate = System.DateTime.Now.ToString(
            "yyyy/MM/dd HH:mm:ss"
            );

        foreach (Transform child in dungeonRoot)
        {
            DungeonObjectData obj = new DungeonObjectData();

            obj.type = child.name;
            obj.position = child.position;

            data.objects.Add(obj);
        }

        LastExportedJson = JsonUtility.ToJson(data, true);

        Debug.Log(LastExportedJson);

        // JSON化後にオブジェクト削除
        foreach (Transform child in dungeonRoot)
        {
            Destroy(child.gameObject);
        }


        string path =
            Application.persistentDataPath +
            "/" +
            dungeonName +
            ".json";

        File.WriteAllText(path, LastExportedJson);

        Debug.Log("保存完了: " + path);

    }
}