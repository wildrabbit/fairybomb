using UnityEngine;
using UnityEditor;

public enum PlayContext
{
    Action,
    ActionTargetting,
    Simulating // AI is taking over
}


public interface IPlayContext
{
    PlayContext Update(BaseContextData context, out bool willSpendTime);
}