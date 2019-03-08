using System;
using System.Collections.Generic;


public interface IHealthTrackingEntity
{
    HPTrait HPTrait { get; }

    bool TakeDamage(int poisonDmg);
}
