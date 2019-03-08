
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New MapData", menuName = "FAIRY BOMB/Map")]
public class FairyBombMapData: ScriptableObject
{
    public FairyBombTile GoalTile;
    public FairyBombTile NoTile;
    public List<FairyBombTile> Palette;
    public Vector2Int PlayerStart;
    public List<MonsterData> MonsterPool;
    public TextAsset MapInfo;
    public bool OriginIsTopLeft;
}