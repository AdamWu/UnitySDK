﻿using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

// 弹簧质点
public class MassPoint {
	public int id;
	public Vector3 pos;
	public List<int> vertices;
}

[RequireComponent(typeof(MeshFilter))]
public class SoftBody : MonoBehaviour {

	// 软组织粘性系数
	[SerializeField]
	public float kViscosity = 1f;

	// 质点弹簧-最大拉伸量
	[SerializeField]
	private float kSpringMax = 2f;	

	// 连通区域最小单位
	[SerializeField]
	public float kConnectAreaSize = 0.02f;

	// 软组织最小形变阈值
	[SerializeField]
	public float kThresholdZero = 0.0001f;

	public bool isDeformed { get; set;}


	Mesh _mesh;
	Vector3[] originalVertices, displacedVertices;

	// 质点弹簧
	List<MassPoint> Masses = new List<MassPoint>();
	Dictionary<int, Dictionary<int, float>> MassSprings = new Dictionary<int, Dictionary<int, float>>();
	Dictionary<int, MassPoint> dicVertex2Mass = new Dictionary<int, MassPoint> ();

	// 所有受力点及受力值
	Dictionary<int, Vector3> dic_VertexForce = new Dictionary<int, Vector3> ();

	bool bVertexForceDirty = false;

	MeshCollider _collider;


	void Start () {
		isDeformed = false;

		kViscosity = Mathf.Max (0.01f, kViscosity);

		_mesh = GetComponent<MeshFilter>().mesh;
		originalVertices = _mesh.vertices;
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
		_collider.sharedMesh = _mesh;

		InitMassSprings ();

	}

	public Mesh GetMesh() {
		return _mesh;
	}

	public void SetMesh(Mesh mesh) {
		GetComponent<MeshFilter> ().mesh = mesh;
		_mesh = mesh;
		originalVertices = _mesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}

		_collider.sharedMesh = _mesh;

