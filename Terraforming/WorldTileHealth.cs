using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTileHealth : MonoBehaviour
{
    LandscapeGenerator landscape;
    TerrainType[] regions;
    float[,] noiseMap;
    public static float[,] tileHealthMap;

    void Start() {
        landscape = GetComponent<LandscapeGenerator>();
        regions = MapVariables.regions;
        noiseMap = MapVariables.noiseMap;
        tileHealthMap = new float[MapVariables.mapSize, MapVariables.mapSize];
        SetTileHealthValues();
    }

    /// Sets the array to contain health of tiles all around the world
    void SetTileHealthValues() {
        for(int y = 0; y < MapVariables.mapSize; y++) {
            for(int x = 0; x < MapVariables.mapSize; x++) {
                for(int i = 0; i < regions.Length; i++) {
                    if(noiseMap[x, y] > regions[i].height) {
                        tileHealthMap[x, y] = regions[i].tileHealth;
                    }
                }
            }
        }
    }
}
