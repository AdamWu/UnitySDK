using UnityEngine;
using System.Diagnostics;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	public float Hardness = 1f;

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

		isDeformed = true;

		Stopwatch sw = new Stopwatch ();
		sw.Start ();


		Matrix4x4 tm = transform.localToWorldMatrix;
		Matrix4x4 tmInv = transform.worldToLocalMatrix;

		for (int i = 0; i < displacedVertices.Length; i++) {

			Vector3 p = tm.MultiplyPoint3x4(displacedVertices[i]);
			//Vector3 p = displacedVertices[i];

			Vector3 pointToVertex = p - point;

			//float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
			float attenuatedForce = Mathf.Max(0, force - pointToVertex.magnitude/Hardness);

			if (attenuatedForce > 0) {
				//p += pointToVertex.normalized * attenuatedForce;
				p += dir.normalized * attenuatedForce;
				//displacedVertices[i] += displacedVertices[i].normalized * 0.1f;
		
				displacedVertices [i] = tmInv.MultiplyPoint3x4 (p);
			}
		}

		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals ();
		//deformingMesh.RecalculateTangents ();
		//NormalSolver.RecalculateNormals(deformingMesh, 30);

		//collider.sharedMesh = deformingMesh;

		sw.Stop ();
		//UnityEngine.Debug.LogFormat ("using {0}", sw.ElapsedMilliseconds);
	}

	public void ClearForce() {
		//UnityEngine.Debug.Log ("ClearForce " + isDeformed);
		if (isDeformed == false) return;

		deformingMesh.vertices = originalVertices;
		deformingMesh.RecalculateNormals ();
		NormalSolver.RecalculateNormals(deformingMesh, 30);

		isDeformed = false;
	}
}