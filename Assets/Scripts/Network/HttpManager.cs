﻿using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class HttpManager : Singleton<HttpManager> 
{

	public delegate void ResponseHandler(string data);


	public void HttpGet(string host, string action, Hashtable args, ResponseHandler handler ) {

		string url = string.Format("{0}/{1}", host, action);

		int i = 0;
		if (args != null) {
			foreach (DictionaryEntry e in args) {
				if (i == 0)
					url = url + "?";
				else
					url = url + "&";

				url = url + e.Key.ToString () + "=" + e.Value.ToString ();
				i++;
			}
		}

		//System.Uri.EscapeUriString(string.Format("http://{0}/{1}?{2}", GameConfig.HttpHost, action, _args));
	
		Debug.Log("HttpGet " + url);

		WWW www = new WWW(url);
		StartCoroutine(WaitForRequest(www, handler));
	}

	public void HttpPost(string host, string action, IDictionary args, ResponseHandler handler)
	{
		string url = string.Format("{0}/{1}", host, action);

		int i = 0;
		string post = "{";
		foreach (DictionaryEntry e in args) {
			if (i>0) post = post + ",";
			if (e.Value is string) {
				post = post  + string.Format("\"{0}\":\"{1}\"", e.Key.ToString(), e.Value.ToString());
			} else {
				post = post  + string.Format("\"{0}\":{1}", e.Key.ToString(), e.Value.ToString());
			}
			i ++;
		}
		post = post + "}";

		byte[] data = System.Text.Encoding.UTF8.GetBytes (post);

		WWW www = new WWW(url, data);
		StartCoroutine(WaitForRequest(www, handler));
	}

	private IEnumerator WaitForRequest(WWW www, ResponseHandler handler) {

		//GUIManager.Instance.ShowWait();

		yield return www;

		//GUIManager.GetInstance().HideWait();

		if (www.error == null)
		{
			if (www.bytes != null && www.bytes.Length > 0)
			{
				string data = Encoding.UTF8.GetString(www.bytes);
				handler(data);
			}
		}
		else
		{
			Debug.LogError(www.error);
			//GUIManager.GetInstance().ShowAlert(LanguageManager.GetInstance().GetValue("TITLE_ERR"), www.error);
		}
	}

}
