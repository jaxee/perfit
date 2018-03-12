using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Winterdust;
public class AttachClothing : MonoBehaviour
{
    #region Fields
    //gameObjects
    public GameObject avatar;
	public int ClothingSize; // 1 = Small 2 = Medium 3 = Large
	public GameObject wornDress;
	public CapsuleCollider[] colliders;
	public AddCloth addClothScript;
	private Color black;


    //lists
	public List<ClothingItem> wornItems = new List<ClothingItem>();
    //ints
    private int totalSlots;
    #endregion

    #region Monobehaviour

	public void OnStart () {
		avatar = GameObject.Find ("UNITY_FEMALE");

		black = new Color (255, 255, 255);

	}
	public void attachGarments(){

		if (GameObject.Find ("underwear") && avatar.transform.Find ("QuickRigCharacter_Reference").gameObject) {
			GameObject undiesGO = GameObject.Find ("underwear");
			wornDress = undiesGO;
			GameObject skeletonGO = avatar.transform.Find ("QuickRigCharacter_Reference").gameObject;// Debug.Log (modelGO.name + skeletonGO.name); 
			//Debug.Log (undiesGO);
			//Debug.Log (skeletonGO);
			MeshSkinner ms = new MeshSkinner (undiesGO, skeletonGO);

			ms.work ();
			ms.finish ();

			SkinnedMeshRenderer sk = GameObject.Find ("SkinnedVersion").GetComponent<SkinnedMeshRenderer> ();
			sk.material.color = black;

		}
	}
    public void InitializeClothingItemsList()
    {
		attachGarments ();
        totalSlots = 1;

        for (int i = 0; i < totalSlots; i++)
        {
			wornItems.Add(new ClothingItem());
        }

        AddClothingToList(0); //Dresses
		//AddClothingToList(1); //Underwear


//        AddEquipmentToList(1); //Chest
//        AddEquipmentToList(2); //Hair 
//        AddEquipmentToList(3); //Beard 
//        AddEquipmentToList(4); //Mustache
//        AddEquipmentToList(5); //HandRight
//        AddEquipmentToList(6); //ChestArmor
//        AddEquipmentToList(7); //Feet

	
    }

    public void AddClothingToList(int id)
    {
        for (int i = 0; i < wornItems.Count; i++)
        {
            if (wornItems[id].ItemID == -1)
            {
                wornItems[id] = ItemDatabase.instance.FetchItemByID(id);
                break;
            }
        }
    }

	public void AddClothes(ClothingItem clothesToAdd)
    {
        if (clothesToAdd.ItemType == "Dress")
            wornDress = AddClothesHelper(wornDress, clothesToAdd);
		//else if
    }

	public GameObject AddClothesHelper(GameObject wornItem, ClothingItem itemToAddToWornItem)
    {
        wornItem = Wear(itemToAddToWornItem.ItemPrefab, wornItem);
		wornItem.name = itemToAddToWornItem.FileName;
        return wornItem;
    }

	public void RemoveClothes(ClothingItem clothesToRemove)
    {
        if (clothesToRemove.ItemType == "Dress")
            wornDress = RemoveClothesHelper(wornDress, 0);
		//else if
    
    }

    public GameObject RemoveClothesHelper(GameObject wornItem, int nakedItemIndex)
    {
        wornItem = RemoveWorn(wornItem);
        wornItems[nakedItemIndex] = ItemDatabase.instance.FetchItemByID(nakedItemIndex);
        return wornItem;
    }

    #endregion

    private GameObject RemoveWorn(GameObject wornClothing)
    {
        if (wornClothing == null)
            return null;

		if (GameObject.Find ("SkinnedVersion")) {
			DestroyImmediate (GameObject.Find ("SkinnedVersion").GetComponent<Cloth> ());
		}
        GameObject.Destroy(wornClothing);
        return null;
    }

    private GameObject Wear(GameObject clothing, GameObject wornClothing)
    {
        if (clothing == null) return null;
        clothing = (GameObject)GameObject.Instantiate(clothing);

		//clothScript.GenerateCloth (clothing);

        wornClothing = AttachModels(clothing, avatar);
        return wornClothing;
    }
	void OnMouseDrag(){
		float rotSpeed = 300;
		float rotX = Input.GetAxis ("Mouse X") *rotSpeed *Mathf.Deg2Rad;

		if (wornDress) {
			wornDress.transform.Find ("QuickRigCharacter_Reference").transform.Rotate (Vector3.up, -rotX);
			//Debug.Log ("meow");
		} else {
			GameObject.Find ("QuickRigCharacter_Reference").transform.Rotate (Vector3.up, -rotX);
		}

	}


