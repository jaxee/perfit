using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeDressColor : MonoBehaviour {

	Button colorBtn;
	Image colorImg;
	Color currentCol;
	public Material[] materialsNew;
	// Use this for initialization
	void Start () {
		colorBtn = this.GetComponent<Button> ();
		colorImg = this.GetComponent<Image> ();
		currentCol = colorImg.color;

	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("My color" + currentCol.g);
		//Debug.Log ("My color" + currentCol);

	}

	public void onClickColor(){

		materialsNew = new Material[20];
		SkinnedMeshRenderer sk = GameObject.Find ("SkinnedVersion").GetComponent<SkinnedMeshRenderer> ();
		Shader shad = sk.material.shader;
		for (int i = 0; i < sk.materials.Length; i++) {

			sk.materials[i].color = currentCol;

		}

		//sk.materials = materialsNew;

		//sk.material.SetColor ("color", Color.black);
		//sk.material.color = Color.black;
			//currentCol;

		//sk.materials = materialsNew;
		//sk.material = null;

	}
}
