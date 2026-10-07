using System.Collections;
using System.Linq;
using Fusion;
using UnityEngine;

/// <summary>
/// マルチプレイで自分のキャラを生成する
/// Sharedモードなので、各自が自分の分だけを生成する
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    public NetworkPrefabRef[] playerPrefabs;

    [Tooltip("マップにスタート地点が無いときだけ使う予備の出現位置")]
    [SerializeField]
    private Transform spawnAreaCenter;

    [SerializeField]
    private DungeonImporter importer;

    private Coroutine spawnCoroutine;

    private Vector3 lastSpawnPos;

    private CursorController _cursorController;

    void Start()
    {
        _cursorController = GetComponent<CursorController>();

        if (importer == null)
        {
            importer = FindObjectOfType<DungeonImporter>();
        }
    }

    public void SpawnLocalPlayer(NetworkRunner runner)
    {
        if (spawnCoroutine != null)
            return;

        spawnCoroutine = StartCoroutine(SpawnWhenStageReady(runner));
    }

    private IEnumerator SpawnWhenStageReady(NetworkRunner runner)
    {
        // ステージを読み込み終わってからでないとスタート地点が分からない
        if (importer != null)
        {
            yield return importer.WaitUntilLoaded(RoomInfo.SelectedDungeon);
        }

        spawnCoroutine = null;

        if (runner == null || !runner.IsRunning)
            yield break;

        // 既に生成済みなら何もしない
        if (runner.TryGetPlayerObject(runner.LocalPlayer, out _))
            yield break;

        // 全員で同じ並び順になるようにPlayerIdで並べる
        PlayerRef[] players =
            runner.ActivePlayers.OrderBy(player => player.PlayerId).ToArray();

        int index = Mathf.Max(0, System.Array.IndexOf(players, runner.LocalPlayer));

        Pose pose = PlayerSpawnPoint.GetSpawnPose(spawnAreaCenter, index, players.Length);

        int prefabIndex =
            runner.LocalPlayer.PlayerId % playerPrefabs.Length;

        NetworkObject obj = runner.Spawn(
            playerPrefabs[prefabIndex],
            pose.position,
            pose.rotation,
            runner.LocalPlayer
        );

        runner.SetPlayerObject(
            runner.LocalPlayer,
            obj
        );

        // Fusionは原点に生成してから位置を移すが、CharacterControllerの内部位置は原点のまま残る
        // （autoSyncTransformsがオフのため）。そのまま動かすと原点＝マップの左下に戻されるので合わせ直す
        PlayerSpawnPoint.Teleport(obj.gameObject, pose);

        lastSpawnPos = pose.position;

        Debug.Log($"プレイヤーを生成しました Player={runner.LocalPlayer} Pos={pose.position}");

        if (_cursorController != null)
        {
            _cursorController.HideCursor();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(
            lastSpawnPos,
            0.5f
        );
    }
}
