using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathUtils
{
    private class PathInfo
    {
        public Vector2Int Coords;
        public Vector2Int? From;
        public int Distance;

        public PathInfo(Vector2Int coords, Vector2Int? from = null, int dist = Int32.MaxValue)
        {
            Coords = coords;
            From = from;
            Distance = dist;
        }

        public override string ToString() => $"{Coords} <- {(From.HasValue ? From.Value.ToString() : "NONE")} [{Distance}]";
    }

    public static void FindPath(FairyBombMap map, Vector2Int from, Vector2Int to, ref List<Vector2Int> path)
    {
        path.Clear();
        if (from == to)
        {
            path.Add(to);
            return;
        }

        Dictionary<Vector2Int, PathInfo> visitedInfo = new Dictionary<Vector2Int, PathInfo>();
        PriorityQueue<Vector2Int> coordsQueue = new PriorityQueue<Vector2Int>();
        BoundsInt mapBounds = map.CellBounds;
        List<Vector2Int> validTiles = new List<Vector2Int>();
        foreach (var position in mapBounds.allPositionsWithin)
        {
            Vector2Int pos2D = (Vector2Int)position;
            if (map.HasTile(position) && map.TileAt(pos2D).Walkable)
            {
                validTiles.Add(pos2D);
                visitedInfo[pos2D] = new PathInfo(pos2D);
                coordsQueue.Enqueue(pos2D, visitedInfo[pos2D].Distance);
            }
        }        
        visitedInfo[from].Distance = 0;
        coordsQueue.UpdateKey(from, visitedInfo[from].Distance);

        while (coordsQueue.Count > 0)
        {
            Vector2Int currentCoords = coordsQueue.Dequeue();
            Vector2Int[] deltas;
            map.GetNeighbourDeltas(currentCoords, out deltas);
            PathInfo currentInfo = visitedInfo[currentCoords];
            int formerDistance = currentInfo.Distance;
            if(formerDistance == Int32.MaxValue)
            {
                break;
            }
            foreach (var delta in deltas)
            {
                Vector2Int neighbourCoords = currentCoords + delta;
                if (!visitedInfo.ContainsKey(neighbourCoords))
                {
                    continue;
                }

                // TODO: Other checks: Doors, etc, etc.
                PathInfo neighbourInfo = visitedInfo[neighbourCoords];
                int distance = formerDistance + 1;
                if (distance < neighbourInfo.Distance)
                {
                    neighbourInfo.From = currentCoords;
                    neighbourInfo.Distance = distance;

                    int count = coordsQueue.Count;
                    if (coordsQueue.Count > 0)
                    {
                        coordsQueue.UpdateKey(neighbourCoords, distance);
                    }
                    if (count != coordsQueue.Count)
                    {
                        Debug.LogError("Whaaaat");
                    }
                }
            }
        }

        if (visitedInfo.TryGetValue(to, out var destinationInfo) && destinationInfo.Distance != Int32.MaxValue)
        {
            List<Vector2Int> rList = new List<Vector2Int>();
            rList.Add(to);
            while (destinationInfo.From.HasValue)
            {
                rList.Add(destinationInfo.From.Value);
                destinationInfo = visitedInfo[destinationInfo.From.Value];
            }

            for (int i = rList.Count - 1; i >= 0; i--)
            {
                path.Add(rList[i]);
            }
        }
        else
        {
            path.Add(from);
        }        
    }
}
