using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KnifeState {
	Out,			// 寻找接触点
	Deformer,		// 变形
	Puncture,		// 穿刺
	Pass,			// 穿透
}

public class Knife : MonoBehaviour {

	FollowForce head;

	KnifeState state = KnifeState.Out;

	private Vector3 HitPos = Vector3.zero;

	// Use this for initialization
	void Awake () {

		head = transform.Find ("head").GetComponent<FollowForce> ();

		head.followForceSpingMax += delegate() {
			Debug.Log("Pin:head.followForceSpingMax");

			head.ClearTargetVertex();
			state = KnifeState.Out;
		};
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		if (state == KnifeState.Out) {
			// 碰撞检测接触质点，进行穿刺
			float dot = Vector3.Dot(head.transform.forward, head.LastMoveDir);
			if (dot <= 0) {
				return;
			}
			if (FindVertexToDeformer ()) {
				state = KnifeState.Deformer;
			}
		} else if (state == KnifeState.Deformer) {

			float dot = Vector3.Dot(head.transform.forward, head.LastMoveDir);
			if (dot > 0) {
				return;
			}

			if (ReturnToOut ()) {
				state = KnifeState.Out;
			}
		} else if (state == KnifeState.Puncture) {
			// 正在进行穿刺
		} else if (state == KnifeState.Pass) {
		
		}
	}

	bool FindVertexToDeformer() {

		Ray inputRay = new Ray (head.LastPosition, head.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (inputRay, out hit)) {
			MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
			if (deformer) {

				Vector3 dir = (head.transform.position - hit.point).normalized;
				float dot = Vector3.Dot (head.transform.forward, dir);
				//Debug.Log ("dot "+dot);

				if (dot > 0) {
	
					int fidx = hit.triangleIndex;
					Vector3 vertex;
					int vidx = deformer.FindNearestVertexInTriangle (fidx, hit.point, out vertex);
					HitPos = hit.point;
					Debug.LogFormat ("FindVertexToDeformer {0}", vidx);
					head.SetTargetVertex (deformer, vidx);
					return true;
				}
			}
		}

		return false;
	}

	bool ReturnToOut() {
		Vector3 dir = (head.transform.position - HitPos).normalized;
		float dot = Vector3.Dot (head.transform.forward, dir);
		Debug.Log ("ReturnToOut check dot "+dot);

		if (dot < 0) {
			head.ClearTargetVertex ();
			return true;
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
