
using System;
using System.Collections.Generic;

[System.Serializable]
public class BombInventoryEntry
{
    public BombData Bomb;
    public int Amount;
    public bool Unlimited;
}

public class BomberTrait
{
    public BombData SelectedBomb
    {
        get => _selectedBomb;
        set { }
    }

    public bool FirstItemFixed => _firstItemFixed;

    BombInventoryEntry[] _inventory;
    int _invSize;
    int _selectedInventoryIdx;
    bool _firstItemFixed;
    // Total bombs placed (for naming, stuff)
    public int TotalBombCount { get; set; }
    BombData _selectedBomb;

    IBomberEntity _owner;
    int _deployedBombLimit;
    int _deployedBombs;

    public void Init(IBomberEntity entity, BomberData bomberData)
    {
        _owner = entity;

        InitInventory(bomberData);
        _deployedBombs = 0;
        _deployedBombLimit = bomberData.DefaultDeployLimit;
    }

    void InitInventory(BomberData data)
    {
        _inventory = new BombInventoryEntry[data.InventorySize];
        _firstItemFixed = data.IsFirstItemFixed;
        int idx = 0;
        foreach(var dataItem in data.InventoryEntry)
        {
            if(dataItem != null && dataItem.Bomb != null)
            {
                BombInventoryEntry item = new BombInventoryEntry();
                item.Bomb = dataItem.Bomb;
                item.Unlimited = dataItem.Unlimited;
                item.Amount = dataItem.Amount;
                _inventory[idx] = item;
            }
            else
            {
                _inventory[idx] = null;
            }
            idx++;
        }

        _selectedInventoryIdx = data.DefaultSelection;
        _selectedBomb = _inventory[_selectedInventoryIdx].Bomb;
        // TODO: Notify inventory update?
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

    public void AddToInventory(BombPickableItem item)
    {
        BombInventoryEntry entry = new BombInventoryEntry();
        entry.Bomb = item.Bomb;
        entry.Amount = item.Amount;
        entry.Unlimited = item.Unlimited;
    }

    public bool HasItemAt(int inventoryIdx)
    {
        return _inventory[inventoryIdx] != null;
    }

    public bool HasItem(BombPickableItem ground)
    {
        foreach(var itemEntry in _inventory)
        {
            if (itemEntry != null && itemEntry.Bomb == ground.Bomb)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasFreeSlot(List<BombPickableItem> itemsToPick)
    {
        int numItems = itemsToPick.FindAll(x => !HasItem(x)).Count;
        int freeSlots = System.Array.FindAll(_inventory, x => x == null).Length;
        return freeSlots > numItems;
    }

    public BombInventoryEntry RemoveInventory(int inventoryIdx)
    {
        // TODO: Notify deletion
        BombInventoryEntry entry = _inventory[inventoryIdx];
        _inventory[inventoryIdx] = null;
        return entry;
    }

    public void SelectBomb(int inventoryIdx)
    {
        if(_inventory[inventoryIdx] != null)
        {
            _selectedInventoryIdx = inventoryIdx;
        }
    }

    public void UseInventoryItem(int idx)
    {
        if(!_inventory[idx].Unlimited)
        {
            _inventory[idx].Amount--;
            // TODO: Notify amount change
            if (_inventory[idx].Amount == 0)
            {
                RemoveInventory(idx);
            }
        }
    }
}
