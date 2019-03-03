using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBomberEntity
{
    // TODO: This is currently a prefab Replace with bomb config.
    Bomb SelectedBomb { get; set; }
    void AddedBomb(Bomb bomb);
    bool HasBombAvailable();
    void OnBombExploded(Bomb bomb);
}
