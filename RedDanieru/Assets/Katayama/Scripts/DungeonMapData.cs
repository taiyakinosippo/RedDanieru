using System;
using System.Collections.Generic;
using UnityEngine;

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
}

[Serializable]
public class ObjectData
{
    public int x;
    public int y;
    public int z;

    public PlaceObjectType type;
}