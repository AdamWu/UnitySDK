using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

[RequireComponent(typeof(SoftBody))]
public class MeshSplit : MonoBehaviour {

	[SerializeField]
	private bool fillCut = true;

	[SerializeField]
	public Vector2 UVStart = Vector2.zero;

	[SerializeField]
	public Vector2 UVSize = Vector2.one;

	SoftBody softbody;
	Splitter splitter;

	void Start() {		

		softbody = GetComponent<SoftBody>();
		
		splitter = new Splitter (softbody, UVStart, UVSize);
	}
		
	public void Split(Vector3 planeNormal, Vector3 planeDir, Vector3 planePointStart, Vector3 planePointEnd, int faceStart, int faceEnd) {
		
		Vector3 localPointStart = transform.InverseTransformPoint(planePointStart);
		Vector3 localPointEnd = transform.InverseTransformPoint(planePointEnd);
		Vector3 localNormal = transform.InverseTransformDirection(planeNormal);
		Vector3 localDir = transform.InverseTransformDirection(planeDir);

		localNormal.Scale(transform.localScale);
		localNormal.Normalize();

		splitter.Split (localNormal, localDir, localPointStart, localPointEnd, faceStart, faceEnd, fillCut);
	}

}