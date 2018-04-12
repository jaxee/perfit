using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSave : Saver
{

    public struct Model {
        public string file;
        public Material skin;
        public Mesh mesh;
    }

    public string file;
    public Material skin;
    public Mesh mesh; 

    protected override void Save()
    {
        Model data = new Model();
        data.file = file;
        data.mesh = mesh;
        data.skin = skin;

        saveManager.Save(data.file, data); 
    }

    protected override void Load()
    {
        Model _model = new Model();
        if (saveManager.Load(file, ref _model)) {
            mesh = _model.mesh;
            skin = _model.skin; 
        }
        else
        {
            Debug.Log("no saved Model data");
        }
    }
}
