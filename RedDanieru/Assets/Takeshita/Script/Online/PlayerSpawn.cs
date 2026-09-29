using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef[] playerPrefabs;

    [SerializeField]
    private Transform[] spawnPoints;
    private List<int> usedSpawnIndexes = new List<int>();

    [SerializeField] private NetworkGameState networkGameState;

    public bool CanSpawn = false;

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (runner.TryGetPlayerObject(player, out _))
            return;

        int prefabIndex =
            player.PlayerId % playerPrefabs.Length;

        int spawnIndex;

        // 未使用SpawnPointを優先
        if (usedSpawnIndexes.Count < spawnPoints.Length)
        {
            List<int> candidates =
                new List<int>();

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (!usedSpawnIndexes.Contains(i))
                {
                    candidates.Add(i);
                }
            }

            spawnIndex =
                candidates[
                    Random.Range(0, candidates.Count)
                ];

            usedSpawnIndexes.Add(spawnIndex);
        }
        else
        {
            spawnIndex =
                Random.Range(0, spawnPoints.Length);
        }

        Vector3 spawnPos =
            spawnPoints[spawnIndex].position;

        Debug.Log(
            $"Player={player.PlayerId} " +
            $"SpawnIndex={spawnIndex}"
        );

        Debug.Log(
            $"SpawnPos={spawnPos}"
        );

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

        Debug.Log(
            $"ActualPos={obj.transform.position}"
        );

        if (obj.HasInputAuthority)
        {
            Debug.Log("これは自分のプレイヤー");
        }
        else
        {
            Debug.Log("これは相手のプレイヤー");
        }

        StartCoroutine(
            CheckPosition(obj.gameObject)
        );

        DungeonUIManager ui =
            FindObjectOfType<DungeonUIManager>();

        if (ui != null)
        {
            ui.HideMatchingUI();
        }
    }

    public void SpawnAllPlayers(NetworkRunner runner)
    {
        if (!runner.IsSharedModeMasterClient)
            return;

        usedSpawnIndexes.Clear();

        foreach (var player in runner.ActivePlayers)
        {
            SpawnPlayer(runner, player);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner,PlayerRef player)
    {
        Debug.Log($"Join:{player}");

        //StartCoroutine(WaitGameStartAndSpawn(runner, player));
    }

    private System.Collections.IEnumerator CheckPosition(GameObject player)
    {
        yield return new WaitForSeconds(3f);

        Debug.Log(
            $"{player.name} Position = " +
            player.transform.position
        );
    }

    private IEnumerator WaitGameStartAndSpawn(NetworkRunner runner,PlayerRef player)
    {
        //while (!NetworkGameState.Instance == null || !NetworkGameState.Instance.CanSpawn)
        //{
        //    Debug.Log($"WaitingStart {runner.LocalPlayer}");
        //    yield return null;
        //}

         if (player != runner.LocalPlayer)
            yield break;

        SpawnPlayer(runner, player);
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