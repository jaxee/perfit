using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddToBag : MonoBehaviour {

	public GameObject model;
	public GameObject AddBtn;
	public string dressNo;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onClickAdd() {
		Debug.Log ("Hi");
		dressNo = this.name;
		Debug.Log (dressNo);

	}
}
