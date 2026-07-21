using System;
using UnityEngine;

/// 配置オブジェクトとPrefabを紐付ける
[Serializable]
public class ObjectPrefab
{
    public PlaceObjectType type;
    public GameObject prefab;
}