using UnityEngine;
using System.Collections;

public class BodyscanSave : Saver
{
	public struct Body{
	    public string file;
	    public float Height;
	    public float Waist;
	    public float Bust;
	};


	private Body data = new Body();
	public string file;
	public float Height;
	public float Waist;
	public float Bust;


	protected override void Save(){
		data.file = file;
		data.Height = Height;
		data.Waist = Waist;
		data.Bust = Bust;

        saveManager.Save(file, data);
    }


    protected override void Load()
    {
		Body _data = new Body();
        if (saveManager.Load(file, ref _data))
        {
            Height = _data.Height;
            Waist = _data.Waist;
            Bust = _data.Bust;
        }
        else {
            Debug.Log("no saved BodyScan data");
        }
    }
}
