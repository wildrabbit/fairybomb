using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType: int
{
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
    public bool BlocksExplosions => _blocksExplosions;
    public FairyBombTile ReplacementTile => _destructionReplacement;

    [SerializeField] TileType _tile;
    [SerializeField] bool _destructible;
    [SerializeField] int _hp = 1;
    [SerializeField] bool _walkable;
    [SerializeField] bool _blocksExplosions;
    [SerializeField] FairyBombTile _destructionReplacement;
}

