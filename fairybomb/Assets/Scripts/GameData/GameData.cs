using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GameData", menuName = "FAIRY BOMB/GameData")]
public class GameData: ScriptableObject
{

    public float InputDelay;
    public float DefaultTimescale;
    public bool BumpingWallsWillSpendTurn;

    public PlayerData PlayerData;
    public FairyBombMapData MapData;
    public EntityCreationData EntityCreationData; // Prefabs, pool stuff, etc
    public List<MonsterData> MonsterDataList;
    public List<BombData> BombDataList;

    public List<PaintData> PaintingDataList;
}