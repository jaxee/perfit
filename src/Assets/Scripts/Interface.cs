using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Interface : MonoBehaviour, IPointerClickHandler
{
	private GameObject humanObject; //Reference to the main model (Human)

    private ChangeClothes changeClothesScript; //Reference to the change Item script
    private AttachClothing attachScript; //Reference 
    private Text textChild;  //Their text lol can prob delete this l8r

	//animation stuff 
	//https://www.youtube.com/watch?v=4Rc3dlERSWg

	public Animator anim; //animation controller for human
	public int activePos; // 0, 1, 2




	/// animation
	/// 
	/// 	//rotation


    private void Start()
    {

		anim = GameObject.FindGameObjectWithTag("Unit").GetComponent<Animator>();
		activePos = 0; // nothing
		//anim.SetInteger("Pose", activePos); 

		if(GameObject.FindGameObjectWithTag("Unit").gameObject)
       		humanObject = GameObject.FindGameObjectWithTag("Unit").gameObject; //finding the human

			//Reference to the changeGear script to use functions
        	changeClothesScript = humanObject.GetComponent<ChangeClothes>();

			//Reference to the equipment script to use functions
        	attachScript = humanObject.GetComponent<AttachClothing>(); 
			//AddOrRemoveClothes ("naked", "Dress", "underwear01", 0);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
		if (gameObject.name == "Dress1") {
			changeClothesScript.RemoveClothingItem ("Dress", "abc");
			Debug.Log ("Dress 1");
			//humanObject.transform.rotation = Quaternion.identity;
			AddOrRemoveClothes ("naked", "Dress", "dress01", 0);
		} else if (gameObject.name == "Dress2") {
			changeClothesScript.RemoveClothingItem ("Dress", "abc");

			Debug.Log ("Dress 2");
			//humanObject.transform.rotation = Quaternion.identity;

			AddOrRemoveClothes ("naked", "Dress", "dress02", 0);
		} else if (gameObject.name == "Dress3") {
			changeClothesScript.RemoveClothingItem ("Dress", "abc");

			Debug.Log ("Dress 3");
			//humanObject.transform.rotation = Quaternion.identity;

			AddOrRemoveClothes ("naked", "Dress", "dress03", 0);
		} else if (gameObject.name == "Dress4") {
			changeClothesScript.RemoveClothingItem ("Dress", "abc");

			Debug.Log ("Dress 4");
			//humanObject.transform.rotation = Quaternion.identity;

			AddOrRemoveClothes ("naked", "Dress", "dress04", 0);

		} else if (gameObject.name == "Dress5") {
			changeClothesScript.RemoveClothingItem ("Dress", "abc");

			Debug.Log ("Dress 5");
			//humanObject.transform.rotation = Quaternion.identity;

			AddOrRemoveClothes ("naked", "Dress", "dress05", 0);

		}
		else if (gameObject.name == "Dress6") {
			changeClothesScript.RemoveClothingItem ("Dress", "abc");

			Debug.Log ("Dress 6");
			//humanObject.transform.rotation = Quaternion.identity;

			AddOrRemoveClothes ("naked", "Dress", "dress06", 0);

		}
		else if (gameObject.name == "Dress7") {
			changeClothesScript.RemoveClothingItem ("Dress", "abc");

			Debug.Log ("Dress 7");
			//humanObject.transform.rotation = Quaternion.identity;

			AddOrRemoveClothes ("naked", "Dress", "dress07", 0);

		}
		else if (gameObject.name == "Dress8") {
			changeClothesScript.RemoveClothingItem ("Dress", "abc");

			Debug.Log ("Dress 8");
			//humanObject.transform.rotation = Quaternion.identity;

			AddOrRemoveClothes ("naked", "Dress", "dress08", 0);

		}



		//add more

		//https://docs.unity3d.com/ScriptReference/Animation-clip.html
		//ANIMATION https://unity3d.com/learn/tutorials/topics/animation/animator-scripting
		else if (gameObject.name == "PoseButton") {
			Debug.Log ("Pose 1 Animation");

			activePos = 1;
			//anim.SetInteger("Pose", activePos); 

		}
		//ANIMATION
		else if (gameObject.name == "PoseButton2") {
			Debug.Log ("Pose 2 Animation");
			activePos = 2;
			//anim.SetInteger("Pose", activePos); 

		}
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