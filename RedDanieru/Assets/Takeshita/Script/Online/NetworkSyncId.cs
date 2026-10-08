using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マップに配置したオブジェクト（敵・ステッカーを貼れる物）をマルチプレイで特定するためのID
/// マップデータ上のマス座標から作るので、同じマップを読み込んだ全員で同じIDになる
/// </summary>
public class NetworkSyncId : MonoBehaviour
{
    private static readonly Dictionary<int, NetworkSyncId> registry =
        new Dictionary<int, NetworkSyncId>();

    // 0 は「IDなし」を表す
    public int Id { get; private set; }

    public static IReadOnlyDictionary<int, NetworkSyncId> All => registry;

    /// <summary>
    /// MapManagerがオブジェクトを配置したときに呼ぶ
    /// </summary>
    public static void Assign(GameObject obj, Vector3Int gridPosition)
    {
        if (obj == null)
            return;

        NetworkSyncId syncId = obj.GetComponent<NetworkSyncId>();

        if (syncId == null)
        {
            syncId = obj.AddComponent<NetworkSyncId>();
        }

        syncId.Register(ToId(gridPosition));
    }

    public static int ToId(Vector3Int gridPosition)
    {
        return 1 +
            gridPosition.x +
            gridPosition.z * 1000 +
            gridPosition.y * 1000000;
    }

    public static bool TryGet(int id, out NetworkSyncId syncId)
    {
        return registry.TryGetValue(id, out syncId) && syncId != null;
    }

    /// <summary>
    /// コンポーネントが属するオブジェクトのIDを取得（無ければ0）
    /// </summary>
    public static int Of(Component component)
    {
        if (component == null)
            return 0;

        NetworkSyncId syncId =
            component.GetComponentInParent<NetworkSyncId>();

        return syncId != null ? syncId.Id : 0;
    }

    private void Register(int id)
    {
        Unregister();

        Id = id;
        registry[id] = this;
    }

    private void Unregister()
    {
        // マップを読み直すと同じIDの新しいオブジェクトが先に登録されるので、自分のときだけ消す
        if (Id != 0 &&
            registry.TryGetValue(Id, out NetworkSyncId current) &&
            current == this)
        {
            registry.Remove(Id);
        }
    }

    private void OnDestroy()
    {
        Unregister();
    }
}
