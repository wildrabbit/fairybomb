using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMonsterAction
{
    public Vector2Int NextCoords;
    public bool RefreshPath;
    public List<Vector2Int> Path;
    public int PathIdx;
}

public class MeleeAttackAction: BaseMonsterAction
{
    public BaseEntity Target;
    // TODO: Melee weapon, stuff
}

public class BombAttackAction: BaseMonsterAction
{}

public enum MonsterState
{
    Idle = 0,
    Wandering,
}


public class Monster : BaseEntity
{
    [Header("Config")]
    public float Speed = 1.0f;
    public int StartHP = 3;
    public float ThinkingDelay = 2.0f;
    public SpriteRenderer ViewPrefab;

    [Header("Structure")]
    [SerializeField] SpriteRenderer _view;


    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;

    public MonsterState CurrentState => _currentState;

    HPTrait _hpTrait;
    MonsterState _currentState;

    public float _elapsedNextAction;
    public float _elapsedPathUpdate;
    public float _decisionDelay;

    AIController _aiController;

    public override void Init(IEntityController entityController, FairyBombMap map)
    {
        base.Init(entityController, map);
        name = "Monsterrr";
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, StartHP, false);
        _decisionDelay = ThinkingDelay;
        _elapsedNextAction = 0.0f;
        _elapsedPathUpdate = 0.0f;
        _currentState = MonsterState.Wandering;
    }

    public void SetAIController(AIController aiController)
    {
        _aiController = aiController;
    }

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {
        _elapsedNextAction += timeUnits;

        while (_elapsedNextAction >= ThinkingDelay)
        {
            BaseMonsterAction action;
            MonsterState nextState = _aiController.MonsterBrain(this, out action);

            // Execute action, change state!
            if (action.NextCoords != Coords)
            {
                Coords = action.NextCoords;
                // TODO: Replace path?
            }

            // TODO: Combat actions!!

            bool changeState = nextState != _currentState;
            if(changeState)
            {
                //.... change views, reset data, etc,etc.
                _currentState = nextState;
            }

            if (_hpTrait.HP > 0 && _hpTrait.Regen)
            {
                _hpTrait.UpdateRegen(timeUnits);
            }
            _elapsedNextAction = Mathf.Max(_elapsedNextAction - ThinkingDelay, 0.0f);
        }
    }

    public void OnBombExploded(Bomb bomb, List<Vector2Int> coords, BaseEntity triggerEntity)
    {
        if (coords.Contains(Coords))
        {
            //if (BombImmunity == BombImmunityType.AnyBombs || (BombImmunity == BombImmunityType.EnemyBombs && !isOwnBomb))
            {
                _hpTrait.Decrease(bomb.Damage);
                Debug.Log($"{name} took {bomb.Damage} damage!. Current HP: {HP}");
                if (HP == 0)
                {
                    _entityController.DestroyEntity(this);
                }
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
}

/*
 using System;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Entity
{
    private enum MonsterAction
    {
        None,
        Wandering,
        Chasing,
        Attacking,
        Escaping
    }

    public float _elapsedNextAction;
    public float _elapsedPathUpdate;
    public float _decisionDelay;

    List<Vector2Int> _path;
    int _pathIdx;

    MonsterConfig _monsterConfig;
    MonsterAction _currentAction;

    private void Awake()
    {
        _path = new List<Vector2Int>();
        _pathIdx = 0;
    }

    protected override void DoSetup()
    {
        _monsterConfig = _config as MonsterConfig;
        _decisionDelay = _monsterConfig.ThinkingDelay;
        _elapsedNextAction = 0.0f;
        _elapsedPathUpdate = 0.0f;
        _path.Clear();
        _pathIdx = 0;
        _currentAction = MonsterAction.None;
    }

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {
        _elapsedNextAction += timeUnits;

        Player p = _entityController.GetPlayer(); // TODO: Replace with attackTargets (eventually we might have pets, companions,...)
        int distance = _entityController.DistanceFunction(p.Coords, this.Coords);
        bool willEscape = 100 * HPPercent <= _monsterConfig.EscapeHPPercent && 100 * p.HPPercent > _monsterConfig.KeepAttackingPlayerIfBelowHPPercent;

        while (_elapsedNextAction >= _decisionDelay)
        {
            switch (_currentAction)
            {
                case MonsterAction.Attacking:
                {
                    if(willEscape)
                    {
                        Vector2Int coords = GetEscapeCoords(p.Coords);
                        _messageQueue.AddEntry(Name + " is too weak to keep fighting and escapes");
                        SetGridPos(coords);
                        _currentAction = MonsterAction.Escaping;
                    }
                    else
                    {
                        if (distance > _monsterConfig.Stats.BaseAttackRange)
                        {
                            if (distance <= _monsterConfig.Stats.VisibilityRange)
                            {
                                _pathIdx = 0;
                                _path.Clear();
                                _elapsedPathUpdate = 0;
                                _currentAction = MonsterAction.Chasing;
                                _messageQueue.AddEntry(Name + " sees you and runs towards you!");
                             }
                            else
                            {
                                Vector2Int coords = GetWanderCoords();
                                if (coords != Coords)
                                {
                                    SetGridPos(coords);
                                }
                                _currentAction = MonsterAction.Wandering;
                            }
                        }
                        else
                        {
                            EngageAttack();
                        }
                    }
                    break;
                }
                case MonsterAction.Chasing:
                {
                    if (willEscape)
                    {
                        Vector2Int coords = GetEscapeCoords(p.Coords);
                        _messageQueue.AddEntry(Name + " escapes");
                        SetGridPos(coords);
                        _currentAction = MonsterAction.Escaping;
                    }
                    else
                    {
                        if (distance <= _monsterConfig.Stats.BaseAttackRange)
                        {
                            EngageAttack();
                        }
                        else if(distance <= _monsterConfig.Stats.VisibilityRange)
                        {
                            if (_path.Count == 0 || _pathIdx == _path.Count - 1 || _elapsedPathUpdate > _monsterConfig.PathUpdateDelay)
                            {
                                _map.FindPath(this, p.Coords, ref _path);
                                _path.RemoveAt(0); // Base node
                                _elapsedPathUpdate = 0.0f;
                                _pathIdx = 0;
                            }
                            else
                            {
                                Vector2Int target = _path[_pathIdx++];
                                _messageQueue.AddEntry(Name + $" moves to {target}");
                                SetGridPos(target);
                            }
                        }
                        else
                        {
                            Vector2Int coords = GetWanderCoords();
                            if (coords != Coords)
                            {
                                SetGridPos(coords);
                            }
                            _currentAction = MonsterAction.Wandering;
                        }
                    }

                    break;
                }
                case MonsterAction.Escaping:
                {
                    if(distance > _monsterConfig.Stats.VisibilityRange)
                    {
                        Vector2Int coords = GetWanderCoords();
                        if (coords != Coords)
                        {
                            SetGridPos(coords);
                        }
                        _currentAction = MonsterAction.Wandering;
                    }
                    else if (willEscape)
                    {
                        Vector2Int coords = GetEscapeCoords(p.Coords);
                        _messageQueue.AddEntry(Name + " escapes");
                        SetGridPos(coords);
                    }
                    else
                    {
                        if (distance > _monsterConfig.Stats.BaseAttackRange)
                        {
                            _pathIdx = 0;
                            _path.Clear();
                            _elapsedPathUpdate = 0;
                            _currentAction = MonsterAction.Chasing;
                            _messageQueue.AddEntry(Name + " charges after you once again!");
                        }
                        else
                        {
                            EngageAttack();
                        }
                    }
                    break;
                }
                case MonsterAction.Wandering:
                {
                    if(distance > _monsterConfig.Stats.VisibilityRange)
                    {
                        Vector2Int coords = GetWanderCoords();
                        if(coords != Coords)
                        {
                            SetGridPos(coords);
                        }
                     }
                    else if (willEscape)
                    {
                        Vector2Int coords = GetEscapeCoords(p.Coords);
                        _messageQueue.AddEntry(Name + " escapes");
                        SetGridPos(coords);
                        _currentAction = MonsterAction.Escaping;
                    }
                    else if(distance <= _monsterConfig.Stats.BaseAttackRange)
                    {
                        EngageAttack();
                    }
                    else
                    {
                        _pathIdx = 0;
                        _path.Clear();
                        _elapsedPathUpdate = 0;
                        _currentAction = MonsterAction.Chasing;
                        _messageQueue.AddEntry(Name + " sees you and starts chasing you!");
                    }
                    break;
                }
                default:
                    {
                        if (distance > _monsterConfig.Stats.VisibilityRange)
                        {
                            Vector2Int coords = GetWanderCoords();
                            if (coords != Coords)
                            {
                                SetGridPos(coords);
                            }
                            _currentAction = MonsterAction.Wandering;
                        }
                        else if (willEscape)
                        {
                            Vector2Int coords = GetEscapeCoords(p.Coords);
                            _messageQueue.AddEntry(Name + " escapes");
                            SetGridPos(coords);
                            _currentAction = MonsterAction.Escaping;
                        }
                        else if (distance <= _monsterConfig.Stats.BaseAttackRange)
                        {
                            _messageQueue.AddEntry(Name + " stats attacking");
                            EngageAttack();
                        }
                        else
                        {
                            _pathIdx = 0;
                            _path.Clear();
                            _elapsedPathUpdate = 0;
                            _currentAction = MonsterAction.Chasing;
                            _messageQueue.AddEntry(Name + " sees you and starts chasing you!");
                        }
                        break;
                    }
            }

            if(HP > 0)
            {
                RegenHealth();
            }
            _elapsedNextAction = Mathf.Max(_elapsedNextAction - _decisionDelay, 0.0f);
            _decisionDelay = _monsterConfig.ThinkingDelay;
        }
    }

    private Vector2Int GetEscapeCoords(Vector2Int targetCoords)
    {
        int bestDistance = _entityController.DistanceFunction(Coords, targetCoords);
        List<Vector2Int> escapeCoords = new List<Vector2Int>();
        escapeCoords.Add(Coords);
        foreach (var delta in MapUtils.GetNeighbourDeltas(_entityController.DistanceStrategy))
        {
            Vector2Int coords = Coords + delta;
            int distance = _entityController.DistanceFunction(coords, targetCoords);
            if (_map.IsWalkable(coords))
            {
                if(distance > bestDistance)
                {
                    bestDistance = distance;
                    escapeCoords.Clear();
                    escapeCoords.Add(coords);
                }
                else if(distance == bestDistance)
                {
                    escapeCoords.Add(coords);
                }
            }
        }
        if(escapeCoords.Count > 0)
        {
            return escapeCoords[UnityEngine.Random.Range(0, escapeCoords.Count)];
        }
        return Coords;
    }

    Vector2Int GetWanderCoords()
    {
        Vector2Int coords = Coords;
        Vector2Int[] deltas = MapUtils.GetNeighbourDeltas(_entityController.DistanceStrategy, true);
        List<Vector2Int> candidateCoords = new List<Vector2Int>();
        for (int i = 0; i < deltas.Length; ++i)
        {
            Vector2Int coord = Coords + deltas[i];
            if (_map.IsWalkable(coord) && !_entityController.FindEntityNearby(coord, 0, this))
            {
                candidateCoords.Add(coord);
            }
        }
        if (candidateCoords.Count > 0)
        {
            coords = candidateCoords[UnityEngine.Random.Range(0, candidateCoords.Count)];
        }
        return coords;
    }

    void EngageAttack()
    {
        ResultData result;
        Player p = _entityController.GetPlayer();
        BattleUtils.SolveAttack(this, p, out result);
        if (result.DefenderDefeated)
        {
            SetGridPos(p.Coords);
        }
        ProcessResultMessages(result);
    }

    void ProcessResultMessages(ResultData result)
    {
        if(result.AttackerFlopped)
        {
            _messageQueue.AddEntry(Name + " missed " + result.DefenderName + " by a hair!");
        }
        if(result.DefenderAvoided)
        {
            _messageQueue.AddEntry(result.DefenderName + " skillfully dodged " + Name + "'s attack!");
        }
        if(result.Critical)
        {
            _messageQueue.AddEntry("Critical hit!");
        }
        if(result.AttackerDmgInflicted > 0)
        {
            string dmgString = Name + " hit " + result.DefenderName + " for " + (int)result.AttackerDmgInflicted + " HP";
            if(result.DefenderDefeated)
            {
                dmgString += ". Killing blow!";
            }
            else
            {
                dmgString += ". Remaining: " + ((int)result.DefenderHP).ToString();
            }
            _messageQueue.AddEntry($"<color=red>{dmgString}</color>");
        }
    }
}

     * */

