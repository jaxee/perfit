using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dummy : MonoBehaviour {
    private float hips, height, waist, bust;
    GameObject bodyscanSave;
    BodyscanSave values;
    // Use this for initialization
    void Start () {
        bodyscanSave = GameObject.Find("ModelData");
        values = bodyscanSave.GetComponent<BodyscanSave>();
        values.Bust = 37.4f;
        values.Waist = 29.5f;
        values.Height = 70f;
        values.Hip = 28.5f;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown("g")) {
            BodyscanSave.Body data = new BodyscanSave.Body();

            data.Height = height;
            data.Hip = hips;
            data.Waist = waist;
            data.Bust = bust;
            values.saveManager.Save("bodyScan", data);
        }
	}
}
