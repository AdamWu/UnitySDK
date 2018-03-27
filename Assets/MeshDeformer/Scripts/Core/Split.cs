using UnityEngine;
using System.Collections.Generic;

public class Splitter
{
	private bool isValid = true;

	private SoftBody softbody;

	private List<Vector3> vertices;
	private List<Vector3> normals;
	private List<Vector4> tangents;
	private List<Vector2> uvs;
	private List<int> indices;

	private List<Vector3> vertices2;
	private List<Vector3> normals2;
	private List<Vector4> tangents2;
	private List<Vector2> uvs2;
	private List<int> indices2;

	private List<int> tri_backlist;

	// new faces list
	private List<int> _tris;
	private List<int> _vertices1;
	private List<int> _vertices2;


	Vector2 UVStart = Vector2.zero;
	Vector2 UVSize = Vector2.one;

	int indexStart, indexEnd;
	Vector3 pointStart, pointEnd;

	Vector3 planeNormal;
	Vector3 planeDir;

	public Splitter(SoftBody softbody, Vector2 uvStart, Vector2 uvSize)
	{
		this.softbody = softbody;

		UVStart = uvStart;
		UVSize = uvSize;
	}

	void Clear() {
		Mesh mesh = softbody.GetMesh ();
		vertices = new List<Vector3>(mesh.vertices);
		indices = new List<int>(mesh.triangles);

		if (mesh.normals.Length > 0)
		{
			normals = new List<Vector3>(mesh.normals);
		}

		if (mesh.tangents.Length > 0)
		{
			tangents = new List<Vector4>(mesh.tangents);
		}

		if (mesh.uv.Length > 0)
		{
			uvs = new List<Vector2>(mesh.uv);
		}

		vertices2 = new List<Vector3>(softbody.GetOriginalVertices());
		indices2 = new List<int>();
		normals2 = new List<Vector3>(mesh.normals);
		tangents2 = new List<Vector4>(mesh.tangents);
		uvs2 = new List<Vector2>(mesh.uv);

		_vertices1 = new List<int> ();
		_vertices2 = new List<int> ();
		_tris = new List<int> ();

		tri_backlist = new List<int> ();
	}

	public Mesh GetNewMesh() {
		Mesh mesh = new Mesh();

		mesh.vertices = vertices2.ToArray();
		mesh.triangles = indices2.ToArray();

		// Optional properties
		if (normals2 != null)
		{
			mesh.normals = normals2.ToArray();
		}

		if (tangents2 != null)
		{
			mesh.tangents = tangents2.ToArray();
		}

		if (uvs2 != null)
		{
			mesh.uv = uvs2.ToArray();
		}

		return mesh;
	}


