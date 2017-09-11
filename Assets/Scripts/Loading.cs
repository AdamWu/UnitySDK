using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Collections;
 
public class Loading : MonoBehaviour
{
	
	#if UNITY_EDITOR
	public static string GetPlatformFolderForAssetBundles(BuildTarget target)
	{
		switch(target)
		{
		case BuildTarget.Android:
			return "Android";
		case BuildTarget.iOS:
			return "iOS";
		case BuildTarget.WebPlayer:
			return "WebPlayer";
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			return "Windows";
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSXUniversal:
			return "OSX";
		default:
			return null;
		}
	}
#endif

	public static string GetPlatformFolderForAssetBundles(RuntimePlatform platform)
	{
		switch(platform) {
		case RuntimePlatform.Android:
			return "Android";
		case RuntimePlatform.IPhonePlayer:
			return "iOS";
		case RuntimePlatform.WindowsWebPlayer:
		case RuntimePlatform.OSXWebPlayer:
			return "WebPlayer";
		case RuntimePlatform.WindowsPlayer:
			return "Windows";
		case RuntimePlatform.OSXPlayer:
			return "OSX";
		default:
			return null;
		}
	}
	
	public static string GetPlatformFolderForAssetBundles() {
#if UNITY_EDITOR
		return GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
		return GetPlatformFolderForAssetBundles(Application.platform);
#endif
	}
	
	void Awake() {

	}
	
	void Start() {
		string path = Path.Combine(Application.persistentDataPath, GetPlatformFolderForAssetBundles());
		StartCoroutine(LoadMain(path + "/main.unity3d"));
	}
 
	//读取一个
	private IEnumerator LoadMain(string path)
	{
		/*
		WWW www = new WWW(path);

		yield return www;

		if (www.error != null) {
			Debug.LogError(www.error);
			www.Dispose();
			yield break;
		}

		www.assetBundle.isStreamedSceneAssetBundle;

		UnityEngine.SceneManagement.SceneManager.LoadScene("main");

		//www.assetBundle.Unload(false);

		*/

		AssetBundle ab = AssetBundle.LoadFromFile (path);

		UnityEngine.SceneManagement.SceneManager.LoadScene("main");

		yield break;
	}
}