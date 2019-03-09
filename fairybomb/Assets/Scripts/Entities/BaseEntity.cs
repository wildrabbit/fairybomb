using System;
using UnityEngine;

public class BaseEntityDependencies
{
    public Transform ParentNode;
    public IEntityController EntityController;
    public FairyBombMap Map;
    public PaintMap PaintMap;
    public Vector2Int Coords;
}

public class TileBasedEffect
{
    public InGameTile Source;
    public int Elapsed;
    public int Duration;
}

public delegate void EntityMovedDelegate(Vector2Int nextCoords, Vector2 worldPos, BaseEntity entity);

public abstract class BaseEntity : MonoBehaviour, IScheduledEntity
{
    public Vector2Int Coords
    {
        get => _coords;
        set
        {
            _coords = value;
            _map.ConstrainCoords(ref _coords);
            EvaluatePaintMap();
            Vector2 targetPos = _map.WorldFromCoords(_coords);
            transform.position = targetPos;
            OnEntityMoved?.Invoke(_coords, targetPos, this);
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
    protected PaintMap _paintMap;
    protected IEntityController _entityController;
    protected Vector2Int _coords;
    protected bool _active;

    public bool Frozen;

    protected TileBasedEffect _tileEffect = null;

    public event EntityMovedDelegate OnEntityMoved;

    public void Init(BaseEntityData entityData, BaseEntityDependencies deps)
    {
        _entityData = entityData;
        _entityController = deps.EntityController;
        _map = deps.Map;
        _paintMap = deps.PaintMap;
        Frozen = false;
        DoInit(deps);
        Coords = deps.Coords;
        CreateView();
    }

    internal void AppliedPaint(PaintData paintData)
    {
        throw new NotImplementedException();
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

    void OnRemovedPaint()
    {
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

    void EvaluatePaintMap()
    {

    }

    public virtual void SetSpeedRate(float speedRate)
    {
    }

    public virtual void ResetSpeedRate()
    {
    }
}