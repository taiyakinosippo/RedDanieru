using System;

[Serializable]
public class DungeonNameArray
{
    public DungeonListItem[] dungeons;
}

[Serializable]
public class DungeonListItem
{
    public string dungeonId;
    public string dungeonName;
}