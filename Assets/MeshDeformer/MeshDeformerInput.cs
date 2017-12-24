using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshDeformerInput : MonoBehaviour {

	private List<MeshDeformer> lastMeshDeformers = new List<MeshDeformer>();
	private List<MeshDeformer> curMeshDeformers = new List<MeshDeformer>();

	private List<MeshForce> meshforces = new List<MeshForce> ();

	void Awake() {
		Physics.queriesHitBackfaces = true;
	}

	void Update () {
			
		HandleInput();

		curMeshDeformers.Clear ();
		for (int i = 0; i < meshforces.Count; i ++) {
			Vector3[] normals = meshforces [i].Normals;
			Vector3 pos = meshforces [i].transform.position;
			float radius = meshforces [i].Radius;
			for (int j = 0; j < normals.Length; j++) {
				RaycastHit[] hits;
				hits = Physics.RaycastAll (pos, normals [j], 1000f);

				for (int k = 0; k < hits.Length; k++) {
					RaycastHit hit = hits [k];

					if (hit.distance > radius) continue;

					//Debug.LogFormat ("hit {0}, {1}, {2}, {3}", hit.collider.name, hit.distance, hit.point, hit.normal);
					
					MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
					if (deformer != null) {
						deformer.AddDeformingForce (pos, normals [j], radius);
						curMeshDeformers.Add (deformer);
					}
				}
			}
		}

		//Debug.LogFormat (" last:{0} cur:{1}", lastMeshDeformers.Count, curMeshDeformers.Count);
		for (int i = 0; i < lastMeshDeformers.Count; i++) {
			if (curMeshDeformers.Contains (lastMeshDeformers [i]) == false) {
				lastMeshDeformers [i].ClearForce ();
			}
		}

		lastMeshDeformers.Clear ();
		for (int i = 0; i < curMeshDeformers.Count; i++) {
			lastMeshDeformers.Add (curMeshDeformers [i]);
		}

		for (int i = 0; i < lastMeshDeformers.Count; i++) {
			lastMeshDeformers [i].ResetVertices ();
		}
	}

	void HandleInput () {

		/*
		float force = 0.1f;

		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("mouse down");
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (inputRay, out hit)) {
				MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
				if (deformer) {
					Vector3 point = hit.point;
					//point += hit.normal * 0.05f;
					deformer.AddDeformingForce (point, -hit.normal, force);
				}
			}
		}
		*/

		if (Input.GetMouseButtonDown (1)) {
			Debug.Log ("mouse down");
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (inputRay, out hit)) {
				MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
				if (deformer) {
					Vector3 point = hit.point;
					point += hit.normal * 0.05f;
					AddMeshForce (point, 0.1f);
				}
			}
		}
	}


	public void AddMeshForce(Vector3 pos, float radius) {

		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		MeshForce meshforce = go.AddComponent<MeshForce> ();
		go.transform.SetParent (transform);
		go.transform.localPosition = pos;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		meshforce.SetRadius (radius);
		meshforces.Add (meshforce);
	}

	void OnGUI() {

		GUI.skin.label.fontSize = 24;
		if (GUILayout.Button ("AddForce", GUILayout.Width(120), GUILayout.Height(60))) {
			AddMeshForce (Vector3.zero, 1f);
		}
	}
}