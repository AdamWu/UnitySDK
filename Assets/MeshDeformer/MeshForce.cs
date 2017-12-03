using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshForce : MonoBehaviour {

	public float radius = 0.1f;

	public Vector3[] Normals {get { return _normals;}}

	private Vector3[] _normals;

	void Awake() {
		_normals = new Vector3[6] {
			Vector3.down,
			Vector3.forward,
			Vector3.left,
			Vector3.back,
			Vector3.right,
			Vector3.up,
		};
	}

	void OnDrawGizmos() {
		
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawSphere(transform.position, radius);

		Gizmos.color =  Color.yellow;
		if (_normals != null) {
			for (int j = 0; j < _normals.Length; j++) {
				Gizmos.DrawLine (transform.position, transform.position + _normals [j] * radius);
			}
		}
		
	}
}