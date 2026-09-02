using System;
using System.Collections.Generic;

[Serializable]
public class DungeonMapData
{
    public int width;
    public int height;
    public int depth;

    // 壁・床情報
    public byte[] tiles;

    // 配置オブジェクト
    public List<ObjectData> objects = new();

    // リスポーンポイント
    public float spawnPointX;
    public float spawnPointY;
    public float spawnPointZ;
    public float spawnPointRotY;

    // リスポーンエリア
    public float respawnAreaX;
    public float respawnAreaY;
    public float respawnAreaZ;

    public float respawnAreaScaleX;
    public float respawnAreaScaleY;
    public float respawnAreaScaleZ;

    // リスポーンObjectの有無
    public bool hasRespawnPoint;
    public bool hasRespawnArea;
}

[Serializable]
public class ObjectData
{
    public int x;
    public int y;
    public int z;

    public PlaceObjectType type;
}