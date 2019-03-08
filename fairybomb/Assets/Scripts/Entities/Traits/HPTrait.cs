using System;
using UnityEngine;

public delegate void ExhaustedHP(BaseEntity owner);

public class HPTrait
{
    public event ExhaustedHP OnExhaustedHP;

    public HPTraitData _data;

    public BaseEntity Owner => _owner;
    public int HP => _hp;
    public int MaxHP => _maxHP;
    public bool Regen => _regen;

    BaseEntity _owner;

    int _hp;
    int _maxHP;
    float _timeUnitsForHPRefill = 1.0f;
    float _elapsedSinceLastRefill = 0.0f;
    bool _regen;

    public void Init(BaseEntity owner, HPTraitData hpData)
    {
        _data = hpData;
        _owner = owner;
        _maxHP = _data.MaxHP;
        _hp = _data.StartHP;
        _regen = _data.Regen;
        _timeUnitsForHPRefill = _data.RegenRate;
    }

    public void IncreaseMaxHP(int newMax, bool refillCurrent = false)
    {
        _maxHP = newMax;
        if(refillCurrent)
        {
            _hp = _maxHP;
        }
    }

    public void SetRegen(bool enabled)
    {
        _regen = enabled;
        _elapsedSinceLastRefill = 0.0f;
    }

    public void UpdateRegen(float units)
    {
        _elapsedSinceLastRefill += units;
        int hpIncrease = 0;
        while(_elapsedSinceLastRefill >= _timeUnitsForHPRefill)
        {
            _elapsedSinceLastRefill -= _timeUnitsForHPRefill;
            hpIncrease++;
        }

        if(hpIncrease > 0)
        {
            // TODO: Notify regen??
            Add(hpIncrease);
        }
    }

    public void Add(int delta)
    {
        _hp = Mathf.Clamp(_hp + delta, 0, _maxHP);
        // Notify max HP??
        Debug.Log($"{_owner.name} gains {delta} HP to a total of {_hp}");
    }

    public void Decrease(int delta)
    {
        _hp = Mathf.Clamp(_hp - delta, 0, _maxHP);
        Debug.Log($"{_owner.name} loses {delta} HP to a total of {_hp}");
        if (_hp == 0)
        {
            OnExhaustedHP?.Invoke(this.Owner);
        }
    }
}