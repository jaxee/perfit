using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangePose : MonoBehaviour {

	private GameObject humanObject;
	public Button yourButton;
	// Use this for initialization
	void Start () {
		humanObject = GameObject.FindGameObjectWithTag("Unit").gameObject; //finding the human
		Button btn = yourButton.GetComponent<Button>();
		btn.onClick.AddListener(RotateDegrees);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void RotateDegrees(){
		Debug.Log ("Rotate");
		humanObject.transform.Rotate (Vector3.up, 5.0f);

	}

	void TaskOnClick()
	{
		Debug.Log("You have clicked the button!");

	}
}
