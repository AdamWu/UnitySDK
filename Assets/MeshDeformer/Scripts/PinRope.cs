using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinRope : MonoBehaviour {

	public static PinRope instance;

	public Pin pin;

	public GameObject start;


	List<Transform> fragments = new List<Transform>();

	LineRenderer linerenderer;

	// Use this for initialization
	void Awake () {
		instance = this;
		linerenderer = GetComponent<LineRenderer>();

		// update rope
		fragments.Add(start.transform);
	}
	
	void LateUpdate()
	{
		// Copy rigidbody positions to the line renderer
		LineRenderer renderer = GetComponent<LineRenderer>();
		renderer.positionCount = fragments.Count + 1;

		// No interpolation

		for (int i = 0; i < fragments.Count; i++)
		{
			renderer.SetPosition(i, fragments[i].position);
		}
		renderer.SetPosition(renderer.positionCount-1, pin.TailPosition);
	}

	// 添加穿刺点
	public void AddPuncturePoint(SoftBody deformer, int vidx) {

		FollowForce force = MeshDeformerInput.instance.AddFollowForce(deformer, vidx, deformer.GetWorldPosOfOriginalVertex(vidx));

		// update rope
		fragments.Add(force.transform);
	}

	public void Tighten() {
		float deltaL = 0.01f;

		Vector3[] offsets = new Vector3[fragments.Count];
		for (int i = 0; i < fragments.Count-1; i++) {
		
			offsets[i] = fragments [i+1].position - fragments [i].position;

		}

		for (int i = 1; i < fragments.Count; i++) {

			FollowForce force = fragments [i].GetComponent<FollowForce> ();
		
			//Debug.Log ("force "+force.Force);

			if (i == 1) {
				Vector3 dir = fragments[i+1].position - fragments[i].position;
				Vector3 dir2 = fragments [i - 1].position - fragments [i].position;	
				Vector3 move = (dir + dir2*0.01f) * 0.1f;
				force.SimpleMove (move);
			} else if (i == fragments.Count - 1) {
				Vector3 dir = pin.TailPosition - fragments [i].position;	
				Vector3 dir2 = fragments [i - 1].position - fragments [i].position;
				Vector3 move = (dir*0.1f + dir2) * 0.1f;
				force.SimpleMove (move);
			} else {
				Vector3 dir = fragments[i+1].position - fragments[i].position;
				Vector3 dir2 = fragments [i - 1].position - fragments [i].position;	
				Vector3 move = (dir + dir2) * 0.1f;
				force.SimpleMove (move);
			}
		} 
	}

	public void Clear() {
		for (int i = 1; i < fragments.Count; i++) {
			FollowForce force = fragments [i].GetComponent<FollowForce> ();
			force.ClearTargetVertex ();
			Destroy (force.gameObject);
		}
		fragments.RemoveRange (1, fragments.Count - 1);
	}
}
