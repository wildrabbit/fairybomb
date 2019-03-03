using System;
using UnityEngine;



public abstract class BaseEntity : MonoBehaviour, IScheduledEntity
{
    public Vector2Int Coords {
        get => _coords;
        set
        {
            _coords = value;
            _map.ConstrainCoords(ref _coords);
            Vector2 playerTargetPos = _map.WorldFromCoords(_coords);
            transform.localPosition = playerTargetPos;
        }
    }

    public bool Active
    {
        get => _active;
        set => _active = value;
    }

    protected FairyBombMap _map;
    protected IEntityController _entityController;
    protected Vector2Int _coords;
    protected bool _active;

    public virtual void Init(IEntityController entityController, FairyBombMap map)
    {
        _entityController = entityController;
        _map = map;
    }

    public virtual void Cleanup()
    {
        _active = false;
    }

    public void RefreshCoords()
    {
        _coords = _map.CoordsFromWorld(transform.position);
    }


    public abstract void AddTime(float timeUnits, ref PlayContext playContext);

    public virtual void OnAdded()
    {
        _active = true;
    }

    public virtual void OnDestroyed()
    {
        Cleanup();
    }
}
