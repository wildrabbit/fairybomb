using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class HPTraitData
{
    public int MaxHP;
    public int StartHP;
    public bool Regen;
    public float RegenRate;
    public int RegenAmount; // amount or percent??
}


[System.Serializable]
public class BomberData
{
    [FormerlySerializedAs("DeployedBombLimit")] public int DefaultDeployLimit;
    public BombInventoryEntry[] InventoryEntry;
    public int DefaultSelection;
    public int InventorySize;
    public bool IsFirstItemFixed;
}

[System.Serializable]
public class MovingEntityData
{
    public BombWalkabilityType BombWalkability;
    public BombImmunityType BombImmunity;
}

public class BaseEntityData: ScriptableObject
{
    public Transform DefaultViewPrefab;
    public string DisplayName;
}
