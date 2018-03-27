using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void OnMouseMoveSelectDelegate(GameObject go);

public class MouseMoveAlongAxis : MonoBehaviour {

	public static MouseMoveAlongAxis instance;
	public static OnMouseMoveSelectDelegate onMouseMoveSelectDelegate;

	[SerializeField]
	[Range(0.1f, 10)]
	public float AxisSize = 1f;

	enum Axis {
		X =0,
		Y, 
		Z
	}

	[SerializeField]
	public GameObject AxisPrefab;

	[SerializeField]
	public LayerMask layerMask;

	GameObject _target;

	GameObject goX, goY, goZ;

	GameObject _drag = null;

	Vector3 screenPos, offset;

	Axis _axis;


	void Awake() {

		instance = this;

		int layer = 25;
		gameObject.layer = layer;

		// axis-x
		goX = GameObject.Instantiate(AxisPrefab);
		goX.name = "axis-x";
		goX.layer = layer;
		goX.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
		goX.transform.SetParent (transform);
		goX.transform.localPosition = Vector3.zero;
		//goX.transform.localRotation = Quaternion.Euler(0f, 0f, 90);
		//goX.transform.localPosition = new Vector3(1f, 0f, 0f);
		goX.GetComponent<MeshRenderer> ().material.color = Color.red;

		// axis-y
		goY = GameObject.Instantiate(AxisPrefab);
		goY.name = "axis-y";
		goY.layer = layer;
		goY.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
		goY.transform.SetParent (transform);
		goY.transform.localPosition = Vector3.zero;
		//goY.transform.localRotation = Quaternion.identity;
		//goY.transform.localPosition = new Vector3(0f, 1f, 0f);
		goY.GetComponent<MeshRenderer> ().material.color = Color.green;

		// axis-z
		goZ = GameObject.Instantiate(AxisPrefab);
		goZ.name = "axis-z";
		goZ.layer = layer;
		goZ.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
		goZ.transform.SetParent (transform);
		goZ.transform.localPosition = Vector3.zero;
		//goZ.transform.localRotation = Quaternion.Euler(90, 0, 0);
		//goZ.transform.localPosition = new Vector3(0f, 0f, 1f);
		goZ.GetComponent<MeshRenderer> ().material.color = Color.blue;

		SetTarget (null);
	}


	void SetTarget(GameObject go) {

		// remove old highlight
		if (_target != null) {
			HighlightableObject highlight = _target.GetComponent<HighlightableObject> ();
			if (highlight != null) {
				highlight.ConstantOffImmediate ();
			}
		}

		if (go == null) {
			goX.SetActive (false);
			goY.SetActive (false);
			goZ.SetActive (false);

			_target = null;
		} else {
			_target = go;
			goX.SetActive (true);
			goY.SetActive (true);
			goZ.SetActive (true);

			transform.position = go.transform.position;

			//goX.transform.SetParent (go.transform);
			goX.transform.up = go.transform.right;

			//goY.transform.SetParent (go.transform);
			goY.transform.up = go.transform.up;

			//goZ.transform.SetParent (go.transform);
			goZ.transform.up = go.transform.forward;

			// add new highlight
			HighlightableObject highlight = go.transform.GetComponent<HighlightableObject>();
			if (highlight == null) {
				highlight = go.AddComponent<HighlightableObject> ();
			}
			highlight.ConstantOn (new Color32(255, 100, 0, 255));
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (_target == null && goX.activeInHierarchy) {
			goX.SetActive (false);
			goY.SetActive (false);
			goZ.SetActive (false);
		}

		if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

		float scale = Camera.main.fieldOfView / 60;
		Vector3 scale_v = new Vector3 (scale, scale, scale) * AxisSize;
		goX.transform.localScale = scale_v;
		goY.transform.localScale = scale_v;
		goZ.transform.localScale = scale_v;
	
		if (_drag == null && Input.GetMouseButtonDown (0)) {

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			// 优先检查axis碰撞
			if (Physics.Raycast (ray, out hit, Mathf.Infinity, 1 << 25)) {
				Debug.Log ("hit axis " + hit.collider.name);

				if (hit.transform.gameObject == goX) {
					_drag = _target;
					_axis = Axis.X;
				} else if (hit.transform.gameObject == goY) {
					_drag = _target;
					_axis = Axis.Y;
				} else if (hit.transform.gameObject == goZ) {
					_drag = _target;
					_axis = Axis.Z;
				}

				if (_drag) {
					Debug.Log ("drag "+_drag.name);
					screenPos = Camera.main.WorldToScreenPoint (_drag.transform.position);
					offset = _drag.transform.position - Camera.main.ScreenToWorldPoint (new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPos.z));
				}
			} 
			// 检查选中碰撞体
			else {
				if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {

					SetTarget (hit.collider.gameObject);

					if (onMouseMoveSelectDelegate != null) {
						onMouseMoveSelectDelegate (hit.collider.gameObject);
					}
				} else {
					SetTarget (null);
					if (onMouseMoveSelectDelegate != null) {
						onMouseMoveSelectDelegate (null);
					}
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

			Vector3 move = pos_cur - _drag.transform.position;

			switch (_axis) {
			case Axis.X:
				{
					float k = Vector3.Dot (move, _drag.transform.right);
					_drag.transform.position += _drag.transform.right * k;
				}
				break;
			case Axis.Y:
				{
					float k = Vector3.Dot (move, _drag.transform.up);
					_drag.transform.position += _drag.transform.up * k;
				}
				break;
			case Axis.Z:
				{
					float k = Vector3.Dot (move, _drag.transform.forward);
					_drag.transform.position += _drag.transform.forward * k;
				}
				break;
			default:
				break;
			}

			transform.position = _drag.transform.position;
		} 

	}
}
