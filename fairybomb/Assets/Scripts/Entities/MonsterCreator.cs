using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterSpawn
{
    public Monster Prefab;
    public Vector2Int Coords;
}

public class MonsterCreator
{
    private IEntityController _entityController;
    private AIController _aiController;
    private FairyBombMap _map;
    private GameEventLog _log;

    public void Init(IEntityController entityController, AIController aiController, FairyBombMap map, GameEventLog log)
    {
        _aiController = aiController;
        _entityController = entityController;
        _map = map;
        _log = log;
    }

    public void AddInitialMonsters(List<MonsterSpawn> spawns)
    {
        foreach(var spawnData in spawns)
        {
            _entityController.CreateMonster(spawnData.Prefab, spawnData.Coords, _aiController);
        }
    }
}
