using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FairyBombMap : MonoBehaviour
{
    [SerializeField] List<FairyBombTile> _lesTiles;
    [SerializeField] FairyBombTile _goalTile;

    [SerializeField] Tilemap _map;
    public Vector2Int PlayerStart;
    public Vector2Int Dimensions;

    // We'll start with neutral, then N and then clockwise
    Vector2Int[][] _neighbourOffsets = new Vector2Int[][]
    { 
        new Vector2Int[]{ new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1,1), new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1)},
        new Vector2Int[]{ new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1),new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1),new Vector2Int(1, -1)}
    };

    public int Height => _map.size.x;
    public int Width => _map.size.y;

    private void Awake()
    {
        _lesTiles.Sort((x1, x2) => x1.TileType.CompareTo(x2.TileType));
    }

    public void InitFromMap()
    {
        Dimensions = new Vector2Int(_map.size.x, _map.size.y);
    }

    public void InitFromArray(Vector2Int dimensions, TileType[] typeArray, Vector2Int playerStart)
    {
        _map.ClearAllTiles();
        if(typeArray.Length != dimensions.x * dimensions.y)
        {
            Debug.LogError("Array dimensions not matching provided size");
            return;
        }

        Dimensions = dimensions;
        PlayerStart = playerStart;
        for(int y = 0; y < Dimensions.y; ++y)
        {
            for(int x = 0; x < Dimensions.x; ++x)
            {
                _map.SetTile(new Vector3Int(dimensions.y - (y + 1), x, 0), GetTileByType(typeArray[dimensions.x * y + x]));
            }
        }
    }

    public void Generate()
    {
        Vector2Int dimensions = Vector2Int.zero;
        TileType[] array = new TileType[] { };
        Vector2Int playerStart = Vector2Int.zero;
        InitFromArray(dimensions, array, playerStart);
    }

    public FairyBombTile GetTileByType(TileType type)
    {
        return _lesTiles[(int)type];
    }

    public Vector2 WorldFromCoords(Vector2Int coords)
    {
        return _map.CellToWorld((Vector3Int)coords);
    }

    public Vector2Int CoordsFromWorld(Vector2 pos)
    {
        return (Vector2Int)_map.WorldToCell(pos);
    }

    internal void ConstrainCoords(ref Vector2Int playerCoords)
    {
        playerCoords.x = Mathf.Clamp(playerCoords.x, 0, _map.size.x - 1);
        playerCoords.y = Mathf.Clamp(playerCoords.y, 0, _map.size.y - 1);
    }

    internal bool IsWalkableTile(Vector2Int playerTargetPos)
    {
        FairyBombTile fairyTile = (FairyBombTile)_map.GetTile((Vector3Int)playerTargetPos);
        return (fairyTile == null) ? false: fairyTile.Walkable;
    }

    public Rect GetBounds()
    {
        // Tilemap uses x: row, y: col. I'm using the opposite.
        BoundsInt cellBounds = _map.cellBounds;
        Vector2 min = _map.CellToWorld(new Vector3Int(cellBounds.yMin, cellBounds.xMin, 0));
        Vector2 max = _map.CellToWorld(new Vector3Int(cellBounds.yMax, cellBounds.xMax, 0));
        return new Rect(min.y, min.x, (max.y - min.y), (max.x - min.x));

    }

    public bool IsGoal(Vector2Int coords)
    {
        var tile = (FairyBombTile)_map.GetTile((Vector3Int)coords);
        return tile != null ? (tile.TileType == _goalTile.TileType) : false;
    }

    internal void BombExploded(Bomb bomb)
    {
        // Check for tile destruction!
    }

    public void Cleanup()
    {
        _map.ClearAllTiles();
    }

    public FairyBombTile TileAt(Vector2Int coords)
    {
        return _map.GetTile((Vector3Int)coords) as FairyBombTile;
    }

    // Redblobgames <3
    public Vector3Int CubeFromCoords(Vector2Int coords)
    {
        Vector3Int cube = new Vector3Int();
        int row = coords.x;
        int col = coords.y;

        int x = col;
        int z = row + (col + (col & 1)) / 2;
        int y = x-z;
        cube.Set(x, y, z);
        return cube;
    }

    public Vector2Int CoordsFromCube(Vector3Int cube)
    {
        int row = cube.z - (cube.x + (cube.x & 1)) / 2; // &1: even check
        int col = cube.x;
        return new Vector2Int(row, col);
    }

    public Vector2Int GetOffset(MoveDirection dir, bool isEven)
    {
        Vector2Int[] offsets = isEven ? _neighbourOffsets[0] : _neighbourOffsets[1];
        return offsets[(int)dir];
    }

    public int CubeDistance(Vector3Int cube1, Vector3Int cube2)
    {
        return (Mathf.Abs(cube1.x - cube2.x) + Mathf.Abs(cube1.y - cube2.y) + Mathf.Abs(cube1.z - cube2.z)) / 2;
    }

    public int Distance(Vector2Int coords1, Vector2Int coords2)
    {
        Vector3Int cube1 = CubeFromCoords(coords1);
        Vector3Int cube2 = CubeFromCoords(coords2);
        return CubeDistance(cube1, cube2);
    }
}
