using System;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

public enum PatternMatchType
{
    None,
    Smaller,
    Exact
}

public class PatternSelection: IComparable<PatternSelection>
{
    public TileType[,] pattern;
    public PatternMatchType matchType;

    public PatternSelection(TileType[,] array, PatternMatchType match)
    {
        pattern = array;
        matchType = match;
    }

    public int CompareTo(PatternSelection other)
    {
        return matchType.CompareTo(other.matchType);
    }
}

public class BSPContext: BaseMapContext
{
    public int MaxDivisions = -1;
    public float horzSplitRatio = 0.3f;
    public float vertSplitRatio = 0.5f;

    public float horzSplitChance = 0.5f;
    public float emptyRoomChance = 0.04f;

    public Vector2Int MinAreaSize;
    public Vector2Int MinRoomSize;
    public Vector2Int MaxRoomSize;

    public bool Seeded;
    public int Seed;

    public Vector2Int MapSize;

    public List<MonsterData> MonsterPool;
    public float monsterSpawnChance = 0.4f;
    public int minMonstersPerRoom = 1;
    public int maxMonstersPerRoom = 3;
    public int maxTotalMonsters = 15;

    public Vector2Int PlayerStart;
    public List<MonsterSpawn> MonsterSpawns;

    public List<TileType[,]> WoodPatterns;
    public float patternRoomsChance = 0.1f;
    public int minWood = 1;
}


public class BSPMapGenerator : IMapGenerator
{
    BSPNode _tree;
    BSPContext _context;

    List<BSPRect> _rooms = new List<BSPRect>();

    public BSPMapGenerator()
    {}

    public void Init(/* dependencies*/)
    {
        
    }

    public void GenerateMap(ref TileType[] map, BaseMapContext mapGenContext)
    {
        _context = (BSPContext)mapGenContext;

        if(_context.Seeded)
            URandom.InitState(_context.Seed);
  
        TileType[,] mapAux = new TileType[_context.Size.x, _context.Size.y];
        mapAux.Fill<TileType>(TileType.None);

        _tree = new BSPNode();
        _tree.context = _context;
        _tree.left = _tree.right = null;
        _tree.area = new BSPRect(1, 1, _context.Size.x - 2, _context.Size.y - 2);

        GenerateRooms(ref mapAux);
        GenerateMonsters(mapAux);
        Connect(_tree, ref mapAux);

        GeneratorUtils.PlaceWalls(_context.WallTile, _context.GroundTile, ref mapAux);
        GeneratorUtils.ConvertGrid(mapAux, out map);
    }

    void GenerateMonsters(TileType[,] map)
    {
        _context.MonsterSpawns = new List<MonsterSpawn>();
        int monsterCount = 0;

        Predicate<Vector2Int> validMonsterPos = (pos) =>
        {
            return _context.PlayerStart != pos && map[pos.x, pos.y] == _context.GroundTile;
        };

        foreach (var r in _rooms)
        {
            float monsterRoll = URandom.value;
            if(monsterRoll <= _context.monsterSpawnChance)
            {
                int monstersLeft = _context.maxTotalMonsters - monsterCount;
                int numMonsters = Mathf.Min(monstersLeft, URandom.Range(_context.minMonstersPerRoom, _context.maxMonstersPerRoom + 1));
                for(int i = 0; i < numMonsters; ++i)
                {
                    
                    MonsterData monsterType = _context.MonsterPool[URandom.Range(0, _context.MonsterPool.Count)];
                    Vector2Int monsterCoords = GetRandomCoordsInRoom(r, validMonsterPos, 10);
                    if(monsterCoords.x >= 0 && monsterCoords.y >= 0)
                    {
                        MonsterSpawn spawn = new MonsterSpawn();
                        spawn.Data = monsterType;
                        spawn.Coords = monsterCoords;
                        _context.MonsterSpawns.Add(spawn);
                        monsterCount++;
                    }
                }
            }

            if (monsterCount >= _context.maxTotalMonsters)
                break;
        }
    }

    public int CompareSelections(PatternSelection one, PatternSelection other)
    {
        return one.CompareTo(other);
    }

    public List<TileType[,]> GetPatternCandidates(BSPRect rect)
    {
        List<PatternSelection> selection = new List<PatternSelection>();
        foreach(var pattern in _context.WoodPatterns)
        {
            var match = PatternFitsInRoom(pattern, rect);
            if (match == PatternMatchType.None)
                continue;
            selection.Add(new PatternSelection(pattern, match));
        }


        return selection.ConvertAll(x => x.pattern);
    }

    PatternMatchType PatternFitsInRoom(TileType[,] woodPattern, BSPRect roomRect)
    {
        int patternHeight = woodPattern.GetLength(0);
        int patternWidth = woodPattern.GetLength(1);

        if (roomRect.Height == patternHeight && roomRect.Width == patternWidth)
        {
            return PatternMatchType.Exact;
        }
        else if (patternHeight <= roomRect.Height && patternWidth <= roomRect.Width )
        {
            return PatternMatchType.Smaller;
        }
        return PatternMatchType.None;
    }

    public void ApplyPattern(BSPRect rect, TileType[,] pattern, ref TileType[,] map)
    {
        int height = pattern.GetLength(0);
        int width = pattern.GetLength(1);

        int row = URandom.Range(rect.Row, rect.Row + rect.Height - height);
        int col = URandom.Range(rect.Col, rect.Col+ rect.Width - width);

        for(var r = 0; r < height; ++r)
        {
            for(var c = 0; c < width; ++c)
            {
                if(pattern[r,c] != TileType.None && map[row + r, col + c] != TileType.Goal && (_context.PlayerStart.x != row + r || _context.PlayerStart.y != col + c))
                {
                    map[row + r, col + c] = pattern[r, c];
                }
            }
        }
    }

