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

	public Splitter(SoftBody softbody)
	{
		this.softbody = softbody;

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

		vertices2 = new List<Vector3>(mesh.vertices);
		indices2 = new List<int>();
		normals2 = new List<Vector3>(mesh.normals);
		tangents2 = new List<Vector4>(mesh.tangents);
		uvs2 = new List<Vector2>(mesh.uv);
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

	public void Split(Vector3 localPointOnPlane, Vector3 localPlaneNormal){
		if (localPlaneNormal == Vector3.zero)
		{
			localPlaneNormal = Vector3.up;
		}

		bool[] vertexAbovePlane = new bool[vertices.Count];

		for (int i = 0; i < vertices.Count; i++)
		{
			Vector3 vertex = vertices[i];

			bool abovePlane = Vector3.Dot(vertex - localPointOnPlane, localPlaneNormal) >= 0.0f;

			vertexAbovePlane[i] = abovePlane;
		}

		IList<Vector3> cutEdges;
		AssignTriangles(vertexAbovePlane, localPointOnPlane, localPlaneNormal, out cutEdges);


		FillCutEdges(cutEdges, localPlaneNormal);
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

			if ((above0 && above1 && above2) || (!above0 && !above1 && !above2)) {
				// 保持不变
				indices2.Add(index0);
				indices2.Add(index1);
				indices2.Add(index2);
			} else {
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

				if (vertexAbovePlane[top])
				{
					SplitTriangle(pointOnPlane, planeNormal, top, cw, ccw, out cutVertex0, out cutVertex1);
				}
				else
				{
					SplitTriangle(pointOnPlane, planeNormal, top, cw, ccw, out cutVertex1, out cutVertex0);
				}

				// Add cut edge
				if (cutVertex0 != cutVertex1)
				{
					cutEdges.Add(cutVertex0);
					cutEdges.Add(cutVertex1);
				}
			}
		}
	}


	private void SplitTriangle(Vector3 pointOnPlane, Vector3 planeNormal, int top, int cw, int ccw, out Vector3 cwIntersection, out Vector3 ccwIntersection)
	{
		Vector3 v0 = vertices[top];
		Vector3 v1 = vertices[cw];
		Vector3 v2 = vertices[ccw];

		// Intersect the top-cw edge with the plane
		float cwDenominator = Vector3.Dot(v1 - v0, planeNormal);
		float cwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / cwDenominator);

		// Intersect the top-ccw edge with the plane
		float ccwDenominator = Vector3.Dot(v2 - v0, planeNormal);
		float ccwScalar = Mathf.Clamp01(Vector3.Dot(pointOnPlane - v0, planeNormal) / ccwDenominator);

		// Interpolate vertex positions
		Vector3 cwVertex = new Vector3();

		cwVertex.x = v0.x + (v1.x - v0.x) * cwScalar;
		cwVertex.y = v0.y + (v1.y - v0.y) * cwScalar;
		cwVertex.z = v0.z + (v1.z - v0.z) * cwScalar;

		Vector3 ccwVertex = new Vector3();

		ccwVertex.x = v0.x + (v2.x - v0.x) * ccwScalar;
		ccwVertex.y = v0.y + (v2.y - v0.y) * ccwScalar;
		ccwVertex.z = v0.z + (v2.z - v0.z) * ccwScalar;

		// Create top triangle
		{
			int cwA = vertices2.Count;
			vertices2.Add (cwVertex);

			int ccwA = vertices2.Count;
			vertices2.Add (ccwVertex);

			indices2.Add (top);
			indices2.Add (cwA);
			indices2.Add (ccwA);
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
		}

		// Interpolate normals
		if (normals != null)
		{
			Vector3 n0 = normals[top];
			Vector3 n1 = normals[cw];
			Vector3 n2 = normals[ccw];

			Vector3 cwNormal = new Vector3();

			cwNormal.x = n0.x + (n1.x - n0.x) * cwScalar;
			cwNormal.y = n0.y + (n1.y - n0.y) * cwScalar;
			cwNormal.z = n0.z + (n1.z - n0.z) * cwScalar;

			cwNormal.Normalize();

			Vector3 ccwNormal = new Vector3();

			ccwNormal.x = n0.x + (n2.x - n0.x) * ccwScalar;
			ccwNormal.y = n0.y + (n2.y - n0.y) * ccwScalar;
			ccwNormal.z = n0.z + (n2.z - n0.z) * ccwScalar;

			ccwNormal.Normalize();

			// Add vertex property
			normals2.Add(cwNormal);
			normals2.Add(ccwNormal);

			normals2.Add(cwNormal);
			normals2.Add(ccwNormal);
		}

		// Interpolate tangents
		if (tangents != null)
		{
			Vector4 t0 = tangents[top];
			Vector4 t1 = tangents[cw];
			Vector4 t2 = tangents[ccw];

			Vector4 cwTangent = new Vector4();

			cwTangent.x = t0.x + (t1.x - t0.x) * cwScalar;
			cwTangent.y = t0.y + (t1.y - t0.y) * cwScalar;
			cwTangent.z = t0.z + (t1.z - t0.z) * cwScalar;

			cwTangent.Normalize();
			cwTangent.w = t1.w;

			Vector4 ccwTangent = new Vector4();

			ccwTangent.x = t0.x + (t2.x - t0.x) * ccwScalar;
			ccwTangent.y = t0.y + (t2.y - t0.y) * ccwScalar;
			ccwTangent.z = t0.z + (t2.z - t0.z) * ccwScalar;

			ccwTangent.Normalize();
			ccwTangent.w = t2.w;

			// Add vertex property
			tangents2.Add(cwTangent);
			tangents2.Add(ccwTangent);

			tangents2.Add(cwTangent);
			tangents2.Add(ccwTangent);
		}

		// Interpolate uvs
		if (uvs != null)
		{
			Vector2 u0 = uvs[top];
			Vector2 u1 = uvs[cw];
			Vector2 u2 = uvs[ccw];

			Vector2 cwUv = new Vector2();

			cwUv.x = u0.x + (u1.x - u0.x) * cwScalar;
			cwUv.y = u0.y + (u1.y - u0.y) * cwScalar;

			Vector2 ccwUv = new Vector2();

			ccwUv.x = u0.x + (u2.x - u0.x) * ccwScalar;
			ccwUv.y = u0.y + (u2.y - u0.y) * ccwScalar;

			// Add vertex property
			uvs2.Add(cwUv);
			uvs2.Add(ccwUv);

			uvs2.Add(cwUv);
			uvs2.Add(ccwUv);
		}

		// Set output
		cwIntersection = cwVertex;
		ccwIntersection = ccwVertex;
	}

	private void FillCutEdges(IList<Vector3> edges, Vector3 planeNormal)
	{
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
			Vector3 normalA = -planeNormal;
			Vector3 normalB = planeNormal;
			Vector4[] tangentsA, tangentsB;
			Vector2[] uvsA, uvsB;

			UVMap(points, planeNormal, out tangentsA, out tangentsB, out uvsA, out uvsB);

			// Add the new vertices
			int offsetA = vertices2.Count;

			for (int i = 0; i < points.Count; i++)
			{
				vertices2.Add(points[i] - normalA*1f);
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

			for (int i = 0; i < newTriangleCount; i++)
			{
				indices2.Add(offsetA + newTriangles[i * 3 + 0]);
				indices2.Add(offsetA + newTriangles[i * 3 + 2]);
				indices2.Add(offsetA + newTriangles[i * 3 + 1]);
			}

			int offsetB = vertices2.Count;

			for (int i = 0; i < points.Count; i++){
				vertices2.Add(points[i] - normalB*1f);
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
			for (int i = 0; i < newTriangleCount; i++){
				indices2.Add(offsetB + newTriangles[i * 3 + 0]);
				indices2.Add(offsetB + newTriangles[i * 3 + 1]);
				indices2.Add(offsetB + newTriangles[i * 3 + 2]);
			}
		}
	}

	public void UVMap(IList<Vector3> points, Vector3 planeNormal, out Vector4[] tangentsA, out Vector4[] tangentsB, out Vector2[] uvsA, out Vector2[] uvsB)
	{

		Vector2 scale = Vector2.one;

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

		for (int i = 0; i < points.Count; i++)
		{
			Vector3 point = points[i];

			uvs[i].x = Vector3.Dot(point, u);
			uvs[i].y = Vector3.Dot(point, v);

			if (i == 0)
			{
				min = uvs[i];
			}
			else
			{
				min = Vector2.Min(uvs[i], min);
			}
		}

		for (int i = 0; i < points.Count; i++)
		{
			uvs[i] -= min;

			uvs[i].x *= scale.x;
			uvs[i].y *= scale.y;
		}

		uvsA = uvs;
		uvsB = uvs;
	}
}
