using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	// 软组织粘性系数
	public float kViscosity = 5f;

	// 质点弹簧-最大拉伸量
	public float kSpringMax = 2f;

	public bool isDeformed { get; set;}

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;

	// 质点弹簧
	Dictionary<int, Dictionary<int, float>> MassSprings = new Dictionary<int, Dictionary<int, float>>();

	// 所有受力点及受力值
	Dictionary<int, Vector3> dic_VertexForce = new Dictionary<int, Vector3> ();

	bool bVertexForceDirty = false;

	MeshCollider collider;


	void Start () {
		isDeformed = false;

		kViscosity = Mathf.Max (0.01f, kViscosity);

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

		// 质点弹簧初始化
		int[] triangles = deformingMesh.triangles;
		for (int i = 0; i < triangles.Length/3; i++) {
			int vidx1 = triangles [i * 3];
			int vidx2 = triangles [i * 3+1];
			int vidx3 = triangles [i * 3+2];
			Vector3 v1 = originalVertices [vidx1];
			Vector3 v2 = originalVertices [vidx2];
			Vector3 v3 = originalVertices [vidx3];
			float dst12 = (v2 - v1).magnitude;
			float dst13 = (v3 - v1).magnitude;
			float dst23 = (v3 - v2).magnitude;

			if (!MassSprings.ContainsKey (vidx1)) {
				MassSprings.Add (vidx1, new Dictionary<int, float> ());
			}
			if (!MassSprings.ContainsKey (vidx2)) {
				MassSprings.Add (vidx2, new Dictionary<int, float> ());
			}
			if (!MassSprings.ContainsKey (vidx3)) {
				MassSprings.Add (vidx3, new Dictionary<int, float> ());
			}
			if (!MassSprings[vidx1].ContainsKey(vidx2)) MassSprings[vidx1].Add(vidx2, dst12);
			if (!MassSprings[vidx1].ContainsKey(vidx3)) MassSprings[vidx1].Add(vidx3, dst13);
			if (!MassSprings[vidx2].ContainsKey(vidx1)) MassSprings[vidx2].Add(vidx1, dst12);
			if (!MassSprings[vidx2].ContainsKey(vidx3)) MassSprings[vidx2].Add(vidx3, dst23);
			if (!MassSprings[vidx3].ContainsKey(vidx1)) MassSprings[vidx3].Add(vidx1, dst13);
			if (!MassSprings[vidx3].ContainsKey(vidx2)) MassSprings[vidx3].Add(vidx2, dst23);
		}

	}


	void FixedUpdate() {
		if (bVertexForceDirty) {
			ResetVertices ();
			UpdateVertices ();
		}
		bVertexForceDirty = false;
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
			float attenuatedForce = Mathf.Max(0, force - pointToVertex.magnitude/kViscosity);

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


	public int FindNearestVertexInTriangle(int triangleIdx, Vector3 wpos, out Vector3 vertex) {
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

		vertex = originalVertices [idx];

		return idx;
	}


	public bool AddForceAtVertex(int vertexIdx, Vector3 dst) {
		//UnityEngine.Debug.Log ("MoveVertex " + vertexIdx);

		Vector3 dst_local = transform.InverseTransformPoint (dst);

		Vector3 vertex = originalVertices [vertexIdx];
		Vector3 delta_s = dst_local - vertex;

		// 判断最大拉伸量
		Dictionary<int, float> edge = MassSprings [vertexIdx];
		Dictionary<int, float>.Enumerator iter = edge.GetEnumerator ();
		bool bSpringMax = false;
		while (iter.MoveNext ()) {
			int idx = iter.Current.Key;
			float value = iter.Current.Value;

			float k = (dst_local - originalVertices [idx]).magnitude / edge [idx];

			if (k >= kSpringMax) {
				bSpringMax = true;
				break;
			}
		}

		//bSpringMax = false;
		if (bSpringMax) {
			// 达到最大形变量
			return false;
		} else {
			if (dic_VertexForce.ContainsKey (vertexIdx)) {
				dic_VertexForce [vertexIdx] = delta_s;
			} else {
				dic_VertexForce.Add (vertexIdx, delta_s);
			}

			bVertexForceDirty = true;
		}


		return true;
	}

	public void ClearForceAtVertex(int vertexIdx) {
		if (dic_VertexForce.ContainsKey (vertexIdx)) {
			dic_VertexForce.Remove (vertexIdx);
		}
		
		bVertexForceDirty = true;
	}

	void UpdateVertices() {

		//UnityEngine.Debug.Log ("Update Vertices");

		Stopwatch sw = new Stopwatch ();
		sw.Start ();

		if (dic_VertexForce.Count == 0) {
			deformingMesh.vertices = originalVertices;
			deformingMesh.RecalculateNormals ();
			//NormalSolver.RecalculateNormals(deformingMesh, 30);
			return;
		}

		Vector3[,] offsets = new Vector3[originalVertices.Length, dic_VertexForce.Count];

		Dictionary<int, Vector3>.Enumerator it = dic_VertexForce.GetEnumerator ();
		int forceIdx = 0;
		while (it.MoveNext ()) {
			int vertexIdx = it.Current.Key;
			Vector3 force = it.Current.Value;
			
			offsets[vertexIdx, forceIdx++] = force;
		}


		// 多组质点弹簧遍历
		it = dic_VertexForce.GetEnumerator ();
		Queue<int> masses = new Queue<int> ();
		HashSet<int> mark = new HashSet<int> ();
		forceIdx = 0;
		while (it.MoveNext ()) {
			int vidx_force = it.Current.Key;
			Vector3 force = it.Current.Value;

			offsets[vidx_force, forceIdx] = force;

			masses.Clear ();
			mark.Clear ();
			masses.Enqueue (vidx_force);
			mark.Add (vidx_force);

			// 一组质点弹簧遍历
			while (masses.Count > 0) {
				int vidx = masses.Dequeue();
				Vector3 delta = offsets[vidx, forceIdx];

				// 遍历此节点所有链接点
				Dictionary<int, float> edge = MassSprings [vidx];
				Dictionary<int, float>.Enumerator iter = edge.GetEnumerator ();
				while (iter.MoveNext ()) {
					int idx = iter.Current.Key;
					float value = iter.Current.Value;

					int key = idx;//vidx > idx ? vidx | (idx << 8) : (vidx << 8) | idx;
					if (mark.Contains (key))
						continue;

					Vector3 v = delta / (1 + value / kViscosity);
					if (!dic_VertexForce.ContainsKey(idx) && offsets[idx, forceIdx].sqrMagnitude < v.sqrMagnitude) {
						offsets[idx, forceIdx] = v;
						 
						masses.Enqueue (idx);
					}
				}

				iter = edge.GetEnumerator ();
				while (iter.MoveNext ()) {
					int idx = iter.Current.Key;
					int key = idx;//vidx > idx ? vidx | (idx << 8) : (vidx << 8) | idx;
					mark.Add (key);
				}
			}

			forceIdx++;
		}


		sw.Stop ();
		//UnityEngine.Debug.LogFormat ("using {0}", sw.ElapsedMilliseconds);


		// 计算各顶点偏移量
		float[] positive = new float[dic_VertexForce.Count];
		float[] negative = new float[dic_VertexForce.Count];
		for (int i = 0; i < offsets.GetLength(0); i ++) {
			Vector3 result = Vector3.zero;
		
			// 计算改顶点最终形变量（x,y,z）
			for (int j = 0; j < 3; j++) {

				int numPositive = 0;
				int numNegative = 0;

				for (int k = 0; k < dic_VertexForce.Count; k++) {
					Vector3 offset = offsets [i, k];
					float v = offsets [i, k] [j];
					if ( v > 0) {
						positive[numPositive] = v;
						numPositive++;
					} else {
						negative[numNegative] = v;
						numNegative++;
					}
				}

				if (numNegative == 0) {
					for (int k = 0; k < numPositive; k++) {
						if (result [j] < positive [k]) {
							result [j] = positive [k];
						}
					}
					continue;
				}

				if (numPositive == 0) {
					for (int k = 0; k < numNegative; k++) {
						if (result [j] > negative [k]) {
							result [j] = negative [k];
						}
					}
					continue;
				}

				for (int k = 0; k < numPositive; k++) {
					for (int w = 0; w < numNegative; w++) {
						float v = positive [k] + negative [w];
						if (Mathf.Abs (result [j]) < Mathf.Abs (v)) {
							result [j] = v;
						}
					}
				}
			}

			// 最终结果保存在第一个位置
			offsets [i, 0] = result;
		}
	
		// 计算出最新的顶点位置
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices [i] = originalVertices [i] + offsets [i, 0];
			
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