using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

// move
public class CameraMove : MonoBehaviour {
    float delta_x, delta_y,speed;
    Quaternion rotation;
    
	void Start () {
        delta_x = 1; delta_y = 1; speed=0.5f;
    }

	void Update () {
		if (EventSystem.current.IsPointerOverGameObject ())
			return;
		
		if (Input.GetMouseButton(1)) {
			
            delta_x = Input.GetAxis("Mouse X") * speed;
            delta_y = Input.GetAxis("Mouse Y") * speed;
            //  camera.transform.localEulerAngles =  new Vector3(0,0, 0);
            rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            transform.position = rotation * new Vector3(-delta_x, -delta_y, 0) + transform.position;
        }
            
        
	}
}
