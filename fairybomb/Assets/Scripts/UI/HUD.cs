using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public delegate int TurnsGetterDelegate();
public delegate float TimeGetterDelegate();

public class HUD : MonoBehaviour
{
    [SerializeField] GameObject _winScreen;
    [SerializeField] GameObject _loseScreen;

    [SerializeField] HUDInventoryItem _hudInventoryItemPrefab;

    [SerializeField] Canvas _canvas;
    [SerializeField] float _minLogDisplayTime = 0.7f;

    [SerializeField] int _lastMessagesToDisplay = 3;
    [SerializeField] TextMeshProUGUI _logMessage;
    [SerializeField] TextMeshProUGUI _hpValue;
    [SerializeField] TextMeshProUGUI _turnCountValue;
    [SerializeField] TextMeshProUGUI _timeUnitsValue;
    [SerializeField] TextMeshProUGUI _mapPosValue;
    [SerializeField] TextMeshProUGUI _layoutLabel;
    [SerializeField] TextMeshProUGUI _layoutCycleLabel;

    // TODO
    [SerializeField] RectTransform _statusRoot;
    [SerializeField] TextMeshProUGUI _statusLabel;
    [SerializeField] Image _statusHighlight;

    [SerializeField] RectTransform _inventoryRoot;

    HUDInventoryItem[] _inventoryItems;
    HUDInventoryItem _selected;
    
    GameEventLog _logger;
    Player _player;

    TurnsGetterDelegate _turnsGetter;
    TimeGetterDelegate _timeGetter;

    public void Init(GameEventLog logger, Player player, TurnsGetterDelegate turnsGetter, TimeGetterDelegate timeGetter, Camera camera)
    {
        _canvas.worldCamera = camera;
        _logger = logger;
        _logger.OnEventAdded += UpdateLog;

        _turnsGetter = turnsGetter;
        _timeGetter = timeGetter;

        _player = player;

        SetLogText("");
        _hpValue.SetText($"{_player.HP}/{_player.MaxHP}");
        _turnCountValue.SetText(_turnsGetter().ToString());
        _timeUnitsValue.SetText(_timeGetter().ToString());
        _mapPosValue.SetText(_player.Coords.ToString());

        InitInventory(_player.BomberTrait.Inventory, _player.BomberTrait.SelectedIdx);
    }

    void SetLogText(string msg)
    {
        _logMessage.SetText(msg);
    }

    public void Cleanup()
    {
        _logger.OnEventAdded -= UpdateLog;
    }

    private void UpdateLog(BaseEvent lastAdded)
    {
        List<string> entries = _logger.GetLastItemMessages(_lastMessagesToDisplay);
        string entryMessage = string.Join("", entries);
        SetLogText(entryMessage);
    }

    void Update()
    {
        
        // TODO: Replace with events
        _hpValue.SetText($"{_player.HP}/{_player.MaxHP}");
        _turnCountValue.SetText(_turnsGetter().ToString());
        _timeUnitsValue.SetText(_timeGetter().ToString());
        _mapPosValue.SetText(_player.Coords.ToString());
    }

    public void AddedToInventory(int idx, BombInventoryEntry entry, IBomberEntity entity)
    {
        _inventoryItems[idx].SetEntry(entry);
    }

    public void DroppedItem(int idx, BombInventoryEntry entry, IBomberEntity entity)
    {
        _inventoryItems[idx].SetEntry(null);
    }

    public void InitInventory(BombInventoryEntry[] inventory, int selected)
    {
        int inventorySize = inventory.Length;
        _inventoryItems = new HUDInventoryItem[inventorySize];
        int idx = 0;
        foreach(var itemEntry in inventory)
        {
            HUDInventoryItem item = Instantiate<HUDInventoryItem>(_hudInventoryItemPrefab, _inventoryRoot);
            _inventoryItems[idx] = item;
            item.Init(idx);
            item.SetEntry(itemEntry);

            if (idx == selected)
            {
                item.Select();
                _selected = item;
            }
            else
            {
                item.Deselect();
            }
            idx++;
        }
    }

    public void DepletedItem(int idx, BombInventoryEntry entry, IBomberEntity entity)
    {
        _inventoryItems[idx].SetEntry(null);
    }

    public void SelectedItem(int idx, BombInventoryEntry entry, IBomberEntity entity)
    {
        if(_selected != null && _selected != _inventoryItems[idx])
        {
            _selected.Deselect();
        }
        _inventoryItems[idx].Select();
        _selected = _inventoryItems[idx];
    }

    public void UsedItem(int idx, BombInventoryEntry entry, IBomberEntity entity)
    {
        _inventoryItems[idx].UpdateItem(entry);
    }

    public void AppliedPaint(PaintData data, IPaintableEntity entity)
    {
        string statusText = GetEffectName(data.Effect);

        _statusRoot.gameObject.SetActive(!string.IsNullOrEmpty(statusText));

        _statusHighlight.color = data.Colour;
        _statusLabel.text = statusText;
    }

    public void RemovedPaint(PaintData data, IPaintableEntity entity)
    {
        _statusRoot.gameObject.SetActive(false);
    }

    public string GetEffectName(PaintingEffect effect)
    {
        switch (effect)
        {
            case PaintingEffect.Poison:
                return "POISON";                
            case PaintingEffect.Heal:
                return "HEAL";
            case PaintingEffect.Freeze:
                return "FROZEN";
            case PaintingEffect.Haste:
                return "HASTE";
            case PaintingEffect.Slow:
                return "SLOW";
            default:
                return string.Empty;
        }

    }

    public void OnInputLayoutChanged(int layout)
    {
        if(layout == GameInput.kLayoutQwerty)
        {
            _layoutLabel.text = "MOVE: QWEASD";
            _layoutCycleLabel.text = "(Switch to AZERTY: TAB)";
        }
        else if (layout == GameInput.kLayoutAzerty)
        {
            _layoutLabel.text = "MOVE: AZEQSD";
            _layoutCycleLabel.text = "(Switch to QWERTY: TAB)";
        }
    }
}
