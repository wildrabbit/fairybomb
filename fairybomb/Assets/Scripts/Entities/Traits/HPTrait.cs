﻿using System;
using UnityEngine;

public delegate void ExhaustedHP(IHealthTrackingEntity owner);
public delegate void HPChangedDelegate(int newHP, IHealthTrackingEntity Owner);

public class HPTrait
{
    public event ExhaustedHP OnExhaustedHP;
    public event HPChangedDelegate OnPlayerHPChanged;

    public HPTraitData _data;

    public IHealthTrackingEntity  Owner => _owner;
    public int HP => _hp;
    public int MaxHP => _maxHP;
    public bool Regen => _regen;

    IHealthTrackingEntity _owner;

    int _hp;
    int _maxHP;
    float _timeUnitsForHPRefill = 1.0f;
    float _elapsedSinceLastRefill = 0.0f;
    int _regenAmount;
    bool _regen;

    public void Init(IHealthTrackingEntity owner, HPTraitData hpData)
    {
        _data = hpData;
        _owner = owner;
        _maxHP = _data.MaxHP;
        _hp = _data.StartHP;
        _regen = _data.Regen;
        _regenAmount = _data.RegenAmount;
        _timeUnitsForHPRefill = _data.RegenRate;
    }

    public void IncreaseMaxHP(int newMax, bool refillCurrent = false)
    {
        _maxHP = newMax;
        if(refillCurrent)
        {
            _hp = _maxHP;
            OnPlayerHPChanged?.Invoke(_hp, _owner);
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
            hpIncrease += _regenAmount;
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
        OnPlayerHPChanged?.Invoke(_hp, _owner);
    }

    public void Decrease(int delta)
    {
        _hp = Mathf.Clamp(_hp - delta, 0, _maxHP);
        OnPlayerHPChanged?.Invoke(_hp, _owner);
        if (_hp == 0)
        {
            OnExhaustedHP?.Invoke(this.Owner);
        }
    }
}