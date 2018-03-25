using UnityEngine;
using System.Collections;

public class BodyscanSave : Saver
{
    public class BodyData
    {
        public string file; 
        public float Height;
        public float Waist;
        public float Bust;
    };

    protected override string FileName()
    {
        return "bodyData";
    }

    protected override void Save()
    {
        BodyData data = new BodyData();
        data.file = FileName(); 
        data.Bust = 0f; 
        saveManager.Save(data); 
    }


    protected override void Load()
    {
    }
}
