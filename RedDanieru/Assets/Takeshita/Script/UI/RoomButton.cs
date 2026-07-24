using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    private RoomData roomData;

    private RoomListLoader roomListLoader;

    public void Setup(
        RoomData room,
        RoomListLoader loader,
        Text text)
    {
        roomData = room;
        roomListLoader = loader;

        text.text =
            $"マップ : {room.map_name}\n" +
            $"RoomID : {room.room_id}\n" +
            $"{room.current_players}/{room.max_players}";
    }

    public void OnClick()
    {
        Debug.Log("roomData = " + roomData);
        Debug.Log("roomListLoader = " + roomListLoader);

        roomListLoader.ShowJoinCaution(roomData);

        roomListLoader.LayCastObj();
    }
}