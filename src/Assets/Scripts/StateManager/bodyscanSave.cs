using UnityEngine;
using System.Collections;

public class BodyscanSave : Saver
{
	public class Body{
	    public string file; 
	    public float Height;
	    public float Waist;
	    public float Bust;
	};
	private Body data;
	public string file; 
	public float Height;
	public float Waist;
	public float Bust;


	private void Awake(){
		data = new Body ();

	}

	protected override void Save(){
		data.file = file; 
		data.Height = Height;
		data.Waist = Waist;
		data.Bust = Bust;

        saveManager.Save(data); 
    }


    protected override void Load()
    {
		Body data = new Body();
		if (saveManager.Load (ref data)) {
			Height = data.Height;
			Waist = data.Waist;
			Bust = data.Bust;
		} 
    }
}