    public GameObject AttachModels(GameObject ClothingModel, GameObject Character)
    {		
		
		DestroyImmediate (GameObject.Find ("underwear"));
		DestroyImmediate (avatar); 
		DestroyImmediate (GameObject.Find("SkinnedVersion")); 
		DestroyImmediate (GameObject.Find("QuickRigCharacter_Reference")); 

		GameObject newHuman = Instantiate (Resources.Load ("UNITY_FEMALE")) as GameObject;
		avatar = newHuman;
		float PIN_CONSTANT = 3;

		GameObject finalProduct;
		Cloth clothComponent;
		DestroyClothing ();
		GameObject modelGO = ClothingModel;
		GameObject skeletonGO = avatar.transform.Find("QuickRigCharacter_Reference").gameObject;// Debug.Log (modelGO.name + skeletonGO.name); 
		GameObject cube  = GameObject.Find("Pin");
		//Debug.Log (skeletonGO);
		MeshSkinner ms = new MeshSkinner(modelGO, skeletonGO);
		ms.work();
		ms.finish();
		finalProduct = GameObject.Find ("SkinnedVersion");


		finalProduct.AddComponent<Cloth> ();

		if (ClothingSize == 3) {
			PIN_CONSTANT = 7.2f; 
			////////L - Dress 01 = 8.4
		}
		if (ClothingSize == 1) {
			PIN_CONSTANT = 4f;
		}
		clothComponent = finalProduct.GetComponent<Cloth> ();
		clothComponent.enabled = false;
		clothComponent.damping = 0.5f;
		clothComponent.bendingStiffness = 1f;
			ClothSkinningCoefficient[] newConstraints; 
			newConstraints = clothComponent.coefficients;

			for (int i = 0; i < clothComponent.vertices.Length; i++) {
				float dist = Vector3.Distance (clothComponent.vertices [i], cube.transform.position);
				//Debug.Log (dist);

				if (dist >4f) {
					newConstraints [i].maxDistance = 0.01f; //https://docs.unity3d.com/ScriptReference/ClothSkinningCoefficient-maxDistance.html

				}
			
			}
			//newConstraints[0].maxDistance = 0;

			//https://answers.unity.com/questions/966554/set-unity-5-cloth-constraints-from-code.html
			clothComponent.coefficients = newConstraints;
			clothComponent.enabled = true;



			//REDO COLLIDERS.... after lena 
			colliders = new CapsuleCollider[9];
			colliders [0] = GameObject.Find ("QuickRigCharacter_LeftUpLeg").GetComponent<CapsuleCollider> ();
		colliders [1] = GameObject.Find ("QuickRigCharacter_RightUpLeg").GetComponent<CapsuleCollider> ();
		colliders [2] = GameObject.Find ("QuickRigCharacter_LeftLeg").GetComponent<CapsuleCollider> ();
		colliders [3] = GameObject.Find ("QuickRigCharacter_RightLeg").GetComponent<CapsuleCollider> ();
		colliders [4] = GameObject.Find ("QuickRigCharacter_Spine").GetComponent<CapsuleCollider> ();
		colliders [5] = GameObject.Find ("QuickRigCharacter_Spine1").GetComponent<CapsuleCollider> ();
		colliders [6] = GameObject.Find ("QuickRigCharacter_Spine2").GetComponent<CapsuleCollider> ();
		colliders [7] = GameObject.Find ("QuickRigCharacter_Rbutt_J").GetComponent<CapsuleCollider> ();
		colliders [8] = GameObject.Find ("QuickRigCharacter_Lbutt_J").GetComponent<CapsuleCollider> ();



			clothComponent.capsuleColliders = colliders;

			//Debug.Log (GameObject.Find ("Character_LeftUpLeg").GetComponent<CapsuleCollider> ().radius);
			//    skinnedMeshRenderers.bones = skinnedCharMeshRenderer.bones;
		wornDress = ClothingModel;

		return ClothingModel;
    }

	private IEnumerator DestroyClothing()
	{
		Destroy(GameObject.Find("SkinnedVersion"));

		yield return new WaitForSeconds(1);




	}

}