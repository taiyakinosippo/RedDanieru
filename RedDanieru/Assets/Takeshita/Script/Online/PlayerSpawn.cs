using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef[] playerPrefabs;

    [SerializeField]
    private Transform spawnAreaCenter;

    [SerializeField]
    private float areaWidth = 7f;

    [SerializeField]
    private float areaDepth = 15f;

    private List<Vector3> usedPositions =
     new List<Vector3>();

    [SerializeField] private NetworkGameState networkGameState;

    public bool CanSpawn = false;

    private Vector3 lastSpawnPos;

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // 既に生成済みなら何もしない
        if (runner.TryGetPlayerObject(player, out _))
            return;

        // プレハブ選択
        int prefabIndex =
            player.PlayerId % playerPrefabs.Length;

        Vector3 spawnPos =
      GetRandomSpawnPosition();

        lastSpawnPos = spawnPos;

        bool insideArea = IsInsideSpawnArea(spawnPos);

        Debug.Log(
            $"SpawnPos = {spawnPos} " +
            $"InsideArea = {insideArea}"
        );

        NetworkObject obj = runner.Spawn(
            playerPrefabs[prefabIndex],
            spawnPos,
            Quaternion.identity,
            player
        );

        StartCoroutine(
      ForceRespawnPosition(
          obj,
          spawnPos
      )
  );

        runner.SetPlayerObject(
            player,
            obj
        );

        //if (obj.HasInputAuthority)
        //{
        //    Debug.Log("これは自分のプレイヤー");
        //}
        //else
        //{
        //    Debug.Log("これは相手のプレイヤー");
        //}

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

        foreach (var player in runner.ActivePlayers)
        {
            SpawnPlayer(runner, player);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner,PlayerRef player)
    {
        //Debug.Log($"Join:{player}");

        //StartCoroutine(WaitGameStartAndSpawn(runner, player));
    }

    private IEnumerator CheckPosition(GameObject player)
    {
        Debug.Log($"Spawn直後 = {player.transform.position}");

        yield return null;

        Debug.Log($"次フレーム = {player.transform.position}");

        yield return new WaitForSeconds(0.1f);

        Debug.Log($"0.1秒後 = {player.transform.position}");

        yield return new WaitForSeconds(0.9f);

        Debug.Log($"1秒後 = {player.transform.position}");
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

    private Vector3 GetRandomSpawnPosition()
    {
        for (int i = 0; i < 50; i++)
        {
            float x = Random.Range(
                -areaWidth * 0.5f,
                 areaWidth * 0.5f
            );

            float z = Random.Range(
                -areaDepth * 0.5f,
                 areaDepth * 0.5f
            );

            Vector3 pos =
                spawnAreaCenter.position +
                new Vector3(x, 0f, z);

            // 地面がある場所だけ許可
            if (Physics.Raycast(
                pos + Vector3.up * 5f,
                Vector3.down,
                out RaycastHit hit,
                20f))
            {
                return hit.point;
            }
        }

        return spawnAreaCenter.position;
    }

    private bool IsInsideSpawnArea(Vector3 pos)
    {
        Vector3 local =
            pos - spawnAreaCenter.position;

        bool inside =
            Mathf.Abs(local.x) <= areaWidth * 0.5f &&
            Mathf.Abs(local.z) <= areaDepth * 0.5f;

        return inside;
    }

    private void OnDrawGizmos()
    {
        if (spawnAreaCenter == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            spawnAreaCenter.position,
            new Vector3(
                areaWidth,
                1f,
                areaDepth
            )
        );

        Gizmos.color = Color.red;

        Gizmos.DrawSphere(
            lastSpawnPos,
            0.5f
        );
    
}

    private IEnumerator ForceRespawnPosition(
    NetworkObject obj,
    Vector3 spawnPos
)
    {
        yield return null;

        obj.transform.position = spawnPos;

        Debug.Log(
            $"位置補正: {spawnPos}"
        );
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