using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System.Collections;
using UnityEngine.SceneManagement;

//ゲームモード
public static class GameModeManager
{
    public static bool IsMultiplayer = false;
}

//選択したダンジョン名保存
public static class RoomInfo
{
    public static string SelectedDungeon;
    public static string SelectedDungeonName;

    public static string RoomId;
    public static string Password;
    public static bool IsPrivate;
    public static int MaxPlayers;
}

public static class RoomIdGenerator
{
    private const string Characters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateRoomId()
    {
        char[] id = new char[4];

        for(int i = 0; i < id.Length; i++)
        {
            id[i] = Characters[
                Random.Range(0, Characters.Length)
                ];
        }

        return new string(id);
    }
}

[System.Serializable]
public class RoomData
{
    public string room_id;

    public string dungeon_id;

    public string map_name;
    public string password;

    public int is_private;
    public int max_players;
    public int current_players;
}

[System.Serializable]
public class RoomList
{
    public RoomData[] rooms;
}

public class DungeonUIManager : MonoBehaviour
{
    public DungeonUploader uploader;
    public DungeonImporter importer;

    public TMP_InputField dungeonNameInput;
    public TMP_InputField creatorNameInput;

    [Header("マップ選択画面")]
    public GameObject ScrolView;

    public GameObject MatchingRoomCreateWindow;

    public GameObject RoomCreateObj;
    public Button RoomHostButton;
    public GameObject RoomJoinObj;
    public Button JoinButton;

    [Header("部屋検索")]
    public GameObject RoomSearchObj;
    public Button RoomInButton;

    private bool roomFound;
    private bool roomHasPassword;
    private string roomPassword;
    [SerializeField] private TMP_InputField roomSearchPasswordInput;
    [SerializeField] private TMP_InputField roomIdInput;
    [SerializeField] private TMP_InputField privateRoomIdInput;
    [SerializeField] private TMP_InputField privatePasswordInput;
    [SerializeField] private Button privateJoinButton;

    public GameObject Laycast;
    public GameObject RoomSearchLaycast;

    [Header("ステージ検索")]
    public GameObject StageSearchObj;
    [SerializeField] private TMP_InputField stageNameSearchInput;
    [SerializeField] private TMP_InputField creatorNameSearchInput;
    [SerializeField] private Button searchButton;
    public GameObject StageSearchLaycast;

    [Header("Caution")]
    public GameObject CautionObj;
    public GameObject RoomInCautionObj;
    public GameObject RoomInCautionLayout;

    [Header("マッチング諸々")]
    public GameObject MatchingObj;
    public GameObject MatchingPlayerObj;
    public Text MatchingPlayerText;
    public GameObject MatchingCautionObj;

    public Button GameStartbutton;

    [Header("UI")]
    [SerializeField]private Text dungeonNameText;
    [SerializeField] private Text RoomIdText;
    [SerializeField] private Text RoomKeyText;
    [SerializeField] private Text CautionText;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Dropdown playerCountDropdown;
    [SerializeField] private TMP_InputField createRoomIdInput;
  
    [Header("大事な奴ら")]
    [SerializeField] private FusionLauncher fusionLauncher;
    [SerializeField] private RoomDBUploader roomDBUploader;

    [SerializeField] private PlayerSpawner playerSpawner;

    [SerializeField] private GameStartManager gameManager;

    [SerializeField] private NetworkGameState networkGameState;

    [SerializeField] private RoomListLoader roomListLoader;

    [SerializeField] private PublicRoomList publicRoomList;
    [SerializeField] private LoadUI loadUI;

    [Header("数値")]
    public static int MaxPlayers = 2;

    public static bool IsPrivateRoom;
    public static string Password="";

    [Header("コルーチン")]
    private Coroutine aliveCoroutine;
    private Coroutine searchCoroutine;

