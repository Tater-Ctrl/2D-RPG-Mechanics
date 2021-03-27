using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseBuilding : MonoBehaviour
{
    bool enableBuilding;
    public Tilemap tilemap;
    public Tilemap houseTilemap;
    Camera cam;

    Inventory inventory;
    TerrainType[] regions;
    public TileBase[] floorTiles;
    public TileBase[] wallTiles;
    public TileBase[] doorTiles;
    public TileBase[] roofTiles;

    public TileBase whiteTile;

    // Start is called before the first frame update
    void Start()
    {
        cam = (Camera)FindObjectOfType(typeof(Camera));
        inventory = GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        if(EnableBuildingMode.buildingMode) {
            if(Input.GetMouseButtonDown(0)) {
                Vector3Int pos = new Vector3Int(Mathf.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x), Mathf.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y), 0);
                if(inventory.hotbarSlots[inventory.index].tile) {
                    tilemap.SetTile(pos, inventory.hotbarSlots[inventory.index].tile);
                    TileBase tileRef = tilemap.GetTile(pos);

                    for(int i = 0; i < floorTiles.Length; i++) {
                        if(tileRef == floorTiles[i]) {
                            PlaceTile(new Vector2Int(pos.x, pos.y));
                            return;
                        }
                    }
                    for(int i = 0; i < wallTiles.Length; i++) {
                        if(tileRef = wallTiles[i]) {
                            PlaceTile(new Vector2Int(pos.x, pos.y));
                            return;
                        }
                    }
                }
            }
        }
    }

    void PlaceTile(Vector2Int pos) {
        UpdateSurroundingTiles(new Vector3Int(pos.x, pos.y, 0));
        Debug.Log(HouseBuilding.FloorFloodFill(pos, wallTiles, floorTiles, doorTiles, tilemap, whiteTile, houseTilemap, roofTiles));
    }

    void UpdateSurroundingTiles(Vector3Int pos) {
        Vector3Int[] surroundingTiles = {
            pos + new Vector3Int(-1, 1, 0),
            pos + new Vector3Int(0, 1, 0),
            pos + new Vector3Int(1, 1, 0),
            pos + new Vector3Int(-1, 0, 0),
            pos + new Vector3Int(1, 0, 0),
            pos + new Vector3Int(-1, -1, 0),
            pos + new Vector3Int(0, -1, 0),
            pos + new Vector3Int(1, -1, 0),
        };

        for(int i = 0; i < surroundingTiles.Length; i++) {
            tilemap.GetTile(surroundingTiles[i]);

            
        }
    }
}

