using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DungeonImporter : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;

    public void ImportDungeon(string dungeonId)
    {
        StartCoroutine(
            ImportDungeonCoroutine(dungeonId)
        );
    }

    private IEnumerator ImportDungeonCoroutine(
        string dungeonId)
    {
        string url =
            "http://10.219.32.66/RedDaniel/download_dungeon.php?id="
            + UnityWebRequest.EscapeURL(dungeonId);

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

        Debug.Log("===== 受信データ =====");
        Debug.Log("受信文字数=" + json.Length);
        Debug.Log("[" + json + "]");

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("PHPから何も返ってきていません");
            yield break;
        }

        DungeonMapData data =
            JsonUtility.FromJson<DungeonMapData>(
                json
            );

        Debug.Log("===== 変換結果 =====");
        Debug.Log("width = " + data.width);
        Debug.Log("height = " + data.height);
        Debug.Log("depth = " + data.depth);

        if (data.tiles != null)
        {
            Debug.Log("tiles数 = " + data.tiles.Length);
        }
        else
        {
            Debug.LogError("tiles が null");
        }

        if (data == null)
        {
            Debug.LogError("JSON変換失敗");
            yield break;
        }

        mapManager.LoadDungeon(data);

        Debug.Log("ダンジョン読込完了");
    }
}