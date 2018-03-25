using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager
{
		protected string filePath;
		//private data;
		
		//private SaveData

	public SaveManager (string filename)
	{
		this.filePath = Application.persistentDataPath + filename;
	}


	public void Save (BodyscanSave.BodyData input)
	{//save file 
        this.filePath = input.file; 
        BinaryFormatter bin = new BinaryFormatter();
		FileStream file = File.Create(this.filePath);
		bin.Serialize(file, input);
		file.Close();
	}

	public void Load (BodyscanSave.BodyData input)
	{//load file
        this.filePath = input.file;
        if (File.Exists (this.filePath)) {
			BinaryFormatter bin = new BinaryFormatter ();
			FileStream file = File.Open (filePath, FileMode.Open);
			//data = (Save)bin.Deserialize (file);
			file.Close ();
		}
		//return data;
	}
}


