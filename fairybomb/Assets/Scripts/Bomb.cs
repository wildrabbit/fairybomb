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

    int _ticksLeft;
    float _elapsedSinceLastTick;
    BaseEntity _owner;

    public override void Init(FairyBombMap map)
    {
        base.Init(map);
        _ticksLeft = _explosionTimeoutTicks;
        _elapsedSinceLastTick = 0.0f;
        _countdown.SetText(_ticksLeft.ToString());
    }

    public void SetOwner(BaseEntity owner)
    {
        _owner = owner;
    }
    
    IEnumerator Explode()
    {
        Debug.Log("BOOM");
        yield return new WaitForSeconds(0.5f);
        GameObject.Destroy(this.gameObject);
    }

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {
        _elapsedSinceLastTick += timeUnits;
        while (_elapsedSinceLastTick > _unitsPerBombTick)
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
