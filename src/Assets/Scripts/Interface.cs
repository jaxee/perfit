using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Interface : MonoBehaviour, IPointerClickHandler
{
	private GameObject humanObject; //Reference to the main model (Human)

    private ChangeClothes changeClothesScript; //Reference to the change Item script
    private AttachClothing attachScript; //Reference 
    private Text textChild;  //Their text lol can prob delete this l8r
	private ClothAnimation clothAnimationScript;

    private void Start()
    {
        humanObject = GameObject.FindGameObjectWithTag("Unit").gameObject; //finding the human

		//Reference to the changeGear script to use functions
        changeClothesScript = humanObject.GetComponent<ChangeClothes>();

		//Reference to the equipment script to use functions
        attachScript = humanObject.GetComponent<AttachClothing>(); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
		
		if (gameObject.name == "Dress1")
			AddOrRemoveClothes ("naked", "Dress", "dress01", 0);
			
		else if (gameObject.name == "Dress2") {
			Debug.Log ("Dress 2");
			AddOrRemoveClothes ("naked", "Dress", "dress02", 0);
		}
		//add more
    }

    public void AddOrRemoveClothes(string naked, string clothesType, string clothesFileName, int wornItemsIndex)
    {
		//If the model is naked then we will attach the dress to the model. 
        if (attachScript.wornItems[wornItemsIndex].FileName == naked)
        {
			changeClothesScript.AttachClothingItem(clothesType, clothesFileName);
        }
        else if (attachScript.wornItems[wornItemsIndex].FileName == clothesFileName)
        {
			changeClothesScript.RemoveClothingItem(clothesType, clothesFileName);
        }
    }
}
