using Fusion;
using UnityEngine;

public class FusionLauncher : MonoBehaviour
{
    [SerializeField]
    private GameObject soloPlayerPrefab;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private NetworkRunner runner;

    [SerializeField]
    private PlayerSpawner playerSpawner;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (runner == null)
        {
            GameObject runnerObj =
                new GameObject("NetworkRunner");

            DontDestroyOnLoad(runnerObj);

            runner =
                runnerObj.AddComponent<NetworkRunner>();

            runnerObj.AddComponent<NetworkSceneManagerDefault>();

            runner.AddCallbacks(playerSpawner);
        }
    }

    public void StartSolo()
    {
        Debug.Log("StartSolo");

        Debug.Log(soloPlayerPrefab);
        Debug.Log(spawnPoint);

        Instantiate(
            soloPlayerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("ソロプレイヤー生成");
    }

    public async void StartMatch(string roomName)
    {
        Debug.Log($"StartMatch開始：{Time.realtimeSinceStartup}");

        if (runner.IsRunning)
        {
            Debug.Log("既に接続中");
            return;
        }

        Debug.Log("Runner IsRunning = " + runner.IsRunning);

        runner.ProvideInput = true;

        float startTime = Time.realtimeSinceStartup;

        var result =
            await runner.StartGame(
                new StartGameArgs()
                {
                    GameMode = GameMode.Shared,
                    SessionName = roomName,
                    DisableNATPunchthrough = true
                });

        Debug.Log(
            $"StartGame完了 : {Time.realtimeSinceStartup - startTime}秒"
        );

        Debug.Log("Result = " + result.Ok);
        Debug.Log("ShutdownReason = " + result.ShutdownReason);

        if (result.Ok)
        {
            int playerCount = 0;

            foreach (var player in runner.ActivePlayers)
            {
                playerCount++;
            }

            Debug.Log(
                $"参加人数 : {playerCount}"
            );
        }
    }

    public async void CancelMatch()
    {
        if (runner == null)
        {
            Debug.Log("Runnerなし");
            return;
        }

        await runner.Shutdown();

        Debug.Log("マッチングを中止しました");
    }
}