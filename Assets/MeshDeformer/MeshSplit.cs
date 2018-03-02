using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MeshSplit : MonoBehaviour {


	private Vector3 start, end;
	private bool started = false;

	Plane splitPlane;

	int startVertexId, endVertexId;
	Vector3 startVertex, endVertex;
		

	Splitter splitter;

	void Start() {		

		SoftBody body = GetComponent<SoftBody>();
		
		splitter = new Splitter (body);
	}

	void Update() {
		if (Input.GetMouseButtonDown(1)) {

			start = Input.mousePosition;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit)) {
				SoftBody deformer = hit.collider.GetComponent<SoftBody> ();
				if (deformer) {
					int fidx = hit.triangleIndex;
					Vector3 vertex;
					endVertexId = deformer.FindNearestVertexInTriangle (fidx, hit.point, out vertex);
					startVertex = deformer.transform.TransformPoint (vertex);

					GameObject go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					go.transform.localRotation = Quaternion.identity;
					//go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					go.transform.position = hit.point + hit.normal * 0.05f;

					started = true;
				}
			}
		}

		if (Input.GetMouseButtonUp(1) && started) {

			end = Input.mousePosition;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
		
			if (Physics.Raycast (ray, out hit)) {
				SoftBody deformer = hit.collider.GetComponent<SoftBody> ();
				if (deformer) {
					int fidx = hit.triangleIndex;
					Vector3 vertex;

					endVertexId = deformer.FindNearestVertexInTriangle (fidx, hit.point, out vertex);
					endVertex = deformer.transform.TransformPoint (vertex);

					GameObject go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					go.transform.localRotation = Quaternion.identity;
					//go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					go.transform.position = hit.point + hit.normal * 0.05f;


					float near = Camera.main.near;

					Vector3 line = Camera.main.ScreenToWorldPoint(new Vector3(end.x, end.y, near)) - Camera.main.ScreenToWorldPoint(new Vector3(start.x, start.y, near));

					splitPlane = new Plane(Vector3.Normalize(Vector3.Cross(line, ray.direction)), hit.point);

					started = false;

					// split
					Vector3 localPoint = transform.InverseTransformPoint(splitPlane.normal * -splitPlane.distance);
					Vector3 localNormal = transform.InverseTransformDirection(splitPlane.normal);

					localNormal.Scale(transform.localScale);
					localNormal.Normalize();

					splitter.Split (localPoint, localNormal);

					deformer.SetMesh (splitter.GetNewMesh());
				}
			}
		}
	}
		

	void OnDrawGizmos() {

		Gizmos.color = new Color(1, 0, 0, 0.5f);
		//Gizmos.DrawSphere(transform.position, 0.01f);

		Gizmos.DrawLine(startVertex, endVertex);

		Gizmos.color =  Color.yellow;

	}
}