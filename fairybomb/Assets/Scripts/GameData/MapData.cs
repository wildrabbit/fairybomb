
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

    public bool GetLevelData(out Vector2Int size, out int[] tiles)
    {
        size = Vector2Int.zero;
        tiles = new int[0];

        string[] lines = MapInfo.text.Split('\n');
        if (lines.Length == 0)
        {
            Debug.LogError("Invalid length");
            return false;
        }
        string[] dims = lines[0].Split(',');
        if (dims.Length != 2)
        {
            Debug.LogError("Invalid dims");
            return false;
        }

        size = new Vector2Int(Int32.Parse(dims[0]), Int32.Parse(dims[1]));
        tiles = new int[size.x * size.y];
        if (lines.Length != (size.x + 1))
        {
            Debug.LogError("Invalid row count");
            return false;
        }

        for (int i = 1; i < lines.Length; ++i)
        {
            string[] tilesRow = lines[i].Trim().TrimEnd(',').Split(',');
            if(tilesRow.Length != size.y)
            {
                Debug.LogError($"Invalid col count @ row {i -1}");
                return false;
            }
            for (int j = 0; j < tilesRow.Length; ++j)
            {
                tiles[(i - 1) * size.y + j] = Int32.Parse(tilesRow[j]);
            }
        }
        return true;
    }
}