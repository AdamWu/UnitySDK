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

	// 缓存前两帧坐标
	Vector3[] lastPositons = new Vector3[2]; 
	int lastPositionsIdx = 0;
	public Vector3 LastPosition {get {return lastPositons[(lastPositionsIdx+1)%2];}}

	// 上一帧移动方向
	Vector3 lastMoveDir;
	public Vector3 LastMoveDir {get{ return lastMoveDir;}}

	public FollowForceSpringMax followForceSpingMax;
	public FollowForceOnTriggerEnter followForceOnTriggerEnter;

	void Awake() {
		
	}

	public void SetTargetVertex(MeshDeformer deformer, int vertexIdx) {

		Debug.LogFormat ("SetTargetVertex {0} {1}", deformer.name, vertexIdx);

		this.target = deformer;
		this.vertexIdx = vertexIdx;

	}

	public void ClearTargetVertex() {

		if (target != null) target.ClearForceAtVertex (vertexIdx);

		this.target = null;
		this.vertexIdx = -1;
	}


	void FixedUpdate() {

		Vector3 lastPos = lastPositons[lastPositionsIdx];
		if (lastPos.Equals (transform.position))
			return;

		Vector3 dir = transform.position - lastPos;
		lastMoveDir = dir.normalized;

		lastPositionsIdx = (lastPositionsIdx + 1) % 2;
		lastPositons [lastPositionsIdx] = transform.position;

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