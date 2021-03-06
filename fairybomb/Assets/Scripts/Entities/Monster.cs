﻿using System;
using System.Collections.Generic;
using UnityEngine;


public class BaseMonsterAction
{
    public Vector2Int NextCoords;
    
}

public class ChaseMonsterAction: BaseMonsterAction
{
    public BaseEntity Target;

    public bool RefreshPath;
    public List<Vector2Int> Path;
    public float PathElapsed;
    public int PathIdx;
}

public class MeleeAttackAction: BaseMonsterAction
{
    public IBattleEntity Target;
    // TODO: Melee weapon, stuff
}

public class PlaceBombAction: BaseMonsterAction
{
    public BombData BombData;
    public Vector2Int BombCoords;
}

public class MonsterDependencies: BaseEntityDependencies
{
    public AIController AIController;
}

public enum MonsterState
{
    Idle = 0,
    Wandering,
    Chasing,
    Escaping,
    BattleAction,
    BombPlacement
}


public class Monster : BaseEntity, IBattleEntity, IBomberEntity, IHealthTrackingEntity, IPaintableEntity
{
    public SpriteRenderer ViewPrefab;

    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;

    public MonsterState CurrentState => _currentState;
    public BomberTrait BomberTrait => _bomberTrait;
    public BombWalkabilityType BombWalkability => _walkOverBombs;
    public LootInfo LootInfo => _monsterData.LootInfoOnDeath;

    public bool IsMelee => _monsterData.IsMelee;

    public bool IsImmuneTo(Bomb bomb)
    {
        bool isOwnBomb = (bomb.Owner == this);
        return (_bombImmunity == BombImmunityType.AnyBombs || (_bombImmunity == BombImmunityType.OwnBombs && isOwnBomb));            
    }

    public bool IsBomber => _monsterData.IsBomber;
    
    public int TurnsInSameState => _turnsInSameState;
    public int TurnLimit => _turnLimit;
    public float EscapeHPRatio => _monsterData.EscapeHPRatio;
    public int EscapeSafeDistance => _monsterData.EscapeSafeDistance;
    public int VisibilityRange => _monsterData.VisibilityRange;
    public List<Vector2Int> Path => _path;
    public int CurrentPathIdx => _currentPathIdx;
    public float PathDelay => _monsterData.PathUpdateDelay;
    public float PathElapsed => _elapsedPathUpdate;

    int IBattleEntity.HP => HP;

    int IBattleEntity.Damage => _monsterData.MeleeDamage;

    string IBattleEntity.Name => name;

    public BombData SelectedBomb
    {
        get => _bomberTrait.SelectedBomb;
        set { }
    }

    public HPTrait HPTrait => _hpTrait;
    public PaintableTrait PaintableTrait => _paintableTrait;

    MonsterData _monsterData; // Should we expose it?

    HPTrait _hpTrait;
    BomberTrait _bomberTrait;
    PaintableTrait _paintableTrait;

    MonsterState _currentState;

    BombImmunityType _bombImmunity;
    BombWalkabilityType _walkOverBombs;

    float _elapsedNextAction;

    float _decisionDelay;

    float _elapsedPathUpdate;
    int _turnLimit;
    int _turnsInSameState;

    List<Vector2Int> _path;
    int _currentPathIdx;

    public int AttackDistance()
    {
        if (_monsterData.IsMelee)
        {
            return 1;
        }
        else if (_monsterData.IsBomber)
        {
            return _bomberTrait.SelectedBomb.Radius;
        }
        return 0; // attack distance == 0? 
    }

    AIController _aiController;
    
    protected override void DoInit(BaseEntityDependencies deps)
    {
        MonsterDependencies monsterDeps = ((MonsterDependencies)deps);
        _aiController = monsterDeps.AIController;

        _monsterData = ((MonsterData)_entityData);
        name = _monsterData.name;
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, _monsterData.HPData);

        _bomberTrait = new BomberTrait();
        _bomberTrait.Init(this, _monsterData.BomberData);

        _paintableTrait = new PaintableTrait();
        _paintableTrait.Init(this, deps.PaintMap);

        _decisionDelay = _monsterData.ThinkingDelay;
        _elapsedNextAction = 0.0f;
        _elapsedPathUpdate = 0.0f;
        _bombImmunity = _monsterData.MovingData.BombImmunity;
        _walkOverBombs = _monsterData.MovingData.BombWalkability;

