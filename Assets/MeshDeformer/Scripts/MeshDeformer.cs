using UnityEngine;
using System.Diagnostics;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	public float Hardness = 2f;

	public bool isDeformed { get; set;}

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;


	MeshCollider collider;


	void Start () {
		isDeformed = false;

		Hardness = Mathf.Max (0.01f, Hardness);

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

	public void ResetVertices() {

		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}
	}

	public void AddDeformingForce (Vector3 point, Vector3 dir, float force) {
		//UnityEngine.Debug.Log ("AddDeformingForce " + force);


		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch ();
		sw.Start ();

		isDeformed = true;

		Vector3 point_local = transform.InverseTransformPoint (point);
		Vector3 dir_local = transform.InverseTransformVector (dir).normalized;

		for (int i = 0; i < displacedVertices.Length; i++) {

			Vector3 p = displacedVertices[i];

			Vector3 pointToVertex = p - point_local;

			//float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
			float attenuatedForce = Mathf.Max(0, force - pointToVertex.magnitude/Hardness);

			if (attenuatedForce > 0) {
				p += dir_local * attenuatedForce;
				//displacedVertices[i] += displacedVertices[i].normalized * 0.1f;
		
				displacedVertices [i] = p;
			}
		}

		deformingMesh.vertices = displacedVertices;
		//deformingMesh.RecalculateNormals ();
		//deformingMesh.RecalculateTangents ();
		//NormalSolver.RecalculateNormals(deformingMesh, 30);

		//collider.sharedMesh = deformingMesh;

		sw.Stop ();
		//UnityEngine.Debug.LogFormat ("AddDeformingForce using {0}", sw.ElapsedMilliseconds);
	}

	public void ClearForce() {
		//UnityEngine.Debug.Log ("ClearForce " + isDeformed);
		if (isDeformed == false) return;

		deformingMesh.vertices = originalVertices;
		//deformingMesh.RecalculateNormals ();
		//NormalSolver.RecalculateNormals(deformingMesh, 30);

		isDeformed = false;
	}
}