    void GenerateRooms(ref TileType[,] mapAux)
    {
        _rooms = new List<BSPRect>();

        List<BSPNode> leaves = new List<BSPNode>();
        _tree.Split();
        _tree.GetLeaves(ref leaves);
        foreach (var leaf in leaves)
        {
            bool skipRoom = URandom.value < _context.emptyRoomChance;
            if (skipRoom) continue;

            int height = URandom.Range(_context.MinRoomSize.x, Mathf.Min(leaf.area.Height, _context.MaxRoomSize.x) + 1);
            int width = URandom.Range(_context.MinRoomSize.y, Mathf.Min(leaf.area.Width, _context.MaxRoomSize.y) + 1);

            int row = leaf.area.Row + URandom.Range(1, leaf.area.Height - height);
            int col = leaf.area.Col + URandom.Range(1, leaf.area.Width - width);

            leaf.roomRect = new BSPRect(row, col, height, width);
            _rooms.Add(leaf.roomRect);
            GeneratorUtils.DrawRoom(new Vector2Int(row, col), new Vector2Int(height, width), _context.GroundTile, ref mapAux);
        }

        // Player start
        int randomPlayerStart = URandom.Range(0, _rooms.Count);
        BSPRect playerStart = _rooms[randomPlayerStart];
        _context.PlayerStart = GetRandomCoordsInRoom(playerStart);

        // Goal
        int goalIdx;
        do
        {
            goalIdx = URandom.Range(0, _rooms.Count);
        } while (goalIdx != randomPlayerStart);

        // TODO: Locate goal somewhere better
        BSPRect goalRoom = _rooms[goalIdx];
        Vector2Int goalCoords = GetRandomCoordsInRoom(goalRoom);

        mapAux[goalCoords.x, goalCoords.y] = TileType.Goal;


        // Place patterns:
        foreach (var r in _rooms)
        {
            bool tryApplyPattern = URandom.value < _context.patternRoomsChance; 
            if(tryApplyPattern && _context.WoodPatterns != null && _context.WoodPatterns.Count > 0)
            {
                var candidates = GetPatternCandidates(r);
                if(candidates != null && candidates.Count > 0)
                {
                    ApplyPattern(r, candidates[URandom.Range(0, candidates.Count)], ref mapAux);
                }
            }
        }

    }

    public Vector2Int GetRandomCoordsInRoom(BSPRect rect)
    {
        return new Vector2Int(URandom.Range(rect.Row, rect.Row + rect.Height),
            URandom.Range(rect.Col, rect.Col + rect.Width));
    }

    public Vector2Int GetRandomCoordsInRoom(BSPRect rect, Predicate<Vector2Int> coordsCheck, int maxAttempts)
    {
        int numAttempts = 0;
        do
        {
            Vector2Int coords = GetRandomCoordsInRoom(rect);
            if (coordsCheck(coords))
            {
                return coords;
            }
            else numAttempts++;
        } while (numAttempts < maxAttempts);
        return new Vector2Int(-1, -1);
    }
    
    public void Connect(BSPNode tree, ref TileType[,] mapAux)
    {
        if(tree.left == null && tree.right == null)
        {
            return;
        }

        if(tree.left != null)
        {
            Connect(tree.left, ref mapAux);
        }

        if(tree.right != null)
        {
            Connect(tree.right, ref mapAux);
        }

        if(tree.left != null && tree.right != null)
        {
            BSPNode leftRoom = tree.left.GetLeafNode();
            BSPNode rightRoom = tree.right.GetLeafNode();
            if(leftRoom != null && rightRoom != null)
            {
                ConnectRooms(leftRoom.roomRect, rightRoom.roomRect, ref mapAux);
            }
        }
    }

    public void ConnectRooms(BSPRect leftRoom, BSPRect rightRoom, ref TileType[,] mapAux)
    {
        int column1 = URandom.Range(leftRoom.Col, leftRoom.Col + leftRoom.Width);
        int row1 = URandom.Range(leftRoom.Row, leftRoom.Row + leftRoom.Height);

        int column2 = URandom.Range(rightRoom.Col, rightRoom.Col + rightRoom.Width);
        int row2 = URandom.Range(rightRoom.Row, rightRoom.Row + rightRoom.Height);

        int deltaRows = row2 - row1;
        int deltaCols = column2 - column1;


        if (deltaRows < 0) // down
        {
            if (deltaCols != 0)
            {
                bool isCornerUp = URandom.value < 0.5f;
                if(isCornerUp)
                {
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row1, column2), _context.GroundTile, ref mapAux);
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column2), new Vector2Int(row2, column2), _context.GroundTile, ref mapAux);
                }
                else
                {
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column1), _context.GroundTile, ref mapAux);
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column2), _context.GroundTile, ref mapAux);
                }
            }
            else
            {
                GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column2), _context.GroundTile, ref mapAux);
            }
        }
        else if (deltaRows > 0) // up
        {
            if (deltaCols != 0)
            {
                bool isCornerUp = URandom.value < 0.5f;
                if (isCornerUp)
                {
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column1), _context.GroundTile, ref mapAux);
                    GeneratorUtils.DrawCorridor(new Vector2Int(row2, column1), new Vector2Int(row2, column2), _context.GroundTile, ref mapAux);
                }
                else
                {
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row1, column2), _context.GroundTile, ref mapAux);
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column2), new Vector2Int(row2, column2), _context.GroundTile, ref mapAux);
                }
            }
            else
            {
                GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column2), _context.GroundTile, ref mapAux);
            }
        }
        else
        {
            GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column2), _context.GroundTile, ref mapAux);
        }
    }

    public BSPNode GetRoomNode()
    {
        return _tree;
    }
}
