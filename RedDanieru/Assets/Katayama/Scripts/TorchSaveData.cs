
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TorchSaveData
{
    public List<TorchInfo> torches = new List<TorchInfo>();
}

[Serializable]
public class TorchInfo
{
    public Vector3 position;
    public Quaternion rotation;
}
