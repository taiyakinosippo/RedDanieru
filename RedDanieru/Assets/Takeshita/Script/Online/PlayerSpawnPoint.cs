using UnityEngine;

/// <summary>
/// プレイヤーの出現位置を決める
/// マップデータに保存されたスタート地点（RespawnPoint）を優先し、無いときだけシーンの予備地点を使う
/// </summary>
public static class PlayerSpawnPoint
{
    // 複数人が同じ場所に重ならないようにずらす距離
    private const float SpreadRadius = 1.2f;

    // 地面を探すときに無視するレイヤー
    private static int GroundMask =>
        ~LayerMask.GetMask("Player", "Enemy", "Ignore Raycast", "UI");

    /// <summary>
    /// マップのスタート地点を取得する
    /// </summary>
    public static bool TryGetMapSpawnPoint(out Pose pose)
    {
        MapManager mapManager = Object.FindObjectOfType<MapManager>();

        GameObject point =
            mapManager != null ? mapManager.GetRespawnPointObject() : null;

        if (point == null)
        {
            pose = default;
            return false;
        }

        // スタート地点の目印（当たり判定付きのキューブ）にプレイヤーが埋まらないようにする
        foreach (Collider marker in point.GetComponentsInChildren<Collider>())
        {
            marker.enabled = false;
        }

        foreach (Renderer marker in point.GetComponentsInChildren<Renderer>())
        {
            marker.enabled = false;
        }

        pose = new Pose(
            point.transform.position,
            Quaternion.Euler(0f, point.transform.eulerAngles.y, 0f));

        return true;
    }

    /// <summary>
    /// playerIndex番目のプレイヤーの出現位置
    /// </summary>
    public static Pose GetSpawnPose(Transform fallback, int playerIndex, int playerCount)
    {
        Pose basePose;

        if (!TryGetMapSpawnPoint(out basePose))
        {
            Debug.LogWarning("マップのスタート地点が見つからないため、予備の出現位置を使います。");

            basePose = fallback != null
                ? new Pose(fallback.position, fallback.rotation)
                : new Pose(Vector3.zero, Quaternion.identity);
        }

        Vector3 position =
            basePose.position + GetSpreadOffset(basePose.rotation, playerIndex, playerCount);

        // ずらした先が壁の中ならスタート地点そのものを使う
        if (!IsFree(position))
        {
            position = basePose.position;
        }

        return new Pose(SnapToGround(position), basePose.rotation);
    }

    /// <summary>
    /// CharacterControllerに邪魔されずに瞬間移動させる
    /// </summary>
    public static void Teleport(GameObject player, Pose pose)
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        bool wasEnabled = controller != null && controller.enabled;

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.SetPositionAndRotation(pose.position, pose.rotation);

        Fusion.NetworkTransform networkTransform =
            player.GetComponent<Fusion.NetworkTransform>();

        if (networkTransform != null &&
            networkTransform.Object != null &&
            networkTransform.Object.IsValid)
        {
            networkTransform.Teleport(pose.position, pose.rotation);
        }

        Physics.SyncTransforms();

        if (controller != null)
        {
            controller.enabled = wasEnabled;
        }
    }

    private static Vector3 GetSpreadOffset(Quaternion rotation, int playerIndex, int playerCount)
    {
        if (playerCount <= 1 || playerIndex <= 0)
            return Vector3.zero;

        // 1人目はスタート地点、2人目以降はその周りに並べる
        float angle = 360f / Mathf.Max(1, playerCount - 1) * (playerIndex - 1);

        return rotation * (Quaternion.Euler(0f, angle, 0f) * Vector3.right) * SpreadRadius;
    }

    private static bool IsFree(Vector3 position)
    {
        return !Physics.CheckCapsule(
            position + Vector3.up * 0.4f,
            position + Vector3.up * 1.4f,
            0.3f,
            GroundMask,
            QueryTriggerInteraction.Ignore);
    }

    private static Vector3 SnapToGround(Vector3 position)
    {
        if (Physics.Raycast(
                position + Vector3.up * 1.5f,
                Vector3.down,
                out RaycastHit hit,
                6f,
                GroundMask,
                QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * 0.05f;
        }

        return position;
    }
}
