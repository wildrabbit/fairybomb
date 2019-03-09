using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New FixedGeneratorData", menuName = "FAIRY BOMB/MapGen/Fixed Generator Data")]
public class FixedMapGeneratorData : BaseMapGeneratorData
{
    public Vector2Int PlayerStart;
    public List<MonsterSpawn> MonsterSpawns;
    public List<LootSpawn> LootSpawns;
    public TextAsset MapInfo;

    public TileType[] LevelData
    {
        get
        {
            //if (_levelTiles == null)
            {
                BuildLevelData();                
            }
            return _levelTiles;
        }
    }
    
    TileType[] _levelTiles;
    Vector2Int _levelSize;

    public override GeneratorType GeneratorType => GeneratorType.Fixed;

    public override Vector2Int MapSize
    {
        get
        {
            //if (_levelSize == Vector2Int.zero)
            {
                BuildLevelData();
            }
            return _levelSize;
        }
    }

    bool BuildLevelData()
    {
        _levelSize = Vector2Int.zero;
        _levelTiles = null;

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

        _levelSize = new Vector2Int(Int32.Parse(dims[0]), Int32.Parse(dims[1]));
        _levelTiles = new TileType[_levelSize.x * _levelSize.y];
        if (lines.Length != (_levelSize.x + 1))
        {
            Debug.LogError("Invalid row count");
            return false;
        }

        for (int i = 1; i < lines.Length; ++i)
        {
            string[] tilesRow = lines[i].Trim().TrimEnd(',').Split(',');
            if (tilesRow.Length != _levelSize.y)
            {
                Debug.LogError($"Invalid col count @ row {i - 1}");
                return false;
            }
            for (int j = 0; j < tilesRow.Length; ++j)
            {
                _levelTiles[(i - 1) * _levelSize.y + j] = (TileType)Int32.Parse(tilesRow[j]);
            }
        }
        return true;
    }
}
