using System;
using System.Collections.Generic;
using UnityEngine;


public delegate void EntitiesAddedDelegate(List<BaseEntity> entities);
public delegate void EntitiesRemovedDelegate(List<BaseEntity> entities);

public delegate void BombDelegate(Bomb bomb);
public delegate void PlayerDestroyedDelegate();


public interface IEntityController
{
    event EntitiesAddedDelegate OnEntitiesAdded;
    event EntitiesRemovedDelegate OnEntitiesRemoved;

    event PlayerDestroyedDelegate OnPlayerKilled;

    event BombDelegate OnBombSpawned;
    event BombDelegate OnBombExploded;

    Player Player { get; }

    void Init(FairyBombMap map);

    Player CreatePlayer(Player prefab, Vector2Int coords);
    Bomb CreateBomb(Bomb prefab, IBomberEntity owner, Vector2Int coords);

    T Create<T>(T prefab, Vector2Int coords) where T : BaseEntity;

    bool ExistsNearbyEntity(Vector2Int coords, int radius, BaseEntity[] excluded = null);
    bool ExistsEntitiesAt(Vector2Int coords, BaseEntity[] excluded = null); // We could use the former with radius == 0, but with this we skip the distance calculations
    List<BaseEntity> GetEntitiesAt(Vector2Int actionTargetCoords, BaseEntity[] excluded = null);
    void AddNewEntities();
    void DestroyEntity(BaseEntity entity);
    void PurgeEntities();
    void RemovePendingEntities();
    void Cleanup();

    void BombExploded(Bomb bomb);
    void PlayerDestroyed();
    void BombSpawned(Bomb bomb);

    void AddBomber(IBomberEntity bomber);
    void RemoveBomber(IBomberEntity bomber);
}
