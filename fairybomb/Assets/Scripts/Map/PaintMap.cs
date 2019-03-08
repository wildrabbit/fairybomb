using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public delegate void TilesPaintUpdateDelegate(List<InGameTile> tiles);

public class PaintMap : MonoBehaviour, IScheduledEntity
{
    [SerializeField] Tilemap _map;
    [SerializeField] TileBase _playerPaint;
    [SerializeField] TileBase _enemyPaint;
    [SerializeField] TileBase _neutralPaint;
    [SerializeField] float UnitsPerTick;

    public event TilesPaintUpdateDelegate OnTilePaintUpdated;

    float _elapsedSinceLastTick;

    InGameTile[,] _paintTiles;

    FairyBombMap _srcMap;
    IEntityController _entityController;
    GameEventLog _eventLog;

    public void AddTime(float timeUnits, ref PlayContext playContext)
    {
        _elapsedSinceLastTick += timeUnits;
        while(_elapsedSinceLastTick >= UnitsPerTick)
        {
            List<InGameTile> degradedTiles = new List<InGameTile>();
            _elapsedSinceLastTick -= UnitsPerTick;
            foreach(var tile in _paintTiles)
            {
                if(tile.Tick())
                {
                    degradedTiles.Add(tile);
                }
            }

            if(degradedTiles.Count > 0)
            {
                RefreshTilemap(degradedTiles);
                OnTilePaintUpdated?.Invoke(degradedTiles);
            }
        }
    }

    public void RefreshTilemap(List<InGameTile> tiles)
    {
        foreach(var t in tiles)
        {
            Vector3Int coordVec = (Vector3Int)(t.Coords);
            if (t.PaintData == null)
            {
                _map.SetTile(coordVec, null);
            }
            else
            {
                _map.SetTile(coordVec, t.TileFaction == Faction.Player ? _playerPaint : (t.TileFaction == Faction.Enemy)? _enemyPaint : _neutralPaint);
                _map.SetTileFlags(coordVec, TileFlags.None);
                _map.SetColor(coordVec, t.PaintData.Colour);
            }
        }
    }

    public InGameTile GetTile(Vector2Int newCoords)
    {
        return _paintTiles[newCoords.x, newCoords.y];
    }

    // Start is called before the first frame update
    public void Init(FairyBombMap srcMap, IEntityController entityController, GameEventLog eventLog)
    {
        _srcMap = srcMap;
        _entityController = entityController;
        _eventLog = eventLog;

        _entityController.OnBombExploded += OnBombExploded;
    }

    public void MapLoaded()
    {
        _map.ClearAllTiles();

        _paintTiles = new InGameTile[_srcMap.Height, _srcMap.Width];
        for(int y = 0; y < _srcMap.Height; ++y)
        {
            for(int x = 0; x < _srcMap.Width; ++x)
            {
                _paintTiles[y, x] = new InGameTile()
                {
                    Owner = this,
                    Coords = new Vector2Int(y, x),
                    PaintData = null,
                    TileFaction = Faction.Neutral,
                    TurnsSinceLastPaintingChange = 0
                };
            }
        }
        _elapsedSinceLastTick = Time.time;
    }

    private void OnBombExploded(Bomb bomb, List<Vector2Int> coordsList, BaseEntity trigger)
    {
        List<InGameTile> updatedTiles = new List<InGameTile>();
        foreach(var coords in coordsList)
        {
            var tile = _paintTiles[coords.x, coords.y];
           if(tile.OnExploded(bomb, _entityController.GetEntitiesAt(coords, new BaseEntity[] {bomb})))
            {
                updatedTiles.Add(tile);
            }
        }
        if(updatedTiles.Count > 0)
        {
            OnTilePaintUpdated?.Invoke(updatedTiles);
            RefreshTilemap(updatedTiles);
        }
    }

    public void Cleanup()
    {
        _entityController.OnBombExploded -= OnBombExploded;
    }


}
