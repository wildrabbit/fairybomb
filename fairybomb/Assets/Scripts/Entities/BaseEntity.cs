using System;
using UnityEngine;

public class BaseEntityDependencies
{
    public Transform ParentNode;
    public IEntityController EntityController;
    public FairyBombMap Map;
    public Vector2Int Coords;
}

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

    protected Transform _view;
    protected BaseEntityData _entityData;
    protected FairyBombMap _map;
    protected IEntityController _entityController;
    protected Vector2Int _coords;
    protected bool _active;

    public void Init(BaseEntityData entityData, BaseEntityDependencies deps)
    {
        _entityData = entityData;
        _entityController = deps.EntityController;
        _map = deps.Map;
        DoInit(deps);
        Coords = deps.Coords;
        CreateView();
    }

    protected abstract void DoInit(BaseEntityDependencies dependencies);

    public virtual void CreateView()
    {
        _view = Instantiate(_entityData.DefaultViewPrefab, transform, false);
        _view.localPosition = Vector3.zero;
        _view.localScale = Vector3.one;
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
