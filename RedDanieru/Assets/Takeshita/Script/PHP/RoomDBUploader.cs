using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RoomDBUploader : MonoBehaviour
{
    public IEnumerator UploadRoom()
    {
        WWWForm form = new WWWForm();

        Debug.Log("RoomId = " + RoomInfo.RoomId);
        Debug.Log("Map = " + RoomInfo.SelectedDungeon);
        Debug.Log("Password = " + DungeonUIManager.Password);

        form.AddField(
            "room_id",
            RoomInfo.RoomId
        );

        form.AddField(
            "map_name",
            RoomInfo.SelectedDungeon
        );

        form.AddField(
            "password",
            DungeonUIManager.Password
        );

        form.AddField(
            "is_private",
            DungeonUIManager.IsPrivateRoom ? 1 : 0
        );

        form.AddField(
            "max_players",
            DungeonUIManager.MaxPlayers
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                "http://localhost/RedDaniel/SaveRoom.php",
                form
            );

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log(
                "ルーム保存成功 : " +
                request.downloadHandler.text
            );
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    public IEnumerator DeleteRoom()
    {
        WWWForm form = new WWWForm();

        form.AddField(
            "room_id",
            RoomInfo.RoomId
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                "http://localhost/RedDaniel/DeleteRoom.php",
                form
            );

        yield return request.SendWebRequest();
    }
}