    public void Start()
    {
         ScrolView.SetActive(true);
        MatchingRoomCreateWindow.SetActive(false);
        Laycast.SetActive(false);
        CautionObj.SetActive(false);
        MatchingObj.SetActive(false);
        MatchingCautionObj.SetActive(false);
        RoomSearchObj.SetActive(false);
        RoomSearchLaycast.SetActive(false);
        RoomInCautionObj.SetActive(false);
        RoomInCautionLayout.SetActive(false);
        StageSearchLaycast.SetActive(false);
        StageSearchObj.SetActive(false);

        RoomInButton.interactable = false;
        GameStartbutton.interactable = false;
        RoomHostButton.interactable = false;
        JoinButton.interactable = true;
        privateJoinButton.interactable = false;
        searchButton.interactable = false;

        privateRoomIdInput.onValueChanged.AddListener(delegate { CheckPrivateRoom(); });

        privatePasswordInput.onValueChanged.AddListener(delegate { CheckPrivateRoom(); });

        stageNameSearchInput.onValueChanged.AddListener(delegate { CheckSearchCondition(); });

        creatorNameSearchInput.onValueChanged.AddListener(delegate { CheckSearchCondition(); });

        passwordInputField.onValueChanged.AddListener(OnPasswordChanged);

        playerCountDropdown.onValueChanged.AddListener(OnPlayerCountChanged);

        OnPlayerCountChanged(playerCountDropdown.value);

        privateRoomIdInput.onValueChanged.AddListener(OnPrivateRoomIdChanged);

        roomSearchPasswordInput.onValueChanged.AddListener(OnRoomSearchPasswordChanged);
        roomIdInput.onValueChanged.AddListener(OnRoomIdChanged);

        createRoomIdInput.onValueChanged.AddListener(OnCreateRoomIdChanged);
    }

    private void Update()
    {
        if (!GameModeManager.IsMultiplayer)
            return;

        NetworkRunner runner = FindObjectOfType<NetworkRunner>();

        if (runner == null)
        {
            GameStartbutton.interactable = false;

            MatchingPlayerText.text =
                $"待機中... (0/{MaxPlayers})";

            return;
        }

        int playerCount = 0;

        foreach (var player in runner.ActivePlayers)
        {
            playerCount++;
        }

        int displayCount = playerCount;

        if (displayCount >= MaxPlayers)
        {
            MatchingPlayerText.text =
                $"マッチング完了！ ({displayCount}/{MaxPlayers})";
        }
        else
        {
            MatchingPlayerText.text =
                $"待機中... ({displayCount}/{MaxPlayers})";
        }

        GameStartbutton.interactable =
            displayCount >= 2;

        foreach(var player in runner.ActivePlayers)
        {
            if(runner.TryGetPlayerObject(
                player,
                out NetworkObject obj))
            {
                HideMatchingUI();
                break;
            }
        }

        Debug.Log($"playerCount = {playerCount}");
        Debug.Log($"displayCount = {displayCount}");
        Debug.Log($"MaxPlayers = {MaxPlayers}");

    }


    public void UploadDungeon()
    {
        uploader.UploadDungeon(
            dungeonNameInput.text,
            creatorNameInput.text
        );
    }

    public void DownloadDungeon()
    {
        importer.ImportDungeon(
            dungeonNameInput.text
        );
    }

    public void SoloMode()
    {
        GameModeManager.IsMultiplayer = false;

        Debug.Log("Solo");

        ScrolView.SetActive(false);

        importer.ImportDungeon(
            RoomInfo.SelectedDungeon
        );

        fusionLauncher.StartSolo();
    }

    public void MultiMode()
    {
        GameModeManager.IsMultiplayer = true;
        Debug.Log("Multi");
        MapSelectButton();
    }

    public void ScrollBackButton()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void  RoomInfoBackButton()
    {
        ScrolView.SetActive(true);
        MatchingRoomCreateWindow.SetActive(false);
    }

    public void RoomSearchButton()
    {
        RoomSearchLaycast.SetActive(true);
        RoomSearchObj.SetActive(true);
    }

    public void RoomSearchBackButton()
    {
        RoomSearchLaycast.SetActive(false);
        RoomSearchObj.SetActive(false);
    }

    public void RoomCreateButton()
    {
        RoomCreateObj.SetActive(true);
        RoomJoinObj.SetActive(false);

        JoinButton.interactable = true;
        RoomHostButton.interactable = false;
    }

    public void RoomJoinButton()
    {
        RoomCreateObj.SetActive(false);
        RoomJoinObj.SetActive(true);

        RoomHostButton.interactable = true;
        JoinButton.interactable = false;

        publicRoomList.RefreshRoomList();
    }

    public void MapSelectButton()
    {
        Debug.Log("MapSelectButton");
        Debug.Log("DungeonName=" + RoomInfo.SelectedDungeonName);

        dungeonNameText.text =
            "マップ：" + RoomInfo.SelectedDungeonName;

        MatchingRoomCreateWindow.SetActive(true);
      
        RoomCreateObj.SetActive(true);
        RoomJoinObj.SetActive(false);
    }

