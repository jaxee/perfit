using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject secondaryMenu;
    public GameObject msgModule;
    public Text msgText;

    private void Awake()
    {
        ResetAll();
    }
    public void ResetAll() {
        Reset(mainMenu);
        Reset(secondaryMenu);
        Reset(msgModule);
    }
    private void Reset(GameObject module)
    {
        module.SetActive(false);
    }

    public void ToggleModule(GameObject module) {
        if (module.activeSelf)
        {
            Reset(module);
        }
        else
        {
            module.SetActive(true);
        }
    }

    public void DisplayMsg(string msg) {
        StartCoroutine(MsgModule(msg));
    }

    public IEnumerator MsgModule(string msg) {
        ToggleModule(msgModule); 
        msgText.text = msg; 
        yield return new WaitForSeconds(2);
        Reset(msgModule);
    }




}
