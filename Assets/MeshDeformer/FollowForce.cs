using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Follow force.
/// binding nearest vertex when collide with meshdeformer, and move vertex together
/// </summary>

public class FollowForce : MonoBehaviour {


	MeshDeformer target;
	int vertexIdx;

	Vector3 lastPos;

	void Awake() {
		
	}


	public void SetTargetVertex(MeshDeformer deformer, int vertexIdx) {
		this.target = deformer;
		this.vertexIdx = vertexIdx;
	}


	void FixedUpdate() {
		if (target == null)
			return;

		if (lastPos.Equals (transform.position))
			return;

		lastPos = transform.position;

		target.AddForceAtVertex (this.vertexIdx, transform.position);
	}

	void OnDrawGizmos() {
		
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawSphere(transform.position, 0.1f);

		Gizmos.color =  Color.yellow;

		
	}
}