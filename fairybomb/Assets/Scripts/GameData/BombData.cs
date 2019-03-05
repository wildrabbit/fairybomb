using UnityEngine;

[CreateAssetMenu(fileName = "New BombData", menuName = "FAIRY BOMB/Bomb")]
public class BombData: BaseEntityData
{
    public int Radius;
    public int BaseDamage;
    public int Timeout;
    public bool IgnoreBlocks;
    public GameObject VFXExplosion;

    public float TickUnits { get; internal set; }
}
