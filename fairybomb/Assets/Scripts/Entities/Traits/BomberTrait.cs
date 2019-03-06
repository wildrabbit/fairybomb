
using System;

public class BomberTrait
{
    public BombData SelectedBomb
    {
        get => _selectedBomb;
        set { }
    }

    // Total bombs placed (for naming, stuff)
    public int TotalBombCount { get; set; }
    BombData _selectedBomb;

    IBomberEntity _owner;
    int _deployedBombLimit;
    int _deployedBombs;

    public void Init(IBomberEntity entity, BomberData bomberData)
    {
        _owner = entity;
        _selectedBomb = bomberData.DefaultBombData;
        _deployedBombs = 0;
        _deployedBombLimit = bomberData.DeployedBombLimit;
    }

    public void Cleanup()
    {

    }

    public bool HasBombAvailable()
    {
        return _selectedBomb != null && _deployedBombs < _deployedBombLimit;
    }

    public void AddedBomb(Bomb bomb)
    {
        if(bomb.Owner == this._owner)
        {
            _deployedBombs++;
            TotalBombCount++;
        }
    }

    public void RestoreBomb(Bomb bomb)
    {
        _deployedBombs--;
        if (_deployedBombs < 0) _deployedBombs = 0; // should never happen
    }
}
