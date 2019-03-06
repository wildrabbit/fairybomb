using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FairyBombMap : MonoBehaviour
{
    public Vector2Int PlayerStart => _playerStart;

    [SerializeField] Tilemap _map;

    FairyBombMapData _mapData;

    List<FairyBombTile> _lesTiles;
    FairyBombTile _goalTile;
    Vector2Int _playerStart;

    Vector3Int[] cubeOffsets = new Vector3Int[]
    {
        new Vector3Int(0, 0, 0), new Vector3Int(0, -1, 1), new Vector3Int(1, 0, 1), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, -1), new Vector3Int(-1, 0, -1), new Vector3Int(-1, -1, 0)
    };

    // We'll start with neutral, then N and then clockwise
    Vector2Int[][] _neighbourOffsets = new Vector2Int[][]
    {
        new Vector2Int[]{ new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1,1), new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1)},
        new Vector2Int[]{ new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1),new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1),new Vector2Int(1, -1)}
    };

    public int Height => _map.size.x;
    public int Width => _map.size.y;

    public bool HasTile(Vector3Int pos)
    {
        return _map.HasTile(pos);
    }

    public int[] AllTileValues
    {
        get
        {
           var tileBase = _map.GetTilesBlock(CellBounds);
            return System.Array.ConvertAll(tileBase, tile => (int)(((FairyBombTile)tile).TileType));
        }
    }

    public FairyBombTile[] AllTiles
    {
        get
        {
            var tileBase = _map.GetTilesBlock(CellBounds);
            return System.Array.ConvertAll(tileBase, tile => (FairyBombTile)tile);
        }
    }

    public BoundsInt CellBounds
    {
        get
        {
            _map.CompressBounds();
            return _map.cellBounds;
        }
    }

    public void GetNeighbourDeltas(Vector2Int currentCoords, out Vector2Int[] offsets)
    {
        var source = _neighbourOffsets[currentCoords.y & 1];
        int deltasLen = source.Length - 1;
        offsets = new Vector2Int[deltasLen];
        Array.Copy(source, 1, offsets, 0, deltasLen);
    }

    public void InitFromData(FairyBombMapData data, GameEventLog gameEventLog)
    {
        _mapData = data;
        _lesTiles = new List<FairyBombTile>(data.Palette);
        _lesTiles.Sort((x1, x2) => x1.TileType.CompareTo(x2.TileType));
        _goalTile = data.GoalTile;
        Vector2Int size;
        int[] level;
        if(GetLevelData(data, out size, out level))
        {
            TileType[] levelTiles = System.Array.ConvertAll(level, intValue => (TileType)intValue);
            InitFromArray(size, levelTiles, data.PlayerStart, data.OriginIsTopLeft);
        }
        // TODO: MAP GENERATION
    }

    bool GetLevelData(FairyBombMapData data, out Vector2Int size, out int[] tiles)
    {
        size = Vector2Int.zero;
        tiles = new int[0];

        string[] lines = data.MapInfo.text.Split('\n');
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
            if (tilesRow.Length != size.y)
            {
                Debug.LogError($"Invalid col count @ row {i - 1}");
                return false;
            }
            for (int j = 0; j < tilesRow.Length; ++j)
            {
                tiles[(i - 1) * size.y + j] = Int32.Parse(tilesRow[j]);
            }
        }
        return true;
    }

    public void InitFromArray(Vector2Int dimensions, TileType[] typeArray, Vector2Int playerStart, bool arrayOriginTopLeft)
    {
        _map.ClearAllTiles();
        int width = dimensions.y;
        int height = dimensions.x;
        if(typeArray.Length != width * height)
        {
            Debug.LogError("Array dimensions not matching provided size");
            return;
        }

        _playerStart = playerStart;
        for(int row = 0; row < height; ++row)
        {
            for(int col = 0; col < width; ++col)
            {
                int rowCoord = (arrayOriginTopLeft) ? height - (row + 1) : row;
                _map.SetTile(new Vector3Int(rowCoord, col, 0), GetTileByType(typeArray[width * row + col]));
            }
        }
    }

    public void Generate()
    {
        Vector2Int dimensions = Vector2Int.zero;
        TileType[] array = new TileType[] { };
        Vector2Int playerStart = Vector2Int.zero;
        InitFromArray(dimensions, array, playerStart, arrayOriginTopLeft: false);
    }

    public List<Vector2Int> GetWalkableNeighbours(Vector2Int coords)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        Vector2Int[] offsets = _neighbourOffsets[coords.y & 1];
        for(int i = 1; i < offsets.Length; ++i)
        {
            Vector2Int neighbourCoords = coords + offsets[i];
            var tile = TileAt(neighbourCoords);
            if (tile.Walkable)
            {
                neighbours.Add(neighbourCoords);
            }
        }
        return neighbours;
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

    public bool InBounds(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < _map.size.x && coords.y >= 0 && coords.y < _map.size.y;
    }

    List<Vector2Int> GetNearbyCoords(Vector2Int reference, int radius)
    {
        List<Vector2Int> pending = new List<Vector2Int>();
        List<Vector2Int> pendingNextDepth = new List<Vector2Int>();
        HashSet<Vector2Int> candidates = new HashSet<Vector2Int>();

        pending.Add(reference);
        int depth = 0;
        while (depth <= radius)
        {
            pendingNextDepth.Clear();
            foreach (var curReference in pending)
            {
                candidates.Add(curReference);
                bool isEven = curReference.y % 2 == 0;
                Vector2Int[] offsets = _neighbourOffsets[isEven ? 0 : 1];
                // TODO: OPTIMIZATION: Make offsets dependent on the lookup direction
                foreach (var offset in offsets)
                {
                    Vector2Int neighbourCoords = curReference + offset;
                    if (!InBounds(neighbourCoords)) continue;
                    if (pending.Contains(neighbourCoords) || pendingNextDepth.Contains(neighbourCoords) || candidates.Contains(neighbourCoords)) continue;
                    pendingNextDepth.Add(neighbourCoords);
                }
            }
            depth++;
        }
        return new List<Vector2Int>(candidates);
    }

    bool IsDestructibleTile(Vector2Int coords, out FairyBombTile replacement)
    {
        var refTile = TileAt(coords);
        replacement = refTile.ReplacementTile;
        return refTile.Destructible;
    }

    public List<Vector2Int> GetExplodingCoords(Bomb bomb)
    {
        List<Vector2Int> explodingTiles = new List<Vector2Int>();
        bool ignoreBlocks = bomb.IgnoreBlocks;
        int radius = bomb.Radius;
        Vector2Int refCoords = bomb.Coords;
     
        var tile = TileAt(refCoords);
        if (!ignoreBlocks && tile.BlocksExplosions)
        {
            return explodingTiles;
        }
        explodingTiles.Add(refCoords);

        int numRays = 6;
        for (int i = 0; i < numRays; ++i)
        {
            MoveDirection rayDir = (MoveDirection)(i + 1);
            Vector2Int rayCurrentCoords = refCoords;
            int evenOffsetIdx = rayCurrentCoords.y & 1;
            for (int depth = 0; depth < radius; ++depth)
            {
                Vector2Int offset = _neighbourOffsets[evenOffsetIdx][(int)rayDir];
                rayCurrentCoords += offset;
                evenOffsetIdx = rayCurrentCoords.y & 1;

                tile = TileAt(rayCurrentCoords);

                if (!ignoreBlocks && tile.BlocksExplosions)
                {
                    break;
                }
                explodingTiles.Add(rayCurrentCoords);
            }
        }
        return explodingTiles;
    }

    public void BombExploded(Bomb bomb, List<Vector2Int> explodingCoordsList, BaseEntity triggerEntity)
    {
        List<Vector2Int> coords = explodingCoordsList.FindAll((x) =>
        {
            var t = TileAt(x);
            return t.Destructible && t.ReplacementTile != null;
        });

        FairyBombTile[] tiles = new FairyBombTile[coords.Count];
        Vector3Int[] coords3D = new Vector3Int[coords.Count];

        int i = 0;
        foreach(var coord in coords)
        {
            tiles[i] = TileAt(coord).ReplacementTile;
            coords3D[i] = (Vector3Int)coord;
            i++;
        }

        _map.SetTiles(coords3D, tiles);
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

    public bool IsOnRay(Vector2Int coords1, Vector2Int coords2)
    {
        Vector3Int cube1 = CubeFromCoords(coords1);
        Vector3Int cube2 = CubeFromCoords(coords2);
        Vector3Int diff = cube2 - cube1;

        int distance = CubeDistance(cube2, cube1);

        Vector3Int[] scaled = System.Array.ConvertAll(cubeOffsets, offset =>
        {
            offset.Scale(new Vector3Int(distance, distance, distance));
            return offset;
        });

        foreach(var offset in scaled)
        {
            if (diff == offset)
            {
                return true;
            }
        }

        return false;
    }
}
