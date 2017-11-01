using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CameraScaleMode {
    FixedHeight=0,          // unity default
    FixedWidth,
    Dynamic,
}

public class CameraScaler : MonoBehaviour {
  
  	public Vector2 	ReferenceResolution		= new Vector2(1080,1920);
	public int 	ReferencePixelsPerUnit		= 100;

    public CameraScaleMode mode             = CameraScaleMode.FixedHeight;

	// Use this for initialization
	void Start () {

        if (mode == CameraScaleMode.FixedHeight) {
            // camera size no change
        } else if (mode == CameraScaleMode.FixedWidth) {
            float currentRatio = (float)Screen.width / Screen.height;
            Camera.main.orthographicSize = ReferenceResolution.x /currentRatio / 2 / ReferencePixelsPerUnit; 

        } else {
            // dynamic
		    float desiredRatio = ReferenceResolution.x / ReferenceResolution.y;
		    float currentRatio = (float)Screen.width/Screen.height;

		    if(currentRatio >= desiredRatio) {
			    // Our resolution has plenty of width, so we just need to use the height to determine the camera size
			    Camera.main.orthographicSize = ReferenceResolution.y / 2 / ReferencePixelsPerUnit;
		    } else {
			    // Our camera needs to zoom out further than just fitting in the height of the image.
			    // Determine how much bigger it needs to be, then apply that to our original algorithm.
			    float differenceInSize = desiredRatio / currentRatio;
			    Camera.main.orthographicSize = ReferenceResolution.y / 2 / ReferencePixelsPerUnit * differenceInSize;
		    }
        }
	}
}