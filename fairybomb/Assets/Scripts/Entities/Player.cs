using UnityEngine;
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

public class Player : BaseEntity, IBattleEntity, IBomberEntity
{
    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;
    public float Speed => _speed;

    public BomberTrait BomberTrait => _bomberTrait;

    public BombWalkabilityType BombWalkability => _walkOverBombs;

    int IBattleEntity.HP => HP;
    int IBattleEntity.Damage => 0;
    string IBattleEntity.Name => name;

    
    BombImmunityType _bombImmunity;
    BombWalkabilityType _walkOverBombs;
    float _speed;

    PlayerData _playerData;
    HPTrait _hpTrait;
    BomberTrait _bomberTrait;


    protected override void DoInit(BaseEntityDependencies deps)
    {
        _playerData = ((PlayerData)_entityData);

        name = "Player";
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, _playerData.HPData);
        _bomberTrait = new BomberTrait();
        _bomberTrait.Init(this, _playerData.BomberData);
        _bombImmunity = _playerData.MobilityData.BombImmunity;
        _walkOverBombs = _playerData.MobilityData.BombWalkability;
        _speed = _playerData.Speed;
    }

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {
        if (_hpTrait.Regen)
        {
            _hpTrait.UpdateRegen(timeUnits);
        }
    }

    public void OnBombExploded(Bomb bomb, List<Vector2Int> coords, BaseEntity triggerEntity)
    {
#pragma warning disable CS0252 // Involuntary reference comparison (What I DO want)
        bool isOwnBomb = (bomb.Owner == this);
#pragma warning restore CS0252

        if (isOwnBomb)
        {
            _bomberTrait.RestoreBomb(bomb);
        }
        if(coords.Contains(Coords))
        {
            if (!IsImmuneTo(bomb) && coords.Contains(Coords))
            {
                TakeDamage(bomb.Damage);
            }
        }
    }

    private bool IsImmuneTo(Bomb bomb)
    {
        bool isOwnBomb = (bomb.Owner == this);
        return (_bombImmunity == BombImmunityType.AnyBombs || (_bombImmunity == BombImmunityType.OwnBombs && isOwnBomb));
    }

    public bool TakeDamage(int damage)
    {
        _hpTrait.Decrease(damage);
        Debug.Log($"Player took {damage} damage!. Current HP: {HP}");
        if (HP == 0)
        {
            _entityController.DestroyEntity(this);
            return true;
        }
        return false;
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

    void IBattleEntity.ApplyBattleResults(BattleActionResult results, BattleRole role)
    {
        if(role == BattleRole.Defender)
        {
            TakeDamage(results.DefenderDmgTaken);
        }
        else
        {
            Debug.Log($"{name} attacked {results.DefenderName} and caused {results.AttackerDmgInflicted} dmg");
        }
    }
}
