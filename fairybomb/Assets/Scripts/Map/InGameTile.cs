using System.Collections.Generic;
using UnityEngine;


public enum Faction
{
    Neutral,
    Player,
    Enemy
}

public class InGameTile
{
    public PaintMap Owner;
    public Vector2Int Coords;
    public Faction TileFaction;
    public PaintData PaintData;
    public int TurnsSinceLastPaintingChange; // To calculate automatic degradation;

    public bool OnExploded(Bomb b, List<BaseEntity> entitiesAtCoords)
    {
        var bombPaint = b.PaintData;
        if(bombPaint == null || bombPaint.Effect == PaintingEffect.None)
        {
            return false;
        }

        RemovePaint();
        if (bombPaint.Effect != PaintingEffect.Remove)
        {
            TileFaction = (b.Owner is Player)
            ? Faction.Player
            : (b.Owner is Monster) ? Faction.Enemy : Faction.Neutral;

            PaintData = bombPaint;
            TurnsSinceLastPaintingChange = 0;
        }
        return true;
    }

    public bool Tick()
    {
        if(PaintData == null)
        {
            return false;
        }

        TurnsSinceLastPaintingChange++;
        if(TurnsSinceLastPaintingChange >= PaintData.TimeToDegrade)
        {
            RemovePaint();
            return true;
        }

        return false;
    }

    void RemovePaint()
    {
        TileFaction = Faction.Neutral;
        PaintData = null;
        TurnsSinceLastPaintingChange = 0;
    }
}
