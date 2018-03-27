using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MeshDeformerManager : MonoBehaviour {

	public GameObject MeshForcePrefab;

	public static MeshDeformerManager instance;
	public static float GlobalScale = 1f;

	private List<MeshDeformer> lastMeshDeformers = new List<MeshDeformer>();
	private List<MeshDeformer> curMeshDeformers = new List<MeshDeformer>();

	private List<MeshForce> meshforces = new List<MeshForce> ();

	void Awake() {
		if (instance == null) {
			instance = this;
		}

		Physics.queriesHitBackfaces = true;
	}

	void FixedUpdate () {
			
		//HandleInput();

		curMeshDeformers.Clear ();


		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch ();
		sw.Start ();

		bool bDirty = false;
		for (int i = 0; i < meshforces.Count; i++) {
			if (meshforces [i].Dirty) {
				bDirty = true;
				break;
			}
		}
		if (!bDirty) return;

		for (int i = 0; i < meshforces.Count; i ++) {
			Vector3[] normals = meshforces [i].Normals;
			Vector3 pos = meshforces [i].transform.position;
			float radius = meshforces [i].Radius * GlobalScale;
			for (int j = 0; j < normals.Length; j++) {
				RaycastHit[] hits;
				hits = Physics.RaycastAll (pos, normals [j], 1000f);

				for (int k = 0; k < hits.Length; k++) {
					RaycastHit hit = hits [k];

					if (hit.distance > radius * 2) continue;

					Debug.LogFormat ("hit {0}, {1}, {2}, {3}", hit.collider.name, hit.distance, hit.point, hit.normal);
					
					MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
					if (deformer != null) {
						deformer.AddDeformingForce (pos, normals [j], radius);
						curMeshDeformers.Add (deformer);
					}
				}
			}
			meshforces [i].Dirty = false;
		}

		sw.Stop ();
		UnityEngine.Debug.LogFormat ("update using {0}", sw.ElapsedMilliseconds);

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

	void Update() {
		HandleInput ();
	}

	void HandleInput () {
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("mouse down");
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (inputRay, out hit)) {
				MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer> ();
				if (deformer) {
					AddMeshForce (hit.point + hit.normal * 0.01f, 1f);
				}
			}
		}

		if (Input.GetMouseButtonDown (1)) {
			Debug.Log ("mouse down");
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (inputRay, out hit)) {
				FollowForce force = hit.collider.GetComponent<FollowForce> ();
				if (force) {
					Destroy (force.gameObject);
				}
			}
		}
	}


	public void AddMeshForce(Vector3 pos, float radius) {
		Debug.Log ("AddMeshForce " + radius);
		GameObject go = Instantiate(MeshForcePrefab);
		MeshForce meshforce = go.GetComponent<MeshForce> ();
		go.transform.SetParent (transform);
		go.transform.localPosition = pos;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = new Vector3(radius*2, radius*2, radius*2);
		meshforce.SetRadius (radius);
		meshforces.Add (meshforce);
	}

	public void AddMeshForce(Transform parent, Vector3 pos, float radius) {
		Debug.Log ("AddMeshForce " + radius);

		GlobalScale = parent.localScale.x;

		GameObject go = Instantiate(MeshForcePrefab);
		MeshForce meshforce = go.GetComponent<MeshForce> ();
		go.transform.SetParent (parent);
		go.transform.position = pos;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = new Vector3(radius*2, radius*2, radius*2);
		meshforce.SetRadius (radius);
		meshforces.Add (meshforce);
	}

	public void ClearAllForces() {
		Debug.Log ("ClearAllForces");

		for (int i = 0; i < meshforces.Count; i++) {
			Destroy (meshforces[i].gameObject);
		}
		meshforces.Clear ();
	}

	void OnGUI() {

		/*
		GUI.skin.label.fontSize = 24;
		if (GUILayout.Button ("AddForce", GUILayout.Width(120), GUILayout.Height(60))) {
			AddMeshForce (Vector3.zero, 0.2f);
		}
		*/
	}
}