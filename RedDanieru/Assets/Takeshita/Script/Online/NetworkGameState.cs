using Fusion;
using UnityEngine;

public class NetworkGameState : NetworkBehaviour
{
    public static NetworkGameState Instance;
    [Networked] public bool IsCleared { get; set; }

    [Networked] public int DeadPlayerCount { get; set; }

    [Networked]
    public int StartPlayerCount { get; set; }

    public override void Spawned()
    {
        Instance = this;
    }

    public void AddDeadPlayer()
    {
        Debug.Log(
            $"AddDeadPlayer State={HasStateAuthority}"
        );

        if (!HasStateAuthority)
            return;

        DeadPlayerCount++;

        Debug.Log(
            $"DeadPlayerCount={DeadPlayerCount}"
        );
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerDied()
    {
        DeadPlayerCount++;

        Debug.Log(
            $"DeadPlayerCount={DeadPlayerCount}"
        );
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_HideAllPlayers()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            Debug.Log($"消す:{player.name}");

            player.SetActive(false);
        }
    }
}