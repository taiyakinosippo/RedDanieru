using UnityEngine;
using System.IO;

public class DungeonExporter : MonoBehaviour
{
    public static string LastExportedJson;

    public void ExportDungeon(string dungeonName)
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json"
            );

        if (!File.Exists(path))
        {
            Debug.LogError("保存データが存在しません：" + path);
            return;
        }

        LastExportedJson = File.ReadAllText(path);

        Debug.Log("エクスポート完了");
        Debug.Log(LastExportedJson);

    }
}