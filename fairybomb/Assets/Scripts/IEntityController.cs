using System;
using System.Collections.Generic;
using UnityEngine;


public delegate void EntitiesAddedDelegate(List<BaseEntity> entities);
public delegate void EntitiesRemovedDelegate(List<BaseEntity> entities);

public delegate void BombDelegate(Bomb bomb);
public delegate void BombDestroyedDelegate(Bomb bomb, List<Vector2Int> coords, BaseEntity trigger);
public delegate void PlayerDestroyedDelegate();
public delegate void MonsterDestroyedDelegate(Monster monster);


public interface IEntityController
{
    event EntitiesAddedDelegate OnEntitiesAdded;
    event EntitiesRemovedDelegate OnEntitiesRemoved;

    event PlayerDestroyedDelegate OnPlayerKilled;
    event MonsterDestroyedDelegate OnMonsterKilled;

    event BombDelegate OnBombSpawned;
    event BombDestroyedDelegate OnBombExploded;

    Player Player { get; }

    void Init(FairyBombMap map, PaintMap paintMap, EntityCreationData creationData);

    Player CreatePlayer(PlayerData data, Vector2Int coords);
    Bomb CreateBomb(BombData data, Vector2Int coords, IBomberEntity Owner);
    Monster CreateMonster(MonsterData data, Vector2Int coords, AIController aiController);
    BombPickableItem CreatePickable(LootItemData lootData, BombData data, Vector2Int coords, int amount, bool unlimited);

    T Create<T>(T prefab, BaseEntityData data, BaseEntityDependencies deps) where T : BaseEntity;

    bool ExistsNearbyEntity(Vector2Int coords, int radius, BaseEntity[] excluded = null);
    bool ExistsEntitiesAt(Vector2Int coords, BaseEntity[] excluded = null); // We could use the former with radius == 0, but with this we skip the distance calculations
    List<BaseEntity> GetEntitiesAt(Vector2Int actionTargetCoords, BaseEntity[] excluded = null);
    void AddNewEntities();
    void DestroyEntity(BaseEntity entity);
    void PurgeEntities();
    void RemovePendingEntities();
    void Cleanup();

    void BombExploded(Bomb bomb, List<Vector2Int> coords, BaseEntity triggerEntity = null);
    void PlayerDestroyed();
    void BombSpawned(Bomb bomb);

    void AddBomber(IBomberEntity bomber);
    void RemoveBomber(IBomberEntity bomber);

    List<Bomb> GetBombs();
    void NotifyMonsterKilled(Monster monster);
}
