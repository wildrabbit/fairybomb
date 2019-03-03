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

    int _radius;

    int _ticksLeft;
    float _elapsedSinceLastTick;
    IBomberEntity _owner;
    int _damage;

    public IBomberEntity Owner { get { return _owner; } set { _owner = value; } }

    public int Radius => _radius;

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
    
    IEnumerator Explode()
    {
        Debug.Log("BOOM");
        yield return new WaitForSeconds(0.5f);
        _entityController.BombExploded(this);
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

    private void BombExploded(Bomb bomb)
    {
        if(bomb == this)
        {
            return;
        }

        int distance = _map.Distance(Coords, bomb.Coords);
        if(distance <= _radius)
        {
            _elapsedSinceLastTick = 0;
            _ticksLeft = 0;
            StartCoroutine(Explode());
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
