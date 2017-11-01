﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PanelMain : MonoBehaviour {

	// Use this for initialization
	void Start () {

		GameObject go = new GameObject ("linerender");
		LineRenderer lr = go.AddComponent<LineRenderer> ();
		lr.startWidth = 0.1f;
		lr.endWidth = 0.1f;
		lr.positionCount = 2;
		lr.SetPosition (0, new Vector3 (0, 0, 0));
		lr.SetPosition (1, new Vector3 (100, 0, 0));

		Button btn = transform.Find ("Button Start").GetComponent<Button> ();
		btn.onClick.AddListener (delegate {
			OnBtnStart(btn.gameObject);
		});
		Button btn1 = transform.Find ("Button Scene1").GetComponent<Button> ();
		btn1.name = "1";
		btn1.onClick.AddListener (delegate {
			OnBtnSwitch(btn1.gameObject);
		});
		Button btn2 = transform.Find ("Button Scene2").GetComponent<Button> ();
		btn2.name = "2";
		btn2.onClick.AddListener (delegate {
			OnBtnSwitch(btn2.gameObject);
		});
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnBtnStart(GameObject sender) {
		Debug.Log ("OnBtnStart");
	}

	void OnBtnSwitch(GameObject sender) {
		Debug.Log ("OnBtnStart");

		if (sender.name == "1") {
			SceneLoadManager.Instance.GotoScene ("lightmap");
		} else if (sender.name == "2") {
			SceneLoadManager.Instance.GotoScene ("bigscene");
		}
	}
}
