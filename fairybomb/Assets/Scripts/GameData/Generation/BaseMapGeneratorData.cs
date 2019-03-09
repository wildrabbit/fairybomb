using System;
using System.Collections.Generic;
using UnityEngine;

public enum GeneratorType
{
    Fixed,
    BSP
}

public abstract class BaseMapGeneratorData : ScriptableObject
{
    public abstract GeneratorType GeneratorType { get; }
    public abstract Vector2Int  MapSize { get; }

    public TileType WallTile;
    public TileType GroundTile;

    public bool OriginIsTopLeft;
}