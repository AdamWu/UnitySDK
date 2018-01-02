using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PinState {
	Out,			// 寻找接触点
	Deformer,		// 变形
	Puncture,		// 穿刺
	Pass,			// 穿透
}

public class Pin : MonoBehaviour {

	public Vector3 headNormal = Vector3.down;

	FollowForce head, tail;

	PinState state = PinState.Out;

	// Use this for initialization
	void Awake () {

		head = transform.Find ("head").GetComponent<FollowForce> ();
		tail = transform.Find ("tail").GetComponent<FollowForce> ();

		head.followForceSpingMax += delegate() {
			Debug.Log("Pin:head.followForceSpingMax");

			head.ClearTargetVertex();
			state = PinState.Out;
		};
		head.followForceOnTriggerEnter += delegate(Collider collider) {
			Debug.Log("Pin:head.followForceOnTriggerEnter");

			if (state == PinState.Out) {
				float dot = Vector3.Dot(head.transform.forward, head.LastMoveDir);
				if (dot <= 0) {
					return;
				}
				if (FindVertexToDeformer ()) {
					state = PinState.Deformer;
				}
			}
		};
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		if (state == PinState.Out) {
			// 碰撞检测接触质点，进行穿刺
		} else if (state == PinState.Deformer) {

			float dot = Vector3.Dot(head.transform.forward, head.LastMoveDir);
			if (dot > 0) {
				return;
			}

			if (ReturnToOut ()) {
				state = PinState.Out;
			}
		} else if (state == PinState.Puncture) {
			// 正在进行穿刺
		} else if (state == PinState.Pass) {
		
		}
	}

	bool FindVertexToDeformer() {

		Ray inputRay = new Ray (head.LastPosition, head.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (inputRay, out hit)) {
			MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
			if (deformer) {

				float dot = Vector3.Dot (head.transform.forward, hit.normal);

				int fidx = hit.triangleIndex;
				Vector3 vertex;
				int vidx = deformer.FindNearestVertexInTriangle (fidx, hit.point, out vertex);
				Debug.LogFormat ("FindVertexToDeformer {0}", vidx);
				head.SetTargetVertex (deformer, vidx);
				return true;
			}
		}

		return false;
	}

	bool ReturnToOut() {
		Ray inputRay = new Ray (head.transform.position, head.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (inputRay, out hit)) {
			MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
			if (deformer) {
				int fidx = hit.triangleIndex;
				Vector3 vertex;
				int vidx = deformer.FindNearestVertexInTriangle (fidx, hit.point, out vertex);

				Vector3 wpos = deformer.transform.TransformPoint (vertex);
				Vector3 dir = (head.transform.position - hit.point).normalized;
				//Debug.Log (dir);
				float cos = Vector3.Dot (head.transform.forward, dir);

				if(cos <= 0f) {
					head.ClearTargetVertex ();
					return true;
				}
			}
		}
		return false;
	}


	void OnDrawGizmos() {

		Gizmos.color = new Color(1, 0, 0, 0.5f);
		//Gizmos.DrawSphere(transform.position, _radius);

		Gizmos.color =  Color.yellow;
		if (head != null) {
			//Gizmos.DrawLine (head.transform.position, head.transform.position + head.transform.forward);
		}
	}
}
