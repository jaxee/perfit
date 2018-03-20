using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEngine.SceneManagement;


public enum AppStates {
    HomePage,
    Login,
    SignUp,
    BodyScan, 
    EditProfile,
    Shop,
    Perfit,
    Cart, 
    logout,
};

public class stateManager : MonoBehaviour {

    private float _loadingProgress;
    public  float LoadingProgress { get { return _loadingProgress; } }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
