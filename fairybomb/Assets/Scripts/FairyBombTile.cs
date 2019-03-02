using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType: int
{
    Block = 0,
    Grass,
    Wood
}

public class FairyBombTile : Tile
{
    public TileType TileType => _tile;
    public bool Destructible => _destructible;
    public bool Walkable => _walkable;

    [SerializeField] TileType _tile;
    [SerializeField] bool _destructible;
    [SerializeField] bool _walkable;


#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/FAIRY BOMB/Create Terrain Tile")]
    public static void CreateTerrainTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Terrain Tile", "New TerrainTile", "Asset", "Save Terrain Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<FairyBombTile>(), path);
    }
#endif
}

