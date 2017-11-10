using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PanelMain : MonoBehaviour {

	// Use this for initialization
	void Start () {


		NotificationCenter.DefaultCenter ().AddListener ((int)NotificationType.ADD_CUBE, addCube);

		GameObject go = new GameObject ("linerender");
		LineRenderer lr = go.AddComponent<LineRenderer> ();
		lr.startWidth = 0.1f;
		lr.endWidth = 0.1f;
		lr.positionCount = 2;
		lr.SetPosition (0, new Vector3 (0, 0, 0));
		lr.SetPosition (1, new Vector3 (100, 0, 0));

		Button btn = transform.Find ("Button Start").GetComponent<Button> ();
		btn.onClick.AddListener (delegate {
			OnBtnStart(btn.gameObject);
		});
		Button btn1 = transform.Find ("Button Scene1").GetComponent<Button> ();
		btn1.name = "1";
		btn1.onClick.AddListener (delegate {
			OnBtnSwitch(btn1.gameObject);
		});
		Button btn2 = transform.Find ("Button Scene2").GetComponent<Button> ();
		btn2.name = "2";
		btn2.onClick.AddListener (delegate {
			OnBtnSwitch(btn2.gameObject);
		});
		Button btn3 = transform.Find ("Button Scene3").GetComponent<Button> ();
		btn3.name = "3";
		btn3.onClick.AddListener (delegate {
			OnBtnSwitch(btn3.gameObject);
		});
			
	}

	void Update() {

	}
	
	void OnDestroy() {
		NotificationCenter.DefaultCenter ().RemoveListener ((int)NotificationType.ADD_CUBE, addCube);
	}

	void OnBtnStart(GameObject sender) {
		Debug.Log ("OnBtnStart");
		NotificationCenter.DefaultCenter ().PostNotification ((int)NotificationType.ADD_CUBE);
	}

	void OnBtnSwitch(GameObject sender) {
		Debug.Log ("OnBtnStart");

		if (sender.name == "1") {
			SceneLoadManager.Instance.GotoScene ("lightmap");
		} else if (sender.name == "2") {
			SceneLoadManager.Instance.GotoScene ("bigscene");
		} else if (sender.name == "3") {
			SceneLoadManager.Instance.GotoScene ("modelshow");
		}
	}


	public void addCube(Notification notification) {
		Debug.Log ("addCube");

		GameObject prefab1 = (GameObject)ResourceManager.Instance.LoadAsset ("Prefabs/Cube", "Cube");
		GameObject go1 = Instantiate (prefab1, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity);
		ResourceManager.Instance.UnloadAsset ("Prefabs/Cube");
		//GameObject prefab1 = (GameObject)ResourceManager.Instance.LoadAsset ("Prefabs/Test", "Cube");
		//GameObject go1 = Instantiate (prefab1, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity);

		ResourceManager.Instance.LoadAssetAsync ("Prefabs/Test", "Cube", delegate(Object obj) {
			GameObject prefab2 = (GameObject)obj;
			GameObject go2 = Instantiate (prefab2, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity);
			ResourceManager.Instance.UnloadAsset("Prefabs/Test");
		});
	}
}
