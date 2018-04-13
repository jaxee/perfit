using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Winterdust;


public class AddHair : MonoBehaviour {

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void OnPointerClick(PointerEventData eventData)
	{
		if (gameObject.name == "hairBtn1") {

			GameObject newHair = Instantiate (Resources.Load ("Hair/Ponytail")) as GameObject;
			GameObject parent = GameObject.Find ("QuickRigCharacter3_Head"); 

			newHair.transform.parent = parent.transform;



//			GameObject modelGO = newHair;
//			GameObject skeletonGO = GameObject.Find ("QuickRigCharacter3_Reference").gameObject;// Debug.Log (modelGO.name + skeletonGO.name); 
//
//			MeshSkinner ms = new MeshSkinner (modelGO, skeletonGO);
//			ms.work ();
//			ms.finish ();
		}

		if (gameObject.name == "hairBtn2") {

			GameObject newHair = Instantiate (Resources.Load ("Hair/Ponytail")) as GameObject;
			GameObject parent = GameObject.Find ("QuickRigCharacter3_Head"); 

			newHair.transform.parent = parent.transform;

//			GameObject newHair = Instantiate (Resources.Load ("Hair/Afro")) as GameObject;
//
//			GameObject modelGO = newHair;
//			GameObject skeletonGO = GameObject.Find ("QuickRigCharacter3_Reference").gameObject;// Debug.Log (modelGO.name + skeletonGO.name); 
//
//			MeshSkinner ms = new MeshSkinner (modelGO, skeletonGO);
//			ms.work ();
//			ms.finish ();
		}

	}
		
}
