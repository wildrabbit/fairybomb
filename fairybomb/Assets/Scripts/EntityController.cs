using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityController: IEntityController
{
    Player _player;
    List<BaseEntity> _allEntities;

    List<BaseEntity> _entitiesToRemove;
    List<BaseEntity> _entitiesToAdd;

    public Player Player => _player;
    FairyBombMap _map;

    public event EntitiesAddedDelegate OnEntitiesAdded;
    public event EntitiesRemovedDelegate OnEntitiesRemoved;
    public event BombDelegate OnBombSpawned;
    public event BombDestroyedDelegate OnBombExploded;
    public event PlayerDestroyedDelegate OnPlayerKilled;

    public void Init(FairyBombMap map)
    {
        _map = map;
        _allEntities = new List<BaseEntity>();
        _entitiesToAdd = new List<BaseEntity>();
        _entitiesToRemove = new List<BaseEntity>();
    }
   
    public Player CreatePlayer(Player prefab, Vector2Int coords)
    {
        _player = Create<Player>(prefab, coords);
        return _player;
    }

    public Bomb CreateBomb(Bomb prefab, IBomberEntity owner, Vector2Int coords)
    {
        Bomb b = Create<Bomb>(prefab, coords);
        b.Owner = owner;
        return b;
    }

    public Monster CreateMonster(Monster prefab, Vector2Int coords, AIController aiController)
    {
        Monster m = Create<Monster>(prefab, coords);
        m.SetAIController(aiController);
        return m;
    }

    public T Create<T>(T prefab, Vector2Int coords) where T : BaseEntity
    {
        T entity = GameObject.Instantiate<T>(prefab);
        entity.Init(this, _map);
        entity.Coords = coords;
        _entitiesToAdd.Add(entity);
        return entity;
    }

    public bool ExistsNearbyEntity(Vector2Int coords, int radius, BaseEntity[] excluded = null)
    {
        var filteredEntities = excluded != null ? GetFilteredEntities(excluded) : _allEntities;
        foreach(var e in filteredEntities)
        {
            if(_map.Distance(e.Coords, coords) <= radius)
            {
                return true;
            }
        }
        return false;
    }

    public List<BaseEntity> GetEntitiesAt(Vector2Int actionTargetCoords, BaseEntity[] excluded = null)
    {
        List<BaseEntity> resultEntities = new List<BaseEntity>();
        List<BaseEntity> candidates = excluded != null ? GetFilteredEntities(excluded) : _allEntities;

        foreach (BaseEntity entity in candidates)
        {
            if(entity.Coords == actionTargetCoords)
            {
                resultEntities.Add(entity);
            }
        }

        return resultEntities;
    }

    public List<BaseEntity> GetFilteredEntities(BaseEntity[] excluded)
    {
        List<BaseEntity> filtered = new List<BaseEntity>(_allEntities);
        foreach(var excludedEntity in excluded)
        {
            filtered.Remove(excludedEntity);
        }
        return filtered;
    }

    public void DestroyEntity(BaseEntity entity)
    {
        entity.Active = false;
        _entitiesToRemove.Add(entity);
    }

    public void AddNewEntities()
    {
        foreach(var e in _entitiesToAdd)
        {
            e.OnAdded();
            _allEntities.Add(e);
            // TODO: Check specific lists
        }
        OnEntitiesAdded?.Invoke(_entitiesToAdd);
        _entitiesToAdd.Clear();
    }

    public void PurgeEntities()
    {
        foreach(var e in _entitiesToAdd)
        {
            e.OnDestroyed();
            GameObject.Destroy(e.gameObject);
        }
        foreach (var e in _allEntities)
        {
            e.OnDestroyed();
            GameObject.Destroy(e.gameObject);
            // Check specific lists
        }
        _entitiesToAdd.Clear();
        _entitiesToRemove.Clear();
        _allEntities.Clear();
    }

    public void RemovePendingEntities()
    {
        if(_entitiesToRemove.Count == 0)
        {
            return;
        }

        OnEntitiesRemoved?.Invoke(_entitiesToRemove);

        foreach (var e in _entitiesToRemove)
        {
            e.OnDestroyed();
            GameObject.Destroy(e.gameObject);
            _allEntities.Remove(e);
            // Check specific lists
        }
        _entitiesToRemove.Clear();
    }

    public void PlayerDestroyed()
    {
        _player = null;
        OnPlayerKilled?.Invoke();
    }

    public void Cleanup()
    {
        PurgeEntities();
    }

    public bool ExistsEntitiesAt(Vector2Int coords, BaseEntity[] excluded = null)
    {
        var filteredEntities = excluded != null ? GetFilteredEntities(excluded) : _allEntities;
        foreach (var e in filteredEntities)
        {
            if (e.Coords == coords)
            {
                return true;
            }
        }
        return false;
    }

    public void BombExploded(Bomb b, List<Vector2Int> coords, BaseEntity triggerEntity = null)
    {
        OnBombExploded?.Invoke(b, coords, triggerEntity);
    }

    public void BombSpawned(Bomb b)
    {
        OnBombSpawned?.Invoke(b);
    }

    public void AddBomber(IBomberEntity bomber)
    {
        OnBombSpawned += bomber.AddedBomb;
        OnBombExploded += bomber.OnBombExploded;
    }

    public void RemoveBomber(IBomberEntity bomber)
    {
        OnBombSpawned -= bomber.AddedBomb;
        OnBombExploded -= bomber.OnBombExploded;
    }
}
