using Fusion;
using UnityEngine;

public class FusionLauncher : MonoBehaviour
{
    async void Start()
    {
       
        var runner = gameObject.AddComponent<NetworkRunner>();

        runner.ProvideInput = true;

        var spawner = GetComponent<PlayerSpawner>();
        runner.AddCallbacks(spawner);

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "Room"
        });

    }
}