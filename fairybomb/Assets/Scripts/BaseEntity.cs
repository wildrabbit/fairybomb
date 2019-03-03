using UnityEngine;

public abstract class BaseEntity : MonoBehaviour, IScheduledEntity
{
    FairyBombMap _map;

    public virtual void Init(FairyBombMap map)
    {
        _map = map;
    }

    public virtual void Cleanup()
    {

    }


    public abstract void AddTime(float timeUnits, ref PlayContext playContext);
}
