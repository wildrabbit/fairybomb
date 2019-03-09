
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New MapData", menuName = "FAIRY BOMB/Map")]
public class FairyBombMapData: ScriptableObject
{
    public FairyBombTile GoalTile;
    public FairyBombTile NoTile;
    public List<FairyBombTile> Palette;

    public BaseMapGeneratorData GenerationData;
}