    public void CreateButton()
    {
        // RoomID決定
        if (string.IsNullOrEmpty(createRoomIdInput.text))
        {
            RoomInfo.RoomId =
                RoomIdGenerator.GenerateRoomId();
        }
        else
        {
            RoomInfo.RoomId =
                createRoomIdInput.text;
        }

        Debug.Log("RoomID = " + RoomInfo.RoomId);

        Laycast.SetActive(true);
        CautionObj.SetActive(true);

        string roomId = RoomInfo.RoomId;

        string roomType;

        if (IsPrivateRoom)
        {
            roomType =
                "非公開ルーム\n" +
                "パスワード：" + Password;
        }
        else
        {
            roomType = "公開ルーム";
        }

        CautionText.text =
            "マップ：" + RoomInfo.SelectedDungeonName +
            "\nRoomID：" + roomId +
            "\n" + roomType +
            "\n最大人数：" + MaxPlayers + "人";
    }

    public void YesButton()
    {
        StartCoroutine(
            roomDBUploader.UploadRoom()
        );

        aliveCoroutine =
            StartCoroutine(
                SendAliveLoop()
            );

        Laycast.SetActive(false);
        CautionObj.SetActive(false);
        ScrolView.SetActive(false);
        MatchingRoomCreateWindow.SetActive(false);
        MatchingObj.SetActive(true);

        if (GameModeManager.IsMultiplayer)
        {
            fusionLauncher.StartMatch(
                RoomInfo.RoomId
            );
        }
        else
        {
            fusionLauncher.StartSolo();
        }
    }

    public void NoButton()
    {
      
        Laycast.SetActive(false);
        CautionObj.SetActive(false);
    }

    public void MatchingBackButton()
    {
        MatchingCautionObj.SetActive(true);
    }

    public void MatchingLeaveButton()
    {
        if (aliveCoroutine != null)
        {
            StopCoroutine(aliveCoroutine);
        }

        StartCoroutine(
            roomDBUploader.DeleteRoom()
        );

        fusionLauncher.CancelMatch();

        MatchingCautionObj.SetActive(false);
        MatchingObj.SetActive(false);
    }

    public void MatchingNoLeaveButton()
    {
        MatchingCautionObj.SetActive(false);
    }

    public void OnClickRoomInButton()
    {
        if (roomDBUploader.foundRoom == null)
            return;

        RoomInfo.SelectedDungeon = roomDBUploader.foundRoom.map_name;

        importer.ImportDungeon(RoomInfo.SelectedDungeon);

        roomListLoader.ShowJoinCaution(roomDBUploader.foundRoom);

        RoomInCautionObj.SetActive(true);
        RoomInCautionLayout.SetActive(true);
    }

    public void GameStartButton()
    {
        if (NetworkGameState.Instance == null)
            return;

        NetworkGameState.Instance.RPC_StartGame();

        HideMatchingUI();
    }

    public void PrivateRoomJoinButton()
    {
        StartCoroutine(SearchPrivateRoom());
    }

    private IEnumerator SearchPrivateRoom()
    {
        yield return roomDBUploader.SearchRoom(privateRoomIdInput.text);

        RoomData room = roomDBUploader.foundRoom;

        if (room == null)
        {
            Debug.Log("部屋が見つからない");
            yield break;
        }

        if (room.password != privatePasswordInput.text)
        {
            Debug.Log("パスワード不一致");
            yield break;
        }

        roomListLoader.ShowJoinCaution(room);
    }

    public void SearchDungeonButton()
    {
        string stageName =
            stageNameSearchInput.text;

        string creatorName =
            creatorNameSearchInput.text;

        loadUI.SearchDungeon(
            stageName,
            creatorName
        );

        StageSearchObj.SetActive(false);
        StageSearchLaycast.SetActive(false);
    }

    public void StageSearchButton()
    {
        StageSearchObj.SetActive(true);
        StageSearchLaycast.SetActive(true);
    }

    public void StageSearchBackButton()
    {
        StageSearchObj.SetActive(false);
        StageSearchLaycast.SetActive(false);
    }

    private void OnPasswordChanged(string value)
    {
        // 数字以外を除去
        string numbersOnly = "";

        foreach (char c in value)
        {
            if (char.IsDigit(c))
            {
                numbersOnly += c;
            }
        }

        // 5文字まで
        if (numbersOnly.Length > 5)
        {
            numbersOnly = numbersOnly.Substring(0, 5);
        }

        // InputFieldへ反映
        if (passwordInputField.text != numbersOnly)
        {
            passwordInputField.text = numbersOnly;
        }

        Password = numbersOnly;

        // 1文字でも入力されたらPrivate
        IsPrivateRoom = !string.IsNullOrEmpty(numbersOnly);

        if (IsPrivateRoom)
        {
            RoomKeyText.text = "Private";
        }
        else
        {
            RoomKeyText.text = "Public";
        }

        Debug.Log(
            IsPrivateRoom
            ? "Private Room"
            : "Public Room"
        );
    }

