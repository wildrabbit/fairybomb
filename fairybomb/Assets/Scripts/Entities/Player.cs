using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BombWalkabilityType
{
    Block,
    CrossOwnBombs,
    CrossAny
}

public enum BombImmunityType
{
    AnyBombs,
    OwnBombs,
    NoBombs
}

public class Player : BaseEntity, IBomberEntity
{
    public BombData SelectedBomb
    {
        get => _selectedBomb;
        set {}
    }
    // Total bombs placed (for naming, stuff)
    public int BombCount { get; set; }

    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;
    public float Speed => _speed;

    float _speed;
    
    BombData _selectedBomb;

    BombImmunityType _bombImmunity;
    BombWalkabilityType _walkOverBombs;

    int _deployedBombLimit;
    int _deployedBombs;
    HPTrait _hpTrait;
    //...whatever

    PlayerData _playerData;

    protected override void DoInit(BaseEntityDependencies deps)
    {
        _playerData = ((PlayerData)_entityData);

        name = "Player";
        BombCount = 0;
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, _playerData.HPData);
        _deployedBombLimit = _playerData.BomberData.DeployedBombLimit;
        _selectedBomb = _playerData.BomberData.DefaultBombData;
        _bombImmunity = _playerData.MobilityData.BombImmunity;
        _walkOverBombs = _playerData.MobilityData.BombWalkability;
        _deployedBombs = 0;
        _speed = _playerData.Speed;
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
        return _selectedBomb != null && _deployedBombs < _deployedBombLimit;
    }

    public void AddedBomb(Bomb bomb)
    {
        _deployedBombs++;
        BombCount++;
    }

    public void OnBombExploded(Bomb bomb, List<Vector2Int> coords, BaseEntity triggerEntity)
    {
#pragma warning disable CS0252 // Involuntary reference comparison (What I DO want)
        bool isOwnBomb = (bomb.Owner == this);
#pragma warning restore CS0252

        if (isOwnBomb)
        {
            _deployedBombs--;
        }
        if(coords.Contains(Coords))
        {
            if(_bombImmunity == BombImmunityType.NoBombs || (_bombImmunity == BombImmunityType.OwnBombs && !isOwnBomb))
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
