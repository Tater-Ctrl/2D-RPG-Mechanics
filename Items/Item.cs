using UnityEngine;

[System.Serializable]
public abstract class Item : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public GameObject itemPrefab;
    public bool isStackable;
    public int maxStackable;
    public int value;
    public float weight;
    public int itemID;
}
