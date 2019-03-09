using UnityEngine;
using System.Collections;

public class BombPickableDependencies: BaseEntityDependencies
{
    public BombData Bomb;
    public int Amount;
    public bool Unlimited;
}

public class BombPickableItem : BaseEntity
{
    BombData _data;
    int _amount;
    bool _unlimited;

    public BombData Bomb => _data;
    public int Amount => _amount;
    public bool Unlimited => _unlimited;

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {}

    protected override void DoInit(BaseEntityDependencies dependencies)
    {
        BombPickableDependencies bombDeps = ((BombPickableDependencies)dependencies);
        _data = bombDeps.Bomb;
        _amount = bombDeps.Amount;
        _unlimited = bombDeps.Unlimited;
    }

    public override void CreateView()
    {
        _view = Instantiate(_data.LootViewPrefab, transform, false);
        _view.localPosition = Vector3.zero;
        _view.localScale = Vector3.one;
    }
}
