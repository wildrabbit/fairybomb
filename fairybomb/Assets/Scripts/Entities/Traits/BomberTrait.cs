
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

public delegate void SelectedItemDelegate(int idx, BombInventoryEntry entry, IBomberEntity entity);
public delegate void AddedToInventoryDelegate(int idx, BombInventoryEntry entry, IBomberEntity entity);
public delegate void UsedFromInventoryDelegate(int idx, BombInventoryEntry entry, IBomberEntity entity);
public delegate void DepletedInventoryDelegate(int idx, BombInventoryEntry entry, IBomberEntity entity);
public delegate void DroppedDelegate(int idx, BombInventoryEntry entry, IBomberEntity entity);
public delegate void InitialisedInventoryDelegate(BombInventoryEntry[] inventory, int selected);

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
    public BombInventoryEntry[] Inventory => _inventory;

    BombData _selectedBomb;

    IBomberEntity _owner;
    int _deployedBombLimit;
    int _deployedBombs;

    public event AddedToInventoryDelegate OnAddedToInventory;
    public event DepletedInventoryDelegate OnItemDepleted;
    public event SelectedItemDelegate OnSelectedItem;
    public event UsedFromInventoryDelegate OnUsedItem;
    public event DroppedDelegate OnDroppedItem;
    public event InitialisedInventoryDelegate OnInventoryInitialised;

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
        OnInventoryInitialised?.Invoke(_inventory, _selectedInventoryIdx);
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
        int idx = 0;
        var itemEntry = GetInventoryEntry(item);
        if(itemEntry == null)
        {
            itemEntry = new BombInventoryEntry();
            itemEntry.Bomb = item.Bomb;
            itemEntry.Amount = item.Amount;
            itemEntry.Unlimited = item.Unlimited;
            idx = GetFreeSlot();
            if (idx >= 0)
            {
                _inventory[idx] = itemEntry;
            }
        }
        else
        {
            idx = System.Array.IndexOf(_inventory, itemEntry);
            itemEntry.Amount += item.Amount;
        }
        OnAddedToInventory?.Invoke(idx, itemEntry, _owner);
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

    public BombInventoryEntry DropFromInventory(int inventoryIdx)
    {
        var item = RemoveInventory(inventoryIdx);
        OnDroppedItem?.Invoke(inventoryIdx, item, _owner);
        return item;
    }

    public BombInventoryEntry RemoveInventory(int inventoryIdx)
    {
        BombInventoryEntry entry = _inventory[inventoryIdx];
        _inventory[inventoryIdx] = null;

        if (inventoryIdx == _selectedInventoryIdx)
        {
            RefreshSelection();
        }
        return entry;
    }

    void RefreshSelection()
    {
        for (int i = 0; i < _inventory.Length; ++i)
        {
            if (_inventory[i] != null)
            {
                SelectBomb(i);
                break;
            }
        }
    }

    public void SelectBomb(int inventoryIdx)
    {
        if(_inventory[inventoryIdx] != null)
        {            
            _selectedInventoryIdx = inventoryIdx;
            _selectedBomb = _inventory[_selectedInventoryIdx].Bomb;
            OnSelectedItem?.Invoke(inventoryIdx, _inventory[inventoryIdx], _owner);
        }
    }

    public void UseInventoryItem(int inventoryIdx)
    {
        if(!_inventory[inventoryIdx].Unlimited)
        {
            _inventory[inventoryIdx].Amount--;
            OnUsedItem?.Invoke(inventoryIdx, _inventory[inventoryIdx], _owner);
            if (_inventory[inventoryIdx].Amount == 0)
            {
                OnItemDepleted?.Invoke(inventoryIdx, _inventory[inventoryIdx], _owner);
                RemoveInventory(inventoryIdx);                
            }
        }
    }
}
