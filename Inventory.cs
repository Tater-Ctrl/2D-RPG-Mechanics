using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Inventory : MonoBehaviour
{
    public tempBuildingStruct[] hotbarSlots;
    public GameObject hotBar;
    public Sprite whiteTile;
    [HideInInspector]
    public int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < hotBar.transform.childCount; i++) {
            Image tempHotBarImage = hotBar.transform.GetChild(i).GetComponent<Image>();
            if(hotbarSlots[i].tileImage) {
                tempHotBarImage.sprite = hotbarSlots[i].tileImage;
            } else {
                tempHotBarImage.sprite = whiteTile;
            }
        }
        UpdateInventoryIndex();

        hotBar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Enables the building mode
        if(Input.GetKeyDown(KeyCode.Tab)) {
            EnableBuildingMode.buildingMode = !EnableBuildingMode.buildingMode;
            if(EnableBuildingMode.buildingMode) {
                hotBar.SetActive(true);
            } else {
                hotBar.SetActive(false);
            }
        }
        // Scrolls through the hotbar slots
        if(Input.GetAxisRaw("Mouse ScrollWheel") < 0) {
            index++;
            if(index > hotbarSlots.Length -1) {
                index = 0;
            }
            UpdateInventoryIndex();
        } else if(Input.GetAxisRaw("Mouse ScrollWheel") > 0) {
            index--;
            if(index < 0) {
                index = hotbarSlots.Length -1;
            }
            UpdateInventoryIndex();
        }
    }

    // Updates the index variable and enables the border for active hotbar slot
    void UpdateInventoryIndex() {
        for(int i = 0; i < hotbarSlots.Length; i++) {
            Image hotBarImage = hotBar.transform.GetChild(i).GetChild(0).GetComponent<Image>();
            if(i == index) {
                hotBarImage.enabled = true;
            } else {
                hotBarImage.enabled = false;
            }
        }
    }
}

public static class EnableBuildingMode {
    public static bool buildingMode;
}

[System.Serializable]
public struct tempBuildingStruct {
    public Sprite tileImage;
    public TileBase tile;
}
