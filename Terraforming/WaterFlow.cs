using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterFlow : WorldDigger
{
    public static Stack<Vector3Int> CheckForWaterFlow(Vector3Int pos, Tilemap tilemap) {
        Stack<Vector3Int> waterTile = new Stack<Vector3Int>();
        Vector3Int[] neighbors = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
        };

        for(int i = 0; i < neighbors.Length; i++) {
            if(tilemap.GetTile(pos + neighbors[i]) == MapVariables.regions[1].tile) {
                waterTile.Push(pos + neighbors[i]);
            }
        }

        return waterTile;
    } 
}
