using System;
using System.Collections.Generic;

[Serializable]
public class DungeonData
{
    public string DungeonID;
    public string DungeonName;
    public string CreatorName;
    public string CreateDate;

    public List<DungeonObjectData> objects =
        new List<DungeonObjectData>();
}
