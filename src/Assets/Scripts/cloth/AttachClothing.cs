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
	public CapsuleCollider[] tummycolliders;
	public CapsuleCollider[] hipscolliders;
	public CapsuleCollider[] legscolliders;


	private Color black;
	public ModelSave model;
	public SkinnedMeshRenderer target; 


    //lists
	public List<ClothingItem> wornItems = new List<ClothingItem>();
    //ints
    private int totalSlots;

    private GameObject block;
    private BodyscanSave bodyData;
    private Sizing sizing;
    #endregion

    #region Monobehaviour

    private void Start()
    {
        bodyData = FindObjectOfType<BodyscanSave>();
        block = new GameObject("block");
        block.AddComponent<Sizing>();
        sizing = block.GetComponent<Sizing>();
    }

    public void OnStart () {
        avatar = GameObject.Find ("UNITY_FEMALE");
		black = new Color (255, 255, 255);
    }
	public void attachGarments(){

	}
    public void InitializeClothingItemsList()
    {
		//attachGarments ();
		//Debug.Log ("hello");
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

		clothing.transform.eulerAngles = new Vector3 (0, -180, 0);

		float heightRatio = sizing.ConvertInput(1, bodyData.Height)*0.1f;
		clothing.transform.position = new Vector3 (0, clothing.transform.position.y + heightRatio, 0);


        wornClothing = AttachModels(clothing, avatar);
        return wornClothing;
    }
	void OnMouseDrag(){
		//Debug.Log ("ROTATE");

		float rotSpeed = 300;
		float rotX = Input.GetAxis ("Mouse X") *rotSpeed *Mathf.Deg2Rad;

		if (wornDress) {

			wornDress.transform.Find ("QuickRigCharacter3_Reference").transform.Rotate (Vector3.up, -rotX);
			//Debug.Log ("meow");
		} else {
			GameObject.Find ("QuickRigCharacter3_Reference").transform.Rotate (Vector3.up, -rotX);
		}

	}


    public GameObject AttachModels(GameObject ClothingModel, GameObject Character)
    {		
		
		DestroyImmediate (avatar); 
		DestroyImmediate (GameObject.Find("SkinnedVersion")); 
		DestroyImmediate (GameObject.Find("QuickRigCharacter3_Reference")); 

		GameObject newHuman = Instantiate (Resources.Load ("UNITY_FEMALE")) as GameObject;
		avatar = newHuman;
		float PIN_CONSTANT = 3;


       // applying ffds
        model = FindObjectOfType<ModelSave>();
        target = avatar.GetComponentInChildren<SkinnedMeshRenderer> ();
		target.sharedMesh = model.mesh;
		target.material = model.skin; 

		GameObject finalProduct;
		Cloth clothComponent;
		DestroyClothing ();
		GameObject modelGO = ClothingModel;
		GameObject skeletonGO = avatar.transform.Find("QuickRigCharacter3_Reference").gameObject;// Debug.Log (modelGO.name + skeletonGO.name); 
		GameObject cube  = ClothingModel.transform.Find("Pin").gameObject;
		Debug.Log (cube);
		MeshSkinner ms = new MeshSkinner(modelGO, skeletonGO);
		ms.work();
		//ms.quickFix ();
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
		//clothComponent.enabled = false;
		clothComponent.damping = 0.5f;
		clothComponent.bendingStiffness = 1f;
		//clothComponent.useGravity = false;
		ClothSkinningCoefficient[] newConstraints; 
		newConstraints = clothComponent.coefficients;

		for (int i = 0; i < clothComponent.vertices.Length; i++) {
			float dist = Vector3.Distance (clothComponent.vertices [i], cube.transform.position);
			//Debug.Log (dist);

			if (dist > 2) {
				
				if (ClothingModel.tag == "sleeved") {
					//Debug.Log ("HEY");
					newConstraints [i].maxDistance = 0.02f;
				}
				else {
					newConstraints [i].maxDistance = 0; //https://docs.unity3d.com/ScriptReference/ClothSkinningCoefficient-maxDistance.html
				}

			} 

			else {
				newConstraints [i].maxDistance = 0.2f;
			}
		}
		//newConstraints[0].maxDistance = 0;

		//https://answers.unity.com/questions/966554/set-unity-5-cloth-constraints-from-code.html
		clothComponent.coefficients = newConstraints;
		clothComponent.enabled = true;


		//ALL COLLIDERS
		colliders = new CapsuleCollider[25];
		colliders [0] = GameObject.Find ("QuickRigCharacter3_LeftUpLeg").GetComponent<CapsuleCollider> ();  //legs
		colliders [1] = GameObject.Find ("QuickRigCharacter3_RightUpLeg").GetComponent<CapsuleCollider> (); //legs
		colliders [2] = GameObject.Find ("QuickRigCharacter3_LeftLeg").GetComponent<CapsuleCollider> (); //legs
		colliders [3] = GameObject.Find ("QuickRigCharacter3_RightLeg").GetComponent<CapsuleCollider> (); //legs
		colliders [4] = GameObject.Find ("QuickRigCharacter3_Hips_J").GetComponent<CapsuleCollider> (); //hips
		colliders [4] = GameObject.Find ("QuickRigCharacter3_RightSide_J").GetComponent<CapsuleCollider> (); //hips
		colliders [5] = GameObject.Find ("QuickRigCharacter3_LeftUpLeg_J").GetComponent<CapsuleCollider> (); //legs
		colliders [6] = GameObject.Find ("QuickRigCharacter3_RightUpLeg_J").GetComponent<CapsuleCollider> (); //legs
		colliders [7] = GameObject.Find ("QuickRigCharacter3_Rbutt_J").GetComponent<CapsuleCollider> (); //hips
		colliders [8] = GameObject.Find ("QuickRigCharacter3_Lbutt_J").GetComponent<CapsuleCollider> (); //hips
		colliders [9] = GameObject.Find ("QuickRigCharacter3_Hips").GetComponent<CapsuleCollider> (); //hips
		colliders [10] = GameObject.Find ("QuickRigCharacter3_LeftSide_J").GetComponent<CapsuleCollider> (); //hips
		colliders [11] = GameObject.Find ("QuickRigCharacter3_HipsCenter_J").GetComponent<CapsuleCollider> (); //hips
		colliders [12] = GameObject.Find ("QuickRigCharacter3_LeftKnee2_J").GetComponent<CapsuleCollider> (); //legs
		colliders [13] = GameObject.Find ("QuickRigCharacter3_RightKnee_J").GetComponent<CapsuleCollider> (); //legs
		colliders [14] = GameObject.Find ("QuickRigCharacter3_LeftKnee_J").GetComponent<CapsuleCollider> (); //legs
		colliders [16] = GameObject.Find ("QuickRigCharacter3_Rear_J").GetComponent<CapsuleCollider> (); //hips
		colliders [17] = GameObject.Find ("QuickRigCharacter3_Rear2_J").GetComponent<CapsuleCollider> (); //hips
		colliders [18] = GameObject.Find ("QuickRigCharacter3_Rear3_J").GetComponent<CapsuleCollider> (); //hips
		colliders [19] = GameObject.Find ("QuickRigCharacter3_RightKnee2_J").GetComponent<CapsuleCollider> (); //legs
		colliders [20] = GameObject.Find ("QuickRigCharacter3_Spine").GetComponent<CapsuleCollider> (); //tummy
		colliders [21] = GameObject.Find ("QuickRigCharacter3_Spine1").GetComponent<CapsuleCollider> (); //tummy

		clothComponent.capsuleColliders = colliders;

		//BODY SPECIFIC COLLIDERS.. modified at different ratios. 
		tummycolliders = new CapsuleCollider[2];
		tummycolliders [0] = GameObject.Find ("QuickRigCharacter3_Spine").GetComponent<CapsuleCollider> (); //tummy
		tummycolliders [1] = GameObject.Find ("QuickRigCharacter3_Spine1").GetComponent<CapsuleCollider> (); //tummy
	
		hipscolliders = new CapsuleCollider[10];
		hipscolliders [0] = GameObject.Find ("QuickRigCharacter3_Hips_J").GetComponent<CapsuleCollider> ();
		hipscolliders [1] = GameObject.Find ("QuickRigCharacter3_RightSide_J").GetComponent<CapsuleCollider> ();
		hipscolliders [2] = GameObject.Find ("QuickRigCharacter3_Rbutt_J").GetComponent<CapsuleCollider> (); //hips
		hipscolliders [3] = GameObject.Find ("QuickRigCharacter3_Lbutt_J").GetComponent<CapsuleCollider> (); //hips
		hipscolliders [4] = GameObject.Find ("QuickRigCharacter3_Hips").GetComponent<CapsuleCollider> (); //hips
		hipscolliders [5] = GameObject.Find ("QuickRigCharacter3_LeftSide_J").GetComponent<CapsuleCollider> (); //hips
		hipscolliders [6] = GameObject.Find ("QuickRigCharacter3_HipsCenter_J").GetComponent<CapsuleCollider> (); //hips
		hipscolliders [7] = GameObject.Find ("QuickRigCharacter3_Rear_J").GetComponent<CapsuleCollider> (); //hips
		hipscolliders [8] = GameObject.Find ("QuickRigCharacter3_Rear2_J").GetComponent<CapsuleCollider> (); //hips
		hipscolliders [9] = GameObject.Find ("QuickRigCharacter3_Rear3_J").GetComponent<CapsuleCollider> (); //hips

		legscolliders = new CapsuleCollider[10];
		legscolliders [0] = GameObject.Find ("QuickRigCharacter3_LeftUpLeg").GetComponent<CapsuleCollider> ();  //legs
		legscolliders [1] = GameObject.Find ("QuickRigCharacter3_RightUpLeg").GetComponent<CapsuleCollider> (); //legs
		legscolliders [2] = GameObject.Find ("QuickRigCharacter3_LeftLeg").GetComponent<CapsuleCollider> (); //legs
		legscolliders [3] = GameObject.Find ("QuickRigCharacter3_RightLeg").GetComponent<CapsuleCollider> (); //legs
		legscolliders [4] = GameObject.Find ("QuickRigCharacter3_LeftUpLeg_J").GetComponent<CapsuleCollider> (); //legs
		legscolliders [5] = GameObject.Find ("QuickRigCharacter3_RightUpLeg_J").GetComponent<CapsuleCollider> (); //legs
		legscolliders [6] = GameObject.Find ("QuickRigCharacter3_LeftKnee2_J").GetComponent<CapsuleCollider> (); //legs
		legscolliders [7] = GameObject.Find ("QuickRigCharacter3_RightKnee_J").GetComponent<CapsuleCollider> (); //legs
		legscolliders [8] = GameObject.Find ("QuickRigCharacter3_LeftKnee_J").GetComponent<CapsuleCollider> (); //legs
		legscolliders [9] = GameObject.Find ("QuickRigCharacter3_RightKnee2_J").GetComponent<CapsuleCollider> (); //legs

		//luk
		//FFDColliders(ratio1,ratio2,ratio3);

		return ClothingModel;
    }

	public void FFDColliders (float hipsRatio, float tummyRatio, float legsRatio ) {

		//luk

		for (int i = 0; i < hipscolliders.Length; i++) {
				
			hipscolliders [i].radius *= hipsRatio; //ratio
					 		
		}
		for (int i = 0; i < tummycolliders.Length; i++) {

			tummycolliders [i].radius  *= tummyRatio; //ratio

		}
		for (int i = 0; i < legscolliders.Length; i++) {

			legscolliders [i].radius *= legsRatio; //ratio

		}

	}

	private IEnumerator DestroyClothing()
	{
		Destroy(GameObject.Find("SkinnedVersion"));

		yield return new WaitForSeconds(1);




	}

}