using UnityEngine;
using System.IO;

public class DungeonImporter : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject spherePrefab;
    public Transform dungeonRoot;

    public void ImportDungeon(string dungeonName)
    {
        foreach (Transform child in dungeonRoot)
        {
            Destroy(child.gameObject);
        }

        string path =
            Application.persistentDataPath +
            "/" +
            dungeonName +
            ".json";

        if (!File.Exists(path))
        {
            Debug.LogError("ファイルが存在しません");
            return;
        }

        string json = File.ReadAllText(path);

        DungeonData data =
         JsonUtility.FromJson<DungeonData>(json);

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