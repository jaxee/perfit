using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddColliders : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setColliders(GameObject clothingModel){
		Cloth a = clothingModel.GetComponent<Cloth>();
		Debug.Log (a);
	}
}
