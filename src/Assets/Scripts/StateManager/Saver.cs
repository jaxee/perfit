using UnityEngine;


public abstract class Saver : MonoBehaviour
{//global inputs to save
	public SaveManager saveManager;
	private StateManager stateManager;
	
	private void Awake()
	{
		stateManager = FindObjectOfType<StateManager>();
        if (!stateManager)
            throw new UnityException("State Manager could not be found, ensure that it exists in the transition scene.");
    }

	private void OnEnable()
    {
        stateManager.BeforeLoad += Save;
        stateManager.AfterLoad += Load;
    }
	private void OnDisable()
    {
        stateManager.BeforeLoad -= Save;
        stateManager.AfterLoad -= Load;
    }
    protected abstract void Save();
    protected abstract void Load();

}
