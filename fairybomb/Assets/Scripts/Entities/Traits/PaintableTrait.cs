using System;
using UnityEngine;

public class PaintableTrait
{
    BaseEntity _owner;
    PaintMap _paintMap;

    PaintData _currentPaint;
    float _elapsedUnits;

    public void Init(BaseEntity owner, PaintMap map)
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
        switch (_currentPaint.Effect)
        {
            case PaintingEffect.Freeze:
            {
                _owner.Frozen = false;
                Debug.Log($"{_owner.name} no longer frozen");
                break;
            }
            case PaintingEffect.Haste:
            {
                _owner.ResetSpeedRate();
                break;
            }
            case PaintingEffect.Heal:
            {
                break;
            }
            case PaintingEffect.Poison:
            {
                break;
            }
            case PaintingEffect.Slow:
            {
                _owner.ResetSpeedRate();
                break;
            }
        }
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
        switch(paint.Effect)
        {
            case PaintingEffect.Freeze:
            {
                if(UnityEngine.Random.value <= paint.FreezeChance)
                {
                    _owner.Frozen = true;
                    Debug.Log($"{_owner.name} is frozen and cannot move!");
                    // TODO: Freeze view!
                }
                break;
            }
            case PaintingEffect.Haste:
            {
                _owner.SetSpeedRate(paint.SpeedRate);
                break;
            }
            case PaintingEffect.Heal:
            {
                int initialRecover = paint.HPDelta;
                ((IHealthTrackingEntity)_owner).HPTrait.Add(initialRecover);
                break;
            }
            case PaintingEffect.Poison:
            {
                int poisonDmg = paint.HPDelta;
                ((IHealthTrackingEntity)_owner).TakeDamage(poisonDmg);
                break;
            }
            case PaintingEffect.Slow:
            {
                _owner.SetSpeedRate(-paint.SpeedRate);
                break;
            }
        }
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
        _elapsedUnits += units;
        switch (_currentPaint.Effect)
        {
            case PaintingEffect.Freeze:
            {
                bool wasFrozen = _owner.Frozen;
                _owner.Frozen = UnityEngine.Random.value <= _currentPaint.FreezeChance;
                if(!_owner.Frozen && wasFrozen)
                {
                    Debug.Log($"{_owner.name} no longer frozen");
                }
                else if (_owner.Frozen && wasFrozen)
                {
                    Debug.Log($"{_owner.name} remains frozen");
                }
                else if(_owner.Frozen && !wasFrozen)
                {
                    Debug.Log($"{_owner.name} becomes frozen");
                    }
                break;
            }
            case PaintingEffect.Haste:
            {
                break;
            }
            case PaintingEffect.Heal:
            {
                while (_elapsedUnits >= _currentPaint.TicksForHPChange)
                {
                    _elapsedUnits -= _currentPaint.TicksForHPChange;
                    ((IHealthTrackingEntity)_owner).HPTrait.Add(_currentPaint.HPDelta);
                }
                break;
            }
            case PaintingEffect.Poison:
            {
                while (_elapsedUnits >= _currentPaint.TicksForHPChange)
                {
                    _elapsedUnits -= _currentPaint.TicksForHPChange;
                    ((IHealthTrackingEntity)_owner).TakeDamage(_currentPaint.HPDelta);
                }
                break;
            }
            case PaintingEffect.Slow:
            {
                break;
            }            
        }

    }
}