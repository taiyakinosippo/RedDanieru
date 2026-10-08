using System;
using System.Collections.Generic;

[Serializable]
public class DungeonData
{
    public string dungeonID;
    public string dungeonName;
    public string creatorName;
    public string createDate;

    public List<DungeonObjectData> objects =
        new List<DungeonObjectData>();
}