        _turnsInSameState = 0;
        StateChanged(_monsterData.InitialState);
    }

    public void SetAIController(AIController aiController)
    {
        _aiController = aiController;
    }

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {
        _elapsedNextAction += timeUnits;

        while (_elapsedNextAction >= _decisionDelay)
        {
            BaseMonsterAction action;
            MonsterState nextState = _aiController.MonsterBrain(this, out action, timeUnits);

            // Execute action, change state!
            if (action != null)
            {
                if(action.NextCoords != Coords)
                {
                    Coords = action.NextCoords;
                    PaintableTrait.OwnerChangedPos(action.NextCoords);
                    if(_entityController.Player.Coords == Coords && _monsterData.PlayerCollisionDmg > 0)
                    {
                        int monsterDmg = PlayerCollided(_entityController.Player);
                        int playerDmg = _entityController.Player.MonsterCollided(this);
                        _entityController.CollisionMonsterPlayer(_entityController.Player, this, playerDmg, monsterDmg);
                    }
                }
                
                if(action is ChaseMonsterAction)
                {
                    ChaseMonsterAction chaseAction = ((ChaseMonsterAction)action);
                    _currentPathIdx = chaseAction.PathIdx;
                    _elapsedPathUpdate = chaseAction.PathElapsed;
                    if (chaseAction.RefreshPath)
                    {
                        _path = chaseAction.Path;
                    }
                }

                else if (action is MeleeAttackAction)
                {
                    IBattleEntity target = ((MeleeAttackAction)action).Target;
                    BattleActionResult results;
                    BattleUtils.SolveAttack(this, target, out results);
                }
                else if (action is PlaceBombAction)
                {
                    var bombAction = ((PlaceBombAction)action);
                    Bomb bomb = _entityController.CreateBomb(bombAction.BombData, bombAction.BombCoords, this);
                    // TODO: Track monster placing bomb
                }
            }

            // TODO: Combat actions!!

            bool changeState = nextState != _currentState;
            if(changeState)
            {
                StateChanged(nextState);
            }
            else
            {
                // UpdateState(state, action)
                //if (_hpTrait.HP > 0 && _hpTrait.Regen)
                //{
                //    _hpTrait.UpdateRegen(timeUnits);
                //}
                _turnsInSameState++;
            }

            _elapsedNextAction = Mathf.Max(_elapsedNextAction - _decisionDelay, 0.0f);
        }
    }

    public void StateChanged(MonsterState monsterState)
    {
        Debug.Log($"{name} changes state to { monsterState}");
        switch (monsterState)
        {
            case MonsterState.Idle:
            {
                _turnLimit = UnityEngine.Random.Range(_monsterData.MinIdleTurns, _monsterData.MaxIdleTurns + 1);
                break;
            }
            case MonsterState.Wandering:
            {
                _turnLimit = UnityEngine.Random.Range(_monsterData.WanderToIdleMinTurns, _monsterData.WanderToIdleMaxTurns + 1);
                break;
            }
            default:
                break;
        }
        _currentState = monsterState;
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

        if(!IsImmuneTo(bomb) && coords.Contains(Coords))
        {
            _entityController.EntityHealthEvent(this, bomb.Damage, true, false, false, false);
            TakeDamage(bomb.Damage);           
        }
    }


    public override void OnAdded()
    {
        base.OnAdded();
        _entityController.OnBombExploded += OnBombExploded;
    }

    public override void OnDestroyed()
    {
        _entityController.OnBombExploded -= OnBombExploded;
        _entityController.NotifyMonsterKilled(this);
    }

    public override void Cleanup()
    {
        base.Cleanup();
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

    public bool TakeDamage(int amount)
    {
        _hpTrait.Decrease(amount);
        Debug.Log($"{name} took {amount} damage!. Current HP: {HP}");
        if (HP == 0)
        {
            _entityController.DestroyEntity(this);
            return true;
        }
        return false;
    }

    public override void SetSpeedRate(float speedRate)
    {
        _decisionDelay *= (1 - speedRate/100);
        Debug.Log($"Monster speed rate changed by {speedRate}% to a value of {_decisionDelay}");
    }

    public override void ResetSpeedRate()
    {
        _decisionDelay = _monsterData.ThinkingDelay;
        Debug.Log($"Monster speed rate restored to {_decisionDelay}");
    }

    public int PlayerCollided(Player p)
    {
        if(_monsterData.PlayerCollisionDmg > 0)
        {
            TakeDamage(_monsterData.PlayerCollisionDmg);
        }
        return _monsterData.PlayerCollisionDmg;
    }

    public void AppliedPaint(PaintData data)
    {

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
                        _entityController.EntityHealthEvent(this, paintData.HPDelta, false, true, false, false);
                    }
                    break;
                }
            case PaintingEffect.Poison:
                {
                    while (ticks >= paintData.TicksForHPChange)
                    {
                        ticks -= paintData.TicksForHPChange;
                        _entityController.EntityHealthEvent(this, paintData.HPDelta, false, false, true, false);
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