using System;
using System.Collections.Generic;

public enum BattleRole
{
    Attacker,
    Defender
}

public interface IBattleEntity
{
    int HP { get; }
    int Damage { get; }
    string Name { get; }
    void ApplyBattleResults(BattleActionResult results, BattleRole role);
    bool TakeDamage(int inflicted);
}
