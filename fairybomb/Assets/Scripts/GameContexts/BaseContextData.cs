using UnityEngine;
using System.Collections;
using System;

public class BaseContextData
{
    public GameInput input;

    public virtual void Refresh(GameController gameController)
    {
    }
    // TODO: Config, event log, etc
}

public class ActionPhaseData: BaseContextData
{
    public IEntityController EntityController;
    public Player Player;
    public FairyBombMap Map;
    public bool BumpingWallsWillSpendTurn;
    public GameEventLog Log;
    public int Turns;
    public float TimeUnits;

    public override void Refresh(GameController gameController)
    {
        Turns = gameController.Turns;
        TimeUnits = gameController.TimeUnits;
    }
}
