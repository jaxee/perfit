using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadModel : MonoBehaviour {

    public ModelSave model;
    public SkinnedMeshRenderer target; 

	// Use this for initialization
	void Start () {
        model = FindObjectOfType<ModelSave>();
        ApplyChange();

    }

    public void ApplyChange() {
        target = GameObject.FindGameObjectWithTag("Unit").GetComponentInChildren< SkinnedMeshRenderer>();
        target.sharedMesh = model.mesh;
        target.material = model.skin;
    }

    private void Update()
    {
        if (!target)
        {
            Debug.Log("new target");
            ApplyChange();
        }
    }

}
