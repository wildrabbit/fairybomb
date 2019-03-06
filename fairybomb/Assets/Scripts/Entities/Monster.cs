﻿using System;
using System.Collections;
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

public class BombAttackAction: BaseMonsterAction
{}

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
    BattleAction
}


public class Monster : BaseEntity, IBattleEntity
{
    public SpriteRenderer ViewPrefab;

    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;

    public MonsterState CurrentState => _currentState;

    public BombWalkabilityType BombWalkability => _walkOverBombs;

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

    MonsterData _monsterData; // Should we expose it?

    HPTrait _hpTrait;
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
            return _monsterData.BomberData.DefaultBombData.Radius;
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
        if (coords.Contains(Coords))
        {
            if (_bombImmunity == BombImmunityType.NoBombs || (_bombImmunity == BombImmunityType.OwnBombs && !(bomb.Owner is Monster)))
            {
                TakeDamage(bomb.Damage);
            }
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
}