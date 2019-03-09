using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseMapContext
{
    public BaseMapGeneratorData GeneratorData;
}

public interface IMapGenerator
{
    void GenerateMap(ref TileType[] map, BaseMapContext mapGenContext);
}
