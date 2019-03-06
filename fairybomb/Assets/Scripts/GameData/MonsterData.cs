using UnityEngine;

[CreateAssetMenu(fileName = "New MonsterData", menuName = "FAIRY BOMB/Monster")]
public class MonsterData: BaseEntityData
{
    public float ThinkingDelay;
    public HPTraitData HPData;
    public MovingEntityData MovingData;
    public MonsterState InitialState;

    public int WanderToIdleMinTurns;
    public int WanderToIdleMaxTurns;

    public int MinIdleTurns;
    public int MaxIdleTurns;

    public bool IsMelee;
    public int MeleeDamage;

    public bool IsBomber;
    public BomberData BomberData;

    public int VisibilityRange; // Wander -> Chase
    public float EscapeHPRatio;
    public int EscapeSafeDistance;

    public float PathUpdateDelay;
    // TODO: Distances, visibility
}
