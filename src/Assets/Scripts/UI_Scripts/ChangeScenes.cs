using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChangeScenes : MonoBehaviour {

    public void ChangeScene(string scenename)
    {
        Application.LoadLevel(scenename);
    }
}
