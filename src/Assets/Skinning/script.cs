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
		GameObject modelGO = GameObject.Find("small_blueDress_noInside");
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
				newConstraints [i].maxDistance = 0;
			}
		}
		//newConstraints[0].maxDistance = 0;

		clothComponent.coefficients = newConstraints;
		clothComponent.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {

	}
}
