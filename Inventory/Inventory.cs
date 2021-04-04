using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Player {

    public class Inventory : NetworkBehaviour
    {
        private UI_Inventory ui_Inventory;

        private Inventory inventory;

        private NetworkManager nm;

        public Item tempItem {get; private set;}

        public int tempAmount {get; private set;}

        public float inventoryWeight {get; private set;}

        private readonly string itemTag     = "Item";

        private List<Item> inventoryList    = new List<Item> ( new Item[18] );
        private List<int> stackAmount       = new List<int> ( new int[18] );

        void Start() {
            nm           =   FindObjectOfType<NetworkManager>();
            ui_Inventory =   FindObjectOfType<UI_Inventory>();

            if(isLocalPlayer) {
                ui_Inventory.SetInventory(this);
            }

            ui_Inventory.ToggleInventoryUI(false);
        }

        /// <summary>
        /// Toggles the inventory UI on or off depending on the b parameter
        /// </summary>
        /// <param name="b"></param>
        public void ToggleMenu(bool b) {
            if(!isLocalPlayer) return;
            ui_Inventory.ToggleInventoryUI(b);
            if(!b && tempItem != null) {
                DropItem();
            }
        }

        /// <summary>
        /// checks if collided object is an item, if it is, it will insert it into the inventory if there is room
        private void OnCollisionEnter2D(Collision2D collision) {
            if(!isLocalPlayer) return;

            if(collision.collider.tag == itemTag) {
                GameObject obj = collision.gameObject;
                if(InsertItem(obj)) {
                    CmdDestroyObject(obj);
                }
            }
        }

        /// <summary>
        /// Sets the tempItem and tempAmount variables when player wants to move an item 
        /// </summary>
        public void MoveItem(int index, int amount) {
            tempItem            = inventoryList[index];
            tempAmount          = amount;
            stackAmount[index] -= amount;

            if(stackAmount[index] <= 0) {
                inventoryList[index] = null;
            }
        }
        
        /// <summary>
        /// Inserts a game object into the inventory of the player
        /// </summary>
        /// <param name="obj"> gameobject to pass into the inventory </param>
        bool InsertItem(GameObject obj) {
            if(!isLocalPlayer) return false;

            PhysicalItem physItem = obj.GetComponent<PhysicalItem>();
            Item item = physItem.scriptableObjectReference;

            if(item.isStackable) {
                for(int i = 0; i < inventoryList.Count; i++) {
                    if(item == inventoryList[i] && stackAmount[i] < item.maxStackable) {
                        stackAmount[i] += physItem.stackedAmount;
                        ui_Inventory.UpdateItemTextAmount(i);

                        if(stackAmount[i] > item.maxStackable) {
                            // if there's more items being picked up then what fits in 1 stack, split it and run add it to a new slot
                            int stackOverflow = stackAmount[i] - item.maxStackable;
                            stackAmount[i] = item.maxStackable;
                            physItem.stackedAmount -= stackOverflow;
                            ui_Inventory.UpdateItemTextAmount(i);
                        } else {
                            CalculateWeight();
                            return true;
                        }
                    }
                }
            }
            for(int i = 0; i < inventoryList.Count; i++) {
                if(inventoryList[i] == null) {
                    // add the item to the inventorylist and amount to the stackamount
                    stackAmount[i]      = physItem.stackedAmount;
                    inventoryList[i]    = item;
        
                    ui_Inventory.UpdateSprites(i, item, true);
                    CalculateWeight();
                    return true;
                }
            }
            CalculateWeight();
            return false;
        }
        
        /// <summary>
        /// Instantiates the item at index in inventoryList into the world and removes it from your inventory
        /// </summary>
        /// <param name="index"> index which to remove from inventoryList </param>
        public void DropItem() {
            if(!isLocalPlayer) return;

            try {
                Item item           =   tempItem as Item;
                int itemId          =   item.itemID;
                int _amount         =   tempAmount;
                Vector3 position    =   new Vector3(transform.position.x + 2, transform.position.y, 0);

                if(!isServer) {
                    CmdInstantiateObject(itemId, position, Quaternion.identity, _amount);
                } else {
                    ServerInstantiateObject(itemId, position, Quaternion.identity, _amount);
                }

                ClearTempItem();
                CalculateWeight();

            } catch (System.ArgumentOutOfRangeException) { return; }
        }

        /// <summary>
        /// Returns true if it did not manage fill the inventory slot with all items currently held, false if it did manage.
        /// </summary>
        /// <param name="index"> The index which you're placing the item in</param>
        public bool SetItemInInventory(int index) {
            if(inventoryList[index] == null) {
                inventoryList[index] = tempItem;
                stackAmount[index] = tempAmount;
                return false;

            }
            // if the inventory is stackable, do the stacking thingy correctly
            if(inventoryList[index].isStackable && inventoryList[index] == tempItem) {
                // if the inventory slot is already max stacked with the item you're putting in, swap the max stack with what you're currently holding
                if(stackAmount[index] == inventoryList[index].maxStackable) {
                    stackAmount[index] = tempAmount;
                    tempAmount = inventoryList[index].maxStackable;
                    return true;

                } //if the stack is not maxed, add the items you're holding into the current stack
                else if(inventoryList[index] == tempItem) {
                    stackAmount[index] += tempAmount;
                    // if what you tried to add to the stack is greater than its max capacity, return the overflow into the "hand"
                    if(stackAmount[index] > inventoryList[index].maxStackable) {
                        int overflow = stackAmount[index] - inventoryList[index].maxStackable;
                        tempAmount = overflow;
                        stackAmount[index] = inventoryList[index].maxStackable;
                        ui_Inventory.MousePreviewSprite(overflow, true);
                        return true;
                    }
                    return false;
                } 
            }
            if(inventoryList[index] != tempItem) {
                Item temp = inventoryList[index];
                inventoryList[index] = tempItem;
                tempItem = temp;

                int tempInt = stackAmount[index];
                stackAmount[index] = tempAmount;
                tempAmount = tempInt;
                ui_Inventory.MousePreviewSprite(tempAmount, true);
                return true;
            }
            return false;
        }

        void CalculateWeight() {
            inventoryWeight = 0;

            for(int i = 0; i < inventoryList.Count; i++) {
                if(inventoryList[i] != null) {
                    inventoryWeight += inventoryList[i].weight * stackAmount[i];
                }
            }
        }

        /// <summary>
        /// Clear out the ref in tempItem and tempAmount
        /// </summary>
        public void ClearTempItem() {
            tempItem = null;
            tempAmount = 0;
        }

        /// <summary>
        /// Destroys the game object from the world when picked up
        /// </summary>
        /// <param name="obj"> object to be destroyed </param> 
        [Command]
        void CmdDestroyObject(GameObject obj) {
            Destroy(obj);
        }

        /// <summary>
        /// Called from clients to instantiate objects in the world on the server
        /// </summary>
        /// <param name="itemId"> Item ID for the Network Manager prefab list </param>
        /// <param name="position"> Position to instantiate the object </param>
        /// <param name="rotation"> Rotation to instantiate the object </param>
        /// <param name="amount"> Amount to instantiate if the item is stackable </param>
        [Command]
        void CmdInstantiateObject(int itemId, Vector3 position, Quaternion rotation, int amount) {
            GameObject obj = Instantiate(nm.spawnPrefabs[itemId], position, rotation);
            obj.GetComponent<PhysicalItem>().stackedAmount = amount;
            NetworkServer.Spawn(obj);
        }

        /// <summary>
        /// Called from the server to instantiate objects for everyone
        /// </summary>
        /// <param name="itemId"> Item ID for the Network Manager prefab list </param>
        /// <param name="position"> Position to instantiate the object </param>
        /// <param name="rotation"> Rotation to instantiate the object </param>
        /// <param name="amount"> Amount to instantiate if the item is stackable </param>
        void ServerInstantiateObject(int itemId, Vector3 position, Quaternion rotation, int amount) {
            GameObject obj = Instantiate(nm.spawnPrefabs[itemId], position, rotation);
            obj.GetComponent<PhysicalItem>().stackedAmount = amount;
            NetworkServer.Spawn(obj);
        }

        /// <summary>
        /// Returns the inventoryList item at the given index
        /// </summary>
        /// <param name="index"> index for the list item </param>
        public Item GetItemInInventory(int index) {
            return inventoryList[index];
        }

        /// <summary>
        /// Set the inventory slot at index to null
        /// </summary>
        /// <param name="index">index in the list to change </param>
        public void ClearItemInInventory(int index) {
            inventoryList[index]    = null;
            stackAmount[index]      = 0;

            ui_Inventory.UpdateSprites(index, null, false);
        }

        /// <summary>
        /// Returns the amount of items stacked at index from the stackAmount list
        /// </summary>
        /// <param name="index"> index of stackAmount to retrieve </param>
        public int GetItemInStack(int index) {
            return stackAmount[index];
        }
    }

}
