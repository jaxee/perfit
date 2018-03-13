using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager
{
		protected string filePath;
		private Save data;
		
		//private SaveData

	public SaveManager (string filename)
		{
			this.filePath = Application.persistentDataPath + filename;
			Debug.Log ("file saved as: " + this.filePath);
			this.data = new Save ();
			this.loadData ();
		}


		public void saveData(Save input)
		{//save file 
			BinaryFormatter bin = new BinaryFormatter();
			FileStream file = File.Create(this.filePath);
			bin.Serialize(file, input);
			file.Close();
		}

		public Save loadData()
		{//load file
			if (File.Exists (this.filePath)) {
				BinaryFormatter bin = new BinaryFormatter ();
				FileStream file = File.Open (filePath, FileMode.Open);
				data = (Save)bin.Deserialize (file);
				file.Close ();
			}
			return data;
		}
}


