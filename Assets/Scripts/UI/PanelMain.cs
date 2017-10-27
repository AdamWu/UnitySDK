using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
