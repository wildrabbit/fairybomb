using System;
using System.Collections.Generic;
using UnityEngine;

using URandom = UnityEngine.Random;

public static class ArrayExtensions
{
    public static void Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = value;
        }
    }

    public static void Fill<T>(this T[,] array, T value)
    {
        for (int i = 0; i < array.GetLength(0); ++i)
            for (int j = 0; j < array.GetLength(1); ++j)
            {
                array[i, j] = value;
            }
    }
}

public class Room
{
    public BSPRect Bounds;
    public List<Vector2Int> Connectors;
    // TODO: Stuff
}

public class BSPNode
{
    public BSPRect area;
    public BSPRect roomRect;
    
    public BSPNode parent;

    public BSPNode left;
    public BSPNode right;

    public BSPContext context;

    public BSPNode()
    {
        area = BSPRect.Zero;
        roomRect = null;
        parent = null;
        left = right = null;
    }

    public void GetLeaves(ref List<BSPNode> leaves)
    {
        if (right == null && left == null)
        {
            leaves.Add(this);
        }
        else
        {
            left.GetLeaves(ref leaves);
            right.GetLeaves(ref leaves);
        }
    }

    public BSPNode GetLeafNode()
    {
        if(roomRect != null)
        {
            return this;
        }

        BSPNode leftLeaf = null;
        BSPNode rightLeaf = null;
        if (left != null) leftLeaf = left.GetLeafNode();
        if (right != null) rightLeaf = right.GetLeafNode();

        if (leftLeaf == null && rightLeaf == null) return null;
        if (leftLeaf == null) return rightLeaf;
        if (rightLeaf == null) return leftLeaf;

        return URandom.value > 0.5f ? leftLeaf : rightLeaf;
    }

    public bool Split()
    {
        float hsplitRoll = URandom.value;
        BSPGeneratorData bspData = context.BSPData;
        bool horizontalSplit = hsplitRoll < bspData.HorizontalSplitChance;
        float hRatio = area.Width / (float)area.Height;
        float vRatio = 1 / hRatio;
        if(hRatio >= 1.0f + bspData.VerticalSplitRatio)
        {
            horizontalSplit = false;
        }
        else if(vRatio > 1.0f + bspData.HorizontalSplitRatio)
        {
            horizontalSplit = true;
        }

        int maxSize = 0;
        int minSize = 0;

        if(horizontalSplit)
        {
            maxSize = area.Height - bspData.MinAreaSize.x;
            minSize = bspData.MinAreaSize.x;
        }
        else
        {
            maxSize = area.Width - bspData.MinAreaSize.y;
            minSize = bspData.MinAreaSize.x;
        }

        if(maxSize <= minSize)
        {
            return false;
        }

        int splitValue = URandom.Range(minSize, maxSize + 1);
        left = new BSPNode();
        left.context = context;

        right = new BSPNode();
        right.context = context;

        if(horizontalSplit)
        {
            left.area = new BSPRect(area.Row, area.Col, splitValue, area.Width);
            right.area = new BSPRect(area.Row + splitValue, area.Col, area.Height - splitValue, area.Width);
        }
        else
        {
            left.area = new BSPRect(area.Row, area.Col, area.Height, splitValue);
            right.area = new BSPRect(area.Row, area.Col + splitValue, area.Height, area.Width - splitValue);
        }

        left.Split();
        right.Split();
        return true;
    }
}