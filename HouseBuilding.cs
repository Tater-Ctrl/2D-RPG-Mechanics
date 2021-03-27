using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class HouseBuilding
{
    // Flood fill technique to check house size and find the outer line for wall checking
    public static bool FloorFloodFill(Vector2Int firstTile, TileBase[] wallTiles, TileBase[] floorTiles, TileBase[] doorTiles, Tilemap tilemap,   /*Temporary Value*/ TileBase whiteTile , Tilemap houseTilemap, TileBase[] roofTiles) {
        // Stack that expands and shrinks as you find more elements
        Stack<Vector2Int> existFloor = new Stack<Vector2Int>();
        // Temporary stack that holds checked floor coordinates
        Stack<Vector2Int> checkedFloor = new Stack<Vector2Int>();
        Stack<TileBase> floorTilesStack = new Stack<TileBase>(floorTiles);
        
        existFloor.Push(firstTile);

        while(existFloor.Count > 0) {
            Vector2Int pos = existFloor.Pop();
            if(!checkedFloor.Contains(new Vector2Int(pos.x, pos.y))) {
                for(int y = -1; y <= 1; y++) {
                    for(int x = -1; x <= 1; x++) {
                        if(floorTilesStack.Contains(tilemap.GetTile(new Vector3Int(pos.x + x, pos.y + y, 0)))) {
                            existFloor.Push(new Vector2Int(pos.x + x, pos.y + y));
                        }
                    }
                }
                checkedFloor.Push(new Vector2Int(pos.x, pos.y));
            }
        }
        // returns true if building is surrounded by walls

        Vector2Int[] wallPos = HouseBuilding.FloorEdgeFound(checkedFloor, wallTiles, floorTiles, doorTiles, tilemap);

        if(wallPos.Length <= 0) {
            return false;
        }
        Debug.Log("Still going!");
        // bool to make sure a door is built
        
        // if house is missing a door it won't build
        bool doorCheck = false;

        //Checks that a door is placed to be able to enter the house 
        for(int i = 0; i < wallPos.Length; i++) {
            for(int j = 0; j < doorTiles.Length; j++) {
                if(tilemap.GetTile(new Vector3Int(wallPos[i].x, wallPos[i].y, 0)) == doorTiles[j]) {
                    doorCheck = true;
                }
            }
        }
        // returns false and won't build the home if door isnt recognized
        if(!doorCheck) {
            return false;
        }

        Stack<Vector2Int> roofPos = BuildWalls(wallTiles, doorTiles, floorTiles, wallPos, tilemap, whiteTile, houseTilemap);

        GetRoofTilePosition(wallTiles, floorTiles, roofTiles, tilemap, /* TEMPORARY VARIABLE */ whiteTile, houseTilemap, roofPos);

        return true;
    }

    static Stack<Vector2Int> BuildWalls(TileBase[] wallTiles, TileBase[] doorTilesStack, TileBase[] floorTiles, Vector2Int[] wallPos, Tilemap tilemap, /* TEMPORARY VARIABLE */ TileBase whiteTile, Tilemap houseTilemap) {
        Stack<TileBase> wallStack = new Stack<TileBase>(wallTiles);

        Stack<Vector2Int> newWallPosition = new Stack<Vector2Int>();

        for(int i = 0; i < wallPos.Length; i++) {
            TileBase wallBlock = tilemap.GetTile(new Vector3Int(wallPos[i].x, wallPos[i].y, 0));

            // Temporary fix incase of doors etc or windows
            
            if(wallStack.Contains(tilemap.GetTile(new Vector3Int(wallPos[i].x, wallPos[i].y, 0)))) {
                wallBlock = tilemap.GetTile(new Vector3Int(wallPos[i].x, wallPos[i].y, 0));
            } else {
                wallBlock = tilemap.GetTile(new Vector3Int(wallPos[i].x -1, wallPos[i].y, 0));
            }

            if(!wallStack.Contains(tilemap.GetTile(new Vector3Int(wallPos[i].x, wallPos[i].y -1, 0)))) {
                for(int j = 0; j < floorTiles.Length; j++) {
                    if(floorTiles[j] != tilemap.GetTile(new Vector3Int(wallPos[i].x, wallPos[i].y -1, 0))) {
                        houseTilemap.SetTile(new Vector3Int(wallPos[i].x, wallPos[i].y +1, 0), wallBlock);
                        newWallPosition.Push(new Vector2Int(wallPos[i].x, wallPos[i].y +1));
                    }
                }
            }
        }
        return newWallPosition;
    }

    static void GetRoofTilePosition(TileBase[] wallTiles, TileBase[] floorTiles, TileBase[] roofTiles, Tilemap tilemap, /* TEMPORARY VARIABLE */ TileBase whiteTile, Tilemap houseTilemap, Stack<Vector2Int> wallPosition) {
        Stack<TileBase> wallStack = new Stack<TileBase>(wallTiles);
        Stack<TileBase> floorStack = new Stack<TileBase>(floorTiles);
        Stack<Vector3Int> roofPosition = new Stack<Vector3Int>();

        while(wallPosition.Count > 0) {
            Vector2Int pos = wallPosition.Pop();
            int y = 0;
            while(wallStack.Contains(tilemap.GetTile(new Vector3Int(pos.x, pos.y + y, 0))) || floorStack.Contains(tilemap.GetTile(new Vector3Int(pos.x, pos.y + y, 0)))) {
                roofPosition.Push(new Vector3Int(pos.x, pos.y + y + 1, 0));
                y++;
            }
        }
        RoofRuleTile.BuildRoofTiles(houseTilemap, roofPosition, roofTiles);
    }

    static Vector2Int[] FloorEdgeFound(Stack<Vector2Int> checkedFloor, TileBase[] wallTiles, TileBase[] floorTiles, TileBase[] doorTiles, Tilemap tilemap) {
        // Temporary stacks 
        Stack<TileBase> tempWallTiles = new Stack<TileBase>(wallTiles);
        Stack<TileBase> tempFloorTiles = new Stack<TileBase>(floorTiles);

        foreach(var item in doorTiles)
        {
            tempWallTiles.Push(item);
        }
        // New stack to place outside consisting of pos of tiles outside of current floor
        Stack<Vector2Int> tempCheck = new Stack<Vector2Int>();

        while(checkedFloor.Count > 0) {
            Vector2Int pos = checkedFloor.Pop();
            if(!tempCheck.Contains(pos)) {
                for(int y = -1; y < 2; y++) {
                    for(int x = -1; x < 2; x++) {
                        if(!tempFloorTiles.Contains(tilemap.GetTile(new Vector3Int(pos.x + x, pos.y + y, 0)))) {
                            // Creates new stack and pushes the tiles just outside the floor 
                            tempCheck.Push(new Vector2Int(pos.x + x, pos.y + y));
                        }
                    }
                }
            }
        }
        // checks the new stack for wall tiles || new stack = tile outside of floor
        Vector2Int[] wallPos = tempCheck.ToArray();

        while(tempCheck.Count > 0) {
            Vector2Int pos = tempCheck.Pop();
            
            tilemap.GetTile(new Vector3Int(pos.x, pos.y, 0));
            if(!tempWallTiles.Contains(tilemap.GetTile(new Vector3Int(pos.x, pos.y, 0)))) {
                wallPos = new Vector2Int[0];
                return wallPos;
            }
        }
        // Returns an array of the wall tiles surrounding the house
        return wallPos;
    }
}
