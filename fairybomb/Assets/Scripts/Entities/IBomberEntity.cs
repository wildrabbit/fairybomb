using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBomberEntity
{
    BomberTrait BomberTrait { get; }

    void OnBombExploded(Bomb bomb, List<Vector2Int> coords, BaseEntity trigger);
}
