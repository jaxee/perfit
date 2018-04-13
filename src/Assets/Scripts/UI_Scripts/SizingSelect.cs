using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizingSelect : MonoBehaviour {
    //small model base measurements 
    private static float s_height = 64f;
    private static float s_waist = 25f;
    private static float s_hip = 35f;
    private static float s_bust = 33f;
    //medium 
    //small model base measurements 
    private static float m_height = 66f;
    private static float m_waist = 26f;
    private static float m_hip = 45f;
    private static float m_bust = 33f;

    //large model base measurements
    private static float l_height = 70f;
    private static float l_waist = 30f;
    private static float l_hip = 50f;
    private static float l_bust = 33f;


    private obj ffd;
    private ModelSave modelSave;
	public BodyscanSave bodyData; 
	public SkinnedMeshRenderer target;

    // Use this for initialization
    void Start() {
        ffd 				= FindObjectOfType<obj>();
		bodyData 			= FindObjectOfType<BodyscanSave>();
        modelSave   		= FindObjectOfType<ModelSave>();
		modelSave.original  = target;

    }

    private void ResetModel() {
		target = modelSave.original;
		target.sharedMesh.RecalculateBounds ();
		target.sharedMesh.RecalculateNormals ();
    }

    public void ApplySizing(int size){
		ResetModel();
        if (size == 1) {
			bodyData.Bust 	= s_bust;
			bodyData.Waist 	= s_waist;
			bodyData.Height = s_height;
			bodyData.Hip 	= s_hip;


			StartCoroutine (ffd.applyFFD ());

        }
        if (size == 2) {
			bodyData.Bust = m_bust;
			bodyData.Waist = m_waist;
			bodyData.Height = m_height;
			bodyData.Hip = m_hip;

			StartCoroutine (ffd.applyFFD ());

        }
        if (size == 3)
        {
			bodyData.Bust = l_bust;
			bodyData.Waist = l_waist;
			bodyData.Height = l_height;
			bodyData.Hip = l_hip;


			StartCoroutine (ffd.applyFFD ());
        }






    }
}
