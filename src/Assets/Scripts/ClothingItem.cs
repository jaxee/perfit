using UnityEngine;

[System.Serializable]
public class ClothingItem
{
    public string FileName;
    public string ItemType;
    public GameObject ItemPrefab;
    public int ItemID;

    public string ItemName;
    public string ItemDescription;

    //public string ClothingType; 
    public bool Stackable; //Can you add more than one item of the same type.. false cuz we can only have 1 dress at a time
    public Sprite ItemIcon;

    //different constructors for empty items or items with models
    public ClothingItem(int itemID, string itemName, string itemDescription, string file, string itemType, GameObject itemPrefab)
    {
		//Characteristics of a clothing item

		//0,1,2,3..
        this.ItemID = itemID;

		//
        this.ItemName = itemName;
        this.ItemDescription = itemDescription;
		this.FileName = file;
        this.ItemType = itemType;

		//Loading the model file
        this.ItemPrefab = itemPrefab;
    }
    //constructor for being naked
    public ClothingItem(int itemID, string itemName, string itemDescription, string file, string itemType)
    {
        this.ItemID = itemID;
        this.ItemName = itemName;
        this.ItemDescription = itemDescription;
		this.FileName = file;
        this.ItemType = itemType;
    }
    //empty constructor
    public ClothingItem()
    {
        this.ItemID = -1;
    }

}