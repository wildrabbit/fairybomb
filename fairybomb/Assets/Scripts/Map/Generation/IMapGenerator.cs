using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseMapContext
{
    public TileType WallTile;
    public TileType GroundTile;
    public Vector2Int Size;
}

public interface IMapGenerator
{
    void GenerateMap(ref TileType[] map, BaseMapContext mapGenContext);
}
