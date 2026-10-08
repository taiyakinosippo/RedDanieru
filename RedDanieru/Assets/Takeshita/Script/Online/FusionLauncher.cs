using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Photon Fusion の接続を管理する
/// NetworkRunner は使い捨て（Shutdown後は再利用できない）なので、マッチングのたびに作り直す
/// シーンをまたいで残すと古い接続が「幽霊プレイヤー」として部屋に残るので、このシーンの中だけで使う
/// </summary>
public class FusionLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionLauncher Instance { get; private set; }

    [SerializeField]
    private GameObject soloPlayerPrefab;

    [Tooltip("マップにスタート地点が無いときだけ使う予備の出現位置")]
    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private PlayerSpawner playerSpawner;

    [SerializeField]
    private NetworkGameState networkGameStatePrefab;

    [SerializeField]
    private DungeonImporter importer;

    private NetworkRunner runner;

    // 自分から抜けたときは「切断された」と表示しない
    private bool isLeaving;

    private CursorController _cursorController;

    /// <summary>接続中のRunner（未接続ならnull）</summary>
    public NetworkRunner Runner =>
        runner != null && runner.IsRunning ? runner : null;

    public bool IsStarting { get; private set; }

    public int PlayerCount =>
        Runner != null ? Runner.ActivePlayers.Count() : 0;

    public int MaxPlayers =>
        Runner != null && Runner.SessionInfo.IsValid
            ? Runner.SessionInfo.MaxPlayers
            : RoomInfo.MaxPlayers;

    public bool IsMasterClient =>
        Runner != null && Runner.IsSharedModeMasterClient;

    public enum MatchFailure
    {
        None,
        RoomInUse,
        RoomNotFound,
        RoomFull,
        Disconnected,
        Other
    }

    public MatchFailure LastFailure { get; private set; }

    public string LastFailMessage { get; private set; }

    /// <summary>マッチングに失敗した・切断された（引数は画面に出す理由）</summary>
    public event Action<string> MatchFailed;

    public event Action MatchStarted;

    public event Action GameStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _cursorController = GetComponent<CursorController>();

        if (importer == null)
        {
            importer = FindObjectOfType<DungeonImporter>();
        }
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;

        // シーンを抜けるときは必ず接続を切る
        if (runner != null && !runner.IsShutdown)
        {
            isLeaving = true;
            runner.Shutdown();
        }
    }

    private void OnGUI()
    {
        // テストモードで動いていることが分かるようにする
        if (OnlineTestMode.Enabled)
        {
            GUI.Label(new Rect(10, 10, 400, 24), "TEST MODE (local rooms / local stages, no PHP server)");
        }
    }

    //==================================================
    // ソロ
    //==================================================

    public void StartSolo()
    {
        Debug.Log("StartSolo");

        StartCoroutine(StartSoloWhenStageReady());
    }

    private IEnumerator StartSoloWhenStageReady()
    {
        // ステージを読み込み終わってからスタート地点に出す
        if (importer != null)
        {
            yield return importer.WaitUntilLoaded(RoomInfo.SelectedDungeon);
        }

        Pose pose = PlayerSpawnPoint.GetSpawnPose(spawnPoint, 0, 1);

        GameObject player = Instantiate(
            soloPlayerPrefab,
            pose.position,
            pose.rotation
        );

        // CharacterControllerの内部位置を出現位置に合わせる
        PlayerSpawnPoint.Teleport(player, pose);

        // カーソルを非表示にする
        _cursorController.HideCursor();
    }

    //==================================================
    // マルチ
    //==================================================

    /// <summary>
    /// ルームに接続する
    /// createRoom = true  : ホストとして新しく作る（同じIDのルームが既にあれば失敗）
    /// createRoom = false : 既存のルームに入る（ルームが無ければ失敗）
    /// </summary>
    public async void StartMatch(string roomName, int maxPlayers, bool createRoom)
    {
        if (IsStarting || Runner != null)
        {
            Debug.Log("既に接続中");
            return;
        }

        IsStarting = true;
        isLeaving = false;

        LastFailure = MatchFailure.None;
        LastFailMessage = null;

        try
        {
            await ShutdownRunner();

            runner = CreateRunner();

            StartGameResult result =
                await runner.StartGame(
                    new StartGameArgs()
                    {
                        GameMode = GameMode.Shared,
                        SessionName = roomName,
                        PlayerCount = Mathf.Clamp(maxPlayers, 2, 3),
                        DisableNATPunchthrough = true,
                        SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
                    });

            if (!result.Ok)
            {
                Debug.LogWarning($"接続に失敗 : {result.ShutdownReason} {result.ErrorMessage}");

                await FailAndShutdown(ToFailure(result.ShutdownReason));
                return;
            }

            // Sharedモードは同じ名前のセッションがあれば入り、無ければ作ってしまうので、
            // 「作るつもりが既にあった」「入るつもりが無かった」を見分ける
            if (createRoom && !runner.IsSharedModeMasterClient)
            {
                await FailAndShutdown(MatchFailure.RoomInUse);
                return;
            }

            if (!createRoom && runner.IsSharedModeMasterClient)
            {
                await FailAndShutdown(MatchFailure.RoomNotFound);
                return;
            }

            if (runner.IsSharedModeMasterClient)
            {
                runner.Spawn(
                    networkGameStatePrefab,
                    Vector3.zero,
                    Quaternion.identity
                );
            }

            Debug.Log($"ルームに接続しました Room={roomName} Players={PlayerCount}/{MaxPlayers}");

            MatchStarted?.Invoke();
        }
        finally
        {
            IsStarting = false;
        }
    }

    public async void CancelMatch()
    {
        isLeaving = true;

        await ShutdownRunner();

        Debug.Log("マッチングを中止しました");
    }

    /// <summary>
    /// 接続を切ってからシーンを移動する（タイトルへ戻る等は必ずここを通す）
    /// </summary>
    public async void ShutdownAndLoadTitle(string sceneName)
    {
        isLeaving = true;

        await ShutdownRunner();

        ResetSessionState();

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// ゲーム開始時に全員の画面で1回だけ呼ばれる（NetworkGameStateから）
    /// </summary>
    public void HandleGameStarted()
    {
        if (BGMManager_Takeshita.Instance != null)
        {
            BGMManager_Takeshita.Instance.PlayBattleBGM();
        }

        GameStopManager gameStopManager = FindObjectOfType<GameStopManager>();

        if (gameStopManager != null)
        {
            gameStopManager.EnablePauseMenu();
        }

        if (Runner != null && playerSpawner != null)
        {
            playerSpawner.SpawnLocalPlayer(Runner);
        }

        GameStarted?.Invoke();
    }

    public static void ResetSessionState()
    {
        Time.timeScale = 1f;

        GameStopManager.ResetPauseState();

        RoomInfo.ClearRoom();
    }

    private NetworkRunner CreateRunner()
    {
        GameObject runnerObj = new GameObject("NetworkRunner (Session)");

        NetworkRunner newRunner = runnerObj.AddComponent<NetworkRunner>();

        runnerObj.AddComponent<NetworkSceneManagerDefault>();

        newRunner.ProvideInput = true;
        newRunner.AddCallbacks(this);

        return newRunner;
    }

    private async Task ShutdownRunner()
    {
        NetworkRunner oldRunner = runner;
        runner = null;

        if (oldRunner == null)
            return;

        if (!oldRunner.IsShutdown)
        {
            // 既定でRunnerのGameObjectも破棄される
            await oldRunner.Shutdown();
        }
        else
        {
            Destroy(oldRunner.gameObject);
        }
    }

    private async Task FailAndShutdown(MatchFailure failure)
    {
        isLeaving = true;

        await ShutdownRunner();

        Fail(failure, GetFailMessage(failure));
    }

    private void Fail(MatchFailure failure, string message)
    {
        LastFailure = failure;
        LastFailMessage = message;

        MatchFailed?.Invoke(message);
    }

    private static MatchFailure ToFailure(ShutdownReason reason)
    {
        switch (reason)
        {
            case ShutdownReason.GameIsFull:
                return MatchFailure.RoomFull;

            case ShutdownReason.GameNotFound:
            case ShutdownReason.GameClosed:
                return MatchFailure.RoomNotFound;

            case ShutdownReason.GameIdAlreadyExists:
                return MatchFailure.RoomInUse;

            default:
                return MatchFailure.Other;
        }
    }

    private static string GetFailMessage(MatchFailure failure)
    {
        switch (failure)
        {
            case MatchFailure.RoomInUse:
                return "そのルームIDは使用中です";

            case MatchFailure.RoomNotFound:
                return "ルームが見つからないか、既に始まっています";

            case MatchFailure.RoomFull:
                return "ルームが満員です";

            case MatchFailure.Disconnected:
                return "通信が切断されました";

            default:
                return "接続に失敗しました";
        }
    }

    //==================================================
    // INetworkRunnerCallbacks
    //==================================================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"参加 : {player}  人数={runner.ActivePlayers.Count()}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"退出 : {player}  人数={runner.ActivePlayers.Count()}");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"接続終了 : {shutdownReason}");

        if (runner == this.runner)
        {
            this.runner = null;
        }

        // 接続開始中の失敗は StartMatch 側で理由を出す
        if (!isLeaving && !IsStarting)
        {
            Fail(MatchFailure.Disconnected, $"通信が切断されました（{shutdownReason}）");
        }
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.LogWarning($"サーバーから切断 : {reason}");
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken token) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
