using System;
using UnityEngine;

public delegate void AppliedPaint(PaintData paint, IPaintableEntity owner);
public delegate void RemovedPaint(PaintData paint, IPaintableEntity owner);
public delegate void UpdatedPaint(PaintData paint, IPaintableEntity owner);

public class PaintableTrait
{
    IPaintableEntity _owner;
    PaintMap _paintMap;

    PaintData _currentPaint;
    float _elapsedUnits;

    public event AppliedPaint OnAppliedPaint;
    public event RemovedPaint OnRemovedPaint;
    public event UpdatedPaint OnUpdatedPaint;

    public void Init(IPaintableEntity owner, PaintMap map)
    {
        _owner = owner;
        _paintMap = map;
        _currentPaint = null;
        _elapsedUnits = 0;
    }

    public void PaintTileUpdated(InGameTile tile)
    {
        if (tile.PaintData == null && _currentPaint == null)
        {
            // SNOOZE
            return;
        }

        if(tile.PaintData == null && _currentPaint != null)
        {
            RemoveEffect();
        }
        else if (tile.PaintData != null && _currentPaint == null)
        {
            TryApplyEffect(tile.PaintData, tile.TileFaction);
        }
        else if(tile.PaintData == _currentPaint)
        {
            if(!CanOwnerBeAffected(tile.PaintData, tile.TileFaction))
            {
                RemoveEffect();
            }
            // Otherwise, we're already applying the same effect.
        }
        else
        {
            // Replace
            RemoveEffect();
            TryApplyEffect(tile.PaintData, tile.TileFaction);
        }
    }

    private bool CanOwnerBeAffected(PaintData paintData, Faction tileFaction)
    {
        Faction ownerFaction = _owner is Player
            ? Faction.Player
            : _owner is Monster
                ? Faction.Enemy
                : Faction.Neutral;

        bool affectedByFaction = (paintData.TargetType == EffectTargetType.Everyone)
            || (paintData.TargetType == EffectTargetType.EveryoneNonNeutral && ownerFaction != Faction.Neutral)
            || (paintData.TargetType == EffectTargetType.SameFaction && ownerFaction == tileFaction)
            || (paintData.TargetType == EffectTargetType.RivalFaction && ownerFaction != tileFaction);

        if (!affectedByFaction) return false;

        switch(paintData.Effect)
        {
            case PaintingEffect.Heal:
            case PaintingEffect.Poison:
            {
                return typeof(IHealthTrackingEntity).IsAssignableFrom(_owner.GetType());                
            }
        }
        return true;
    }

    private void RemoveEffect()
    {
        OnRemovedPaint?.Invoke(_currentPaint, _owner);
        _owner.RemovedPaint(_currentPaint);
        _currentPaint = null;
        _elapsedUnits = 0.0f;
    }

    private bool TryApplyEffect(PaintData paint, Faction faction)
    {
        if(!CanOwnerBeAffected(paint, faction))
        {
            return false;
        }

        _currentPaint = paint;
        _elapsedUnits = 0.0f;
        _owner.AppliedPaint(_currentPaint);
        OnAppliedPaint?.Invoke(_currentPaint, _owner);
        return true;
    }

    public void OwnerChangedPos(Vector2Int newCoords)
    {
        InGameTile tile = _paintMap.GetTile(newCoords);
        PaintTileUpdated(tile);
    }

    public void AddTime(float units)
    {
        if(_currentPaint == null)
        {
            return;
        }

        UpdateEffect(units);
    }

    private void UpdateEffect(float units)
    {
        _elapsedUnits = _owner.UpdatedPaint(_currentPaint, _elapsedUnits + units);
        OnUpdatedPaint?.Invoke(_currentPaint, _owner);
    }
}