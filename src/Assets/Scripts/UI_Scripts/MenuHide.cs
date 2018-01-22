using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuHide : MonoBehaviour {

    public GameObject Panel;
    int counter;

    public void hidePanel(){
    Panel.gameObject.SetActive(false);
    
    }
}
