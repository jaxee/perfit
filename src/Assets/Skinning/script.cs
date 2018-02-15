using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;

public class script : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject modelGO = GameObject.Find("blackBodycon_small");
		GameObject skeletonGO = GameObject.Find("Rig");

		MeshSkinner ms = new MeshSkinner(modelGO, skeletonGO);
		ms.work();
		ms.finish();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
