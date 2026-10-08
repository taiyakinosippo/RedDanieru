using System.Collections;
using TMPro;
using UnityEngine;
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

    public GameObject MatchingObj;
    public GameObject SelecCanvas;
    public GameObject MachingRoomCreateText;
    public GameObject SclollView;

    [SerializeField]
    private TextMeshProUGUI JoinCautionRoomText;

    [Tooltip("参加待ち画面の状態表示（未設定ならMatchingObj内の「MatchingNow_text (1)」を使う）")]
    [SerializeField]
    private TMP_Text joinStatusText;

    private RoomData selectedRoom;

    // 確認画面を開いたときに入力されていたパスワード
    private string selectedRoomPassword;

    // 接続に失敗したときに出す文
    private string joinMessage;

    [SerializeField]
    private FusionLauncher fusionLauncher;
    [SerializeField]
    private RoomDBUploader roomDBUploader;

    [SerializeField]
    private DungeonUIManager dungeonUIManager;

    [SerializeField]
    private DungeonImporter importer;

    private Coroutine refresCoroutine;

    private void Start()
    {
        JoinCautionObj.SetActive(false);
        LaycastObj.SetActive(false);
        PSWCautionObj.SetActive(false);
        MatchingObj.SetActive(false);
        MaxPlayerCautionObj.SetActive(false);

        if (joinStatusText == null)
        {
            Transform status = MatchingObj.transform.Find("MatchingNow_text (1)");

            if (status != null)
            {
                joinStatusText = status.GetComponent<TMP_Text>();
            }
        }

        fusionLauncher.MatchFailed += OnMatchFailed;
    }

    private void OnDestroy()
    {
        if (fusionLauncher != null)
        {
            fusionLauncher.MatchFailed -= OnMatchFailed;
        }
    }

    private void OnEnable()
    {
        // 一覧の表示先が無いときは取得しない
        if (content != null)
        {
            refresCoroutine =
                StartCoroutine(RefreshLoop());
        }
    }

    private void OnDisable()
    {
        if (refresCoroutine != null)
        {
            StopCoroutine(refresCoroutine);
        }
    }

    private void Update()
    {
        if (!MatchingObj.activeSelf || joinStatusText == null)
            return;

        if (!string.IsNullOrEmpty(joinMessage))
        {
            joinStatusText.text = joinMessage;
        }
        else if (fusionLauncher.Runner == null)
        {
            joinStatusText.text = "接続中...";
        }
        else
        {
            joinStatusText.text =
                $"ホストの開始を待っています ({fusionLauncher.PlayerCount}/{fusionLauncher.MaxPlayers})\n" +
                $"ルームID : {RoomInfo.RoomId}";
        }
    }

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            yield return LoadRooms();

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator LoadRooms()
    {
        RoomData[] rooms = null;

        yield return roomDBUploader.GetRooms(result => rooms = result);

        if (content == null || rooms == null)
            yield break;

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomData room in rooms)
        {
            if (room.is_private == 1)
                continue;

            if (room.map_name != RoomInfo.SelectedDungeonName)
                continue;

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

    public void ShowJoinCaution(RoomData room)
    {
        ShowJoinCaution(room, "");
    }

    /// <summary>
    /// 参加確認を出す（ここではまだ接続しない）
    /// password : 非公開ルームのときに入力されていたパスワード
    /// </summary>
    public void ShowJoinCaution(
     RoomData room, string password)
    {
        selectedRoom = room;
        selectedRoomPassword = password ?? "";

        JoinCautionObj.SetActive(true);

        string roomType =
            room.is_private == 1
            ? "非公開ルーム"
            : "公開ルーム";

        JoinCautionRoomText.text =
            $"マップ : {room.map_name}\n" +
            $"RoomID : {room.room_id}\n" +
            $"{roomType}\n" +
            $"{room.current_players}/{room.max_players}";

        Debug.Log("map_name = " + room.map_name);
    }

    public void YesButton()
    {
        if (selectedRoom == null)
            return;

        StartCoroutine(JoinFlow(selectedRoom.room_id, selectedRoomPassword));
    }

    /// <summary>
    /// ルームに参加する（最新の情報で満員・パスワードを確認 → ステージ読込 → 接続）
    /// </summary>
    private IEnumerator JoinFlow(string roomId, string password)
    {
        yield return roomDBUploader.SearchRoom(roomId);

        RoomData room = roomDBUploader.foundRoom;

        if (room == null)
        {
            Debug.Log("ルームが見つかりません : " + roomId);

            JoinCautionRoomText.text =
                $"RoomID : {roomId}\n" +
                "ルームが見つかりません\n" +
                "（終了したか、既に始まっています）";
            yield break;
        }

        Debug.Log("dungeon_id = " + room.dungeon_id);

        if (room.current_players >=
            room.max_players)
        {
            StartCoroutine(MaxPlayer());
            yield break;
        }

        // プライベートルームの場合
        if (room.is_private == 1 &&
            password.Trim() != (room.password ?? "").Trim())
        {
            Debug.Log("パスワード不一致");

            StartCoroutine(PswObj());
            yield break;
        }

        JoinCautionObj.SetActive(false);
        LaycastObj.SetActive(false);
        MatchingObj.SetActive(true);
        MachingRoomCreateText.SetActive(false);

        GameModeManager.IsMultiplayer = true;

        RoomInfo.RoomId = room.room_id;
        RoomInfo.SelectedDungeon = room.dungeon_id;
        RoomInfo.SelectedDungeonName = room.map_name;
        RoomInfo.MaxPlayers = room.max_players;
        RoomInfo.Password = room.password;
        RoomInfo.IsPrivate = room.is_private == 1;

        joinMessage = null;

        importer.ImportDungeon(
            room.dungeon_id
        );

        // 人数はホストが実際の接続数で更新するので、ここではDBの人数を増やさない
        fusionLauncher.StartMatch(
            room.room_id,
            room.max_players,
            false
        );

        dungeonUIManager.MatchingNow();
    }

    private void OnMatchFailed(string message)
    {
        if (MatchingObj.activeSelf)
        {
            joinMessage = message + "\n戻るボタンで戻ってください";
        }
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
       JoinCautionObj.SetActive(false);
        LaycastObj.SetActive(false);
    }

    public void LayCastObj()
    {
        LaycastObj.SetActive(true);
    }

    public void MatchingBack()
    {
        fusionLauncher.CancelMatch();

        // 前の部屋の情報を次に持ち越さない
        RoomInfo.ClearRoom();
        joinMessage = null;

        MatchingObj.SetActive(false);
        JoinCautionObj.SetActive(false);
        LaycastObj.SetActive(false);

        SclollView.SetActive(true);
    }

}
