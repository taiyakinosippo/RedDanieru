using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DungeonUploader : MonoBehaviour
{
    public void UploadDungeon(
        string dungeonName,
        string creatorName)
    {
        StartCoroutine(
            UploadCoroutine(
                dungeonName,
                creatorName
            )
        );
    }

    private IEnumerator UploadCoroutine(
        string dungeonName,
        string creatorName)
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json"
        );

        if (!File.Exists(path))
        {
            Debug.LogError(
                "JSONファイルが見つかりません"
            );
            yield break;
        }

        string jsonData =
            File.ReadAllText(path);

        WWWForm form = new WWWForm();

        form.AddField(
            "dungeonName",
            dungeonName
        );

        form.AddField(
            "creatorName",
            creatorName
        );

        form.AddField(
            "jsonData",
            jsonData
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                "http://10.219.32.66/RedDaniel/upload_dungeon.php",
                form
            );

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
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
                request.error
            );
        }
    }
}
