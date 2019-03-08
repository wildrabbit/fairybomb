using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneratorUtils
{
    public static void ConvertGrid(TileType[,] multi, out TileType[] single)
    {
        int rows = multi.GetLength(0);
        int cols = multi.GetLength(1);
        single = new TileType[rows * cols];
        for(int row = 0; row < rows; ++row)
        {
            for(int col = 0; col < cols; ++col)
            {
                single[row * cols + col] = multi[row, col];
            }
        }
    }

    public static void DrawRoom(Vector2Int botLeft, Vector2Int size, TileType groundTile, ref TileType[,] grid)
    {
        for(int row = botLeft.x; row < botLeft.x + size.x; ++row)
        {
            for(int col = botLeft.y; col < botLeft.y + size.y; ++col)
            {
                if(row < 0 || row >= grid.GetLength(0) || col < 0 || col >= grid.GetLength(1))
                {
                    Debug.Log($"Invalid coords! {row}, {col}");
                    continue;
                }
                grid[row,col] = groundTile;
            }
        }
    }

    public static void DrawCorridor(Vector2Int from, Vector2Int to, TileType groundTile, ref TileType[,] grid)
    {
        // Straight for now:
        bool horizontal = from.x == to.x;
        bool vertical = from.y == to.y;
        if(horizontal)
        {
            DrawHorizontalCorridor(Mathf.Min(from.y, to.y), Mathf.Max(from.y, to.y), from.x, groundTile, ref grid);
            return;
        }
        if(vertical)
        {
            DrawVerticalCorridor(Mathf.Min(from.x, to.x), Mathf.Max(from.x, to.x), from.y, groundTile, ref grid);
            return;
        }

        //DrawLine(from, to, groundTile, ref grid);

    }

    private static void DrawVerticalCorridor(int startRow, int endRow, int col, TileType groundTile, ref TileType[,] grid)
    {
        for(int row = startRow; row <= endRow; ++row)
        {
            grid[row, col] = groundTile;
        }
    }

    private static void DrawHorizontalCorridor(int startCol, int endCol, int row, TileType groundTile, ref TileType[,] grid)
    {
        for(int col = startCol; col <= endCol; ++col)
        {
            grid[row, col] = groundTile;
        }
    }

    public static void PlaceWalls(TileType wallTile, TileType groundTile, ref TileType[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        Vector2Int[] offsets = new Vector2Int[]
        {
            new Vector2Int(-1,-1), new Vector2Int(-1,0), new Vector2Int(-1,1),
            new Vector2Int(0,-1), new Vector2Int( 0,1),
            new Vector2Int(1,-1), new Vector2Int(1,0), new Vector2Int(1,1)
        };

        for(int row = 0; row < rows; ++row)
        {
            for(int col = 0; col < cols; ++col)
            {
                if (grid[row, col] != TileType.None) continue;
                for (int i = 0; i < offsets.Length; ++i)
                {
                    Vector2Int refCoord = new Vector2Int(row + offsets[i].x, col + offsets[i].y);
                    if(refCoord.x < 0 || refCoord.x >= rows || refCoord.y < 0 || refCoord.y >= cols)
                    {
                        continue;
                    }
                    if(grid[refCoord.x, refCoord.y] == groundTile)
                    {
                        grid[row, col] = wallTile;
                    }
                }
            }
        }
    }
}
