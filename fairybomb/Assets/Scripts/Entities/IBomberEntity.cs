using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBomberEntity
{
    // TODO: This is currently a prefab Replace with bomb config.
    BombData SelectedBomb { get; set; }
    int BombCount { get; set; }
    void AddedBomb(Bomb bomb);
    bool HasBombAvailable();
    void OnBombExploded(Bomb bomb, List<Vector2Int> coords, BaseEntity triggerEntity);
}
