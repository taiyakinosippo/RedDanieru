using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DungeonImporter : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;

    // 最後に読み込みを頼んだステージ（連打されたときは最後の1回だけ反映する）
    private int requestSerial;

    private int importingCount;

    public bool IsImporting => importingCount > 0;

    public string LoadedDungeonId { get; private set; }

    // 最後に読み込みを頼まれたステージ（失敗したらnullに戻す）
    private string requestedDungeonId;

    public MapManager MapManager => mapManager;

    public void ImportDungeon(string dungeonId)
    {
        StartCoroutine(
            ImportDungeonCoroutine(dungeonId)
        );
    }

    /// <summary>
    /// 指定したステージの読み込みが終わるまで待つ
    /// </summary>
    public IEnumerator WaitUntilLoaded(string dungeonId, float timeoutSeconds = 15f)
    {
        float limit = Time.realtimeSinceStartup + timeoutSeconds;

        // このImporterに頼まれていないステージ（LoadManagerで直接読んだ等）は待たない
        while (IsImporting ||
               (requestedDungeonId == dungeonId && LoadedDungeonId != dungeonId))
        {
            if (Time.realtimeSinceStartup > limit)
            {
                Debug.LogWarning($"ステージの読み込みを待ちきれませんでした : {dungeonId}");
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator ImportDungeonCoroutine(
        string dungeonId)
    {
        int serial = ++requestSerial;

        importingCount++;
        requestedDungeonId = dungeonId;
        bool loaded = false;

        try
        {
            DungeonMapData data = null;

            if (OnlineTestMode.Enabled)
            {
                // テストモードはローカル保存したマップから読む
                data = LocalTestServer.LoadStage(dungeonId);

                if (data == null)
                {
                    Debug.LogError("ローカルにステージがありません : " + dungeonId);
                    yield break;
                }
            }
            else
            {
                string url =
                    ServerApi.Url("download_dungeon.php?id=")
                    + UnityWebRequest.EscapeURL(dungeonId);

                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    yield return request.SendWebRequest();

                    if (request.result !=
                        UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(request.error);
                        yield break;
                    }

                    string json =
                        request.downloadHandler.text;

                    Debug.Log("受信文字数=" + json.Length);

                    if (string.IsNullOrEmpty(json))
                    {
                        Debug.LogError("PHPから何も返ってきていません");
                        yield break;
                    }

                    data =
                        JsonUtility.FromJson<DungeonMapData>(
                            json
                        );
                }
            }

            if (data == null || data.tiles == null)
            {
                Debug.LogError("JSON変換失敗");
                yield break;
            }

            // 後から別のステージが頼まれていたら、古い結果は捨てる
            if (serial != requestSerial)
                yield break;

            mapManager.LoadDungeon(data);

            LoadedDungeonId = dungeonId;
            loaded = true;

            Debug.Log($"ダンジョン読込完了 : {dungeonId} ({data.width}x{data.depth})");
        }
        finally
        {
            importingCount--;

            // 失敗したときに待ち続けないようにする
            if (!loaded && serial == requestSerial)
            {
                requestedDungeonId = null;
            }
        }
    }
}
