using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PanelTestWebService : MonoBehaviour {

	// Use this for initialization
	void Start () {

		Button btn_send = transform.Find ("Button Send").GetComponent<Button> ();
		btn_send.onClick.AddListener (delegate {
			OnBtnSend(btn_send.gameObject);

			HttpManager.Instance.HttpGet("http://api.map.baidu.com/telematics/v3/weather?location=%E5%98%89%E5%85%B4&output=json&ak=5slgyqGDENN7Sy7pw29IUvrZ", "", null, delegate(string data) {
				Debug.Log(data);
				Dictionary<string, object> dic = MiniJSON.Json.Deserialize(data) as Dictionary<string, object>;
				Debug.Log(dic["status"].ToString());
			});
		});

		Button btn_exit = transform.Find ("Button Exit").GetComponent<Button> ();
		btn_exit.onClick.AddListener (delegate {
			OnBtnExit(btn_exit.gameObject);
		});

		JSManager.Instance.GetUrlFromJs ();
	}


	void OnBtnSend(GameObject sender) {
		Debug.Log ("OnBtnSend");


	}


	void OnBtnExit(GameObject sender) {
		Debug.Log ("OnBtnExit");


	}


}
