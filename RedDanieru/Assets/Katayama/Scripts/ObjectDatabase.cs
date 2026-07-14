using UnityEngine;

/// <summary>
/// 配置可能オブジェクトのデータベース
/// </summary>
public class ObjectDatabase : MonoBehaviour
{
    [SerializeField]
    private ObjectPrefab[] objectPrefabs;

    /// <summary>
    /// 種類からPrefabを取得
    /// </summary>
    public GameObject GetPrefab(PlaceObjectType type)
    {
        foreach (ObjectPrefab data in objectPrefabs)
        {
            if (data.type == type)
            {
                return data.prefab;
            }
        }

        return null;
    }
}