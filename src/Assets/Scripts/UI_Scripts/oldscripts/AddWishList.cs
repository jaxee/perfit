using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddWishList : MonoBehaviour {


	Color redc;
	int counter;
	// Use this for initialization
	Button b;

	void Start () {
		b = this.GetComponent<Button>(); 
		redc = new Color (234, 57, 83);
		//ColorBlock cb = b.colors;
		//cb.normalColor = Color.white;
		//b.colors = cb;
	}

	public void changeColor()
	{
		Debug.Log ("1");
		counter++;
		if (counter % 2 == 1)
		{
			Debug.Log ("2");

			//ColorBlock cb = b.colors;
			//cb.normalColor = Color.red;
			//b.colors = cb;
			b.image.color = Color.red;
		}
		else {
			Debug.Log ("3");

			//ColorBlock cb = b.colors;
			//cb.normalColor = Color.white;
			//b.image.color = new Color (255, 255,255);
			b.image.color = Color.white;
		}
	}
}