		InitMassSprings ();
	}


	void InitMassSprings() {

		float epsilon = 0.00001f;

		Masses.Clear ();
		MassSprings.Clear ();
		dicVertex2Mass.Clear ();

		Dictionary<int, List<int>> dicSameVerex = new Dictionary<int, List<int>> ();
		int vertexCount = originalVertices.Length;
		for (int i = 0; i < vertexCount; i++) {
			dicSameVerex.Add (i, new List<int> ());
			for (int j = i + 1; j < vertexCount; j++) {
				if ((originalVertices [i] - originalVertices [j]).sqrMagnitude < epsilon) {
					dicSameVerex [i].Add (j);
				}
			}
		}

		for(int i = 0; i < dicSameVerex.Count; i ++) {

			MassPoint mass;
			if (dicVertex2Mass.ContainsKey (i) == false) {
				mass = new MassPoint ();
				mass.id = Masses.Count;
				mass.pos = originalVertices [i];
				mass.vertices = new List<int> ();
				mass.vertices.Add (i);	
				Masses.Add (mass);
				dicVertex2Mass [i] = mass;
			} else {
				mass = dicVertex2Mass [i];
			}

			List<int> refs = dicSameVerex [i];

			for (int j = 0; j < refs.Count; j++) {
				int vid = refs [j];
				dicVertex2Mass [vid] = mass;
				if (mass.vertices.Contains(vid) == false) {
					mass.vertices.Add (vid);	
				}
			}
		}

		// 质点弹簧初始化
		int[] triangles = _mesh.triangles;
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

			MassPoint m1 = dicVertex2Mass [vidx1];
			MassPoint m2 = dicVertex2Mass [vidx2];
			MassPoint m3 = dicVertex2Mass [vidx3];

			if (!MassSprings.ContainsKey (m1.id)) {
				MassSprings.Add (m1.id, new Dictionary<int, float> ());
			}
			if (!MassSprings.ContainsKey (m2.id)) {
				MassSprings.Add (m2.id, new Dictionary<int, float> ());
			}
			if (!MassSprings.ContainsKey (m3.id)) {
				MassSprings.Add (m3.id, new Dictionary<int, float> ());
			}
			if (!MassSprings[m1.id].ContainsKey(m2.id)) MassSprings[m1.id].Add(m2.id, dst12);
			if (!MassSprings[m1.id].ContainsKey(m3.id)) MassSprings[m1.id].Add(m3.id, dst13);
			if (!MassSprings[m2.id].ContainsKey(m1.id)) MassSprings[m2.id].Add(m1.id, dst12);
			if (!MassSprings[m2.id].ContainsKey(m3.id)) MassSprings[m2.id].Add(m3.id, dst23);
			if (!MassSprings[m3.id].ContainsKey(m1.id)) MassSprings[m3.id].Add(m1.id, dst13);
			if (!MassSprings[m3.id].ContainsKey(m2.id)) MassSprings[m3.id].Add(m2.id, dst23);
		}


		UnityEngine.Debug.LogFormat ("InitMassSprings count {0} vertex {1}", Masses.Count, vertexCount);
	}

	public void AddMassSpring(int vid1, int vid2) {

		MassPoint m1 = dicVertex2Mass [vid1];
		MassPoint m2 = dicVertex2Mass [vid2];

		if (MassSprings [m1.id].ContainsKey (m2.id)) {
			MassSprings [m1.id].Remove (m2.id);
		}

		if (MassSprings [m2.id].ContainsKey (m1.id)) {
			MassSprings [m2.id].Remove (m1.id);
		}
	}

	public void RemoveMassSpring(int vid1, int vid2) {

		MassPoint m1 = dicVertex2Mass [vid1];
		MassPoint m2 = dicVertex2Mass [vid2];

		if (MassSprings [m1.id].ContainsKey (m2.id)) {
			MassSprings [m1.id].Remove (m2.id);
		}

		if (MassSprings [m2.id].ContainsKey (m1.id)) {
			MassSprings [m2.id].Remove (m1.id);
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

	public int FindNearestVertexInTriangle(int triangleIdx, Vector3 wpos, out Vector3 vertex) {
		UnityEngine.Debug.Log ("FindNearestVertexInTriangle " + triangleIdx);

		Vector3 pos_local = transform.InverseTransformPoint (wpos);

		int idx = 0;
		float dist = float.MaxValue;

		int[] triangles = _mesh.triangles;
		for (int i = 0; i < 3; i++) {
			int vidx = triangles [triangleIdx * 3 + i];
			Vector3 offset = originalVertices [vidx] - pos_local;
			if (offset.sqrMagnitude < dist) {
				idx = vidx;
				dist = offset.sqrMagnitude;
			}
		}

		vertex = originalVertices [idx];

		return dicVertex2Mass[idx].id;
	}


	public bool AddForceAtVertex(int massid, Vector3 dst) {
		//UnityEngine.Debug.Log ("MoveVertex " + vertexIdx);

		Vector3 dst_local = transform.InverseTransformPoint (dst);

		Vector3 vertex = Masses [massid].pos;
		Vector3 delta_s = dst_local - vertex;

		// 判断最大拉伸量
		bool bSpringMax = false;
		/*
		Dictionary<MassPoint, float> edge = MassSprings [massid];
		Dictionary<MassPoint, float>.Enumerator iter = edge.GetEnumerator ();
		while (iter.MoveNext ()) {
			MassPoint mass = iter.Current.Key;
			float value = iter.Current.Value;

			Vector3 v_new = mass.pos + delta_s / (1 + value / kViscosity);

			float k = (dst_local - v_new).magnitude / value;

			if (k >= kSpringMax) {
				bSpringMax = true;
				break;
			}
		}
		*/

		//bSpringMax = false;
		if (bSpringMax) {
			// 达到最大形变量
			return false;
		} else {
			if (dic_VertexForce.ContainsKey (massid)) {
				dic_VertexForce [massid] = delta_s;
			} else {
				dic_VertexForce.Add (massid, delta_s);
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

		UnityEngine.Debug.Log ("Update Vertices");

		Stopwatch sw = new Stopwatch ();
		sw.Start ();

		if (dic_VertexForce.Count == 0) {
			_mesh.vertices = originalVertices;
			_mesh.RecalculateNormals ();
			//NormalSolver.RecalculateNormals(_mesh, 30);
			return;
		}

		Vector3[,] offsets = new Vector3[Masses.Count, Masses.Count];

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

					if (v.magnitude <= kThresholdZero) continue;
					
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

		for (int i = 0; i < Masses.Count; i ++) {
			MassPoint mass = Masses [i];

			for (int j = 0; j < mass.vertices.Count; j++) {
				int vid = mass.vertices [j];
				displacedVertices [vid] = originalVertices [vid] + offsets [i, 0];
			}
		}

		_mesh.vertices = displacedVertices;
		_mesh.RecalculateNormals ();
		//NormalSolver.RecalculateNormals(_mesh, 30);
	
	}

	void UpdateCollision() {
		//UnityEngine.Debug.Log ("UpdateCollision");
		_collider.sharedMesh = _mesh;
	}

	public void ClearForce() {
		//UnityEngine.Debug.Log ("ClearForce " + isDeformed);
		if (isDeformed == false) return;

		_mesh.vertices = originalVertices;
		_mesh.RecalculateNormals ();
		//NormalSolver.RecalculateNormals(_mesh, 30);

		isDeformed = false;
	}

	void OnDrawGizmos() {

		Gizmos.color = new Color(1, 0, 0, 0.5f);

		for (int i = 0; i < Masses.Count; i ++) {
			Vector3 wpos = transform.TransformPoint (Masses [i].pos);
			Gizmos.DrawSphere(wpos, 0.5f);
		}

		for (int i = 0; i < MassSprings.Count; i ++) {
			Vector3 wpos = transform.TransformPoint (Masses [i].pos);

			Dictionary<int, float> dic = MassSprings [i];


			Dictionary<int, float>.Enumerator iter = dic.GetEnumerator ();
			while (iter.MoveNext ()) {
				int massid = iter.Current.Key;

				Vector3 wpos2 = transform.TransformPoint (Masses[massid].pos);
				Gizmos.DrawLine(wpos, wpos2);
			}
		}

		Gizmos.color =  Color.yellow;

	}
}