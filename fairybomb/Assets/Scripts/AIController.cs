using System;
using System.Collections.Generic;
using UnityEngine;

public class AIController
{
    IEntityController _entityController;
    FairyBombMap _map;
    GameEventLog _eventLog;

    // TODO: Instead of having a monster ask the director to think, keep a list and then update all the monsters!

    public void Init(IEntityController entityController, FairyBombMap map, GameEventLog eventsLog)
    {
        _map = map;
        _eventLog = eventsLog;
        _entityController = entityController;
    }

    public MonsterState MonsterBrain(Monster leMonster, out BaseMonsterAction actionData, float units)
    {
        // Dead can't dance
        if(!leMonster.Active)
        {
            actionData = null;
            return leMonster.CurrentState;
        }

        Player p = _entityController.Player;
        int playerDistance = _map.Distance(p.Coords, leMonster.Coords);
        float hpRatio = leMonster.HP / (float)leMonster.MaxHP;
        bool isEscapeHPRatio = hpRatio <= leMonster.EscapeHPRatio;

        if(isEscapeHPRatio)
        {
            List<Bomb> placedBombs = _entityController.GetBombs();
            placedBombs.RemoveAll(x => leMonster.IsImmuneTo(x));
            Bomb escapeBomb = null;
            int minTurns = Int32.MaxValue;
            foreach (var bomb in placedBombs)
            {
                if (_map.IsOnRay(bomb.Coords, leMonster.Coords) && bomb.TurnsLeftToExplosion < minTurns)
                {
                    escapeBomb = bomb;
                    minTurns = escapeBomb.TurnsLeftToExplosion;
                }
            }

            if (escapeBomb != null)
            {
                return SetupEscapeState(leMonster, escapeBomb.Coords, out actionData);
            }

            MonsterState currentState = leMonster.CurrentState;
            if (isEscapeHPRatio && playerDistance <= leMonster.EscapeSafeDistance)
            {
                return SetupEscapeState(leMonster, p.Coords, out actionData);
            }
        }


        if (playerDistance <= leMonster.VisibilityRange)
        {
            if (playerDistance <= leMonster.AttackDistance())
            {
                if (leMonster.IsMelee)
                {
                    return SetupAttackState(leMonster, out actionData);
                }

                if (leMonster.IsBomber)
                {
                    return SetupBomberState(leMonster, units, false, out actionData);
                }
                else
                {
                    return SetupChaseState(leMonster, units, out actionData);
                }
            }
            else
            {
                return SetupChaseState(leMonster, units, out actionData);
            }
        }

        switch (leMonster.CurrentState)
        {
            case MonsterState.Idle:
                {
                    return (leMonster.TurnsInSameState >= leMonster.TurnLimit)
                        ? SetupWanderState(leMonster, out actionData)
                        : SetupIdleState(leMonster, out actionData);
                }
            case MonsterState.Wandering:
                {
                    return (leMonster.TurnsInSameState >= leMonster.TurnLimit)
                        ? SetupIdleState(leMonster, out actionData)
                        : SetupWanderState(leMonster, out actionData);
                }
            case MonsterState.Chasing:
                {
                    break;
                }
            case MonsterState.Escaping:
                {
                    break;
                }
        }
        actionData = null;
        return leMonster.CurrentState;
    }

