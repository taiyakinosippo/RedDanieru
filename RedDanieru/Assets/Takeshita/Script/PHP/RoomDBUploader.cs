using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RoomDBUploader : MonoBehaviour
{
    public RoomData foundRoom;

    public IEnumerator UploadRoom()
    {
        WWWForm form = new WWWForm();

        Debug.Log("RoomId = " + RoomInfo.RoomId);
        Debug.Log("Map = " + RoomInfo.SelectedDungeonName);
        Debug.Log("Password = " + DungeonUIManager.Password);

        form.AddField(
            "room_id",
            RoomInfo.RoomId
        );

        form.AddField(
            "dungeon_id",
            RoomInfo.SelectedDungeon
            );

        form.AddField(
            "map_name",
            RoomInfo.SelectedDungeonName
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
                "http://10.219.32.66/RedDaniel/SaveRoom.php",
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
                "http://10.219.32.66/RedDaniel/DeleteRoom.php",
                form
            );

        yield return request.SendWebRequest();
    }

    public IEnumerator UpdateAlive()
    {
        WWWForm form = new WWWForm();

        form.AddField(
            "room_id",
            RoomInfo.RoomId ?? ""
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                "http://10.219.32.66/RedDaniel/UpdateRoomAlive.php",
                form
            );

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log("Alive更新");
        }
    }

    public IEnumerator JoinRoom()
    {
        WWWForm form = new WWWForm();

        form.AddField(
            "room_id",
            RoomInfo.RoomId
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                "http://10.219.32.66/RedDaniel/JoinRoom.php",
                form
            );

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log("人数追加成功");
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    public IEnumerator LeaveRoom()
    {
        WWWForm form = new WWWForm();

        form.AddField(
            "room_id",
            RoomInfo.RoomId
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                "http://10.219.32.66/RedDaniel/LeaveRoom.php",
                form
            );

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log("人数減少成功");
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    public IEnumerator SearchRoom(string roomId)
    {
        WWWForm form = new WWWForm();

        form.AddField(
            "room_id",
            roomId
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                "http://10.219.32.66/RedDaniel/SearchRoom.php",
                form
            );

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            string json =
                request.downloadHandler.text;

            Debug.Log("検索結果 : " + json);

            if (string.IsNullOrEmpty(json) ||
                json == "NOT_FOUND")
            {
                foundRoom = null;
            }
            else
            {
                foundRoom =
                    JsonUtility.FromJson<RoomData>(
                        json
                    );
            }
        }
        else
        {
            Debug.LogError(request.error);

            foundRoom = null;
        }
    }
}