using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ルーム情報（ルームID・マップ・パスワード・人数）の保存と検索
/// テストモードのときはPHPサーバーの代わりにLocalTestServerを使う
/// </summary>
public class RoomDBUploader : MonoBehaviour
{
    public RoomData foundRoom;

    // サーバーが落ちているときに固まらないようにする
    private const int TimeoutSeconds = 5;

    public IEnumerator UploadRoom()
    {
        RoomData room = new RoomData
        {
            room_id = RoomInfo.RoomId,
            dungeon_id = RoomInfo.SelectedDungeon,
            map_name = RoomInfo.SelectedDungeonName,
            password = RoomInfo.Password ?? "",
            is_private = RoomInfo.IsPrivate ? 1 : 0,
            max_players = RoomInfo.MaxPlayers,
            current_players = 1
        };

        Debug.Log($"ルーム保存 ID={room.room_id} Map={room.map_name} Private={room.is_private} Max={room.max_players}");

        if (OnlineTestMode.Enabled)
        {
            LocalTestServer.SaveRoom(room);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("room_id", room.room_id);
        form.AddField("dungeon_id", room.dungeon_id ?? "");
        form.AddField("map_name", room.map_name ?? "");
        form.AddField("password", room.password);
        form.AddField("is_private", room.is_private);
        form.AddField("max_players", room.max_players);

        yield return Post("SaveRoom.php", form, "ルーム保存");
    }

    public IEnumerator DeleteRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            yield break;

        if (OnlineTestMode.Enabled)
        {
            LocalTestServer.DeleteRoom(roomId);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("room_id", roomId);

        yield return Post("DeleteRoom.php", form, "ルーム削除");
    }

    /// <summary>
    /// ホストが定期的に呼ぶ。生存通知と、DBの人数を実際の接続数に合わせる
    /// </summary>
    public IEnumerator UpdateAlive(string roomId, int currentPlayers)
    {
        if (string.IsNullOrEmpty(roomId))
            yield break;

        if (OnlineTestMode.Enabled)
        {
            LocalTestServer.KeepAlive(roomId, currentPlayers);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("room_id", roomId);

        yield return Post("UpdateRoomAlive.php", form, null);

        // PHP側は +1/-1 しかできないので、差分の回数だけ呼んで実際の人数に合わせる
        RoomData room = null;

        yield return FetchRoom(roomId, result => room = result);

        if (room == null)
            yield break;

        int diff = Mathf.Clamp(currentPlayers - room.current_players, -3, 3);

        for (int i = 0; i < Mathf.Abs(diff); i++)
        {
            WWWForm countForm = new WWWForm();
            countForm.AddField("room_id", roomId);

            yield return Post(diff > 0 ? "JoinRoom.php" : "LeaveRoom.php", countForm, null);
        }
    }

    public IEnumerator SearchRoom(string roomId)
    {
        RoomData room = null;

        yield return FetchRoom(roomId, result => room = result);

        foundRoom = room;
    }

    public IEnumerator GetRooms(Action<RoomData[]> onLoaded)
    {
        if (OnlineTestMode.Enabled)
        {
            onLoaded?.Invoke(LocalTestServer.GetRooms());
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(ServerApi.Url("GetRooms.php")))
        {
            request.timeout = TimeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("ルーム一覧の取得に失敗 : " + request.error);
                onLoaded?.Invoke(Array.Empty<RoomData>());
                yield break;
            }

            RoomList list =
                JsonUtility.FromJson<RoomList>("{\"rooms\":" + request.downloadHandler.text + "}");

            onLoaded?.Invoke(list?.rooms ?? Array.Empty<RoomData>());
        }
    }

    private IEnumerator FetchRoom(string roomId, Action<RoomData> onLoaded)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            onLoaded(null);
            yield break;
        }

        if (OnlineTestMode.Enabled)
        {
            onLoaded(LocalTestServer.FindRoom(roomId));
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("room_id", roomId);

        using (UnityWebRequest request = UnityWebRequest.Post(ServerApi.Url("SearchRoom.php"), form))
        {
            request.timeout = TimeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("ルーム検索に失敗 : " + request.error);
                onLoaded(null);
                yield break;
            }

            string json = request.downloadHandler.text;

            if (string.IsNullOrEmpty(json) || json == "NOT_FOUND")
            {
                onLoaded(null);
                yield break;
            }

            onLoaded(JsonUtility.FromJson<RoomData>(json));
        }
    }

    private IEnumerator Post(string file, WWWForm form, string logLabel)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(ServerApi.Url(file), form))
        {
            request.timeout = TimeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"{file} に失敗 : {request.error}");
            }
            else if (logLabel != null)
            {
                Debug.Log($"{logLabel}成功 : {request.downloadHandler.text}");
            }
        }
    }
}
