using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;

public class script : MonoBehaviour {

	public AddCloth addClothScript;
	public GameObject finalProduct;
	public Cloth clothComponent;
	public GameObject cube;
	// Use this for initialization
	void Start () {
		Debug.Log ("HI");
		GameObject modelGO = GameObject.Find("small_blackBodycon_noinside");
		GameObject skeletonGO = GameObject.Find("Rig");
		GameObject cube  = GameObject.Find("PinningCube");
		Debug.Log (skeletonGO);
		MeshSkinner ms = new MeshSkinner(modelGO, skeletonGO);
		ms.work();
	    ms.finish();

		finalProduct = GameObject.Find ("SkinnedVersion");
		finalProduct.AddComponent<Cloth> ();

		clothComponent = finalProduct.GetComponent<Cloth> ();
		clothComponent.enabled = false;

		ClothSkinningCoefficient[] newConstraints; 
		newConstraints = clothComponent.coefficients;

		for (int i = 0; i < clothComponent.vertices.Length; i++) {
			float dist = Vector3.Distance(clothComponent.vertices[i], cube.transform.position);
			Debug.Log (dist);
			if (dist > 425) {
				newConstraints [i].maxDistance = 0.01f; //https://docs.unity3d.com/ScriptReference/ClothSkinningCoefficient-maxDistance.html
			}
		}
		//newConstraints[0].maxDistance = 0;

		//https://answers.unity.com/questions/966554/set-unity-5-cloth-constraints-from-code.html
		clothComponent.coefficients = newConstraints;
		clothComponent.enabled = true;

		//add colliders
	}
	
	// Update is called once per frame
	void Update () {

	}
}
