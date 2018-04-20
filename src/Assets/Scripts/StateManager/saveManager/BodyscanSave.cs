using UnityEngine;
using System.Collections;

public class BodyscanSave : Saver
{
	public struct Body{
	    public string file;
	    public float Height;
	    public float Waist;
        public float Hip;
        public float Bust;
        public bool isScanned; 
	};


	private Body data = new Body();
	public string file;
	public float Height;
	public float Waist;
    public float Hip;
    public float Bust;
    public bool isScanned;


    protected override void Save(){
		data.file   = file;
		data.Height = Height;
		data.Waist  = Waist;
		data.Bust   = Bust;
        data.Hip    = Hip;
        data.isScanned = isScanned; 

        saveManager.Save(file, data);
    }


    protected override void Load()
    {
		Body _data = new Body();
        if (saveManager.Load(file, ref _data))
        {
            Height = _data.Height;
            Hip    = _data.Hip;
            Waist  = _data.Waist;
            Bust   = _data.Bust;
            isScanned = _data.isScanned;
        }
        else {
            Debug.Log("no saved BodyScan data");
        }
    }
}
