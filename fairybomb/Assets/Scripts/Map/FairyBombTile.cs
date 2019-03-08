using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public enum TileType: int
{
    None = -1,
    Block = 0,
    Grass,
    Wood,
    Goal
}
[CreateAssetMenu(fileName = "New FairyBombTile", menuName = "FAIRY BOMB/FairyBombTile")]
public class FairyBombTile : Tile
{
    public TileType TileType => _tile;
    public bool Destructible => _destructible;
    public bool Walkable => _walkable;
    public bool Impassable => _impassable;
    public FairyBombTile ReplacementTile => _destructionReplacement;

    [SerializeField] TileType _tile;
    [SerializeField] bool _destructible;
    [SerializeField] int _hp = 1;
    [SerializeField] bool _walkable;
    [FormerlySerializedAs("_blocksExplosions")][SerializeField] bool _impassable;
    [SerializeField] FairyBombTile _destructionReplacement;
}

