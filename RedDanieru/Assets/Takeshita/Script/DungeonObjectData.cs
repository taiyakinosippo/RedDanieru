using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonObjectData
{
    public string type;
    public Vector3 position;
}

[Serializable]
public class DungeonData
{
    public List<DungeonObjectData> objects =
        new List<DungeonObjectData>();
}