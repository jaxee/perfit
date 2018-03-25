using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateModel : MonoBehaviour {
	public float speed = 1.0f; //how fast the object should rotate
	public float offset = 0.0f;

	public GameObject humanObject;
	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		humanObject.transform.Rotate(0,20*Time.deltaTime,0);
		GameObject.Find("Platform").transform.Rotate(0,20*Time.deltaTime,0);

	}
}
