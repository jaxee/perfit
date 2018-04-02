using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalMessage : UIManager {

	// Update is called once per frame
	void SendMessage (string msg) {
        StartCoroutine(MsgModule(msg));
	}
}
