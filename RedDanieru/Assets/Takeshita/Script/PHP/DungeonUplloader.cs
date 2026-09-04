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

            if (data.dungeonId == SaveManager.LastDungeonId)
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
