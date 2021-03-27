using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldDigger : MonoBehaviour
{
    public Tilemap tilemap;

    // Update is called once per frame
    void Update()
    {
        if(!EnableBuildingMode.buildingMode) {
            if(Input.GetMouseButtonDown(0)) {
                Vector3Int pos = new Vector3Int(Mathf.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x), Mathf.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y), 0);
                // Temp forXY loop to destroy larger area
                for(int y = -3; y <= 3; y++) {
                    for(int x = -3; x <= 3; x++) {
                        if(MapVariables.noiseMap[pos.x + x, pos.y + y] >= MapVariables.regions[6].height) continue;
                        WorldTileHealth.tileHealthMap[pos.x + x, pos.y + y] -= 100;

                        if(WorldTileHealth.tileHealthMap[pos.x + x, pos.y + y] <= 0) {
                            ReplaceTile(new Vector3Int(pos.x + x, pos.y + y, 0));
                        }
                    }
                }
            }

            if(Input.GetMouseButtonDown(1)) {
                Vector3Int pos = new Vector3Int(Mathf.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x), Mathf.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y), 0);
                Debug.Log(MapVariables.noiseMap[pos.x, pos.y]);
            }
        }
    }

    void ReplaceTile(Vector3Int pos) {
        float tempMap = MapVariables.noiseMap[pos.x, pos.y];
        Stack<Vector3Int> waterPos = new Stack<Vector3Int>();
        // RETURN IF ITS STONE
        if(MapVariables.noiseMap[pos.x, pos.y] <= MapVariables.regions[2].height) return;
        for(int i = 2; i < MapVariables.regions.Length; i++) {
            if(tempMap >= MapVariables.regions[i].height) {
                MapVariables.noiseMap[pos.x, pos.y] = MapVariables.regions[i -1].height;
                WorldTileHealth.tileHealthMap[pos.x, pos.y] = MapVariables.regions[i -1].tileHealth;
                tilemap.SetTile(pos, MapVariables.regions[i -1].tile);
            }
        }
        if(tilemap.GetTile(pos) == MapVariables.regions[2].tile) {
            // Run a check for water tile
            waterPos = WaterFlow.CheckForWaterFlow(pos, tilemap);
            StartCoroutine(StartWaterFlow(waterPos));
        }
    }

    IEnumerator StartWaterFlow(Stack<Vector3Int> waterPos) {

        while(waterPos.Count > 0) {
            Stack<Vector3Int> tempWater = new Stack<Vector3Int>(waterPos);
            waterPos.Clear();
        
            while(tempWater.Count > 0) {
                Vector3Int pos = tempWater.Pop();
                tilemap.SetTile(pos, MapVariables.regions[1].tile);
                Debug.Log(tempWater.Count);
        
                for(int y = -1; y <= 1; y++) {
                    for(int x = -1; x <= 1; x++) {
                        if(MapVariables.noiseMap[pos.x + x, pos.y + y] == MapVariables.regions[2].height) {
                            waterPos.Push(new Vector3Int(pos.x + x, pos.y + y, 0));
                            MapVariables.noiseMap[pos.x + x, pos.y + y] = MapVariables.regions[1].height;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
