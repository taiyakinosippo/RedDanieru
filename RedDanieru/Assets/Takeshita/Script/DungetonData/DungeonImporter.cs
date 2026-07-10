using UnityEngine;
using System.IO;

public class DungeonImporter : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject spherePrefab;

    public Transform dungeonRoot;

    public void ImportDungeon(
        string dungeonName)
    {
        string path =
            Application.persistentDataPath +
            "/" +
            dungeonName +
            ".json";

        if (!File.Exists(path))
        {
            Debug.LogError(
                "ダンジョンが見つかりません"
            );
            return;
        }

        string json =
            File.ReadAllText(path);

        DungeonData data =
            JsonUtility.FromJson<DungeonData>(
                json
            );

        foreach (Transform child in dungeonRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (DungeonObjectData obj in data.objects)
        {
            if (obj.type == "Cube")
            {
                Instantiate(
                    cubePrefab,
                    obj.position,
                    Quaternion.identity,
                    dungeonRoot
                );
            }
            else if (obj.type == "Sphere")
            {
                Instantiate(
                    spherePrefab,
                    obj.position,
                    Quaternion.identity,
                    dungeonRoot
                );
            }
        }
    }
}
