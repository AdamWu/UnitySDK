using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MeshClean : MonoBehaviour {

	public float radius = 0.2f;

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;


	MeshCollider collider;


	void Start () {

		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}

		Collider col = GetComponent<Collider> ();
		if (col && col is MeshCollider) {
			collider = col as MeshCollider;
		} else {
			if (col != null) Destroy (col);
			collider = gameObject.AddComponent<MeshCollider> ();
		}
		collider.sharedMesh = deformingMesh;

	}


	void Update() {

	
		Collider[] colliders = Physics.OverlapSphere (transform.position, radius);

		float sqrRadius = radius * radius;

		for (int k = 0; k < colliders.Length; k++) {
			Collider col = colliders [k];

			if (col.gameObject == gameObject) continue;

			//UnityEngine.Debug.Log (col.name);

			Transform tf = col.transform;
			MeshFilter meshfilter = col.GetComponent<MeshFilter> ();
			if (meshfilter) {
				Mesh mesh = meshfilter.mesh;
				Vector3[] vertices = mesh.vertices;
				int[] tris = mesh.triangles;

				//UnityEngine.Debug.Log (tris.Length);

				List<int> tris_new = new List<int> ();

				bool bRemove = false;
				for (int i = 0; i < tris.Length/3;i++) {
					int idx0 = tris [i*3];
					int idx1 = tris [i*3+1];
					int idx2 = tris [i*3+2];
					Vector3 p0 = tf.TransformPoint(vertices [idx0]);
					Vector3 p1 = tf.TransformPoint(vertices [idx1]);
					Vector3 p2 = tf.TransformPoint(vertices [idx2]);

					if ((p0 - transform.position).sqrMagnitude > sqrRadius &&
					    (p1 - transform.position).sqrMagnitude > sqrRadius &&
					    (p2 - transform.position).sqrMagnitude > sqrRadius) {
						//UnityEngine.Debug.Log ("found "+ i);
						tris_new.Add (idx0);
						tris_new.Add (idx1);
						tris_new.Add (idx2);
					} else {
						bRemove = true;
					}

				}
					
				if (bRemove) {
					UnityEngine.Debug.LogFormat ("found tris {0}->{1}", tris.Length, tris_new.Count);
					
					mesh.triangles = tris_new.ToArray ();
				}
			}
		}

	}


	void OnDrawGizmos() {

		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawSphere(transform.position, radius);

	}
}