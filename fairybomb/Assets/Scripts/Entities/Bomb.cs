using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bomb : BaseEntity
{
    [SerializeField] SpriteRenderer _view;
    [SerializeField] TextMeshPro _countdown;
    [SerializeField] float _unitsPerBombTick = 1;
    [SerializeField] int _baseRadius = 1;
    [SerializeField] int _explosionTimeoutTicks = 3;
    [SerializeField] int _baseDamage = 1;
    [SerializeField] bool _ignoreBlocks = true;

    int _radius;

    int _ticksLeft;
    float _elapsedSinceLastTick;
    IBomberEntity _owner;
    int _damage;

    public IBomberEntity Owner { get { return _owner; }
        set
        {
            _owner = value;
            name = $"BMB_{((BaseEntity)_owner).name}_{_owner.BombCount}";

        }
    }

    public int Radius => _radius;
    public bool IgnoreBlocks => _ignoreBlocks;
    public int Damage => _damage;

    public override void Init(IEntityController entityController, FairyBombMap map)
    {
        base.Init(entityController, map);
        _radius = _baseRadius;
        _damage = _baseDamage;
        _ticksLeft = _explosionTimeoutTicks;
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
