using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class RoofRuleTile
{
    public static void BuildRoofTiles(Tilemap tilemap, Stack<Vector3Int> position, TileBase[] roofTiles) {
        Stack<Vector3Int> newPos = new Stack<Vector3Int>(position);

        while(newPos.Count > 0) {
            Vector3Int pos = newPos.Pop();

            // top, down, left, right positions around current position
            Vector3Int posRight = new Vector3Int(pos.x +1, pos.y, 0);
            Vector3Int posLeft = new Vector3Int(pos.x -1, pos.y, 0);
            Vector3Int posUp = new Vector3Int(pos.x, pos.y +1, 0);
            Vector3Int posDown = new Vector3Int(pos.x, pos.y -1, 0);

            Vector3Int posDownRight = new Vector3Int(pos.x +1, pos.y -1, 0);
            Vector3Int posDownLeft = new Vector3Int(pos.x -1, pos.y -1, 0);

            // Bitmask for ruletile
            int left = 1;
            int right = 2;
            int up = 4;
            int down = 8;
            int downRight = 16;
            int downLeft = 32;

            int totalTile = 0;

            //Find the tiles around and assign correct bitmask
            if(!position.Contains(posRight) )       totalTile += right;
            if(!position.Contains(posLeft))         totalTile += left;
            if(!position.Contains(posUp))           totalTile += up;
            if(!position.Contains(posDown))         totalTile += down;
            if(!position.Contains(posDownRight))    totalTile += downRight;
            if(!position.Contains(posDownLeft))     totalTile += downLeft;

            switch(totalTile) {
                // center tile
                case 0:  tilemap.SetTile(pos, roofTiles[5]);  break;
                // left tile
                case 1:  tilemap.SetTile(pos, roofTiles[4]);  break;
                case 33:  tilemap.SetTile(pos, roofTiles[4]);  break;
                // right tile
                case 2:  tilemap.SetTile(pos, roofTiles[6]);  break;
                case 18:  tilemap.SetTile(pos, roofTiles[6]);  break;
                // top tile
                case 4:  tilemap.SetTile(pos, roofTiles[1]);  break;
                // top left tile
                case 5:  tilemap.SetTile(pos, roofTiles[0]);  break;
                case 37:  tilemap.SetTile(pos, roofTiles[0]);  break;
                // top right tile
                case 6:  tilemap.SetTile(pos, roofTiles[2]);  break;
                case 22:  tilemap.SetTile(pos, roofTiles[2]);  break;
                // bottom tile
                case 8:  tilemap.SetTile(pos, roofTiles[11]); break;
                case 24:  tilemap.SetTile(pos, roofTiles[11]); break;
                case 40:  tilemap.SetTile(pos, roofTiles[11]); break;
                case 56:  tilemap.SetTile(pos, roofTiles[11]); break;
                // bottom left tile
                case 9:  tilemap.SetTile(pos, roofTiles[10]); break;
                case 25:  tilemap.SetTile(pos, roofTiles[10]); break;
                case 41:  tilemap.SetTile(pos, roofTiles[10]); break;
                case 57:  tilemap.SetTile(pos, roofTiles[10]); break;
                // bottom right tile
                case 10: tilemap.SetTile(pos, roofTiles[12]); break;
                case 26: tilemap.SetTile(pos, roofTiles[12]); break;
                case 42: tilemap.SetTile(pos, roofTiles[12]); break;
                case 58: tilemap.SetTile(pos, roofTiles[12]); break;
                // inverted down left tile
                case 32: tilemap.SetTile(pos, roofTiles[8]); break;
                // inverted down right tile
                case 16: tilemap.SetTile(pos, roofTiles[9]); break;
                default: tilemap.SetTile(pos, roofTiles[5]);  break;
            }                    
        }
    }
}
