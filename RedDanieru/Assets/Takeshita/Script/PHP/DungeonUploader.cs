using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DungeonUploader : MonoBehaviour
{
    [SerializeField]
    private DungeonUIManager dungeonUIManager;

    public void UploadDungeon( string dungeonName, string creatorName)
    {
        StartCoroutine(
            UploadCoroutine(
                dungeonName,
                creatorName
            )
        );
    }

    private IEnumerator UploadCoroutine(string dungeonName,  string creatorName)
    {
        if (string.IsNullOrEmpty(SaveManager.LastDungeonId))
        {
            Debug.LogError("保存済みのダンジョンIDがありません");
            yield break;
        }

        string[]files=Directory.GetFiles(
            Application.persistentDataPath,
            "*.json"
        );

        DungeonMapData targetData = null;

        string targetPath = "";

        foreach(string file in files)
        {
            string json = File.ReadAllText(file);

            DungeonMapData data = JsonUtility.FromJson<DungeonMapData>(json);

            if (data != null && data.dungeonId == SaveManager.LastDungeonId)
            {
                targetData = data;
                targetPath = file;
                break;
            }
        }

        if (targetData == null)
        {
            Debug.LogError("JSONファイルが見つかりません");
            yield break;
        }

        string jsonData = File.ReadAllText(targetPath);

        WWWForm form = new WWWForm();

        form.AddField("dungeonId", targetData.dungeonId);

        form.AddField("dungeonName", dungeonName);

        form.AddField("creatorName", creatorName);

        form.AddField("jsonData", jsonData);

        // タグ未設定の場合はNORMAL
        string tag = "NORMAL";

        if (dungeonUIManager != null)
        {
            tag = dungeonUIManager.UploadTag;
        }

        form.AddField("tag", tag);

        UnityWebRequest request =
            UnityWebRequest.Post(
                ServerApi.Url("upload_dungeon.php"),
                form
            );

        yield return request.SendWebRequest();

        // 通信成功でもPHPが"success"を返さなければ失敗扱い
        if (request.result ==
            UnityWebRequest.Result.Success &&
            request.downloadHandler.text.Trim() == "success")
        {
            Debug.Log(
                "アップロード成功 : " +
                request.downloadHandler.text
            );
        }
        else
        {
            Debug.LogError(
                "アップロード失敗 : " +
                request.error + " / " +
                request.downloadHandler.text
            );
        }
    }
}
