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

	public void AddDeformingForce (Vector3 pos, Vector3 dir, float force) {
		//UnityEngine.Debug.Log ("AddDeformingForce " + force);

		isDeformed = true;

		Stopwatch sw = new Stopwatch ();
		sw.Start ();


		Vector3 pos_local = transform.InverseTransformPoint (pos);
		Vector3 dir_local = transform.InverseTransformDirection (dir.normalized);

		for (int i = 0; i < displacedVertices.Length; i++) {

			Vector3 p = displacedVertices[i];

			Vector3 pointToVertex = p - pos_local;

			//float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
			float attenuatedForce = Mathf.Max(0, force - pointToVertex.magnitude/Hardness);

			if (attenuatedForce > 0) {
				//p += pointToVertex.normalized * attenuatedForce;
				p += dir_local * attenuatedForce;
				//displacedVertices[i] += displacedVertices[i].normalized * 0.1f;
		
				displacedVertices [i] = p;
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


	public int FindNearestVertexInTriangle(int triangleIdx, Vector3 wpos) {
		UnityEngine.Debug.Log ("FindNearestVertexInTriangle " + triangleIdx);

		Vector3 pos_local = transform.InverseTransformPoint (wpos);

		int idx = 0;
		float dist = float.MaxValue;

		int[] triangles = deformingMesh.triangles;
		for (int i = 0; i < 3; i++) {
			int vidx = triangles [triangleIdx * 3 + i];
			Vector3 offset = originalVertices [vidx] - pos_local;
			if (offset.sqrMagnitude < dist) {
				idx = vidx;
				dist = offset.sqrMagnitude;
			}
		}

		return idx;
	}


	public void AddForceAtVertex(int vertexIdx, Vector3 dst) {
		UnityEngine.Debug.Log ("MoveVertex " + vertexIdx);

		Vector3 dst_local = transform.InverseTransformPoint (dst);

		Vector3 vertex = originalVertices [vertexIdx];
		Vector3 offset = dst_local - vertex;

		float k = 2f;
		for (int i = 0; i < originalVertices.Length; i++) {

			float kx = (originalVertices [i] - vertex).sqrMagnitude / k;

			displacedVertices [i] = originalVertices[i] + offset / (1 + kx);
		}

		deformingMesh.vertices = displacedVertices;
		//deformingMesh.RecalculateNormals ();
		NormalSolver.RecalculateNormals(deformingMesh, 30);

	}

	public void MoveVertex(int vertexIdx) {
		UnityEngine.Debug.Log ("MoveVertex " + vertexIdx);
	
		Vector3[] normals = deformingMesh.normals;

		for (int i = 0; i < originalVertices.Length; i++) {

			if (i == vertexIdx) {
				displacedVertices [i] = originalVertices [i] + normals [i];
			} else {
				displacedVertices [i] = originalVertices [i];
			}
		}

		deformingMesh.vertices = displacedVertices;
		//deformingMesh.RecalculateNormals ();
		NormalSolver.RecalculateNormals(deformingMesh, 30);

	}

	public void ClearForce() {
		//UnityEngine.Debug.Log ("ClearForce " + isDeformed);
		if (isDeformed == false) return;

		deformingMesh.vertices = originalVertices;
		deformingMesh.RecalculateNormals ();
		//NormalSolver.RecalculateNormals(deformingMesh, 30);

		isDeformed = false;
	}
}