	public void Split(Vector3 localPlaneNormal, Vector3 localPlaneDir, Vector3 localPointStart, Vector3 localPointEnd, int faceStart, int faceEnd, bool fillCut){

		Debug.LogFormat ("Split Point {0} {1} Face {2} {3} Normal {4}", localPointStart, localPointEnd, faceStart, faceEnd, localPlaneNormal);

		Clear ();

		planeNormal = localPlaneNormal;
		planeDir = localPlaneDir;

		indexStart = faceStart;
		indexEnd = faceEnd;
		pointStart = localPointStart;
		pointEnd = localPointEnd;

		// 剪切范围
		Vector3 center = (pointEnd + pointStart)/2f;

		if (localPlaneNormal == Vector3.zero)
		{
			localPlaneNormal = Vector3.up;
		}

		bool[] vertexAbovePlane = new bool[vertices.Count];

		for (int i = 0; i < vertices.Count; i++)
		{
			Vector3 vertex = vertices[i];
		
			bool abovePlane = Vector3.Dot (vertex - pointEnd, localPlaneNormal) >= 0.0f;

			vertexAbovePlane [i] = abovePlane;

		}

		// remove 
		int triangleCount = indices.Count / 3;
		for (int i = 0; i < triangleCount; i++) {
			int index0 = indices [i * 3 + 0];
			int index1 = indices [i * 3 + 1];
			int index2 = indices [i * 3 + 2];

			bool above0 = vertexAbovePlane [index0];
			bool above1 = vertexAbovePlane [index1];
			bool above2 = vertexAbovePlane [index2];

			if ((above0 && above1 && above2) || (!above0 && !above1 && !above2)) {
				// 保持不变
				//Debug.LogFormat(" old {0} {1} {2}", index0, index1, index2);
			} else {

				Vector3 v0 = vertices [index0];
				Vector3 v1 = vertices [index1];
				Vector3 v2 = vertices [index2];

				float a1 = Vector3.Dot (v0 - pointEnd, localPlaneNormal);
				float a2 = Vector3.Dot (v1 - pointEnd, localPlaneNormal);
				float a3 = Vector3.Dot (v2 - pointEnd, localPlaneNormal);


				// 切割点在tri上投影
				Vector3 fN = Vector3.Cross(v1-v0, v2-v0).normalized;
				Vector3 projS = MathUtils.LineIntersectPlane (fN, v0, planeDir, pointStart);
				Vector3 projE = MathUtils.LineIntersectPlane (fN, v0, planeDir, pointEnd);

				bool InS = MathUtils.PointInTriangle (v0, v1, v2, projS);
				bool InE = MathUtils.PointInTriangle (v0, v1, v2, projE);

				bool b1 = MathUtils.LineIntersect (v0, v1, projS, projE);
				bool b2 = MathUtils.LineIntersect (v0, v2, projS, projE);
				bool b3 = MathUtils.LineIntersect (v1, v2, projS, projE);

				//Debug.LogFormat ("Face {0} intersection {1} {2} {3}", i, b1, b2, b3);
				if (!InS && !InE && !b1 && !b2 && !b3) {
					// 不相交
					tri_backlist.Add (i);
				} else {
					// 检查到切割范围内？
					float d0 = MathUtils.PointDistPlane(planeDir, center, v0);
					float d1 = MathUtils.PointDistPlane(planeDir, center, v1);
					float d2 = MathUtils.PointDistPlane(planeDir, center, v2);

					if (d0 > 1 && d1 > 1 && d2 > 1) {
						tri_backlist.Add (i);
					}
				}
			}
		}

		IList<Vector3> cutEdges;
		AssignTriangles(vertexAbovePlane, localPointStart, localPlaneNormal, out cutEdges);

		if (fillCut) {

			cutEdges.Clear ();
			cutEdges.Add(pointStart);
			cutEdges.Add(pointStart + planeDir);
			cutEdges.Add(pointStart + planeDir);
			cutEdges.Add(pointEnd + planeDir);
			cutEdges.Add(pointEnd + planeDir);
			cutEdges.Add(pointEnd);
			cutEdges.Add(pointEnd);
			cutEdges.Add(pointStart);

			FillCutEdges (cutEdges, localPlaneNormal);

		}
		// update mesh & mass springs
		softbody.SetMesh(GetNewMesh(), _vertices1, _vertices2, _tris);
	}

	private void AssignTriangles(bool[] vertexAbovePlane, Vector3 pointOnPlane, Vector3 planeNormal, out IList<Vector3> cutEdges)
	{
		cutEdges = new List<Vector3>();

		int triangleCount = indices.Count / 3;

		for (int i = 0; i < triangleCount; i++) {
			int index0 = indices[i * 3 + 0];
			int index1 = indices[i * 3 + 1];
			int index2 = indices[i * 3 + 2];

			bool above0 = vertexAbovePlane[index0];
			bool above1 = vertexAbovePlane[index1];
			bool above2 = vertexAbovePlane[index2];

			if ((above0 && above1 && above2) || (!above0 && !above1 && !above2) || tri_backlist.Contains(i)) {
				// 保持不变
				indices2.Add(index0);
				indices2.Add(index1);
				indices2.Add(index2);

				//Debug.LogFormat("{0} {1} {2}", index0, index1, index2);
			} else {
				//continue;

				// Split triangle
				int top, cw, ccw;

				if (above1 == above2 && above0 != above1)
				{
					top = index0;
					cw = index1;
					ccw = index2;
				}
				else if (above2 == above0 && above1 != above2)
				{
					top = index1;
					cw = index2;
					ccw = index0;
				}
				else
				{
					top = index2;
					cw = index0;
					ccw = index1;
				}

				Vector3 cutVertex0, cutVertex1;

				//Debug.LogFormat ("{0} above {1}", top, vertexAbovePlane[top]);
				if (vertexAbovePlane[top])
				{
					SplitTriangle(pointOnPlane, planeNormal, i, top, cw, ccw, true, out cutVertex0, out cutVertex1);
				}
				else
				{
					SplitTriangle(pointOnPlane, planeNormal, i, top, cw, ccw, false, out cutVertex1, out cutVertex0);
				}

				// Add cut edge
				if (cutVertex0 != cutVertex1)
				{
					cutEdges.Add(cutVertex0);
					cutEdges.Add(cutVertex1);

					//Debug.LogFormat ("{0} cutedge {1} {2}", i, cutVertex0, cutVertex1);
				}

				// ADD remove springs
				softbody.RemoveMassSpring(top, cw);
				softbody.RemoveMassSpring(top, ccw);
			}
		}
	}


