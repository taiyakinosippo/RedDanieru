using UnityEngine;

/// 配置可能オブジェクトのデータベース
public class ObjectDatabase : MonoBehaviour
{
    [SerializeField]
    private ObjectPrefab[] objectPrefabs;

    /// 種類からPrefabを取得
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