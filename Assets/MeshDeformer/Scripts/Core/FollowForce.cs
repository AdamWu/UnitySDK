using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Follow force.
/// binding nearest vertex when collide with softbody, and move vertex together
/// </summary>

public delegate void FollowForceSpringMax();

public class FollowForce : MonoBehaviour {

	public float kPunctureFactor = 1f;

	public SoftBody target { get; set;}
	public int VertexIdx { get; set;}

	public Vector3 Force { get; set;}

	// 缓存前两帧坐标
	Vector3[] lastPositons = new Vector3[2]; 
	int lastPositionsIdx = 0;
	public Vector3 LastPosition {get {return lastPositons[(lastPositionsIdx+1)%2];}}

	// 上一帧移动方向
	Vector3 lastMoveDir;
	public Vector3 LastMoveDir {get{ return lastMoveDir;}}

	public FollowForceSpringMax followForceSpingMax;

	void Awake() {
		
	}

	public void SetTargetVertex(SoftBody deformer, int vertexIdx) {

		Debug.LogFormat ("SetTargetVertex {0} {1}", deformer.name, vertexIdx);

		if (target != null) {
			Debug.LogError ("target already added");
			return;
		}

		this.target = deformer;
		this.VertexIdx = vertexIdx;

	}

	public void ClearTargetVertex() {

		if (target != null) target.ClearForceAtVertex (VertexIdx);

		this.target = null;
		this.VertexIdx = -1;
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

	
		bool bValid = target.AddForceAtVertex (VertexIdx, transform.position, kPunctureFactor);

		if (bValid == true) {
			//Force = force;
		} else {
			// 达到最大形变量
			if (followForceSpingMax!=null) followForceSpingMax();
		}
	}

	public void SimpleMove(Vector3 move) {

		bool bValid = target.CheckForceAtVertex (VertexIdx, transform.position + move, kPunctureFactor);

		if (bValid == true) {
			transform.position += move;
		} 
		
	}


	void OnDestroy() {
		ClearTargetVertex ();
	}

	void OnDrawGizmos() {
		
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		//Gizmos.DrawSphere(transform.position, 0.01f);

		Gizmos.color =  Color.yellow;

	}
		
}