    MonsterState SetupAttackState(Monster leMonster, out BaseMonsterAction leAction)
    {
        leAction = new MeleeAttackAction
        {
            NextCoords = leMonster.Coords,
            Target = _entityController.Player
        };
        return MonsterState.BattleAction;
    }
    MonsterState SetupChaseState(Monster leMonster, float timeUnits, out BaseMonsterAction leAction)
    {
        List<Vector2Int> currentPath = leMonster.Path ?? new List<Vector2Int>();
        bool willRefreshPath = false;
        int pathIdx = leMonster.CurrentPathIdx;
        float elapsed = leMonster.PathElapsed;
        if (currentPath.Count == 0 || leMonster.CurrentPathIdx == currentPath.Count - 1 || elapsed > leMonster.PathDelay)
        {
            PathUtils.FindPath(_map, leMonster.Coords, _entityController.Player.Coords, ref currentPath);
            willRefreshPath = true;
            pathIdx = 0;
            elapsed = 0.0f;

            if(currentPath.Count < 2)
            {
                if(_map.GetDestructibleNeighbours(leMonster.Coords).Count > 0)
                {
                    return SetupBomberState(leMonster, timeUnits, true, out leAction);                    
                }
                else
                {
                    return SetupWanderState(leMonster, out leAction);                    
                }
            }
        }
        else
        {
            pathIdx++;
            elapsed += timeUnits;
        }

        leAction = new ChaseMonsterAction()
        {
            NextCoords = currentPath.Count > 0 && pathIdx >= 0 && pathIdx < currentPath.Count ? currentPath[pathIdx] : leMonster.Coords,
            Path = currentPath,
            PathIdx = pathIdx,
            RefreshPath = willRefreshPath,
            PathElapsed = elapsed
        };
        return MonsterState.Chasing;
    }
    MonsterState SetupEscapeState(Monster leMonster, Vector2Int targetCoords, out BaseMonsterAction leAction)
    {
        leAction = new BaseMonsterAction()
        {
            NextCoords = EscapeCoords(leMonster, targetCoords),
        };
        return MonsterState.Escaping;
    }
    MonsterState SetupIdleState(Monster leMonster, out BaseMonsterAction leAction)
    {
        leAction = new BaseMonsterAction()
        {
            NextCoords = leMonster.Coords,
        };
        return MonsterState.Idle;
    }
    MonsterState SetupWanderState(Monster leMonster, out BaseMonsterAction leAction)
    {
        leAction = new BaseMonsterAction()
        {
            NextCoords = WanderCoords(leMonster),
        };
        return MonsterState.Wandering;
    }

    Vector2Int WanderCoords(Monster leMonster)
    {
        Vector2Int coords = leMonster.Coords;
        List<Vector2Int> neighbours = _map.GetWalkableNeighbours(coords); // this excludes coords!

        // TODO: Extract this
        List<BaseEntity> otherEntities = null;
        int idx = neighbours.Count - 1;
        while (idx >= 0)
        {
            otherEntities = _entityController.GetEntitiesAt(neighbours[idx]);
            bool remove = false;

            foreach (var other in otherEntities)
            {
                if (other is Bomb)
                {
                    BombWalkabilityType walksOverBombs = leMonster.BombWalkability;
                    bool isOwnBomb = ((Bomb)other).Owner == leMonster;
                    remove = (walksOverBombs == BombWalkabilityType.Block || walksOverBombs == BombWalkabilityType.CrossOwnBombs && !isOwnBomb);
                }
                else if (other is Player)
                {
                    // We'll probably change to a different state, but still we can't just cross
                    // to the player's pos.
                    remove = true;
                }
            }
            if (remove)
            {
                neighbours.Remove(neighbours[idx]);
            }
            idx--;
        }

        if (neighbours.Count > 0)
        {
            coords = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
        }
        return coords;
    }
    Vector2Int EscapeCoords(Monster leMonster, Vector2Int targetCoords)
    {
        Vector2Int monsterCoords = leMonster.Coords;
        int bestDistance = _map.Distance(monsterCoords, targetCoords);
        List<Vector2Int> escapeCoords = _map.GetWalkableNeighbours(monsterCoords);
        escapeCoords.Add(monsterCoords);

        List<Vector2Int> escapeCandidates = new List<Vector2Int>();
        foreach (var coords in escapeCoords)
        {
            int distance = _map.Distance(monsterCoords, coords);
            if (distance > bestDistance)
            {
                bestDistance = distance;
                escapeCandidates.Clear();
                escapeCandidates.Add(coords);
            }
            else if (distance == bestDistance)
            {
                escapeCandidates.Add(coords);
            }
        }
        if (escapeCandidates.Count > 0)
        {
            return escapeCoords[UnityEngine.Random.Range(0, escapeCoords.Count)];
        }
        return monsterCoords;
    }
    List<Vector2Int> GetChasePath(Monster leMonster, Vector2Int target)
    {
        return null;
    }

    public void Cleanup()
    {
           
    }

    private MonsterState SetupBomberState(Monster leMonster, float units, bool ignoreEntities, out BaseMonsterAction actionData)
    {
        BaseEntity[] blackList = new BaseEntity[] { leMonster };
        Vector2Int coords = leMonster.Coords;
        if (_map.TileAt(coords).Walkable && (ignoreEntities || !_entityController.ExistsEntitiesAt(coords, blackList)) && leMonster.BomberTrait.HasBombAvailable())
        {
            actionData = new PlaceBombAction()
            {
                NextCoords = coords,
                BombData = leMonster.BomberTrait.SelectedBomb,
                BombCoords = coords
            };
            return MonsterState.BombPlacement;
        }
        return SetupChaseState(leMonster, units, out actionData);
    }
}
