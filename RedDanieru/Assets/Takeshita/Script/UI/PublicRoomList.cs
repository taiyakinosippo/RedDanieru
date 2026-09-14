using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PublicRoomList : MonoBehaviour
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private GameObject roomButtonPrefab;

    [SerializeField]
    private RoomListLoader roomListLoader;

    //private void OnEnable()
    //{
    //    StartCoroutine(LoadRooms());
    //}

    IEnumerator LoadRooms()
    {
             UnityWebRequest request =
            UnityWebRequest.Get(
                "http://10.219.32.66/RedDaniel/GetRooms.php"
            );

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        string json =
            "{\"rooms\":"
            + request.downloadHandler.text
            + "}";

        Debug.Log(json);

        RoomList list =
            JsonUtility.FromJson<RoomList>(
                json
            );

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomData room in list.rooms)
        {
            Debug.Log(
                $"取得部屋：{room.room_id} " +
                $"map={room.map_name} " +
                $"selected={RoomInfo.SelectedDungeonName}"
            );
        }

        foreach (RoomData room in list.rooms)
        {
            Debug.Log($"取得部屋：{room.room_id}/{room.map_name}");

            // 公開ルームのみ
            if (room.is_private == 1)
                continue;

            // 選択中マップのみ
            if (room.map_name !=
                RoomInfo.SelectedDungeonName)
                continue;

            CreateRoomButton(
                room.room_id,
                room.current_players +
                "/" +
                room.max_players,
                room
            );
        }
    }

    private void CreateRoomButton(string roomId,string playerCount,RoomData room)
    {
        GameObject obj =
            Instantiate(
                roomButtonPrefab,
                content
            );

        Button button = obj.GetComponent<Button>();

        button.onClick.AddListener(() =>
        {
            roomListLoader.ShowJoinCaution(room);
        });

        PublicRoomButton roomButton =
            obj.GetComponent<PublicRoomButton>();

        roomButton.roomIdText.text =
            roomId;

        roomButton.playerCountText.text =
            playerCount;
    }

    public void RefreshRoomList()
    {
        StartCoroutine(LoadRooms());
    }
}