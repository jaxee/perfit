using UnityEngine;

public class ChangeClothes : MonoBehaviour
{
    private AttachClothing attachScript; //CHANGE LATER

    private void Start()
    {
		attachScript = GetComponent<AttachClothing>(); //change name of this later
        //create equipment list
		attachScript.InitializeClothingItemsList();
        //equip stuff
       // EquipItem("Legs", "pants"); 
    }

    public void AttachClothingItem(string itemType, string itemFile)
    {
		for (int i = 0; i < attachScript.wornItems.Count; i++)
        {
			if (attachScript.wornItems[i].ItemType == itemType)
            {
				attachScript.wornItems[i] = ItemDatabase.instance.FetchItemByFileName(itemFile);
				attachScript.AddClothes(attachScript.wornItems[i]);
                break;
            }
        }
    }

    public void RemoveClothingItem(string itemType, string itemFile)
    {
		for (int i = 0; i < attachScript.wornItems.Count; i++)
        {
			if (attachScript.wornItems[i].ItemType == itemType)
            {
				attachScript.RemoveClothes(attachScript.wornItems[i]);
                break;
            }
        }
    }
}