    private void CheckPrivateRoom()
    {
        if (roomDBUploader.foundRoom == null)
        {
            Debug.Log("foundRoom NULL");
            privateJoinButton.interactable = false;
            return;
        }

        bool roomMatch =
            privateRoomIdInput.text ==
            roomDBUploader.foundRoom.room_id;

        bool passwordMatch =
            privatePasswordInput.text ==
            roomDBUploader.foundRoom.password;

        Debug.Log("roomMatch=" + roomMatch);
        Debug.Log("passwordMatch=" + passwordMatch);

        privateJoinButton.interactable =
            roomMatch && passwordMatch;

        Debug.Log(
            "interactable=" +
            privateJoinButton.interactable
        );
    }

    private void OnPrivateRoomIdChanged(string roomId)
    {
        StartCoroutine(SearchPrivateRoom(roomId));
    }

    private IEnumerator SearchPrivateRoom(string roomId)
    {
        yield return roomDBUploader.SearchRoom(roomId);

        CheckPrivateRoom();
    }

    private void OnPlayerCountChanged(int index)
    {
        MaxPlayers = index + 2;

        Debug.Log(
            $"最大人数 : {MaxPlayers}人"
        );
    }

    private void CheckSearchCondition()
    {
        bool found =
            loadUI.ExistsDungeon(
                stageNameSearchInput.text,
                creatorNameSearchInput.text
            );

        searchButton.interactable = found;

        if (found)
        {
            Debug.Log("見つけた");
        }
    }

    private IEnumerator SendAliveLoop()
    {
        while (true)
        {
            yield return roomDBUploader.UpdateAlive();

            //yield return new WaitForSeconds(5f);
        }
    }

    private void OnRoomSearchPasswordChanged(string value)
    {
        if (!roomFound)
        {
            RoomInButton.interactable = false;
            return;
        }

        if (!roomHasPassword)
        {
            RoomInButton.interactable = true;
            return;
        }

        RoomInButton.interactable =
            !string.IsNullOrEmpty(value);
    }

    private void OnRoomIdChanged(string roomId)
    {
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
        }

        searchCoroutine = StartCoroutine(DelayedSearch(roomId));
    }

    private void OnCreateRoomIdChanged(string value)
    {
        string validText = "";

        foreach (char c in value.ToUpper())
        {
            if ((c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9'))
            {
                validText += c;
            }
        }

        if (validText.Length > 4)
        {
            validText = validText.Substring(0, 4);
        }

        if (createRoomIdInput.text != validText)
        {
            createRoomIdInput.text = validText;
        }

        RoomHostButton.interactable =
            validText.Length == 4;
    }

    private IEnumerator DelayedSearch(string roomId)
    {
        yield return new WaitForSeconds(0.5f);

        yield return SearchRoomCoroutine(roomId);
    }

    private IEnumerator SearchRoomCoroutine(string roomId)
    {
        yield return roomDBUploader.SearchRoom(roomId);

        RoomData room = roomDBUploader.foundRoom;

        if (room == null)
        {
            roomFound = false;

            RoomInButton.interactable = false;
            yield break;
        }

        roomFound = true;

        roomHasPassword = room.is_private == 1;

        roomPassword = room.password;

        if (!roomHasPassword)
        {
            RoomInButton.interactable = true;
        }
        else
        {
            RoomInButton.interactable = false;
        }
    }

    public string RoomSearchPassword
    {
        get
        {
            return roomSearchPasswordInput.text;
        }
    }

    public void HideMatchingUI()
    {
        MatchingObj.SetActive(false);
        MatchingCautionObj.SetActive(false);
        MatchingPlayerObj.SetActive(false);

        ScrolView.SetActive(false);
        MatchingRoomCreateWindow.SetActive(false);

        RoomCreateObj.SetActive(false);
        RoomJoinObj.SetActive(false);

        Laycast.SetActive(false);
        CautionObj.SetActive(false);

        RoomSearchObj.SetActive(false);
    }

    public void MatchingNow()
    {
        ScrolView.SetActive(false);
        RoomSearchObj.SetActive(false);
    }

    private IEnumerator SearchDungeon(string stageName,string creatorName)
    {
        yield return null;
    }

    public string PrivatePassword
    {
        get
        {
            return privatePasswordInput.text;
        }
    }

}