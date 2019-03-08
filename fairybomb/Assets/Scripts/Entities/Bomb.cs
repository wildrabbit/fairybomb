using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BombDependencies: BaseEntityDependencies
{
    public IBomberEntity Owner;
}

public class Bomb : BaseEntity
{
    public IBomberEntity Owner { get { return _owner; } }
    public int Radius => _radius;
    public int Damage => _damage;
    public bool IgnoreBlocks => _bombData.IgnoreBlocks;

    public GameObject ExplosionPrefab => _bombData.VFXExplosion;

    public int TurnsLeftToExplosion => _ticksLeft;

    public PaintData PaintData => _bombData.PaintData;

    [SerializeField] TextMeshPro _countdown;

    BombData _bombData;

    int _ticksLeft; // TODO: Consider no-timeout bombs
    float _unitsPerBombTick;
    float _elapsedSinceLastTick;
    IBomberEntity _owner;
    int _damage;
    int _radius;

    protected override void DoInit(BaseEntityDependencies deps)
    {
        BombDependencies bombDeps = ((BombDependencies)deps);
        _owner = bombDeps.Owner;
        name = $"BMB_{((BaseEntity)_owner).name}_{_owner.BomberTrait.TotalBombCount}";

        _bombData = ((BombData)_entityData);
        _radius = _bombData.Radius;
        _damage = _bombData.BaseDamage;
        _ticksLeft = _bombData.Timeout;
        _unitsPerBombTick = _bombData.TickUnits;
        
        _elapsedSinceLastTick = 0.0f;

        _countdown.SetText(_ticksLeft.ToString());
    }

    IEnumerator Explode(BaseEntity triggerEntity = null)
    {
        List<Vector2Int> affectedTiles = _map.GetExplodingCoords(this);
        Active = false;
        _entityController.BombExploded(this, affectedTiles, triggerEntity);

        yield return new WaitForSeconds(0.5f);
        _entityController.DestroyEntity(this);
    }

    public override void OnAdded()
    {
        base.OnAdded();
        _entityController.BombSpawned(this);
        _entityController.OnBombExploded += BombExploded; 
    }

    public override void OnDestroyed()
    {
        base.OnDestroyed();
        _entityController.OnBombExploded -= BombExploded;
    }

    private void BombExploded(Bomb bomb, List<Vector2Int> affectedCoords, BaseEntity triggerEntity = null)
    {
        if(bomb == this)
        {
            return;
        }

        if(!Active)
        {
            return;
        }

        if (affectedCoords.Contains(Coords))
        {
            _elapsedSinceLastTick = 0;
           StartCoroutine(Explode(bomb));
        }
    }

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {
        _elapsedSinceLastTick += timeUnits;
        while (_elapsedSinceLastTick >= _unitsPerBombTick)
        {
            _elapsedSinceLastTick -= _unitsPerBombTick;
            _ticksLeft--;
            _countdown.SetText(_ticksLeft.ToString());
            if (_ticksLeft == 0)
            {
                StartCoroutine(Explode());
            }
        }
    }

}
