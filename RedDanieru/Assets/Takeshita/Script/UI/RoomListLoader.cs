using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoomListLoader : MonoBehaviour
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private GameObject roomButtonPrefab;

    private void OnEnable()
    {
        StartCoroutine(LoadRooms());
    }

    IEnumerator LoadRooms()
    {
        UnityWebRequest request =
            UnityWebRequest.Get(
                "http://localhost/RedDaniel/GetRooms.php"
            );

        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        string json =
            "{\"rooms\":" +
            request.downloadHandler.text +
            "}";

        RoomList list =
            JsonUtility.FromJson<RoomList>(json);

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomData room in list.rooms)
        {
            GameObject obj =
                Instantiate(
                    roomButtonPrefab,
                    content
                );

            Text text =
                obj.GetComponentInChildren<Text>();

            text.text =
                $"マップ : {room.map_name}\n" +
                $"RoomID : {room.room_id}\n" +
                $"{(room.is_private == 1 ? "非公開" : "公開")}\n" +
                $"{room.current_players}/{room.max_players}";
        }
    }
}