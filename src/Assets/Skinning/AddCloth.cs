using UnityEngine;
using System.Collections.Generic;

public class AddCloth : MonoBehaviour
{
	public GameObject go1;
	public GameObject go2;
	public void addClothToMe(string clothingName)
	{
		//https://answers.unity.com/questions/530178/how-to-get-a-component-from-an-object-and-add-it-t.html
		go1 = GameObject.Find (clothingName);
		go2 = GameObject.Find ("SkinnedVersion");

			var t1 = go1.GetComponent<Cloth>();
			var t2 = go2.GetComponent<Cloth>();
			var temp = new GameObject("_TEMP").AddComponent<Cloth>();
		    temp = t1;
			//t1 = t2;
			t2 = temp;
			if (UnityEditorInternal.ComponentUtility.CopyComponent(t1)) {
				if (UnityEditorInternal.ComponentUtility.PasteComponentValues(temp)) {
					if (UnityEditorInternal.ComponentUtility.CopyComponent(t2)) {
						if (UnityEditorInternal.ComponentUtility.PasteComponentValues(t1)) {
							if (UnityEditorInternal.ComponentUtility.CopyComponent(temp)) {
								if (UnityEditorInternal.ComponentUtility.PasteComponentValues(t2)) {
								Debug.Log("DONE");
								}
							}
						}
					}
				}
			}
			Destroy(temp.gameObject);

	}

}