using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FairyBombMap : MonoBehaviour
{
    [SerializeField] List<FairyBombTile> _lesTiles;

    [SerializeField] Tilemap _map;
    public Vector2Int PlayerStart;
    public Vector2Int Dimensions;

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

    public void Cleanup()
    {
        _map.ClearAllTiles();
    }
}
