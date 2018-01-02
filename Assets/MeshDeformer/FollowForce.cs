using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Follow force.
/// binding nearest vertex when collide with meshdeformer, and move vertex together
/// </summary>


public delegate void FollowForceSpringMax(); 
public delegate void FollowForceOnTriggerEnter(Collider collider); 

public class FollowForce : MonoBehaviour {

	MeshDeformer target;
	int vertexIdx;

	Vector3 lastPos;
	Vector3 lastMoveDir;
	public Vector3 LastMoveDir {get{ return lastMoveDir;}}

	public FollowForceSpringMax followForceSpingMax;
	public FollowForceOnTriggerEnter followForceOnTriggerEnter;

	void Awake() {
		
	}

	public void SetTargetVertex(MeshDeformer deformer, int vertexIdx) {

		this.target = deformer;
		this.vertexIdx = vertexIdx;

	}

	public void ClearTargetVertex() {

		if (target != null) target.ClearForceAtVertex (vertexIdx);

		this.target = null;
		this.vertexIdx = -1;
	}


	void FixedUpdate() {

		if (lastPos.Equals (transform.position))
			return;


		Vector3 dir = transform.position - lastPos;
		//Debug.Log (dir.normalized);
		lastMoveDir = dir.normalized;

		lastPos = transform.position;

		if (target == null)
			return;


		bool bValid = target.AddForceAtVertex (vertexIdx, transform.position);

		if (bValid == false) {
			// 达到最大形变量
			if (followForceSpingMax!=null) followForceSpingMax();
		}
	}


	void OnDestroy() {
		ClearTargetVertex ();
	}


	void OnTriggerEnter(Collider collider) {
		Debug.Log ("OnTriggerEnter "+collider.name);
		followForceOnTriggerEnter (collider);
	}

	void OnTriggerStay(Collider collider) {
		//Debug.Log ("OnTriggerStay "+collider.name);
	}

	void OnTriggerExit(Collider collider) {
		//Debug.Log ("OnTriggerExit "+collider.name);
	}

	void OnDrawGizmos() {
		
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawSphere(transform.position, 0.01f);

		Gizmos.color =  Color.yellow;

	}
		
}