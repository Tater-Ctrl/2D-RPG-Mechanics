using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandscapeGenerator : MonoBehaviour
{
    public int mapSize;
    public int caveSize;

    public enum Tilemode {LandscapeMap, NoiseMap, FalloffMap, CaveGeneration}
    public bool autoUpdate;
    public bool randomSeed;
    public bool smoothMountains;
    public bool useFalloffMap;
    public bool generateCaveEntrances;
    public bool generateTrees;
    public Tilemode tilemode;
    [Header("Landscape Settings")]
    public GameObject[] natureObjects;
    [Header("Mountain Settings")]
    public int cavesAmount;
    public int minMountainSize;
    public Tile whiteTile;
    [Header("Cave Generation Settings")]
    public int simulationSteps;
    public TileBase blackTile;
    [Header("Map Noise Settings")]
    public float scale = 1f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public int seed;
    [Header("Dirt Layer Blend")]
    public Tile[] dirtRuleTile;

    // Just makes sure it won't loop for infinity if there's not enough room for caves
    int caveSpawnIterations = 0;
    [Header("Map Info")]
    public TerrainType[] regions;

    public Tilemap tilemap;
    public Tilemap tilemapOverlay;

    float[,] noiseMap;

    void Awake() {
        TileGenerator();
        MapVariables.mapSize = mapSize;
        MapVariables.noiseMap = noiseMap;
        MapVariables.regions = regions;
    }

    public void TileGenerator() {
        if(randomSeed) {
            seed = Random.Range(0, 100000);
        }
        noiseMap = new float[mapSize, mapSize];
        float [,] falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize);

        for(int y = 0; y < mapSize; y++) {
            for(int x = 0; x < mapSize; x++) {
                noiseMap[x, y] = NoiseMap.fBM(x, y, seed, scale, octaves, persistence);
            }
        }

        tilemap.ClearAllTiles();

        if(tilemode == Tilemode.LandscapeMap) {
            GenerateLandscape(noiseMap, falloffMap);
        } else if(tilemode == Tilemode.NoiseMap) {
            NoiseMapGenerator(noiseMap);
        } else if(tilemode == Tilemode.FalloffMap) {
            NoiseMapGenerator(falloffMap);
        } else if(tilemode == Tilemode.CaveGeneration) {
            GenerateCave();
        }
        RuleTileGenerator(dirtRuleTile);
    }

    void GenerateLandscape(float[,] noiseMap, float[,] falloffMap) {
        // Places tiles according to the Regions struct and Noisemap height values
        for(int y = 0; y < mapSize; y++) {
            for(int x = 0; x < mapSize; x++) {
                if(useFalloffMap) {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
                for(int j = 0; j < regions.Length; j++) {
                    if(noiseMap[x,y] >= regions[j].height && !regions[j].dontUseTile) {
                        tilemap.SetTile(new Vector3Int(x, y, 0), regions[j].tile);
                    }
                }
            }
        }
        if(smoothMountains) {
            // pass in mountain layer and layer below
            SmoothMountainHills(regions[7], regions[5], regions[6]);
        }
        if(generateTrees) {
            GenerateForests();
        }
    }

    void SmoothMountainHills (TerrainType mountainType, TerrainType grassType, TerrainType mountainBase) {
        // Iterates over the map checking for hills that have single blocks sticking out and removing them
        for(int y = 2; y < mapSize -2; y++) {
            for(int x = 2; x < mapSize -2; x++) {
                if(noiseMap[x, y] >= mountainType.height) {
                    if(noiseMap[x -1, y] <= mountainType.height && noiseMap[x +1, y] <= mountainType.height) {
                        tilemap.SetTile(new Vector3Int(x, y, 0), grassType.tile);
                        noiseMap[x, y] = grassType.height;
                    }
                }
            }
        }
        // Removes small mountains according to min mountain size
        for(int y = 0; y < mapSize; y++) {
            for(int x = 0; x < mapSize; x++) {
                if(noiseMap[x, y] >= mountainType.height) {
                    int mountainTilesAround = 0;
                    for(int yAround = -5; yAround < 5; yAround++) {
                        try {
                            for(int xAround = -5; xAround < 5; xAround++) {
                                if(noiseMap[x + xAround, y + yAround] >= mountainType.height) {
                                    mountainTilesAround++;
                                }
                            }
                        } catch (System.IndexOutOfRangeException) {
                            continue;
                        }
                    }
                    if(mountainTilesAround < minMountainSize) {
                        tilemap.SetTile(new Vector3Int(x, y, 0), grassType.tile);
                        noiseMap[x, y] = grassType.height;
                    }
                }
            }
        }

        // Iterates over map again and fixed jagged edges on mountain and fills in top parts
        for(int y = 2; y < mapSize -2; y++) {
            for(int x = 2; x < mapSize -2; x++) {
                if(noiseMap[x, y] >= mountainType.height) {
                    if(noiseMap[x, y -1] < mountainType.height) {
                        if(noiseMap[x, y -2] < mountainType.height) {
                            // places the mountain tile beneath the mountain on the Y axis
                            tilemap.SetTile(new Vector3Int(x, y -1, 0), mountainBase.tile);
                            noiseMap[x, y -1] = mountainType.height;
                        } else {
                        // Place rock tile beneath bottom mountain tile
                            tilemap.SetTile(new Vector3Int(x, y -1, 0), grassType.tile);
                            noiseMap[x, y -1] = mountainType.height;
                        }
                        if(noiseMap[x, y +1] < mountainType.height) {  
                            tilemap.SetTile(new Vector3Int(x, y +1, 0), mountainType.tile);
                            noiseMap[x, y +1] = mountainType.height;
                            if(noiseMap[x +1, y] >= mountainType.height) {
                                tilemap.SetTile(new Vector3Int(x +1, y +1, 0), mountainType.tile);
                                noiseMap[x +1, y +1] = mountainType.height;
                            }
                            if(noiseMap[x -1, y] >= mountainType.height) {
                                tilemap.SetTile(new Vector3Int(x -1, y +1, 0), mountainType.tile);
                                noiseMap[x -1, y +1] = mountainType.height;
                            }
                        }
                }
                // Fix little gaps to prevent less than 1 tile blocks on mountain in thin places
                    if(noiseMap[x + 2, y] >= mountainType.height) {
                        tilemap.SetTile(new Vector3Int(x + 1, y, 0), mountainType.tile);
                    }
                    if(noiseMap[x -2, y] >= mountainType.height) {
                        tilemap.SetTile(new Vector3Int(x - 1, y, 0), mountainType.tile);
                    }
                }
            }
        }
        if(generateCaveEntrances) {
            RemoveCaveEntrances();
        }
    }

    public void RemoveCaveEntrances() {
        // Remove old cave entrances before placing new ones
        for(int y = 0; y < mapSize; y++) {
            for(int x = 0; x < mapSize; x++) {
                TileBase tempTile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if(tempTile == whiteTile) {
                    tilemap.SetTile(new Vector3Int(x, y, 0), regions[6].tile);
                }
            }
        }
        caveSpawnIterations = 0;
        SpawnCaveEntrances();
    }

    void SpawnCaveEntrances() {
        TileBase mountainBase = regions[6].tile;
        int cavesPlaced = 0;
        // Checks how many entrances are currently existing
        for(int y = 0; y < mapSize; y++) {
            for(int x = 0; x < mapSize; x++) {
                TileBase tempTile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if(tempTile == whiteTile) {
                    cavesPlaced++;
                }
            }
        }

        if(cavesPlaced >= cavesAmount) return;

        int caveRadiusCheck = 10;

        for(int y = 0; y < mapSize; y++) {
            for(int x = 0; x < mapSize; x++) {
                TileBase tempTile = tilemap.GetTile(new Vector3Int(x, y, 0));

                if(tempTile == mountainBase) {
                    float rng = Random.Range(0f, 1f);
                    TileBase leftTile = tilemap.GetTile(new Vector3Int(x - 1, y, 0));
                    TileBase rightTile = tilemap.GetTile(new Vector3Int(x + 1, y, 0));

                    if(leftTile == mountainBase && rightTile == mountainBase && rng < 0.01f) {
                        bool anotherCaveTooClose = false;
                        // for loop checking tiles in vicinity for another cave being already close
                        for(int yAround = -caveRadiusCheck; yAround < caveRadiusCheck; yAround++) {
                            for(int xAround = -caveRadiusCheck; xAround < caveRadiusCheck; xAround++) {
                                TileBase tileAround = tilemap.GetTile(new Vector3Int(x + xAround, y + yAround, 0));

                                if(tileAround == whiteTile) {
                                    anotherCaveTooClose = true;
                                    break;
                                }
                            }
                        }
                        if(cavesPlaced >= cavesAmount) return;

                        if(!anotherCaveTooClose) {
                            tilemap.SetTile(new Vector3Int(x, y, 0), whiteTile);
                            cavesPlaced++;
                        }
                    }
                }
            }
        }

        if(cavesPlaced < cavesAmount && caveSpawnIterations < 50) {
            caveSpawnIterations++;
            SpawnCaveEntrances();
        }  
    }

    void NoiseMapGenerator(float[,] noiseMap) {
        // function showing the noisemap being generated
        for(int y = 0; y < mapSize; y++) {
            for(int x = 0; x < mapSize; x++) {
                Tile tempTile = whiteTile;
                float value = noiseMap[x, y];
                tempTile.color = new Color(value, value, value);
                tilemap.SetTile(new Vector3Int(x, y, 0), tempTile);
            }
        }
    }

    public void GenerateCave() {
        bool[,] caveMap = CaveGenerator.GenerateCave(caveSize, simulationSteps);

        for(int y = 0; y < caveSize; y++) {
            for(int x = 0; x < caveSize; x++) {
                if(x == 0 || y == 0 || x == caveSize -1 || y == caveSize -1) {
                    // creates a wall around the border of the cave
                    caveMap[x, y] = true;
                }
                if(caveMap[x, y]) {
                    tilemap.SetTile(new Vector3Int(x, y, 0), blackTile);
                } else {
                    tilemap.SetTile(new Vector3Int(x, y, 0), regions[2].tile);
                }
            }
        }

        caveMap = CaveGenerator.FloodFill(caveMap, caveSize);
        int totalCaveSize = 0;
        for(int y = 0; y < caveSize; y++) {
            for(int x = 0; x < caveSize; x++) {
                if(caveMap[x, y]) {
                    tilemap.SetTile(new Vector3Int(x, y ,0), regions[2].tile);
                    totalCaveSize++;
                } else {
                    tilemap.SetTile(new Vector3Int(x, y , 0), blackTile);
                }
            }
        }

        int minCaveSize = (caveSize * caveSize) / 2;

        if(totalCaveSize <= minCaveSize) {
            GenerateCave();
        } else {
            for(int y = 1; y < caveSize -1; y++) {
                for(int x = 1; x < caveSize -1; x++) {
                    if(tilemap.GetTile(new Vector3Int(x, y, 0)) == blackTile) {
                        //Checks 8 coords around current x and y to see if it's wall or black
                        
                        if(caveMap[x + 1, y] == regions[2].tile || caveMap[x -1, y] == regions[2].tile 
                        || caveMap[x, y +1] == regions[2].tile || caveMap[x, y -1] == regions[2].tile 
                        || caveMap[x +1, y +1] == regions[2].tile || caveMap[x -1, y +1] == regions[2].tile 
                        || caveMap[x +1, y -1] == regions[2].tile || caveMap[x -1, y -1] == regions[2].tile)
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), regions[6].tile);
                        }

                    }
                }
            }
        }
    }

    void GenerateForests() {
        for(int y = 0; y < mapSize; y++) {
            for(int x = 0; x < mapSize; x++) {
                float rng = Random.Range(0f, 1f);
                if(tilemap.GetTile(new Vector3Int(x, y ,0)) == regions[4].tile) {
                    if(rng <= 0.05f) {
                        if(tilemap.GetTile(new Vector3Int(x +1, y ,0)) == regions[4].tile && tilemap.GetTile(new Vector3Int(x -1, y ,0)) == regions[4].tile && tilemap.GetTile(new Vector3Int(x, y -1 ,0)) == regions[4].tile && tilemap.GetTile(new Vector3Int(x -1, y -1 ,0)) == regions[4].tile) {
                            GameObject spawnedObj = Instantiate(natureObjects[0], new Vector3(x, y ,0), Quaternion.identity, transform.GetChild(0).transform);
                        }
                    }
                }
            }
        }
    }

    void RuleTileGenerator(Tile[] dirtVariations) {
        // Not Active!
        // Manual Rule tile for map generation test
        for(int y = 1; y < caveSize -1; y++) {
            for(int x = 1; x < caveSize -1; x++) {
                TileBase tempTile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if(tempTile == regions[3].tile) {
                    Vector2Int tilePosition = new Vector2Int(x, y);
                    ManualRuleTile.CheckSurroundingTile(tilemap, tilemapOverlay, tilePosition, regions, dirtVariations);
                }
            }
        }
    }
}

public static class MapVariables {
    public static int mapSize;
    public static float[,] noiseMap;
    public static TerrainType[] regions;
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public TileBase tile;
    public float tileHealth;
    public bool dontUseTile;
}