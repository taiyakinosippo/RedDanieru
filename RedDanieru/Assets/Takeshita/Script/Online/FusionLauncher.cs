using Fusion;
using UnityEngine;

public class FusionLauncher : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("FusionLauncher Start");

        var runner = gameObject.AddComponent<NetworkRunner>();

        runner.ProvideInput = true;

        var spawner = GetComponent<PlayerSpawner>();
        runner.AddCallbacks(spawner);

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "Room"
        });

        Debug.Log($"OK = {result.Ok}");
        Debug.Log($"Reason = {result.ShutdownReason}");
        Debug.Log($"Error = {result.ErrorMessage}");
    }
}