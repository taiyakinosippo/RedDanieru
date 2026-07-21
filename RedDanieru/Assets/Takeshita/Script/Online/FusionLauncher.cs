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

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void StartSolo()
    {
        Instantiate(
            soloPlayerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("ソロプレイヤー生成");
    }

    public async void StartMatch(string roomName)
    {
        if (runner == null)
        {
            GameObject runnerObj =
                new GameObject("NetworkRunner");

            runner =
                runnerObj.AddComponent<NetworkRunner>();
        }

        runner.ProvideInput = true;

        var result =
            await runner.StartGame(
                new StartGameArgs()
                {
                    GameMode = GameMode.Shared,
                    SessionName = roomName
                });

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

        //Destroy(runner);

        runner = null;

        Debug.Log("マッチングを中止しました");
    }
}