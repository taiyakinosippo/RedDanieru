using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonMapData
{
    // マップサイズ
    public int width;
    public int height;
    public int depth;

    // 壁・床情報
    public byte[] tiles;


    // 配置オブジェクト情報
    public List<ObjectData> objects = new List<ObjectData>();
}


// 配置物データ
[Serializable]
public class ObjectData
{
    public int x;
    public int y;
    public int z;
}