	private void SplitTriangle(Vector3 pointOnPlane, Vector3 planeNormal, int fid, int top, int cw, int ccw, bool bTop, out Vector3 cwIntersection, out Vector3 ccwIntersection)
	{
		//Debug.Log ("SplitTriangle " + fid);

		Vector3 v0 = vertices[top];
		Vector3 v1 = vertices[cw];
		Vector3 v2 = vertices[ccw];

		Vector3 v00 = vertices2[top];
		Vector3 v10 = vertices2[cw];
		Vector3 v20 = vertices2[ccw];


		// Intersect the top-cw edge with the plane
		float cwDenominator = Vector3.Dot(v1 - v0, planeNormal);
		float cwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / cwDenominator);

		// Intersect the top-ccw edge with the plane
		float ccwDenominator = Vector3.Dot(v2 - v0, planeNormal);
		float ccwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / ccwDenominator);

		// Interpolate vertex positions
		Vector3 cwVertex = new Vector3();
		cwVertex.x = v00.x + (v10.x - v00.x) * cwScalar;
		cwVertex.y = v00.y + (v10.y - v00.y) * cwScalar;
		cwVertex.z = v00.z + (v10.z - v00.z) * cwScalar;

		Vector3 ccwVertex = new Vector3();
		ccwVertex.x = v00.x + (v20.x - v00.x) * ccwScalar;
		ccwVertex.y = v00.y + (v20.y - v00.y) * ccwScalar;
		ccwVertex.z = v00.z + (v20.z - v00.z) * ccwScalar;

		Vector3 cwNormal = new Vector3();
		Vector3 ccwNormal = new Vector3();
		Vector4 cwTangent = new Vector4();
		Vector4 ccwTangent = new Vector4();
		Vector2 cwUv = new Vector2();
		Vector2 ccwUv = new Vector2();
	
		// Interpolate normals
		if (normals != null) {
			Vector3 n0 = normals[top];
			Vector3 n1 = normals[cw];
			Vector3 n2 = normals[ccw];

			cwNormal.x = n0.x + (n1.x - n0.x) * cwScalar;
			cwNormal.y = n0.y + (n1.y - n0.y) * cwScalar;
			cwNormal.z = n0.z + (n1.z - n0.z) * cwScalar;
			cwNormal.Normalize();

			ccwNormal.x = n0.x + (n2.x - n0.x) * ccwScalar;
			ccwNormal.y = n0.y + (n2.y - n0.y) * ccwScalar;
			ccwNormal.z = n0.z + (n2.z - n0.z) * ccwScalar;
			ccwNormal.Normalize();
		}
		// Interpolate tangents
		if (tangents != null) {
			Vector4 t0 = tangents[top];
			Vector4 t1 = tangents[cw];
			Vector4 t2 = tangents[ccw];

			cwTangent.x = t0.x + (t1.x - t0.x) * cwScalar;
			cwTangent.y = t0.y + (t1.y - t0.y) * cwScalar;
			cwTangent.z = t0.z + (t1.z - t0.z) * cwScalar;
			cwTangent.Normalize();
			cwTangent.w = t1.w;

			ccwTangent.x = t0.x + (t2.x - t0.x) * ccwScalar;
			ccwTangent.y = t0.y + (t2.y - t0.y) * ccwScalar;
			ccwTangent.z = t0.z + (t2.z - t0.z) * ccwScalar;
			ccwTangent.Normalize();
			ccwTangent.w = t2.w;
		}
		// Interpolate uvs
		if (uvs != null) {
			Vector2 u0 = uvs[top];
			Vector2 u1 = uvs[cw];
			Vector2 u2 = uvs[ccw];

			cwUv.x = u0.x + (u1.x - u0.x) * cwScalar;
			cwUv.y = u0.y + (u1.y - u0.y) * cwScalar;

			ccwUv.x = u0.x + (u2.x - u0.x) * ccwScalar;
			ccwUv.y = u0.y + (u2.y - u0.y) * ccwScalar;
		}

		// ADD - Start-End Split  
		if (fid == indexStart || fid == indexEnd) {
		//if (false) {

			Vector3 fN = Vector3.Cross (v1 - v0, v2 - v0).normalized;
			Vector3 projS = MathUtils.LineIntersectPlane (fN, v0, planeDir, pointStart);
			Vector3 projE = MathUtils.LineIntersectPlane (fN, v0, planeDir, pointEnd);

			bool b1 = MathUtils.LineIntersect (v0, v1, projS, projE);
			bool b2 = MathUtils.LineIntersect (v0, v2, projS, projE);

			Vector3 p;
			if (fid == indexStart) {
				p = projS;
			} else {
				p = projE;
			}

			float area = Mathf.Abs(Vector3.Cross (v1 - v0, v2 - v0).magnitude);
			float k0 = Mathf.Abs(Vector3.Cross (v2 - p, v1 - p).magnitude) / area; 
			float k1 = Mathf.Abs(Vector3.Cross (v2 - p, v0 - p).magnitude) / area; 
			float k2 = Mathf.Abs(Vector3.Cross (v0 - p, v1 - p).magnitude) / area; 

			Vector3 pp = k0 * v00 + k1 * v10 + k2 * v20;
	
			if (b1) {
				ccwVertex = pp;
				
				int cwA = vertices2.Count;
				vertices2.Add (cwVertex);

				int pIdx = vertices2.Count;
				vertices2.Add (pp);

				indices2.Add (top);
				indices2.Add (pIdx);
				indices2.Add (ccw);

				// add new triangle
				_tris.Add (top);
				_tris.Add (pIdx);
				_tris.Add (ccw);

				indices2.Add (cw);
				indices2.Add (ccw);
				indices2.Add (pIdx);

				// add new triangle
				_tris.Add (cw);
				_tris.Add (ccw);
				_tris.Add (pIdx);

				indices2.Add (top);
				indices2.Add (cwA);
				indices2.Add (pIdx);

				// add new triangle
				_tris.Add (top);
				_tris.Add (cwA);
				_tris.Add (pIdx);
			
				int cwB = vertices2.Count;
				vertices2.Add (cwVertex);

				indices2.Add (cw);
				indices2.Add (pIdx);
				indices2.Add (cwB);

				// add new triangle
				_tris.Add (cw);
				_tris.Add (pIdx);
				_tris.Add (cwB);

				_vertices1.Add (pIdx);
				if (bTop) {
					_vertices2.Add (cwA);
					_vertices1.Add (cwB);
				} else {
					_vertices1.Add (cwA);
					_vertices2.Add (cwB);
				}
			} else if (b2) {
				cwVertex = pp;

				int ccwA = vertices2.Count;
				vertices2.Add (ccwVertex);

				int pIdx = vertices2.Count;
				vertices2.Add (pp);

				indices2.Add (top);
				indices2.Add (cw);
				indices2.Add (pIdx);

				// add new triangle
				_tris.Add (top);
				_tris.Add (cw);
				_tris.Add (pIdx);

				indices2.Add (cw);
				indices2.Add (ccw);
				indices2.Add (pIdx);

				// add new triangle
				_tris.Add (cw);
				_tris.Add (ccw);
				_tris.Add (pIdx);

				indices2.Add (top);
				indices2.Add (pIdx);
				indices2.Add (ccwA);

				// add new triangle
				_tris.Add (top);
				_tris.Add (pIdx);
				_tris.Add (ccwA);

				int ccwB = vertices2.Count;
				vertices2.Add (ccwVertex);

				indices2.Add (ccw);
				indices2.Add (ccwB);
				indices2.Add (pIdx);

				// add new triangle
				_tris.Add (ccw);
				_tris.Add (ccwB);
				_tris.Add (pIdx);

				_vertices1.Add (pIdx);
				if (bTop) {
					_vertices2.Add (ccwA);
					_vertices1.Add (ccwB);
				} else {
					_vertices1.Add (ccwA);
					_vertices2.Add (ccwB);
				}
			}


			if (normals != null) {
				normals2.Add (cwNormal);
				normals2.Add (ccwNormal);
				normals2.Add (ccwNormal);
			}

			if (tangents != null) {
				tangents2.Add (cwTangent);
				tangents2.Add (ccwTangent);
				tangents2.Add (ccwTangent);
			}

			if (uvs != null) {
				uvs2.Add (cwUv);
				uvs2.Add (ccwUv);
				uvs2.Add (ccwUv);
			}

			//Debug.LogFormat ("SplitTriangle Face {0} intersection {1} {2} ", fid, b1, b2);
		} else {

			// Create top triangle
			{
				int cwA = vertices2.Count;
				vertices2.Add (cwVertex);

				int ccwA = vertices2.Count;
				vertices2.Add (ccwVertex);

				indices2.Add (top);
				indices2.Add (cwA);
				indices2.Add (ccwA);

				// add new triangle
				_tris.Add (top);
				_tris.Add (cwA);
				_tris.Add (ccwA);
			}

			// Create bottom triangles
			{
				int cwB = vertices2.Count;
				vertices2.Add (cwVertex);

				int ccwB = vertices2.Count;
				vertices2.Add (ccwVertex);

				indices2.Add (cw);
				indices2.Add (ccw);
				indices2.Add (ccwB);

				indices2.Add (cw);
				indices2.Add (ccwB);
				indices2.Add (cwB);

				// add new triangle
				_tris.Add (cw);
				_tris.Add (ccw);
				_tris.Add (ccwB);

				_tris.Add (cw);
				_tris.Add (ccwB);
				_tris.Add (cwB);
			}

			if (bTop) {
				int vc = vertices2.Count;
				_vertices2.Add (vc - 4);
				_vertices2.Add (vc - 3);

				_vertices1.Add (vc - 2);
				_vertices1.Add (vc - 1);
			} else {
				int vc = vertices2.Count;
				_vertices1.Add (vc-4);
				_vertices1.Add (vc-3);

				_vertices2.Add (vc-2);
				_vertices2.Add (vc-1);
			}

			if (normals != null) {
				normals2.Add (cwNormal);
				normals2.Add (ccwNormal);

				normals2.Add (cwNormal);
				normals2.Add (ccwNormal);
			}

			if (tangents != null) {
				tangents2.Add (cwTangent);
				tangents2.Add (ccwTangent);

				tangents2.Add (cwTangent);
				tangents2.Add (ccwTangent);
			}

			if (uvs != null) {
				uvs2.Add (cwUv);
				uvs2.Add (ccwUv);

				uvs2.Add (cwUv);
				uvs2.Add (ccwUv);
			}
		}

		// Set output
		cwIntersection = cwVertex;
		ccwIntersection = ccwVertex;
	}

	private void FillCutEdges(IList<Vector3> edges, Vector3 planeNormal)
	{
		Debug.Log ("FillCutEdges " + planeNormal);
		int edgeCount = edges.Count / 2;

		List<Vector3> points = new List<Vector3>(edgeCount);
		List<int> outline = new List<int>(edgeCount * 2);

		int start = 0;

		for (int current = 0; current < edgeCount; current++)
		{
			int next = current + 1;

			// Find the next edge
			int nearest = start;
			float nearestDistance = (edges[current * 2 + 1] - edges[start * 2 + 0]).sqrMagnitude;

			for (int other = next; other < edgeCount; other++)
			{
				float distance = (edges[current * 2 + 1] - edges[other * 2 + 0]).sqrMagnitude;

				if (distance < nearestDistance)
				{
					nearest = other;
					nearestDistance = distance;
				}
			}

			// Is the current edge the last edge in this edge loop?
			if (nearest == start && current > start)
			{
				int pointStart = points.Count;
				int pointCounter = pointStart;

				// Add this edge loop to the triangulation lists
				for (int edge = start; edge < current; edge++)
				{
					points.Add(edges[edge * 2 + 0]);
					outline.Add(pointCounter++);
					outline.Add(pointCounter);
				}

				points.Add(edges[current * 2 + 0]);
				outline.Add(pointCounter);
				outline.Add(pointStart);

				// Start a new edge loop
				start = next;
			}
			else if (next < edgeCount)
			{
				// Move the nearest edge so that it follows the current edge
				Vector3 n0 = edges[next * 2 + 0];
				Vector3 n1 = edges[next * 2 + 1];

				edges[next * 2 + 0] = edges[nearest * 2 + 0];
				edges[next * 2 + 1] = edges[nearest * 2 + 1];

				edges[nearest * 2 + 0] = n0;
				edges[nearest * 2 + 1] = n1;
			}
		}

		if (points.Count > 0)
		{
			// Triangulate the outline
			int[] newEdges, newTriangles, newTriangleEdges;

			ITriangulator triangulator = new Triangulator(points, outline, planeNormal);

			triangulator.Fill(out newEdges, out newTriangles, out newTriangleEdges);

			// Calculate the vertex properties
			Vector3 normalA = planeNormal;
			Vector3 normalB = -planeNormal;
			Vector4[] tangentsA, tangentsB;
			Vector2[] uvsA, uvsB;

			UVMap(points, planeNormal, out tangentsA, out tangentsB, out uvsA, out uvsB);

			// Add the new vertices
			int offsetA = vertices2.Count;

			for (int i = 0; i < points.Count; i++)
			{
				vertices2.Add(points[i]);

				_vertices1.Add (offsetA+i);
			}

			if (normals2 != null){
				for (int i = 0; i < points.Count; i++){
					normals2.Add(normalA);
				}
			}

			if (tangents2 != null){
				for (int i = 0; i < points.Count; i++)
				{
					tangents2.Add(tangentsA[i]);
				}
			}

			if (uvs2 != null){
				for (int i = 0; i < points.Count; i++){
					uvs2.Add(uvsA[i]);
				}
			}

			// Add the new triangles
			int newTriangleCount = newTriangles.Length / 3;

			int offsetB = vertices2.Count;

			for (int i = 0; i < points.Count; i++){
				vertices2.Add(points[i]);

				_vertices2.Add (offsetB+i);
			}

			if (normals != null){
				for (int i = 0; i < points.Count; i++){
					normals2.Add(normalB);
				}
			}

			if (tangents2 != null){
				for (int i = 0; i < points.Count; i++){
					tangents2.Add(tangentsB[i]);
				}
			}

			if (uvs2 != null){
				for (int i = 0; i < points.Count; i++){
					uvs2.Add(uvsB[i]);
				}
			}

			// Add the new triangles

			Vector3 n = Vector3.Cross(points[2]-points[0], points[1]-points[0]).normalized;
			Debug.Log ("cut normal " + n);
			float fN = Vector3.Dot (n, planeNormal);
			for (int i = 0; i < newTriangleCount; i++){

				/*
				if (fN > 0) {
					indices2.Add (offsetA + newTriangles [i * 3 + 0]);
					indices2.Add (offsetA + newTriangles [i * 3 + 2]);
					indices2.Add (offsetA + newTriangles [i * 3 + 1]);

					indices2.Add (offsetB + newTriangles [i * 3 + 0]);
					indices2.Add (offsetB + newTriangles [i * 3 + 1]);
					indices2.Add (offsetB + newTriangles [i * 3 + 2]);
				} else {
					indices2.Add (offsetA + newTriangles [i * 3 + 0]);
					indices2.Add (offsetA + newTriangles [i * 3 + 1]);
					indices2.Add (offsetA + newTriangles [i * 3 + 2]);

					indices2.Add (offsetB + newTriangles [i * 3 + 0]);
					indices2.Add (offsetB + newTriangles [i * 3 + 2]);
					indices2.Add (offsetB + newTriangles [i * 3 + 1]);
				}
				*/

				indices2.Add (offsetA + newTriangles [i * 3 + 0]);
				indices2.Add (offsetA + newTriangles [i * 3 + 1]);
				indices2.Add (offsetA + newTriangles [i * 3 + 2]);

				indices2.Add (offsetB + newTriangles [i * 3 + 0]);
				indices2.Add (offsetB + newTriangles [i * 3 + 2]);
				indices2.Add (offsetB + newTriangles [i * 3 + 1]);

				for (int j = 0; j < 6; j++) {
					_tris.Add (indices2.Count - 1 - j);
				}
			}
		}
	}

	public void UVMap(IList<Vector3> points, Vector3 planeNormal, out Vector4[] tangentsA, out Vector4[] tangentsB, out Vector2[] uvsA, out Vector2[] uvsB)
	{

		// Calculate texture direction vectors
		Vector3 u = Vector3.Cross(planeNormal, Vector3.up);

		if (u == Vector3.zero)
		{
			u = Vector3.Cross(planeNormal, Vector3.forward);
		}

		Vector3 v = Vector3.Cross(u, planeNormal);

		u.Normalize();
		v.Normalize();

		// Set tangents
		Vector4 tangentA = new Vector4(u.x, u.y, u.z, 1.0f);
		Vector4 tangentB = new Vector4(u.x, u.y, u.z, -1.0f);

		tangentsA = new Vector4[points.Count];
		tangentsB = new Vector4[points.Count];

		for (int i = 0; i < points.Count; i++)
		{
			tangentsA[i] = tangentA;
			tangentsB[i] = tangentB;
		}

		// Set uvs
		Vector2[] uvs = new Vector2[points.Count];

		Vector2 min = Vector2.zero;
		Vector2 max = Vector2.zero;

		for (int i = 0; i < points.Count; i++)
		{
			Vector3 point = points[i];

			uvs[i].x = Vector3.Dot(point, u);
			uvs[i].y = Vector3.Dot(point, v);

			if (i == 0)
			{
				min = uvs[i];
				max = uvs[i];
			}
			else
			{
				min = Vector2.Min(uvs[i], min);
				max = Vector2.Max(uvs[i], max);
			}
		}

		Vector2 originalSize = max - min;

		if (false)
		{
			float largestSide = Mathf.Max(originalSize.x, originalSize.y);

			Vector2 offset = new Vector2();

			offset.x = (largestSide - originalSize.x) * 0.5f;
			offset.y = (largestSide - originalSize.y) * 0.5f;

			min -= offset;
			max += offset;
		}

		if (false)
		{
			Vector2 largestExtent = new Vector2();

			largestExtent.x = Mathf.Max(Mathf.Abs(min.x), Mathf.Abs(max.x));
			largestExtent.y = Mathf.Max(Mathf.Abs(min.y), Mathf.Abs(max.y));

			min = -largestExtent;
			max = largestExtent;
		}

		Vector2 size = max - min;
		Vector2 invSize = new Vector2(1.0f / size.x, 1.0f / size.y);

		for (int i = 0; i < points.Count; i++)
		{
			// Convert uvs to the range [0, 1]
			uvs[i].x = (uvs[i].x - min.x) * invSize.x;
			uvs[i].y = (uvs[i].y - min.y) * invSize.y;

			// Convert uvs to the range [targetStart, targetStart + targetSize]
			uvs[i].x = UVStart.x + UVSize.x * uvs[i].x;
			uvs[i].y = UVStart.y + UVSize.y * uvs[i].y;
		}

		uvsA = uvs;
		uvsB = uvs;
	}
}
