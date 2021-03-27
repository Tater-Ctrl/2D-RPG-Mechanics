using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ManualRuleTile
{
    public static void CheckSurroundingTile(Tilemap tilemap, Tilemap tilemapOverlay, Vector2Int position, TerrainType[] regions, Tile[] tileVariations) {
        Vector3Int[] neighborTiles = {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
        };

        for(int i = 0; i < neighborTiles.Length; i++) {
            tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0) + neighborTiles[i], null);
        }

        TileBase posRight = tilemap.GetTile(new Vector3Int(position.x +1, position.y, 0));
        TileBase posLeft = tilemap.GetTile(new Vector3Int(position.x -1, position.y, 0));
        TileBase posUp = tilemap.GetTile(new Vector3Int(position.x, position.y +1, 0));
        TileBase posDown = tilemap.GetTile(new Vector3Int(position.x, position.y -1, 0));
        int totalTile = 0;

        int left = 1;
        int right = 8;
        int up = 4;
        int down = 16;

        if(posRight == regions[4].tile) {
            totalTile += right;
        }
        if(posLeft == regions[4].tile) {
            totalTile += left;
        }
        if(posUp == regions[4].tile) {
            totalTile += up;
        }
        if(posDown == regions[4].tile) {
            totalTile +=down;
        }

        // Checks north, south, west and east then follows the if rules below to place correct Tile
        switch(totalTile) {
            case 0: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[5]); break;
            case 1: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[4]); break;
            case 4: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[1]); break;
            case 5: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[0]); break;
            case 8: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[6]); break;
            case 9: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[13]); break;
            case 12: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[2]); break;
            case 13: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[7]); break;
            case 16: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[9]); break;
            case 17: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[8]); break;
            case 20: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[12]); break;
            case 21: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[11]); break;
            case 24: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[10]); break;
            case 25: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[3]); break;
            case 28: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[15]); break;
            case 29: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[14]); break;
            default: tilemapOverlay.SetTile(new Vector3Int(position.x, position.y, 0), tileVariations[5]); break;
        }
    }
}
