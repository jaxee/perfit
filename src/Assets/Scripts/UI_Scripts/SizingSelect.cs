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
    private static float m_waist = 25f;
    private static float m_hip = 35f;
    private static float m_bust = 33f;

    //large model base measurements
    private static float l_height = 70f;
    private static float l_waist = 25f;
    private static float l_hip = 35f;
    private static float l_bust = 33f;
    obj ffd;
    BodyscanSave.Body data;
    ModelSave modelSave;
    private SaveManager bodyData;

    // Use this for initialization
    void Start() {
        ffd = FindObjectOfType<obj>();
        data = new BodyscanSave.Body();
        modelSave = FindObjectOfType<ModelSave>();
        bodyData = FindObjectOfType<SaveManager>();
    }

    private void ResetModel() {

    }

    public void ApplySizing(int size){
        if (size == 1) {
            ResetModel();
            data.Bust = s_bust;
            data.Waist = s_waist;
            data.Height = s_height;
            data.Hip = s_hip;

            bodyData.Save("bodyScan", data);

        }
        if (size == 2) {
            ResetModel();
            data.Bust = m_bust;
            data.Waist = m_waist;
            data.Height = m_height;
            data.Hip = m_hip;

        }
        if (size == 3)
        {
            ResetModel();
            data.Bust = l_bust;
            data.Waist = l_waist;
            data.Height = l_height;
            data.Hip = l_hip;

        }






    }
}
