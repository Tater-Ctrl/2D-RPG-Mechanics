using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CaveGenerator 
{
    public static bool[,] GenerateCave(int caveSize, int simulationSteps) {
        bool[,] caveMap = new bool[caveSize, caveSize];

        for(int y = 0; y < caveSize; y++) {
            for(int x = 0; x < caveSize; x++) {
                float rng = Random.Range(0f, 1f);

                if(rng < 0.4f) {
                    caveMap[x, y] = true;
                } else {
                    caveMap[x, y] = false;
                }
            }
        }
        for(int i = 0; i < simulationSteps; i++) {
            caveMap = DoSimulationStep(caveMap, caveSize);
        }
        return caveMap;
    }

    public static bool[,] DoSimulationStep(bool[,] oldMap, int caveSize) {
        bool[,] newMap = new bool[caveSize, caveSize];

        for(int y = 1; y < caveSize - 1; y++) {
            for(int x = 1; x < caveSize - 1; x++) {
                int livingCell = 0;
                for(int yAround = -1; yAround < 2; yAround++) {
                    for(int xAround = -1; xAround < 2; xAround++) {
                        if(oldMap[x + xAround,y + yAround]) {
                            livingCell++;
                        }
                    }
                }
                // Game of life rules to generate map
                if(livingCell >= 4) {
                    newMap[x, y] = false;
                }
                if(livingCell <= 3) {
                    newMap[x, y] = true;
                }
            }
        }
        return newMap;
    }

    public static bool[,] FloodFill(bool[,] oldMap, int caveSize) {
        
        bool[,] connectingCavePoints = FindFirstCaveTile(oldMap, caveSize);

        Stack<Vector2Int> caveLoc = new Stack<Vector2Int>();

        //If oldMap == False the tile is walkable
        for(int x = 1, y = 1; x < connectingCavePoints.Length -1; x++) {
            if(!oldMap[x, y]) {
                // Finds first available tile and pushes it to the stack to start flood filling
                caveLoc.Push(new Vector2Int(x, y));
                break;
            }

            if(x >= caveSize -1) {
                y++;
            }
        }
        // Resets array to do flood filling from Vector2 stack position
        connectingCavePoints = new bool[caveSize, caveSize];

        while(caveLoc.Count > 0) {
            //Flood fills the whole available area
            Vector2Int location = caveLoc.Pop();
            if(!connectingCavePoints[location.x, location.y]) {
                connectingCavePoints[location.x, location.y] = true;

                if(!oldMap[location.x + 1, location.y] && !connectingCavePoints[location.x + 1, location.y]) {
                    caveLoc.Push(new Vector2Int(location.x +1, location.y));
                }
                if(!oldMap[location.x - 1, location.y] && !connectingCavePoints[location.x - 1, location.y]) {
                    caveLoc.Push(new Vector2Int(location.x -1, location.y));
                }
                if(!oldMap[location.x, location.y + 1] && !connectingCavePoints[location.x, location.y + 1]) {
                    caveLoc.Push(new Vector2Int(location.x, location.y +1));
                }
                if(!oldMap[location.x, location.y - 1] && !connectingCavePoints[location.x, location.y - 1]) {
                    caveLoc.Push(new Vector2Int(location.x, location.y -1));
                }
            }
        }
        return connectingCavePoints;
    }

    static bool[,] FindFirstCaveTile (bool[,] oldMap, int caveSize) {
        bool[,] connectingCavePoints = new bool[caveSize,caveSize];
        for(int y = 1; y < caveSize -1; y++) {
            for(int x = 1; x < caveSize -1; x++) {
                if(!oldMap[x,y]) {
                    connectingCavePoints[x, y] = true;
                    return connectingCavePoints;
                }
            }
        }
        return connectingCavePoints;
    }
}

