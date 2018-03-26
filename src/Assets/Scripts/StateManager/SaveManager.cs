using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class SaveManager: ResettableScriptableObject
{
	protected string filePath;

	[Serializable]
	public class DataList<T>
	{

		public List<string> keys = new List<string> ();
		public List<T> values = new List<T> ();

		public void Clear ()
		{
			keys.Clear ();
			values.Clear ();
		}

		public void SetValue(string key, T value){
			// Find the index of the keys and values based on the given key.
			int index = keys.FindIndex(x => x == key);
			if (index > -1)
			{
				values[index] = value;
			}
			else
			{
				keys.Add (key);
				values.Add (value);
			}
		}

		public bool GetValue (string key, ref T value)
		{
			int index = keys.FindIndex(x => x == key);

			if (index > -1)
			{
				value = values[index];
				return true;
			}
			return false;
		}

	}



	public DataList<BodyscanSave.Body> bodyDataList = new DataList<BodyscanSave.Body> ();

	public override void Reset ()
	{
		bodyDataList.Clear ();
	}

	private void Save<T>(DataList<T> lists, string key, T value)
	{
		lists.SetValue(key, value);
	}

	private bool Load<T>(DataList<T> lists, string key, ref T value)
	{
		return lists.GetValue(key, ref value);
	}



	public void Save (String fileName, BodyscanSave.Body input)
	{//save file
		Save(bodyDataList, fileName, input);
	}
	public bool Load (String fileName,ref BodyscanSave.Body input)
	{//load file
		return Load(bodyDataList, fileName, ref input);
	}
}
