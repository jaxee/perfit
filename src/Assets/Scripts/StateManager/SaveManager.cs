using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;


[CreateAssetMenu]
public class SaveManager:ResettableScriptableObject
{
		protected string filePath;
		
		//private SaveData
	public override void Reset ()
	{
		this.filePath = "";
		
	}
	public SaveManager ()
	{
		this.filePath = "";
	}


	public void Save (BodyscanSave.Body input)
	{//save file 
        this.filePath = input.file; 
        BinaryFormatter bin = new BinaryFormatter();
		FileStream file = File.Create(this.filePath);
		bin.Serialize(file, input);
		Debug.Log ("Saved" + this.filePath);
		file.Close();
	}

	public bool Load (ref BodyscanSave.Body input)
	{//load file
        this.filePath = input.file;
		BodyscanSave body = new BodyscanSave();
		if (File.Exists (this.filePath)) {
			BinaryFormatter bin = new BinaryFormatter ();
			FileStream file = File.Open (filePath, FileMode.Open);
			input = (BodyscanSave.Body)bin.Deserialize (file);
			Debug.Log ("loaded" + this.filePath);
			file.Close ();
			return true;
		}
		return false;
	}
}


