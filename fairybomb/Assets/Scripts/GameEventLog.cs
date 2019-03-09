using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum EventCategory
{
    GameSetup,
    PlayerAction,
    Information,
    GameEnd
}

public abstract class BaseEvent
{
    public abstract EventCategory Category { get; }
    public abstract string Message();

    public int Turns;
    public float Time;

    public BaseEvent(int turns, float timeUnits)
    {
        Turns = turns;
        Time = timeUnits;
    }

    public string PrintTime()
    {
        return $"<color=blue>[T:{Turns}, TU: {Time}]</color> ";
    }

}

public class GameSetupEvent: BaseEvent
{
    public override EventCategory Category => EventCategory.GameSetup;
    public int Seed; // Random stuff (TODO)

    public Vector2Int PlayerCoords;
    public int HP;
    public int MaxHP;
    public Vector2Int MapSize;
    public int[] MapTiles;

    public GameSetupEvent()
        :base(0, 0)
    {
    }
    public override string Message()
    {
        StringBuilder b = new StringBuilder("=====Welcome to the Dungeon Garden, Berry. Find the exit!\n");
        return b.ToString();
    }
}

public class GameFinishedEvent : BaseEvent
{
    public override EventCategory Category => EventCategory.GameEnd;
    public GameResult Result;
    public GameFinishedEvent(int turns, float time, GameResult result)
        :base(turns, time)
    {
        Result = result;
    }

    public override string Message()
    {
        StringBuilder builder = new StringBuilder(PrintTime());
        if(Result == GameResult.Won)
        {
            builder.Append("Congratulations, you managed to escape from the Dungeon Garden!");
        }
        else if (Result == GameResult.Lost)
        {
            builder.Append("You've died.");
        }
        builder.AppendLine("Tap any key to restart");
        return builder.ToString();
    }
}


public class PlayerActionEvent: BaseEvent
{
    enum ActionType
    {
        Movement,
        Idle,
        BombPlacement,
        BombDetonation,
        // TODO: Grabbing loot, equipping, consuming, etc
    }

    public override EventCategory Category => EventCategory.PlayerAction;
    ActionType PlayerActionType;
    public MoveDirection PlayerMoveDirection;
    public Vector2Int EventCoords;

    public PlayerActionEvent(int turns, float time)
        :base(turns, time)
    {

    }

    public void SetMovement(MoveDirection moveDir, Vector2Int coords)
    {
        PlayerActionType = ActionType.Movement;
        PlayerMoveDirection = moveDir;
        EventCoords = coords;
    }

    public void SetIdle()
    {
        PlayerActionType = ActionType.Idle;
    }

    public void SetBomb(Vector2Int coords)
    {
        PlayerActionType = ActionType.BombPlacement;
        EventCoords = coords;
    }

    public override string Message()
    {
        StringBuilder builder = new StringBuilder(PrintTime());
        switch(PlayerActionType)
        {
            case ActionType.Movement:
            {
                builder.AppendLine($"Player moves <b>{PlayerMoveDirection}</b> to <b>{EventCoords}</b>");
                break;
            }
            case ActionType.Idle:
            {
                builder.AppendLine($"Player remains idle and contemplates her own existence");
                break;
            }
            case ActionType.BombPlacement:
            {
                builder.AppendLine($"Player places bomb @ <b>{EventCoords}</b>");
                break;
            }
        }
        return builder.ToString();
    }
}


public class BombActionEvent : BaseEvent
{
    enum ActionType
    {
        BombSpawned,
        BombTimesOut,
        BombChainExplosion
        // Detonated by owner 
    }
    public override EventCategory Category => EventCategory.Information;

    public BombActionEvent(int turns, float timeUnits)
        :base(turns, timeUnits)
    {

    }

    public Vector2Int Coords;
    public string Name;
    public string DetonatorName;
    public Vector2Int DetonatorCoords;

    ActionType BombActionType;
    public void SetSpawned(Bomb bomb)
    {
        BombActionType = ActionType.BombSpawned;
        Name = bomb.name;
        Coords = bomb.Coords;
    }

    public void SetTimedOut(Bomb bomb)
    {
        BombActionType = ActionType.BombTimesOut;
        Name = bomb.name;
        Coords = bomb.Coords;
    }

    public void SetChainExplosion(Bomb exploding, Bomb detonator)
    {
        BombActionType = ActionType.BombChainExplosion;
        Name = exploding.name;
        Coords = exploding.Coords;
        DetonatorName = detonator.name;
        DetonatorCoords = detonator.Coords;
    }

    public override string Message()
    {
        StringBuilder builder = new StringBuilder(PrintTime());
        switch (BombActionType)
        {
            case ActionType.BombSpawned:
                builder.AppendLine($"<b>{Name}</b> Spawns @ <b>{Coords}</b>");
                break;
            case ActionType.BombTimesOut:
                builder.AppendLine($"<b>{Name}</b> Times out and explodes");
                break;
            case ActionType.BombChainExplosion:
                builder.AppendLine($"<b>{DetonatorName}</b> @ <b>{DetonatorCoords}</b> Causes a chain reaction and makes <b>{Name}</b> explode");
                break;
        }
        return builder.ToString();
    }
}

public delegate void EventAddedDelegate(BaseEvent lastAdded);
public delegate void SessionStartedDelegate();
public delegate void SessionFinishedDelegate();

public class GameEventLog
{
    public event EventAddedDelegate OnEventAdded;
    public event SessionStartedDelegate OnSessionStarted;
    public event SessionFinishedDelegate OnSessionFinished;


    List<BaseEvent> _events;

    public void Init()
    {
        _events = new List<BaseEvent>();
    }

    public void StartSession(GameSetupEvent evt)
    {
        Clear();
        OnSessionStarted?.Invoke();
        AddEvent(evt);
    }

    public void AddEvent(BaseEvent evt)
    {
        _events.Add(evt);
        OnEventAdded?.Invoke(evt);
    }

    public void EndSession(GameFinishedEvent evt)
    {
        AddEvent(evt);
        OnSessionFinished?.Invoke();
        Debug.Log(Flush());
    }

    public void Clear()
    {
        _events.Clear();
    }

    public string Flush()
    {
        StringBuilder builder = new StringBuilder();
        foreach(var evt in _events)
        {
            builder.Append(evt.Message());
        }
        return builder.ToString();
    }

    public List<string> GetLastItemMessages(int lastMessagesToDisplay)
    {
        List<string> messages = new List<string>();
        int positionIdx = Mathf.Max(0, _events.Count - lastMessagesToDisplay);
        for(int i = positionIdx; i < _events.Count; ++i)
        {
            messages.Add(_events[i].Message());
        }
        return messages;
    }
}
