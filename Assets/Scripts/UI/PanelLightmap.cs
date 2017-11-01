using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PanelLightmap : MonoBehaviour {

	// Use this for initialization
	void Start () {

		Button btn = transform.Find ("Button Back").GetComponent<Button> ();
		btn.onClick.AddListener (delegate {
			OnBtnExit(btn.gameObject);
		});
	}


	void OnBtnExit(GameObject sender) {
		Debug.Log ("OnBtnExit");

		SceneLoadManager.Instance.GotoScene ("main");
	}
}
