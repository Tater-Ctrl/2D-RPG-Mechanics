using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player {

    public class UI_Inventory : MonoBehaviour
    {
        private List<Transform> inventorySlot   = new List<Transform>();
        private List<Image> inventorySprites    = new List<Image>();
        private List<Text> spriteTextAmount     = new List<Text>();

        private Image mousePreview;
        private Text textPreview;

        private Inventory inventory;
        private Transform playerMenu;

        private Item item;

        private Tooltip tooltip;

        private int amount;

        private bool mousePreviewItem;
        private bool tooltipActive;

        private bool _holdingItem;

        public bool holdingItem {
            get {
                return _holdingItem;
            } set {
                _holdingItem = holdingItem;
            }
        }

        /// <summary>
        /// Set the reference to the correct player inventory
        /// </summary>
        public void SetInventory (Inventory inventory) {
            this.inventory = inventory;
        }

        private void Start()
        {
            GetInventorySlotFrame();

            mousePreview    = GameObject.Find("UI_MouseClickPreview").GetComponent<Image>();
            textPreview     = mousePreview.transform.GetChild(0).GetComponent<Text>();
            tooltip         = FindObjectOfType<Tooltip>();
            playerMenu      = GameObject.Find("Inventory").transform;
        }

        /// <summary>
        /// moves the sprite preview along the mouse whilst moving an object in the inventory
        /// </summary>
        private void Update() {
            if(mousePreviewItem) {
                mousePreview.transform.position = Input.mousePosition;
            }
            if(tooltipActive) {
                tooltip.transform.position = Input.mousePosition;
            }
        }

        /// <summary>
        /// Toggles the inventory UI on or off
        /// </summary>
        /// <param name="b"> decides if inventory turns on or off </param>
        public void ToggleInventoryUI(bool b) {
            // hide the mouse preview and tooltip when closing the menu so it doesn't popup when you open again next time
            mousePreview.enabled = false;
            tooltipActive = tooltip.HideTooltip();

            playerMenu.gameObject.SetActive(b);
        }

        /// <summary>
        /// Matches the backend inventory with the front end inventory_UI with the correct sprites
        /// </summary>
        /// <param name="item"> the item reference which contains the sprite information. </param>
        /// <param name="index"> the index of the item in the inventory script. </param>
        private void ShowItemSprite(Item item, int index) {
            Sprite sprite = item.itemPrefab.GetComponent<SpriteRenderer>().sprite;

            inventorySprites[index].enabled = true;
            inventorySprites[index].sprite = sprite;
        }

        /// <summary>
        /// Removes the sprite from the inventory at the given index location
        /// </summary>
        /// <param name="index"> index in the list which to remove </param>
        private void HideItemSprite(int index) {
            inventorySprites[index].enabled = false;
        }

        /// <summary>
        /// Updates the stack number on the text
        /// </summary>
        /// <param> index in the inventory to update at </param>
        public void UpdateItemTextAmount(int index) {
            if(inventory.GetItemInStack(index) <= 1) {
                spriteTextAmount[index].enabled = false;
            } else {
                spriteTextAmount[index].enabled = true;
                spriteTextAmount[index].text    = inventory.GetItemInStack(index).ToString();
            }
        }

        /// <summary>
        /// Picks up an item from the inventory UI.
        /// </summary>
        /// <param name="obj">The object to pick up</param>
        public void PickUpItem(Transform obj) {
            int index = inventorySlot.IndexOf(obj);

            // hide the tooltip when picking up an item
            HideTooltip();

            if(item = inventory.GetItemInInventory(index)) {
                amount       = inventory.GetItemInStack(index);
                _holdingItem = true;

                inventory.MoveItem(index, amount);
                MousePreviewSprite(amount, true);
                UpdateSprites(index, item, false);
            }
        }

        /// <summary> 
        /// used to split the stack if the item amount is greater than 1, if it's less or equal it will just call a regular PickUpItem.
        /// </summary>
        /// <param name="obj"> The object to split the stack from </param>
        public void SplitItemStack(Transform obj) {
            int index = inventorySlot.IndexOf(obj);

            if(inventory.GetItemInStack(index) <= 1) {
                PickUpItem(obj);
            } else {
                // hide the tooltip when picking up an item
                HideTooltip();

                if(item = inventory.GetItemInInventory(index)) {
                    amount      = Mathf.CeilToInt((float)inventory.GetItemInStack(index) / 2f);
                    _holdingItem = true;
                    inventory.MoveItem(index, amount);
                    UpdateSprites(index, item, true);
                    MousePreviewSprite(amount, true);
                }
            }
        }

        /// <summary>
        /// Updates the sprites in the inventory slots
        /// </summary>
        /// <param name="index"> index of the inventory slot to update </param>
        /// <param name="b"> if sprite should be active or not </param>
        public void UpdateSprites(int index, Item _item, bool b) {
            try {
                UpdateItemTextAmount(index);
                if(b) {
                    ShowItemSprite(_item, index);
                } else {
                    HideItemSprite(index);
                }
            } catch(System.NullReferenceException) {}
        }

        /// <summary>
        /// Enables / Disables the sprite overlay on the mouse when you click to drag an item
        /// </summary>
        public void MousePreviewSprite(int value, bool b) {
            if(!mousePreview) return;
            mousePreview.sprite     = inventory.tempItem.itemPrefab.GetComponent<SpriteRenderer>().sprite;
            mousePreview.enabled    = b;
            mousePreviewItem        = b;
            textPreview.enabled     = false;

            if(value > 1 && b) {
                textPreview.text    = value.ToString();
                textPreview.enabled = true;
            }
        }

        /// <summary>
        /// Moves an item either to another inventory slot or drops it in the world
        /// </summary>
        /// <param name="obj">The object to move / drop in world</param>
        public void PlaceItem(Transform obj) {

            if(obj == null) {
                // if UI element is null, drop the item in the world instead and delete it from the inventory
                MousePreviewSprite(0, false);
                _holdingItem = false;
                inventory.DropItem();
            } else {
                // Get index of the inventory slot
                int index = inventorySlot.IndexOf(obj);
                // Update the item to match the one you're holding
                item = inventory.tempItem;
                // check if the inventory slot has room for the item held
                _holdingItem = inventory.SetItemInInventory(index);
                MousePreviewSprite(inventory.tempAmount, _holdingItem);
                UpdateSprites(index, item, true);

                // show the tooltip once you drop the item into the slot
                ShowTooltip(index);
                if(_holdingItem) {
                    return;
                }
            }
            item = null;
            inventory.ClearTempItem();
        }

        /// <summary>
        /// Shows the inventory tooltip for the Item if the obj exists in the inventory
        /// </summary>
        /// <param name="obj"> The object to look for in the inventory </param>
        public void ShowTooltip(Transform obj) {
            try {
                int index = inventorySlot.IndexOf(obj);
                Item itemToDisplay;
                try {
                    if(itemToDisplay    = inventory.GetItemInInventory(index)) {
                        tooltipActive   = tooltip.GenerateTooltip(itemToDisplay);
                    }
                } catch (System.NullReferenceException) {}
            } catch(System.ArgumentOutOfRangeException) {}
        }
        /// <summary>
        /// Shows the inventory tooltip for the Item if the index exists in the inventory
        /// </summary>
        public void ShowTooltip(int index) {
            Item itemToDisplay;
            try {
                if(itemToDisplay    = inventory.GetItemInInventory(index)) {
                    tooltipActive   = tooltip.GenerateTooltip(itemToDisplay);
                }
            } catch (System.NullReferenceException) {}
        }

        /// <summary>
        /// Hides the inventory tooltip from the screen
        /// </summary>
        public void HideTooltip() {
            tooltipActive = tooltip.HideTooltip();
        }

        /// <summary>
        /// Gets the inventory slots from the UI and stores them to inventorySlots
        /// </summary>
        private void GetInventorySlotFrame() {
            Transform temp = GameObject.Find("Inventory").transform;
            for(int i = 0; i < temp.childCount; i++) {
                Transform tempObj = temp.GetChild(i); 
                inventorySlot.Add(tempObj);
                inventorySprites.Add(tempObj.GetChild(0).GetComponent<Image>());
                spriteTextAmount.Add(inventorySprites[i].transform.GetChild(0).GetComponent<Text>());
            }

            for(int i = 0; i < inventorySlot.Count; i++) {
                inventorySprites[i].enabled = false;
                spriteTextAmount[i].enabled = false;
            }
        }
    }
}
