using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	// 软组织粘性系数
	public float kViscosity = 5f;

	// 质点弹簧-最大拉伸量
	public float kSpringMax = 2f;

	// 软组织最小形变阈值
	public float kZero = 0.0001f;

	public bool isDeformed { get; set;}

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;

	// 质点弹簧
	Dictionary<int, Dictionary<int, float>> MassSprings = new Dictionary<int, Dictionary<int, float>>();

	// 所有受力点及受力值
	Dictionary<int, Vector3> dic_VertexForce = new Dictionary<int, Vector3> ();

	bool bVertexForceDirty = false;

	MeshCollider _collider;


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
			_collider = col as MeshCollider;
		} else {
			if (col != null) Destroy (col);
			_collider = gameObject.AddComponent<MeshCollider> ();
		}
		_collider.sharedMesh = deformingMesh;

		InitMassSprings ();

	}

	void InitMassSprings() {

		int i, j, k, w;

		// 连通区域判定
		Bounds bounds = deformingMesh.bounds;
		float size = 0.02f;
		float sizeHalf = size / 2;
		int xNum = Mathf.CeilToInt( bounds.extents.x / size) * 2;
		int yNum = Mathf.CeilToInt( bounds.extents.y / size) * 2;
		int zNum = Mathf.CeilToInt( bounds.extents.z / size) * 2;
		bool [,,] spheres = new bool[xNum,yNum,zNum];
		Dictionary<int, Vector3> dicSpheres = new Dictionary<int, Vector3> ();
		for (i = 0; i < xNum; i ++) {
			for (j = 0; j < yNum; j ++) {
				for (k = 0; k < zNum; k ++) {
					Vector3 localpos = bounds.min + new Vector3 (i, j, k) * size;
					Vector3 pos = transform.TransformPoint (localpos);

					if (Physics.CheckSphere(pos, sizeHalf)) {


						GameObject go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
						Destroy (go.GetComponent<SphereCollider> ());
						//go.transform.SetParent (transform);
						go.transform.localRotation = Quaternion.identity;
						go.transform.localScale = new Vector3(size, size, size);
						go.transform.position = pos;


						spheres [i, j, k] = true;
						dicSpheres.Add (i << 20 | j << 10 | k, localpos);
					}
				}
			}
		}


		Dictionary<int, List<int>> dicSphere2Vertices = new Dictionary<int, List<int>> ();
		for (i = 0; i < originalVertices.Length; i++) {
			Vector3 v = originalVertices[i];
		
			Dictionary<int, Vector3>.Enumerator it = dicSpheres.GetEnumerator ();
			while (it.MoveNext()) {
				int id = it.Current.Key;
				Vector3 pos = it.Current.Value;

				Vector3 offset = v - pos;
				if (offset.magnitude <= size) {
					if (!dicSphere2Vertices.ContainsKey (id)) {
						dicSphere2Vertices.Add(id, new List<int>());
					}
					dicSphere2Vertices [id].Add (i);
					break;
				}
			}
		}

		// 依次查找最近连通区域 plane-z
		for (i = 0; i < xNum; i++) {
			for (j = 0; j < yNum; j++) {
				for (k = 0; k < zNum; k++) {
					// 无连通
					if (!spheres[i, j, k]) continue;

					// 连通区域无质点
					int id = i << 20 | j << 10 | k;
					if (!dicSphere2Vertices.ContainsKey (id))
						continue;

					// 查找相连的质点
					for (w = k+1; w < zNum; w++) {
						if (!spheres [i, j, w]) {
							break;
						}

						int id2 = i << 20 | j << 10 | w;
						if (!dicSphere2Vertices.ContainsKey (id2))
							continue;

						// 找到相连质点区域
						k = w + 1;
						UnityEngine.Debug.LogFormat("Found {0} {1}", id, id2);
						break;
					}
				}
			}
		}
			
		// 依次查找最近连通区域 plane-y
		for (j = 0; j < yNum; j++) {
			for (k = 0; k < zNum; k++) {
				for (i = 0; i < xNum; i++) {
					// 无连通
					if (!spheres[i, j, k]) continue;

					// 连通区域无质点
					int id = i << 20 | j << 10 | k;
					if (!dicSphere2Vertices.ContainsKey (id))
						continue;

					// 查找相连的质点
					for (w = i+1; w < xNum; w++) {
						if (!spheres [w, j, k]) {
							break;
						}

						int id2 = w << 20 | j << 10 | k;
						if (!dicSphere2Vertices.ContainsKey (id2))
							continue;

						// 找到相连质点区域
						i = w + 1;
						UnityEngine.Debug.LogFormat("Found {0} {1}", id, id2);
						break;
					}
				}
			}
		}

		// 依次查找最近连通区域 plane-x
		for (i = 0; i < xNum; i++) {
			for (k = 0; k < zNum; k++) {
				for (j = 0; j < yNum; j++) {
					// 无连通
					if (!spheres[i, j, k]) continue;

					// 连通区域无质点
					int id = i << 20 | j << 10 | k;
					if (!dicSphere2Vertices.ContainsKey (id))
						continue;

					// 查找相连的质点
					for (w = j+1; w < yNum; w++) {
						if (!spheres [i, w, k]) {
							break;
						}

						int id2 = i << 20 | w << 10 | k;
						if (!dicSphere2Vertices.ContainsKey (id2))
							continue;

						// 找到相连质点区域
						UnityEngine.Debug.LogFormat("Found {0} {1}", id, id2);
						j = w + 1;
						break;
					}
				}
			}
		}


		// 质点弹簧初始化
		int[] triangles = deformingMesh.triangles;
		for (i = 0; i < triangles.Length/3; i++) {
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

		// 同一连通区域的质点
		Dictionary<int, List<int>>.Enumerator iter = dicSphere2Vertices.GetEnumerator ();
		while (iter.MoveNext()) {
			int id = iter.Current.Key;
			List<int> list = iter.Current.Value;

			for (i = 0; i < list.Count; i++) {
				int vid1 = list [i];
				for (j = i + 1; j < list.Count; j++) {
					int vid2 = list [j];
					if (!MassSprings [vid1].ContainsKey (vid2)) {
						float dist = (originalVertices [vid1] - originalVertices [vid2]).magnitude;
						MassSprings[vid1].Add(vid2, dist);
						MassSprings[vid2].Add(vid1, dist);
					}
				}
			}
		}
	}


	void FixedUpdate() {
		if (bVertexForceDirty) {
			ResetVertices ();
			UpdateVertices ();

			UpdateCollision ();
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

			Vector3 v_new = originalVertices [idx] + delta_s / (1 + value / kViscosity);

			float k = (dst_local - v_new).magnitude / value;

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

					int key = vidx > idx ? vidx | (idx << 16) : (vidx << 16) | idx;
					if (mark.Contains (key))
						continue;

					Vector3 v = delta / (1 + value / kViscosity);

					if (v.magnitude <= 0.0001f) continue;
					
					if (!dic_VertexForce.ContainsKey(idx) && offsets[idx, forceIdx].sqrMagnitude < v.sqrMagnitude) {
						offsets[idx, forceIdx] = v;
						 
						masses.Enqueue (idx);
					}
				}

				iter = edge.GetEnumerator ();
				while (iter.MoveNext ()) {
					int idx = iter.Current.Key;
					int key = vidx > idx ? vidx | (idx << 16) : (vidx << 16) | idx;
					mark.Add (key);
				}
			}

			forceIdx++;
		}


		sw.Stop ();
		//UnityEngine.Debug.LogFormat ("using {0}", sw.ElapsedMilliseconds);


		// 计算各顶点偏移量
		for (int i = 0; i < offsets.GetLength(0); i ++) {
			Vector3 result = Vector3.zero;
		
			// 计算改顶点最终形变量（x,y,z）
			for (int j = 0; j < 3; j++) {

				float positive = 0f;
				float negative = 0f;

				for (int k = 0; k < dic_VertexForce.Count; k++) {
					Vector3 offset = offsets [i, k];
					float v = offset [j];
					if ( v > 0 && v > positive) {
						positive = v;
					} else if (v < 0 && v < negative) {
						negative = v;
					}
				}

				result [j] = positive + negative;
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

	void UpdateCollision() {
		//UnityEngine.Debug.Log ("UpdateCollision");
		_collider.sharedMesh = deformingMesh;
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