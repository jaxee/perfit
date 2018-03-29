using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class customizer : MonoBehaviour {

    ModelSave model;
    public SkinnedMeshRenderer modelSkin;
    public Material shade0;
    public Material shade1;
    public Material shade2;
    public Material shade3;



    // Use this for initialization
    void Start () {
        model = GameObject.FindObjectOfType<ModelSave>();
	}

    public void setSkin(int skin) {
        switch (skin) {
            case 1:
                modelSkin.material = shade1;
                model.skin = shade1;
                break;
            case 2:
                modelSkin.material = shade2;
                model.skin = shade2;
                break;
            case 3:
                modelSkin.material = shade3;
                model.skin = shade3;
                break;
            default:
                modelSkin.material = shade0;
                model.skin = shade0;
                break;


        }

    }
}
