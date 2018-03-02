using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseAddForce : MonoBehaviour {

	Rigidbody _drag = null;

	Vector3 screenPos, offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	

		if (_drag == null && Input.GetMouseButtonDown (0)) {

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			// 优先检查axis碰撞
			if (Physics.Raycast (ray, out hit)) {
				Debug.Log ("hit " + hit.collider.name);

				_drag = hit.collider.GetComponent<Rigidbody>();

				if (_drag) {

					screenPos = Camera.main.WorldToScreenPoint (_drag.transform.position);
					offset = _drag.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPos.z));
				}

			}

		}

		if (Input.GetMouseButtonUp (0)) {
			_drag = null;
		}


		if (_drag) {
			Vector3 screenPos_cur = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPos.z);
			Vector3 pos_cur = Camera.main.ScreenToWorldPoint (screenPos_cur) + offset;
			//_drag.transform.position = new Vector3 (pos_cur.x, pos_cur.y, pos_cur.z);

			Vector3 force = pos_cur - _drag.transform.position;

			_drag.AddForce (force*1000f);
			_drag.velocity *= 0.8f;
		} 

	}
}
