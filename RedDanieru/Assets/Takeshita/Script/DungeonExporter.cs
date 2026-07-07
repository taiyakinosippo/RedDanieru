using UnityEngine;

public class DungeonExporter : MonoBehaviour
{
    public Transform dungeonRoot;

    void Start()
    {
        DungeonData data = new DungeonData();

        foreach (Transform child in dungeonRoot)
        {
            DungeonObjectData obj = new DungeonObjectData();

            obj.type = child.name;
            obj.position = child.position;

            data.objects.Add(obj);
        }

        string json = JsonUtility.ToJson(data, true);

        Debug.Log(json);
    }
}