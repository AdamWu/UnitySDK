using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshDeformerInput : MonoBehaviour {

	public GameObject MeshForcePrefab;
	public GameObject FollowForcePrefab;

	public static MeshDeformerInput instance;

	public float GlobalScale = 1f;

	private List<MeshDeformer> lastMeshDeformers = new List<MeshDeformer>();
	private List<MeshDeformer> curMeshDeformers = new List<MeshDeformer>();

	private List<MeshForce> meshforces = new List<MeshForce> ();

	void Awake() {
		instance = this;

		Physics.queriesHitBackfaces = true;
	}

	void Update () {
			
		HandleInput();
	}

	void HandleInput () {

		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("mouse down");
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (inputRay, out hit)) {
				SoftBody deformer = hit.collider.GetComponent<SoftBody> ();
				if (deformer) {
					int fidx = hit.triangleIndex;
					Vector3 vertex;
					int vidx = deformer.FindNearestVertexInTriangle (fidx, hit.point, out vertex);

					AddFollowForce (deformer, vidx, hit.point + hit.normal * 0.01f);
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


	public FollowForce AddFollowForce(SoftBody meshdeformer, int vidx, Vector3 pos) {
		GameObject go = GameObject.Instantiate (FollowForcePrefab);
		FollowForce followForce = go.GetComponent<FollowForce> ();
		followForce.SetTargetVertex (meshdeformer, vidx);
		go.transform.SetParent (transform);
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one * GlobalScale;
		go.transform.position = pos;
		return followForce;
	}


	public MeshForce AddMeshForce(Vector3 pos, float radius) {
		GameObject go = GameObject.Instantiate (MeshForcePrefab);
		MeshForce meshforce = go.GetComponent<MeshForce> ();
		go.transform.SetParent (transform);
		go.transform.localPosition = pos;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		meshforce.SetRadius (radius);
		meshforces.Add (meshforce);
		return meshforce;
	}

	void OnGUI() {
		/*
		GUI.skin.label.fontSize = 24;
		if (GUILayout.Button ("AddForce", GUILayout.Width(120), GUILayout.Height(60))) {
			AddMeshForce (Vector3.zero, 1f);
		}
		*/
	}
}