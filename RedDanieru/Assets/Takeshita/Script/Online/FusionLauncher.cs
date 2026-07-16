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


        var runner = FindObjectOfType<NetworkRunner>();

        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        Debug.Log("runner = " + runner);

        runner.ProvideInput = true;

        var spawner = FindObjectOfType<PlayerSpawner>();

        Debug.Log("spawner = " + spawner);

        if (spawner == null)
        {
            Debug.LogError("PlayerSpawnerが見つかりません");
            return;
        }


        Debug.Log("StartGame開始");

        var result = await runner.StartGame(
            new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = roomName
            });

        Debug.Log("StartGame終了");

        if (result.Ok)
        {
            Debug.Log("Fusionルーム参加成功 : " + roomName);
        }
        else
        {
            Debug.LogError(
                $"接続失敗 : {result.ShutdownReason}"
            );
        }
    }
}