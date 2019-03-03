using UnityEngine;
using System.Collections;
using System;

public class BaseContextData
{
    public GameInput input;
    // TODO: Config, event log, etc
}

public class ActionPhaseData: BaseContextData
{
    public Player Player;
    public FairyBombMap Map;
    public bool BumpingWallsWillSpendMoves;
}
