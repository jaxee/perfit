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


    //lists
	public List<ClothingItem> wornItems = new List<ClothingItem>();
    //ints
    private int totalSlots;
    #endregion

    #region Monobehaviour

	public void OnStart () {
		avatar = GameObject.Find ("UNITY_FEMALE");
	}
    public void InitializeClothingItemsList()
    {
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

    public GameObject AttachModels(GameObject ClothingModel, GameObject Character)
    {		

		DestroyImmediate (avatar); 
		DestroyImmediate (GameObject.Find("SkinnedVersion")); 
		GameObject newHuman = Instantiate (Resources.Load ("UNITY_FEMALE")) as GameObject;
		avatar = newHuman;
		float PIN_CONSTANT = 3;

		GameObject finalProduct;
		Cloth clothComponent;
		DestroyClothing ();
		GameObject modelGO = ClothingModel;
		GameObject skeletonGO = GameObject.Find("Rig");// Debug.Log (modelGO.name + skeletonGO.name); 
		GameObject cube  = GameObject.Find("Pin");
		//Debug.Log (skeletonGO);
		MeshSkinner ms = new MeshSkinner(modelGO, skeletonGO);
		ms.work();
		ms.finish();
		finalProduct = GameObject.Find ("SkinnedVersion");

		finalProduct.AddComponent<Cloth> ();

		if (ClothingSize == 3) {
			PIN_CONSTANT = 7.2f;
		}

		clothComponent = finalProduct.GetComponent<Cloth> ();
		clothComponent.enabled = false;

			ClothSkinningCoefficient[] newConstraints; 
			newConstraints = clothComponent.coefficients;

			for (int i = 0; i < clothComponent.vertices.Length; i++) {
				float dist = Vector3.Distance (clothComponent.vertices [i], cube.transform.position);
				//Debug.Log (dist);

				if (dist > 3) {
					newConstraints [i].maxDistance = 0.01f; //https://docs.unity3d.com/ScriptReference/ClothSkinningCoefficient-maxDistance.html

				}
			
			}
			//newConstraints[0].maxDistance = 0;

			//https://answers.unity.com/questions/966554/set-unity-5-cloth-constraints-from-code.html
			clothComponent.coefficients = newConstraints;
			clothComponent.enabled = true;

			//renderer

			//add colliders
			colliders = new CapsuleCollider[4];
			colliders [0] = GameObject.Find ("Character_LeftUpLeg").GetComponent<CapsuleCollider> ();
			colliders [1] = GameObject.Find ("Character_RightUpLeg").GetComponent<CapsuleCollider> ();
			colliders [2] = GameObject.Find ("Character_LeftLeg").GetComponent<CapsuleCollider> ();
			colliders [3] = GameObject.Find ("Character_RightLeg").GetComponent<CapsuleCollider> ();


			clothComponent.capsuleColliders = colliders;

			//Debug.Log (GameObject.Find ("Character_LeftUpLeg").GetComponent<CapsuleCollider> ().radius);
			//    skinnedMeshRenderers.bones = skinnedCharMeshRenderer.bones;


		return ClothingModel;
    }

	private IEnumerator DestroyClothing()
	{
		Destroy(GameObject.Find("SkinnedVersion"));

		yield return new WaitForSeconds(1);




	}

}