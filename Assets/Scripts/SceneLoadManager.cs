using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SceneLoadManager : Singleton<SceneLoadManager> {

	public string CurSceneName { get; set;}

	public string LoadSceneName { get; set;}


	public void GotoScene(string sceneName) {
		Debug.LogFormat ("GotoScene {0}", sceneName);
		LoadSceneName = sceneName;

		UnityEngine.SceneManagement.SceneManager.LoadScene ("loading");
	}

	public void LoadSceneComplete() {
		CurSceneName = LoadSceneName;
		LoadSceneName = null;
	}
}