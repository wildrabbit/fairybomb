using UnityEngine;
using System.Collections.Generic;
using System;

public enum BombWalkabilityType
{
    Block,
    CrossOwnBombs,
    CrossAny
}

public enum BombImmunityType
{
    AnyBombs,
    OwnBombs,
    NoBombs
}


public class Player : BaseEntity, IBattleEntity, IBomberEntity, IPaintableEntity, IHealthTrackingEntity
{
    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;
    public float Speed => _speed;

    public BomberTrait BomberTrait => _bomberTrait;

    public BombWalkabilityType BombWalkability => _walkOverBombs;

    public bool CanMoveIntoMonsterCoords => _playerData.CanMoveIntoMonsterCoords;
    public int DmgFromMonsterCollision => _playerData.MonsterCollisionDmg;

    int IBattleEntity.HP => HP;
    int IBattleEntity.Damage => 0;
    string IBattleEntity.Name => name;

    public PaintableTrait PaintableTrait => _paintableTrait;

    public HPTrait HPTrait => _hpTrait;

    BombImmunityType _bombImmunity;
    BombWalkabilityType _walkOverBombs;

    float _oldSpeed;
    float _speed;

    PlayerData _playerData;
    HPTrait _hpTrait;
    BomberTrait _bomberTrait;
    PaintableTrait _paintableTrait;


    protected override void DoInit(BaseEntityDependencies deps)
    {
        _playerData = ((PlayerData)_entityData);

        name = "Player";
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, _playerData.HPData);
        _bomberTrait = new BomberTrait();
        _bomberTrait.Init(this, _playerData.BomberData);
        _bombImmunity = _playerData.MobilityData.BombImmunity;
        _walkOverBombs = _playerData.MobilityData.BombWalkability;
        _speed = _playerData.Speed;
        _paintableTrait = new PaintableTrait();
        _paintableTrait.Init(this, deps.PaintMap);
    }

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {
        if (_hpTrait.Regen)
        {
            _hpTrait.UpdateRegen(timeUnits);
        }
        if(_paintableTrait != null)
        {
            _paintableTrait.AddTime(timeUnits);
        }
    }

    public void OnBombExploded(Bomb bomb, List<Vector2Int> coords, BaseEntity triggerEntity)
    {
#pragma warning disable CS0252 // Involuntary reference comparison (What I DO want)
        bool isOwnBomb = (bomb.Owner == this);
#pragma warning restore CS0252

        if (isOwnBomb)
        {
            _bomberTrait.RestoreBomb(bomb);
        }
        if(coords.Contains(Coords))
        {
            if (!IsImmuneTo(bomb) && coords.Contains(Coords))
            {
                _entityController.EntityHealthEvent(this, bomb.Damage, true, false, false, false);
                TakeDamage(bomb.Damage);
            }
        }
    }

    private bool IsImmuneTo(Bomb bomb)
    {
        bool isOwnBomb = (bomb.Owner == this);
        return (_bombImmunity == BombImmunityType.AnyBombs || (_bombImmunity == BombImmunityType.OwnBombs && isOwnBomb));
    }

    public bool TakeDamage(int damage)
    {
        _hpTrait.Decrease(damage);
        Debug.Log($"Player took {damage} damage!. Current HP: {HP}");
        if (HP == 0)
        {
            _entityController.PlayerKilled();
            _entityController.DestroyEntity(this);
            return true;
        }
        return false;
    }

    public override void Cleanup()
    {
        base.Cleanup();
    }

    public override void OnAdded()
    {
        base.OnAdded();
       _entityController.AddBomber(this);
    }

    public override void OnDestroyed()
    {
        base.OnDestroyed();
        _entityController.RemoveBomber(this);
        _entityController.PlayerDestroyed();
    }

    void IBattleEntity.ApplyBattleResults(BattleActionResult results, BattleRole role)
    {
        if(role == BattleRole.Defender)
        {
            TakeDamage(results.DefenderDmgTaken);
        }
        else
        {
            Debug.Log($"{name} attacked {results.DefenderName} and caused {results.AttackerDmgInflicted} dmg");
        }
    }

    public override void SetSpeedRate(float speedRate)
    {
        _oldSpeed = _speed;
        _speed *= (1 + speedRate / 100);
    }

    public override void ResetSpeedRate()
    {
        _speed = _oldSpeed;
    }

    public int MonsterCollided(Monster monster)
    {
        if (_playerData.MonsterCollisionDmg > 0)
        {
            TakeDamage(_playerData.MonsterCollisionDmg);
        }
        return _playerData.MonsterCollisionDmg;
    }

    public void AppliedPaint(PaintData paint)
    {
        switch (paint.Effect)
        {
            case PaintingEffect.Freeze:
            {
                if (UnityEngine.Random.value <= paint.FreezeChance)
                {
                    Frozen = true;                        
                }
                break;
            }
            case PaintingEffect.Haste:
            {
                SetSpeedRate(paint.SpeedRate);
                break;
            }
            case PaintingEffect.Heal:
            {
                int initialRecover = paint.HPDelta;
                _entityController.EntityHealthEvent(this, paint.HPDelta, false, true, false, false);
                HPTrait.Add(initialRecover);
                break;
            }
            case PaintingEffect.Poison:
            {
                int poisonDmg = paint.HPDelta;
                _entityController.EntityHealthEvent(this, paint.HPDelta, false, false, true, false);

                TakeDamage(poisonDmg);
                break;
            }
            case PaintingEffect.Slow:
            {
                SetSpeedRate(-paint.SpeedRate);
                break;
            }
        }
    }
    public void RemovedPaint(PaintData data)
    {
        switch (data.Effect)
        {
            case PaintingEffect.Freeze:
                {
                    Frozen = false;
                    break;
                }
            case PaintingEffect.Haste:
                {
                    ResetSpeedRate();
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
                    ResetSpeedRate();
                    break;
                }
        }
    }
    public float UpdatedPaint(PaintData paintData, float ticks)
    {
        switch (paintData.Effect)
        {
            case PaintingEffect.Freeze:
                {
                    bool wasFrozen = Frozen;
                    Frozen = UnityEngine.Random.value <= paintData.FreezeChance;
                    break;
                }
            case PaintingEffect.Haste:
                {
                    break;
                }
            case PaintingEffect.Heal:
                {
                    while (ticks >= paintData.TicksForHPChange)
                    {
                        ticks -= paintData.TicksForHPChange;
                        HPTrait.Add(paintData.HPDelta);
                    }
                    break;
                }
            case PaintingEffect.Poison:
                {
                    while (ticks >= paintData.TicksForHPChange)
                    {
                        ticks -= paintData.TicksForHPChange;
                        TakeDamage(paintData.HPDelta);
                    }
                    break;
                }
            case PaintingEffect.Slow:
                {
                    break;
                }
        }
        return ticks;
    }
}
