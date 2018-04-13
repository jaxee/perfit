using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DressInfo :MonoBehaviour {

	public GameObject scrolllist;

	// Use this for initialization
	void Start () {
		
	}
	public void DiableClick(){
		
		foreach (Interface btn in scrolllist.GetComponentsInChildren<Interface>()) {
			scrolllist.SetActive(false);
			btn.enabled = false;
		}
	}

	public void EnableClick(){

		foreach (Interface btn in scrolllist.GetComponentsInChildren<Interface>()) {
			scrolllist.SetActive(true);
			btn.enabled = true;
		}
	}

	public void Reset(GameObject module)
	{
		module.SetActive(false);
	}

	public void ToggleModule(GameObject module) {
		if (module.activeSelf)
		{
			Reset(module);
		}
		else
		{
			module.SetActive(true);
		}
	}
}
