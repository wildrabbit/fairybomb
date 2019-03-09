using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName ="New PlayerData", menuName ="FAIRY BOMB/Player")]
public class PlayerData: BaseEntityData
{
    public bool CanMoveIntoMonsterCoords;
    public int MonsterCollisionDmg;

    public HPTraitData HPData;
    public MovingEntityData MobilityData;
    public BomberData BomberData;
    public float Speed;

}