using Fusion;
using UnityEngine;

public class FusionLauncher : MonoBehaviour
{
    public async void StartMatch(string roomName)
    {
        var runner = gameObject.AddComponent<NetworkRunner>();

        runner.ProvideInput = true;

        var spawner = GetComponent<PlayerSpawner>();
        runner.AddCallbacks(spawner);

        var result = await runner.StartGame(
            new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = roomName
            });

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