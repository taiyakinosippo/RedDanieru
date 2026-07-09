using System;

[Serializable]
public class DungeonMapData
{
    public int width;
    public int height;
    public int depth;

    public byte[] tiles;
}