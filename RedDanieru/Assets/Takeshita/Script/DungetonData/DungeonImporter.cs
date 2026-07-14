using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DungeonImporter : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;

    public void ImportDungeon(string dungeonName)
    {
        StartCoroutine(
            ImportDungeonCoroutine(dungeonName)
        );
    }

    private IEnumerator ImportDungeonCoroutine(
        string dungeonName)
    {
        string url =
            "http://localhost/RedDaniel/download_dungeon.php?name="
            + UnityWebRequest.EscapeURL(dungeonName);

        UnityWebRequest request =
            UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        string json =
            request.downloadHandler.text;

        DungeonMapData data =
            JsonUtility.FromJson<DungeonMapData>(
                json
            );

        if (data == null)
        {
            Debug.LogError("JSON変換失敗");
            yield break;
        }

        mapManager.LoadDungeon(data);

        Debug.Log("ダンジョン読込完了");
    }
}