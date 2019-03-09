using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LootSpawn
{
    public BombData item;
    public int amount;
    public Vector2Int coords;
}

[System.Serializable]
public class LootInfo
{
    public float LootChance;
    public List<BombData> Items;

    public int MinItems; // amount of same item
    public int MaxItems;

    public int MinRolls; // number of items
    public int MaxRolls;
}

public class LootController
{
    FairyBombMap _map;
    IEntityController _entityController;
    GameEventLog _eventLog;
    LootItemData _data;

    public void Init(LootItemData _itemData, FairyBombMap map, IEntityController entityController, GameEventLog eventLog)
    {
        _map = map;
        _entityController = entityController;
        _eventLog = eventLog;

        _entityController.OnMonsterKilled += OnMonsterKilled;
        _map.OnTileDestroyed += OnTileDestroyed;
    }

    private void OnTileDestroyed(FairyBombTile destroyedTile, Vector2Int coords)
    {
        GenerateLootAt(destroyedTile.LootInfo, coords);
    }

    private void OnMonsterKilled(Monster monster)
    {
        GenerateLootAt(monster.LootInfo, monster.Coords);
    }

    public void LoadLootSpawns(List<LootSpawn> lootSpawns)
    {
        foreach(var spawn in lootSpawns)
        {
            _entityController.CreatePickable(_data, spawn.item, spawn.coords, spawn.amount, false);
        }
    }

    public bool GenerateLootAt(LootInfo info, Vector2Int coords)
    {
        if(UnityEngine.Random.value < info.LootChance)
        {
            int numRolls = UnityEngine.Random.Range(info.MinRolls, info.MaxRolls + 1);
            for(int i = 0; i < numRolls; ++i)
            {
                int amount = UnityEngine.Random.Range(info.MinItems, info.MaxItems + 1);
                BombData item = info.Items[UnityEngine.Random.Range(0, info.Items.Count)];
                _entityController.CreatePickable(_data, item, coords, amount, false);
            }
            return true;
        }
        return false;
    }

    internal void LoadLootSpawns(object lootSpawns)
    {
        throw new NotImplementedException();
    }

    public void Cleanup()
    {
        _entityController.OnMonsterKilled -= OnMonsterKilled;
        _map.OnTileDestroyed -= OnTileDestroyed;
    }
}