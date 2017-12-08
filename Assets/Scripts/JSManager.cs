using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSManager : Singleton<JSManager> {

	public void GetUrlFromJs() {
		Application.ExternalCall ("GetUrlFromJs", "adamwu");
	}

	void GetUrlFromJsCallback(string str) {
		Debug.Log("CallFromJS " + str);
	}


}
