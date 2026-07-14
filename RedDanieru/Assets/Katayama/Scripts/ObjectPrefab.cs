using System;
using UnityEngine;

/// <summary>
/// 配置オブジェクトとPrefabを紐付ける
/// </summary>
[Serializable]
public class ObjectPrefab
{
    public PlaceObjectType type;
    public GameObject prefab;
}