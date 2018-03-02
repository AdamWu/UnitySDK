using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PinRopeState {
	Out,			// 寻找接触点
	Deformer,		// 变形
	Puncture,		// 穿刺
	Pass,			// 穿透
}

public class PinRope : MonoBehaviour {

	public Pin pin;
	public Rigidbody rope;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void FixedUpdate () {

	}


	void OnDrawGizmos() {

		Gizmos.color = new Color(1, 0, 0, 0.5f);
		//Gizmos.DrawSphere(transform.position, _radius);

		Gizmos.color =  Color.yellow;

	}
}
