
using System;
using System.Collections.Generic;
using UnityEngine;

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

    public int SelectedIdx => _selectedInventoryIdx;

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
        var itemEntry = GetInventoryEntry(item);
        if(itemEntry == null)
        {
            BombInventoryEntry entry = new BombInventoryEntry();
            entry.Bomb = item.Bomb;
            entry.Amount = item.Amount;
            entry.Unlimited = item.Unlimited;
            int idx = GetFreeSlot();
            if (idx >= 0)
            {
                Debug.Log($"Added {entry.Bomb.name}  at slot {idx}");
                _inventory[idx] = entry;
            }
        }
        else
        {
            int idx = System.Array.IndexOf(_inventory, itemEntry);

            Debug.Log($"Increased slot {idx} ({itemEntry.Bomb.name})  by {item.Amount} units");
            itemEntry.Amount += item.Amount;
        }
    }

    public int GetFreeSlot()
    {
        for(int i = 0; i < _inventory.Length; ++i)
        {
            if(_inventory[i] == null)
            {
                return i;
            }
        }
        return -1;
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

    public BombInventoryEntry GetInventoryEntry(BombPickableItem item)
    {
        foreach (var itemEntry in _inventory)
        {
            if (itemEntry != null && itemEntry.Bomb == item.Bomb)
            {
                return itemEntry;
            }
        }
        return null;
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
        Debug.Log($"Removed item [{inventoryIdx}] - {entry.Bomb.name} x{entry.Amount}");
        _inventory[inventoryIdx] = null;

        if(inventoryIdx == _selectedInventoryIdx)
        {
            for(int i = 0; i < _inventory.Length; ++i)
            {
                if(_inventory[i] != null)
                {
                    SelectBomb(i);
                    break;
                }
            }
        }
        return entry;
    }

    public void SelectBomb(int inventoryIdx)
    {
        if(_inventory[inventoryIdx] != null)
        {
            Debug.Log($"Selected item [{inventoryIdx}] - {_inventory[inventoryIdx].Bomb.name} x{_inventory[inventoryIdx].Amount}");
            _selectedInventoryIdx = inventoryIdx;
            _selectedBomb = _inventory[_selectedInventoryIdx].Bomb;
        }
    }

    public void UseInventoryItem(int inventoryIdx)
    {
        if(!_inventory[inventoryIdx].Unlimited)
        {
            _inventory[inventoryIdx].Amount--;
            Debug.Log($"Consumed item [{inventoryIdx}] - {_inventory[inventoryIdx].Bomb.name} x{_inventory[inventoryIdx].Amount}");
            // TODO: Notify amount change
            if (_inventory[inventoryIdx].Amount == 0)
            {
                RemoveInventory(inventoryIdx);
            }
        }
    }
}
