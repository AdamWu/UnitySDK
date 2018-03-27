using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMoveInPlane : MonoBehaviour {

	public Vector3 normal;

	GameObject _drag = null;

	Vector3 screenPos, offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonUp (0)) {
			_drag = null;
		}

		if (_drag) {
			Vector3 screenPos_cur = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPos.z);
			Vector3 pos_cur = Camera.main.ScreenToWorldPoint (screenPos_cur) + offset;
			_drag.transform.position = new Vector3 (pos_cur.x, pos_cur.y, pos_cur.z);
		}
	}

	void OnMouseDown() {

		_drag = gameObject;

		screenPos = Camera.main.WorldToScreenPoint (_drag.transform.position);
		offset = _drag.transform.position - Camera.main.ScreenToWorldPoint (new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPos.z));

	}
}
