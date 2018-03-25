using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Saver:MonoBehaviour
{//global inputs to save 
    public StateManager stateManager;
    public SaveManager saveManager;


    void Awake()
    {
        stateManager = GameObject.FindObjectOfType<StateManager>();
        if (!stateManager)
            throw new UnityException("State Manager could not be found, ensure that it exists in the transition scene.");
    }

    private void OnEnable()
    {
        stateManager.beforeLoad += Save;
        stateManager.afterLoad += Load;
    }
    private void OnDisable()
    {
        stateManager.beforeLoad -= Save;
        stateManager.afterLoad -= Load;
    }
    protected abstract string FileName();
    protected abstract void Save();
    protected abstract void Load();

}

