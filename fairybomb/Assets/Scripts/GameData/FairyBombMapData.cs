
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New MapData", menuName = "FAIRY BOMB/Map")]
public class FairyBombMapData: ScriptableObject
{
    public FairyBombTile GoalTile;
    public List<FairyBombTile> Palette;
    public Vector2Int PlayerStart;
    public List<MonsterSpawn> MonsterSpawns;
    public TextAsset MapInfo;
    public bool OriginIsTopLeft;
}