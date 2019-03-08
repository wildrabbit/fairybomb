using UnityEngine;

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
    public int DeployedBombLimit;
    public BombData DefaultBombData;
    // TODO: Infinite bomb count?
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
}
