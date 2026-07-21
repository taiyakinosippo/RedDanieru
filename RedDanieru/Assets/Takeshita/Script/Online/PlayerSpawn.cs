using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef[] playerPrefabs;

    [SerializeField]
    private Transform[] spawnPoints;

    public void SpawnPlayer(
        NetworkRunner runner,
        PlayerRef player)
    {
        if (runner.TryGetPlayerObject(player, out _))
            return;

        int prefabIndex =
            player.PlayerId % playerPrefabs.Length;

        Vector3 spawnPos =
            spawnPoints[player.PlayerId % spawnPoints.Length]
            .position;

        var obj = runner.Spawn(
            playerPrefabs[prefabIndex],
            spawnPos,
            Quaternion.identity,
            player
        );

        runner.SetPlayerObject(
            player,
            obj
        );

        Debug.Log($"Spawn : {player}");
    }

    public void SpawnAllPlayers(
        NetworkRunner runner)
    {
        foreach (var player in runner.ActivePlayers)
        {
            SpawnPlayer(
                runner,
                player
            );
        }
    }

    public void OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
        Debug.Log($"Join : {player}");
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken token) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}