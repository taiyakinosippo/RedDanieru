using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PublicRoomList : MonoBehaviour
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private GameObject roomButtonPrefab;

    [SerializeField]
    private RoomListLoader roomListLoader;

    [SerializeField]
    private RoomDBUploader roomDBUploader;

    private void Awake()
    {
        if (roomDBUploader == null)
        {
            roomDBUploader = GetComponent<RoomDBUploader>();
        }
    }

    IEnumerator LoadRooms()
    {
        RoomData[] rooms = null;

        // テストモードならローカル、通常はPHPサーバーから取得
        yield return roomDBUploader.GetRooms(result => rooms = result);

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomData room in rooms)
        {
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
