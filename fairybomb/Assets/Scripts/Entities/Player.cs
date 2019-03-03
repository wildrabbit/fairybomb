using UnityEngine;
using System.Collections;


public enum BombWalkabilityType
{
    Block,
    CrossOwnBombs,
    CrossAny
}

public enum BombImmunityType
{
    AnyBombs,
    EnemyBombs,
    NoBombs
}

public class Player : BaseEntity, IBomberEntity
{
    public Bomb SelectedBomb
    {
        get => _bombPrefab;
        set {}
    }

    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;

    [SerializeField] SpriteRenderer _view;
    [Header("Config")]
    [SerializeField] int DeployedBombLimit = 1;
    public BombWalkabilityType BombWalkability = BombWalkabilityType.Block;
    public BombImmunityType BombImmunity = BombImmunityType.AnyBombs;
    public float Speed = 1.0f;
    public int StartHP = 3;

    public Bomb _bombPrefab;
    
    int _deployedBombs;
    HPTrait _hpTrait;
    //...whatever

    public override void Init(IEntityController entityController, FairyBombMap map)
    {
        base.Init(entityController, map);
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, StartHP, false);
        _deployedBombs = 0;
    }

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {
        if (_hpTrait.Regen)
        {
            _hpTrait.UpdateRegen(timeUnits);
        }
    }

    public bool HasBombAvailable()
    {
        return _bombPrefab != null && _deployedBombs < DeployedBombLimit;
    }

    public void AddedBomb(Bomb bomb)
    {
        _deployedBombs++;
    }

    public void OnBombExploded(Bomb bomb)
    {
#pragma warning disable CS0252 // Involuntary reference comparison (What I DO want)
        bool isOwnBomb = (bomb.Owner == this);
#pragma warning restore CS0252

        if (isOwnBomb)
        {
            _deployedBombs--;
        }
        int explosionDistance = _map.Distance(Coords, bomb.Coords);
        if(explosionDistance <= bomb.Radius)
        {
            if(BombImmunity == BombImmunityType.AnyBombs || (BombImmunity == BombImmunityType.EnemyBombs && !isOwnBomb))
            {
                _hpTrait.Decrease(bomb.Damage);
                Debug.Log($"Player took {bomb.Damage} damage!. Current HP: {HP}");
                if (HP == 0)
                {
                    _entityController.DestroyEntity(this);
                }
            }
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
    }

    public override void OnAdded()
    {
        base.OnAdded();
       _entityController.AddBomber(this);
    }


    public override void OnDestroyed()
    {
        base.OnDestroyed();
        _entityController.RemoveBomber(this);
        _entityController.PlayerDestroyed();
    }
}
