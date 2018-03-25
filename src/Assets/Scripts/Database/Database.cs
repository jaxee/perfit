using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;


public class Database : MonoBehaviour {

	void Start()
	{
		//StartCoroutine(getRequest("localhost:3000/users"));
		StartCoroutine(Upload());
	}

	IEnumerator getRequest(string uri)
	{
		UnityWebRequest uwr = UnityWebRequest.Get(uri);
		yield return uwr.SendWebRequest();

		if (uwr.isNetworkError)
		{
			Debug.Log("Error While Sending: " + uwr.error);
		}
		else
		{
			Debug.Log("Received: " + uwr.downloadHandler.text);
		}
	}

	IEnumerator Upload()
	{
		WWWForm form = new WWWForm();
		form.AddField("password", "password");
		form.AddField("email", "john@doe.com");

		using (UnityWebRequest www = UnityWebRequest.Post("localhost:3000/login", form))
		{
			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError)
			{
				Debug.Log(www.error);
			}
			else
			{
				Debug.Log("Form upload complete!");
			}
		}
	}
}
