using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshDeformerInput : MonoBehaviour {

	private List<MeshDeformer> lastMeshDeformers = new List<MeshDeformer>();
	private List<MeshDeformer> curMeshDeformers = new List<MeshDeformer>();

	private List<MeshForce> meshforces = new List<MeshForce> ();

	void Update () {
			
		//HandleInput();

		curMeshDeformers.Clear ();
		for (int i = 0; i < meshforces.Count; i ++) {
			Vector3[] normals = meshforces [i].Normals;
			Vector3 pos = meshforces [i].transform.position;
			float radius = meshforces [i].radius;
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

		if (Input.GetMouseButtonDown (1)) {
			Debug.Log ("mouse down");
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (inputRay, out hit)) {
				MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
				if (deformer) {
					Vector3 point = hit.point;
					deformer.AddDeformingForce (point, hit.normal, force);
				}
			}
		}
	}


	public void AddMeshForce(Vector3 pos) {
		
	}

	void OnDrawGizmos() {

	}

	void OnGUI() {

		GUI.skin.label.fontSize = 24;
		if (GUILayout.Button ("AddForce", GUILayout.Width(120), GUILayout.Height(60))) {
			GameObject go = new GameObject ();
			MeshForce meshforce = go.AddComponent<MeshForce> ();
			go.transform.SetParent (transform);
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			meshforces.Add (meshforce);
		}
	}
}