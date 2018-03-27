using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

public class MeshSplitInput : MonoBehaviour {

	public GameObject SplitPlane;

	private Vector3 start, end;
	private bool started = false;

	int startVertexId, endVertexId;
	Vector3 startVertex, endVertex;

	Vector3 startPoint, endPoint;

	int faceStart;

	MeshSplit _meshsplit;

	void Start() {		
		//Physics.queriesHitBackfaces = true;
	}

	void Update() {
		if (Input.GetMouseButtonDown(1)) {

			start = Input.mousePosition;
			//start = new Vector3 (971, 247, 0);

			Ray ray = Camera.main.ScreenPointToRay(start);
			RaycastHit hit;

			RaycastHit[] hits = Physics.RaycastAll (ray);

			if (Physics.Raycast (ray, out hit)) {
				MeshSplit split = hit.collider.GetComponent<MeshSplit> ();
				if (split != null) {

					startPoint = hit.point;

					_meshsplit = split;

					faceStart = hit.triangleIndex;

					started = true;
				}
			}
		}
		else if (Input.GetMouseButtonUp(1) && started) {

			end = Input.mousePosition;
			//end = new Vector3 (1037, 126, 0);

			UnityEngine.Debug.LogFormat ("{0} {1}", start, end);

			started = false;

			Ray ray = Camera.main.ScreenPointToRay(end);
			RaycastHit hit;
		
			if (Physics.Raycast (ray, out hit)) {
				MeshSplit split = hit.collider.GetComponent<MeshSplit> ();
				if (split != null && split == _meshsplit) {

					endPoint = hit.point;

					int fidx = hit.triangleIndex;

					if (fidx == faceStart) {
						UnityEngine.Debug.LogWarning ("face same!");
						return;
					}
						
					float near = Camera.main.nearClipPlane;

					Vector3 line = Camera.main.ScreenToWorldPoint(new Vector3(end.x, end.y, near)) - Camera.main.ScreenToWorldPoint(new Vector3(start.x, start.y, near));
					line.Normalize ();

					Vector3 planeN = Vector3.Cross (line, ray.direction).normalized;


					Vector3 center = (startPoint + endPoint) / 2;
					Vector3 offset = endPoint - startPoint;
					float distance = Vector3.Dot(offset, line);

					SplitPlane.transform.localRotation = Quaternion.identity;
					SplitPlane.transform.forward = planeN;

					Vector3 dir = (Camera.main.transform.position - center).normalized;
					float angle = Vector3.Angle (SplitPlane.transform.up, dir);
					//UnityEngine.Debug.Log ("angle "+angle);

					if (Vector3.Dot (SplitPlane.transform.right, dir) > 0) {
						angle = -angle;
					}

					Vector3 angles = SplitPlane.transform.eulerAngles;
					SplitPlane.transform.localRotation = Quaternion.Euler(new Vector3(angles[0], angles[1], angle-angles[2]));
					SplitPlane.transform.position = (startPoint+endPoint)/2f;
					SplitPlane.transform.localScale = new Vector3 (distance, 1, 1);
				
					// split
					_meshsplit.Split (planeN, -SplitPlane.transform.up, startPoint, endPoint, faceStart, fidx);

				}
			}
		}
	}
		

	void OnDrawGizmos() {

		Gizmos.color = new Color(0, 1, 0, 1f);

		Gizmos.DrawSphere(startPoint, 0.01f);
		Gizmos.DrawSphere(endPoint, 0.01f);
		Gizmos.DrawSphere((startPoint+endPoint)/2, 0.01f);

		Gizmos.DrawLine(startPoint, endPoint);

		if (_meshsplit) {
			Vector3 dir = _meshsplit.transform.TransformVector (-transform.up);
			Gizmos.color = new Color(1, 0, 0, 1f);
			Gizmos.DrawSphere (startPoint + dir, 0.02f);
			Gizmos.color = new Color(0, 1, 0, 1f);
			Gizmos.DrawSphere (endPoint + dir, 0.02f);
		}
		Gizmos.color = new Color(0, 0, 1, 1f);
		Gizmos.DrawLine(Camera.main.transform.position, (startPoint+endPoint)/2);


		Gizmos.color =  Color.yellow;

	}
}