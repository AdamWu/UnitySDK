using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshForce : MonoBehaviour {

	private float _radius = 0f;
	public float Radius {get {return _radius;}}
	public void SetRadius(float radius) {
		_radius = radius;

		float scale = _radius / 0.5f;
		transform.localScale = new Vector3 (scale, scale, scale);
	}

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
		Gizmos.DrawSphere(transform.position, _radius);

		Gizmos.color =  Color.yellow;
		if (_normals != null) {
			for (int j = 0; j < _normals.Length; j++) {
				Gizmos.DrawLine (transform.position, transform.position + _normals [j] * _radius);
			}
		}
		
	}
}