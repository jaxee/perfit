using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadModel : MonoBehaviour {

    public ModelSave model;
    public SkinnedMeshRenderer target; 

	// Use this for initialization
	void Start () {
        model = FindObjectOfType<ModelSave>();
        target = GameObject.FindObjectOfType<SkinnedMeshRenderer>();
        target.sharedMesh = model.mesh;
        target.material = model.skin; 
	}

}
