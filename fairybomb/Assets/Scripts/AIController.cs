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

    public MonsterState MonsterBrain(Monster leMonster, out BaseMonsterAction actionData)
    {
        // Dead can't dance
        if(!leMonster.Active)
        {
            actionData = null;
            return leMonster.CurrentState;
        }

        Player p = _entityController.Player;
        int playerDistance = _map.Distance(p.Coords, leMonster.Coords);

        MonsterState currentState = leMonster.CurrentState;
        switch (currentState)
        {
            // RESTING. IT COULD REGEN, OR JUST WAIT FOR SOMETHING TO HAPPEN
            case MonsterState.Idle:
            {
                actionData = new BaseMonsterAction()
                {
                    NextCoords = leMonster.Coords,
                    Path = new List<Vector2Int>(),
                    PathIdx = 0,
                    RefreshPath = false
                };

                return MonsterState.Wandering;
            }
            case MonsterState.Wandering:
            {
                actionData = new BaseMonsterAction()
                {
                    NextCoords = WanderCoords(leMonster),
                    Path = new List<Vector2Int>(),
                    PathIdx = 0,
                    RefreshPath = false
                };
                return MonsterState.Wandering;
            }
        }

        actionData = null;
        return leMonster.CurrentState;
    }

    private Vector2Int WanderCoords(Monster leMonster)
    {
        Vector2Int coords = leMonster.Coords;
        List<Vector2Int> neighbours = _map.GetWalkableNeighbours(coords); // this excludes coords!
        neighbours.RemoveAll(neighbourCoords => _entityController.ExistsEntitiesAt(neighbourCoords));

        if (neighbours.Count > 0)
        {
            coords = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
        }
        return coords;
    }

    public void Cleanup()
    {
           
    }
}
