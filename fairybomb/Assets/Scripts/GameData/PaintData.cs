using UnityEngine;

public enum PaintingEffect
{
    None, //??
    Remove,
    Poison,
    Heal,
    Freeze,
    Haste,
    Slow
}

public enum EffectTargetType
{
    Everyone,
    EveryoneNonNeutral,
    SameFaction,
    RivalFaction,
}

[CreateAssetMenu(fileName ="New Paint Data", menuName = "FAIRY BOMB/PaintData")]
public class PaintData: ScriptableObject
{
    public PaintingEffect Effect;
    public EffectTargetType TargetType;
    public Color Colour;
    public GameObject TilePrefab;

    public int TimeToDegrade;

    public int TicksForHPChange;
    public int HPDelta;

    public float SpeedRate;

    public float FreezeChance;

    public float TeleportChance;
    public int TeleportMinRange;
    public int TeleportMaxRange;
}
