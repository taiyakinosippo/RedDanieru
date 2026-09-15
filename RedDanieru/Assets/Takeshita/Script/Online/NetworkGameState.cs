using Fusion;
using UnityEngine;

public class NetworkGameState : NetworkBehaviour
{
    public static NetworkGameState Instance;

    public override void Spawned()
    {
        Instance = this;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_StartGame()
    {
        NetworkRunner runner =
            FindObjectOfType<NetworkRunner>();

        PlayerSpawner spawner =
            FindObjectOfType<PlayerSpawner>();

        if (runner == null || spawner == null)
            return;

        spawner.SpawnPlayer(
            runner,
            runner.LocalPlayer
        );
    }
}