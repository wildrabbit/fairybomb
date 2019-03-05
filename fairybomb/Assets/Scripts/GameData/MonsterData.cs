using UnityEngine;

[CreateAssetMenu(fileName = "New MonsterData", menuName = "FAIRY BOMB/Monster")]
public class MonsterData: BaseEntityData
{
    public float ThinkingDelay;
    public HPTraitData HPData;
    public MovingEntityData MovingData;
    public MonsterState InitialState;
    public int MinIdleTicks;
    public int MaxIdleTicks;

    public bool IsMelee;
    public int MeleeDamage;

    public bool IsBomber;
    public BomberData BomberData;
    // TODO: Distances, visibility
}
