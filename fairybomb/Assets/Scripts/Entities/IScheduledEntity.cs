using UnityEngine;
using System.Collections;

public interface IScheduledEntity
{
    void AddTime(float timeUnits, ref PlayContext playContext);
}