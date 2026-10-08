using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    // 部屋を出たら前の部屋の情報を残さない（次の部屋で使い回されるのを防ぐ）
    public static void ClearRoom()
    {
        RoomId = null;
        Password = null;
        IsPrivate = false;
        MaxPlayers = 0;
    }
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
    public GameObject MatchingRoomCreateLaycast;

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
    [SerializeField] private Button EASYButton;
    [SerializeField] private Button NORMALButton;
    [SerializeField] private Button HARDButton;
    private readonly Color normalColor = new Color32(0x96, 0xCD, 0xFF, 255);
    private readonly Color selectedColor = new Color32(0x8B, 0xA4, 0xBA, 255);

    [Header("Caution")]
    public GameObject CautionObj;
    public GameObject RoomInCautionObj;
    public GameObject RoomInCautionLayout;

    [Header("マッチング諸々")]
    public GameObject MatchingObj;
    public GameObject MatchingPlayerObj;
    public TextMeshProUGUI MatchingPlayerText;
    public GameObject MatchingCautionObj;

    public Button GameStartbutton;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI RoomKeyText;
    [SerializeField] private TextMeshProUGUI CautionText;
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

    private HashSet<string> selectedTags =new HashSet<string>();

    [Header("数値")]
    public static int MaxPlayers = 2;

    public static bool IsPrivateRoom;
    public static string Password="";

    [Header("コルーチン")]
    private Coroutine aliveCoroutine;
    private Coroutine searchCoroutine;
    private Coroutine hostCoroutine;

    // ホスト（ルームを作った人）か
    private bool isRoomHost;

    // 接続に失敗したときなどに人数表示の代わりに出す文
    private string matchingMessage;

    // ルームIDを手入力したか（自動生成なら使用中のとき作り直せる）
    private bool isManualRoomId;

    // マルチのゲームが始まっているか
    private bool isInGame;

    private const float AliveInterval = 3f;

    public void Start()
    {
         ScrolView.SetActive(true);
        MatchingRoomCreateWindow.SetActive(false);
        MatchingRoomCreateLaycast.SetActive(false);
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

        if (BGMManager_Takeshita.Instance != null)
        {
            BGMManager_Takeshita.Instance.PlayNormalBGM();
        }

        fusionLauncher.MatchFailed += OnMatchFailed;
        fusionLauncher.GameStarted += OnGameStarted;
    }

    private void OnDestroy()
    {
        if (fusionLauncher != null)
        {
            fusionLauncher.MatchFailed -= OnMatchFailed;
            fusionLauncher.GameStarted -= OnGameStarted;
        }
    }

    private void Update()
    {
        if (!GameModeManager.IsMultiplayer || !MatchingObj.activeSelf)
            return;

        string roomId = string.IsNullOrEmpty(RoomInfo.RoomId) ? "未設定" : RoomInfo.RoomId;

        string password = string.IsNullOrEmpty(RoomInfo.Password) ? "ナシ" : RoomInfo.Password;

        // 人数は実際に接続している数（Fusion）を表示する
        int playerCount = fusionLauncher.PlayerCount;
        int maxPlayers = fusionLauncher.MaxPlayers;

        string status;

        if (!string.IsNullOrEmpty(matchingMessage))
        {
            status = matchingMessage;
        }
        else if (fusionLauncher.Runner == null)
        {
            status = "接続中...";
        }
        else if (playerCount >= maxPlayers)
        {
            status = $"マッチング完了！ ({playerCount}/{maxPlayers})";
        }
        else
        {
            status = $"待機中... ({playerCount}/{maxPlayers})";
        }

        MatchingPlayerText.text =
             $"{status}\n" +
             $"ルームID : {roomId}\n" +
             $"パスワード : {password}";

        // 開始できるのは部屋の代表（マスタークライアント）だけ。2人以上で押せる
        GameStartbutton.interactable =
            fusionLauncher.IsMasterClient &&
            playerCount >= 2 &&
            NetworkGameState.Instance != null &&
            !NetworkGameState.Instance.GameStarted;
    }

    public string UploadTag
    {
        get
        {
            if (selectedTags.Contains("EASY"))
                return "EASY";

            if (selectedTags.Contains("NORMAL"))
                return "NORMAL";

            if (selectedTags.Contains("HARD"))
                return "HARD";

            return "NORMAL";
        }
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
        BGMManager_Takeshita.Instance.PlayBattleBGM();

        GameModeManager.IsMultiplayer = false;

        GameStopManager gameStopManager =FindObjectOfType<GameStopManager>();

        if (gameStopManager != null)
        {
            gameStopManager.EnablePauseMenu();
        }

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
        //Debug.Log("Multi");
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
        MatchingRoomCreateLaycast.SetActive(false);
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
        //Debug.Log("MapSelectButton");
        //Debug.Log("DungeonName=" + RoomInfo.SelectedDungeonName);

        MatchingRoomCreateWindow.SetActive(true);
        MatchingRoomCreateLaycast.SetActive(true);

        RoomCreateObj.SetActive(true);
        RoomJoinObj.SetActive(false);
    }

    public void CreateButton()
    {
        // RoomID決定（空欄なら自動生成）
        isManualRoomId = !string.IsNullOrEmpty(createRoomIdInput.text);

        RoomInfo.RoomId = isManualRoomId
            ? createRoomIdInput.text
            : RoomIdGenerator.GenerateRoomId();

        RoomInfo.Password = Password;
        RoomInfo.IsPrivate = IsPrivateRoom;
        RoomInfo.MaxPlayers = MaxPlayers;

       // Debug.Log("RoomID = " + RoomInfo.RoomId);

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
        Laycast.SetActive(false);
        CautionObj.SetActive(false);
        ScrolView.SetActive(false);
        MatchingRoomCreateWindow.SetActive(false);
        MatchingRoomCreateLaycast.SetActive(false);

        if (!GameModeManager.IsMultiplayer)
        {
            fusionLauncher.StartSolo();
            return;
        }

        MatchingObj.SetActive(true);

        matchingMessage = null;
        isRoomHost = true;

        hostCoroutine = StartCoroutine(HostRoomFlow());
    }

    /// <summary>
    /// ルームを作る（ID重複チェック → 接続 → ルーム一覧に登録 → 生存通知）
    /// </summary>
    private IEnumerator HostRoomFlow()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            // ルーム一覧に同じIDがあれば使わない
            yield return roomDBUploader.SearchRoom(RoomInfo.RoomId);

            if (roomDBUploader.foundRoom != null)
            {
                if (isManualRoomId)
                {
                    FailHosting("そのルームIDは使用中です");
                    yield break;
                }

                RoomInfo.RoomId = RoomIdGenerator.GenerateRoomId();
                continue;
            }

            Debug.Log("RoomID = " + RoomInfo.RoomId);

            fusionLauncher.StartMatch(RoomInfo.RoomId, RoomInfo.MaxPlayers, true);

            while (fusionLauncher.IsStarting)
            {
                yield return null;
            }

            if (fusionLauncher.Runner != null)
            {
                // 接続できてからルーム一覧に載せる
                yield return roomDBUploader.UploadRoom();

                aliveCoroutine = StartCoroutine(SendAliveLoop(RoomInfo.RoomId));
                hostCoroutine = null;
                yield break;
            }

            // 自動生成のIDが通信側で使われていたときだけ作り直して再挑戦
            if (isManualRoomId ||
                fusionLauncher.LastFailure != FusionLauncher.MatchFailure.RoomInUse)
            {
                FailHosting(fusionLauncher.LastFailMessage);
                yield break;
            }

            RoomInfo.RoomId = RoomIdGenerator.GenerateRoomId();
        }

        FailHosting("ルームを作成できませんでした");
    }

    private void FailHosting(string message)
    {
        hostCoroutine = null;

        StopHosting();

        matchingMessage = message + "\n戻るボタンで戻ってください";
    }

    /// <summary>
    /// ルーム一覧から消して生存通知を止める
    /// </summary>
    private void StopHosting()
    {
        if (hostCoroutine != null)
        {
            StopCoroutine(hostCoroutine);
            hostCoroutine = null;
        }

        if (aliveCoroutine != null)
        {
            StopCoroutine(aliveCoroutine);
            aliveCoroutine = null;
        }

        if (isRoomHost)
        {
            StartCoroutine(
                roomDBUploader.DeleteRoom(RoomInfo.RoomId)
            );
        }

        isRoomHost = false;
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
        StopHosting();

        fusionLauncher.CancelMatch();

        // 前の部屋のIDやメッセージを次に持ち越さない
        RoomInfo.ClearRoom();
        createRoomIdInput.text = "";
        matchingMessage = null;

        ScrolView.SetActive(true);
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

        // ここではまだ接続しない（確認画面の「はい」で、検索したルームに接続する）
        roomListLoader.ShowJoinCaution(roomDBUploader.foundRoom, RoomSearchPassword);

        RoomInCautionObj.SetActive(true);
        RoomInCautionLayout.SetActive(true);
    }

    public void GameStartButton()
    {
        if (NetworkGameState.Instance == null)
            return;

        GameStartbutton.interactable = false;

        // BGMやプレイヤー生成は、開始が確定したあと全員の画面でFusionLauncher.HandleGameStartedが行う
        NetworkGameState.Instance.RequestStartGame();
    }

    private void OnGameStarted()
    {
        isInGame = true;

        HideMatchingUI();

        // 始まったルームは一覧から消す（途中参加させない）
        StopHosting();
    }

    private void OnMatchFailed(string message)
    {
        // 接続中の失敗はHostRoomFlowが処理する
        if (hostCoroutine != null)
            return;

        // ゲーム中に切断されたらタイトルへ戻る
        if (isInGame)
        {
            fusionLauncher.ShutdownAndLoadTitle("TitleScene");
            return;
        }

        if (MatchingObj.activeSelf)
        {
            FailHosting(message);
        }
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

        roomListLoader.ShowJoinCaution(room, privatePasswordInput.text);
    }

    public void SearchDungeonButton()
    {
        string stageName =
            stageNameSearchInput.text;

        string creatorName =
            creatorNameSearchInput.text;

        loadUI.SearchDungeon(
            stageName,
            creatorName,
            selectedTags
        );

        StageSearchObj.SetActive(false);
        StageSearchLaycast.SetActive(false);
    }

    public void StageSearchButton()
    {
        StageSearchObj.SetActive(true);
        StageSearchLaycast.SetActive(true);
    }

    public void EasyButton()
    {
        if (selectedTags.Contains("EASY"))
        {
            selectedTags.Remove("EASY");
        }
        else
        {
            selectedTags.Add("EASY");
        }

        UpdateTagButtonColor();

        CheckSearchCondition();
    }

    public void NormalButton()
    {
        if (selectedTags.Contains("NORMAL"))
        {
            selectedTags.Remove("NORMAL");
        }
        else
        {
            selectedTags.Add("NORMAL");
        }

        UpdateTagButtonColor();

        CheckSearchCondition();
    }

    public void HardButton()
    {
        if (selectedTags.Contains("HARD"))
        {
            selectedTags.Remove("HARD");
        }
        else
        {
            selectedTags.Add("HARD");
        }

        UpdateTagButtonColor();

        CheckSearchCondition();
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

        //Debug.Log(
        //    $"最大人数 : {MaxPlayers}人"
        //);
    }

    private void CheckSearchCondition()
    {
        bool foundByName =
            loadUI.ExistsDungeon(
                stageNameSearchInput.text,
                creatorNameSearchInput.text,
                selectedTags
            );

        bool tagSelected =
           selectedTags.Count > 0;

        searchButton.interactable =
            foundByName || tagSelected;

        Debug.Log(
            $"tag={selectedTags} interactable={searchButton.interactable}"
        );
    }

    private IEnumerator SendAliveLoop(string roomId)
    {
        while (true)
        {
            // 生存通知と一緒に、ルーム一覧の人数を実際の接続数に合わせる
            yield return roomDBUploader.UpdateAlive(
                roomId,
                Mathf.Max(1, fusionLauncher.PlayerCount)
            );

            // 間を空けないとサーバーに休みなくリクエストを送り続けてしまう
            yield return new WaitForSecondsRealtime(AliveInterval);
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
        MatchingRoomCreateLaycast.SetActive(false);

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

    private void UpdateTagButtonColor()
    {
        EASYButton.image.color =
            selectedTags.Contains("EASY")
                ? selectedColor
                : normalColor;

        NORMALButton.image.color =
            selectedTags.Contains("NORMAL")
                ? selectedColor
                : normalColor;

        HARDButton.image.color =
            selectedTags.Contains("HARD")
                ? selectedColor
                : normalColor;
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