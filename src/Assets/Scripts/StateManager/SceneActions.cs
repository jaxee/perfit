using UnityEngine;
using System.Collections;

public class SceneActions : MonoBehaviour
{ 
	private StateManager stateManager; 

	// Use this for initialization
	void Start ()
	{
		stateManager = FindObjectOfType<StateManager> ();
	
	}

	public void action(string sceneName){
		
		stateManager.loadAndFade (sceneName);
	}


}

