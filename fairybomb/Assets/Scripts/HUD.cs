using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public delegate int TurnsGetterDelegate();
public delegate float TimeGetterDelegate();

public class HUD : MonoBehaviour
{
    [SerializeField] Canvas _canvas;
    [SerializeField] float _minLogDisplayTime = 0.7f;

    [SerializeField] TextMeshProUGUI _logMessage;
    [SerializeField] TextMeshProUGUI _hpValue;
    [SerializeField] TextMeshProUGUI _turnCountValue;
    [SerializeField] TextMeshProUGUI _timeUnitsValue;
    [SerializeField] TextMeshProUGUI _mapPosValue;

    Queue<BaseEvent> _displayPendingEvents;
    float _lastDisplayed;

    GameEventLog _logger;
    Player _player;

    TurnsGetterDelegate _turnsGetter;
    TimeGetterDelegate _timeGetter;

    public void Init(GameEventLog logger, Player player, TurnsGetterDelegate turnsGetter, TimeGetterDelegate timeGetter, Camera camera)
    {
        _canvas.worldCamera = camera;
        _logger = logger;
        _logger.OnEventAdded += UpdateLog;
        _displayPendingEvents = new Queue<BaseEvent>();
        _lastDisplayed = -1;

        _turnsGetter = turnsGetter;
        _timeGetter = timeGetter;

        _player = player;

        SetLogText("");
        _hpValue.SetText($"{_player.HP}/{_player.MaxHP}");
        _turnCountValue.SetText(_turnsGetter().ToString());
        _timeUnitsValue.SetText(_timeGetter().ToString());
        _mapPosValue.SetText(_player.Coords.ToString());
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
        if(_displayPendingEvents.Count == 0)
        {
            SetLogText(lastAdded.Message());
            _lastDisplayed = Time.time;
        }
        _displayPendingEvents.Enqueue(lastAdded);
    }

    void Update()
    {
        if(Time.time - _lastDisplayed >= _minLogDisplayTime)
        {
            if(_displayPendingEvents.Count > 0)
            {
                _displayPendingEvents.Dequeue();
                if(_displayPendingEvents.Count > 0)
                {
                    SetLogText(_displayPendingEvents.Peek().Message());
                    _lastDisplayed = Time.time;
                }                
            }
        }

        // TODO: Replace with events
        _hpValue.SetText($"{_player.HP}/{_player.MaxHP}");
        _turnCountValue.SetText(_turnsGetter().ToString());
        _timeUnitsValue.SetText(_timeGetter().ToString());
        _mapPosValue.SetText(_player.Coords.ToString());
    }
}
