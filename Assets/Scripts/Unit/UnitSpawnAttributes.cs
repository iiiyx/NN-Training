using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UnitSpawnAttributes
{
    public Vector3 SpawnPoint;
    public int Count;
    public Material Material;
    public SingleUnityLayer Layer;
    public List<GameObject> UnitsCollection;
}
