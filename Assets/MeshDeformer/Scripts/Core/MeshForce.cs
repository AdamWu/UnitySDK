using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshForce : MonoBehaviour {

	private float _radius = 0f;
	public float Radius {get {return _radius;}}
	public void SetRadius(float radius) {
		_radius = radius;
	}

	public Vector3[] Normals {get { return _normals;}}
	private Vector3[] _normals;


	private Vector3 lastPos = Vector3.zero;
	public bool Dirty { get; set;}

	void Awake() {
		_normals = new Vector3[] {
			Vector3.down,
			Vector3.forward,
			Vector3.left,
			Vector3.back,
			Vector3.right,
			Vector3.up,
			//*
			(Vector3.down + Vector3.left + Vector3.forward).normalized,
			(Vector3.down + Vector3.left + Vector3.back).normalized,
			(Vector3.down + Vector3.right + Vector3.forward).normalized,
			(Vector3.down + Vector3.right + Vector3.back).normalized,
			(Vector3.up + Vector3.left + Vector3.forward).normalized,
			(Vector3.up + Vector3.left + Vector3.back).normalized,
			(Vector3.up + Vector3.right + Vector3.forward).normalized,
			(Vector3.up + Vector3.right + Vector3.back).normalized,
			//*/

		};

		Dirty = true;
	}

	void FixedUpdate() {
		if (!lastPos.Equals (transform.position)) {
			Dirty = true;
			lastPos = transform.position;
		}
	}

	void OnDrawGizmos() {
		
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawSphere(transform.position, _radius * MeshDeformerManager.GlobalScale);

		Gizmos.color =  Color.yellow;
		if (_normals != null) {
			for (int j = 0; j < _normals.Length; j++) {
				Gizmos.DrawLine (transform.position, transform.position + _normals [j] * _radius * MeshDeformerManager.GlobalScale);
			}
		}
		
	}
}