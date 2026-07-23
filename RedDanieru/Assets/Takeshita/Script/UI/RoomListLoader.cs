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

    [SerializeField]
    private GameObject JoinCautionObj;

    public GameObject LaycastObj;
    public GameObject PSWCautionObj;
    public GameObject MaxPlayerCautionObj;

    public GameObject RoomInfoObj;
    public GameObject MatchingObj;
    public GameObject SelecCanvas;

    [SerializeField]
    private Text JoinCautionRoomText;

    [SerializeField]
    private InputField passwordInputField;

    private RoomData selectedRoom;

    [SerializeField]
    private FusionLauncher fusionLauncher;
    [SerializeField]
    private RoomDBUploader roomDBUploader;

    private void Start()
    {
        JoinCautionObj.SetActive(false);
        LaycastObj.SetActive(false);
        PSWCautionObj.SetActive(false);
        MatchingObj.SetActive(false);
        MaxPlayerCautionObj.SetActive(false);

        passwordInputField.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(LoadRooms());
    }

    IEnumerator LoadRooms()
    {
        UnityWebRequest request =
            UnityWebRequest.Get(
                "http://10.219.32.66/RedDaniel/GetRooms.php"
            );

        yield return request.SendWebRequest();

        //Debug.Log(request.downloadHandler.text);

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

        Debug.Log("Room Count : " + list.rooms.Length);

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

            RoomButton button =
                obj.GetComponent<RoomButton>();

            button.Setup(
                room,
                this,
                text
            );

            Transform lockImage =
                obj.transform.Find("LockImage");

            Transform unlockImage =
                obj.transform.Find("UnlockImage");

            if (lockImage != null)
            {
                lockImage.gameObject.SetActive(
                    room.is_private == 1
                );
            }

            if (unlockImage != null)
            {
                unlockImage.gameObject.SetActive(
                    room.is_private == 0
                );
            }
        }
    }

    public void RefreshRoomList()
    {
        StopAllCoroutines();

        StartCoroutine(
            LoadRooms()
        );
    }

    public void ShowJoinCaution(
     RoomData room)
    {
        selectedRoom = room;

        JoinCautionObj.SetActive(true);

        bool isPrivate = room.is_private == 1;

        passwordInputField.gameObject.SetActive(isPrivate);

        string roomType =
            room.is_private == 1
            ? "非公開ルーム"
            : "公開ルーム";

        JoinCautionRoomText.text =
            $"マップ : {room.map_name}\n" +
            $"RoomID : {room.room_id}\n" +
            $"{roomType}\n" +
            $"{room.current_players}/{room.max_players}";
    }

    public void YesButton()
    {
        if (selectedRoom == null)
            return;

        if (selectedRoom.current_players >= selectedRoom.max_players)
        {
            StartCoroutine(MaxPlayer());
            return;
        }

        if (selectedRoom.is_private == 1)
        {
            if (passwordInputField.text != selectedRoom.password)
            {
                StartCoroutine(PswObj());
                return;
            }
        }

        JoinCautionObj.SetActive(false);
        LaycastObj.SetActive(false);
        RoomInfoObj.SetActive(false);
        MatchingObj.SetActive(true);


        RoomInfo.RoomId =
            selectedRoom.room_id;

        RoomInfo.SelectedDungeon =
            selectedRoom.map_name;

        StartCoroutine(
            roomDBUploader.JoinRoom()
        );

        fusionLauncher.StartMatch(
            selectedRoom.room_id
        );

    }


    public IEnumerator PswObj()
    {
        PSWCautionObj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        PSWCautionObj.SetActive(false);
    }

    public IEnumerator MaxPlayer()
    {
        MaxPlayerCautionObj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        MaxPlayerCautionObj.SetActive(false);
    }

    public void NoButton()
    {
        passwordInputField.text = "";
        JoinCautionObj.SetActive(false);
        LaycastObj.SetActive(false);
    }

    public void LayCastObj()
    {
        LaycastObj.SetActive(true);
    }

    public void MatchingBack()
    {
        StartCoroutine(
            roomDBUploader.LeaveRoom()
        );

        fusionLauncher.CancelMatch();

        MatchingObj.SetActive(false);
        JoinCautionObj.SetActive(false);
        LaycastObj.SetActive(false);

        SelecCanvas.SetActive(true);
    }

}