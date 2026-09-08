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

    [SerializeField]
    private NetworkGameState networkGameStatePrefab;

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
        Debug.Log(networkGameStatePrefab);

        if (runner.IsRunning)
        {
            Debug.Log("既に接続中");
            return;
        }

      
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

        if (result.Ok)
        {
            if (runner.IsSharedModeMasterClient)
            {
                var obj=runner.Spawn(
                    networkGameStatePrefab,
                    Vector3.zero,
                    Quaternion.identity
                );

                Debug.Log($"Spawned GameState = {obj}");
            }

            int playerCount = 0;

            foreach (var player in runner.ActivePlayers)
            {
                playerCount++;
            }
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