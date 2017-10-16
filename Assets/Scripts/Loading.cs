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
		case BuildTarget.WebGL:
			return "WebGL";
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
		Debug.Log ("GetPlatformFolderForAssetBundles " + platform);
		switch(platform) {
		case RuntimePlatform.Android:
			return "Android";
		case RuntimePlatform.IPhonePlayer:
			return "iOS";
		case RuntimePlatform.WebGLPlayer:
			return "WebGL";
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
		string path = "";
		#if !UNITY_EDITOR && UNITY_WEBGL
		path = Path.Combine (Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#else
		path = Path.Combine ("file:///" + Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#endif
		StartCoroutine (LoadMain (path + "/main.unity3d.data"));
	}

	
 
	//读取一个
	private IEnumerator LoadMain(string path)
	{
		Debug.Log ("LoadMain " + path);
		WWW www = new WWW(path);

		yield return www;

		if (www.error != null) {
			Debug.LogError(www.error);
			www.Dispose();
			yield break;
		}

		byte[] data = AES.AESDecrypt (www.bytes);

		AssetBundle ab = AssetBundle.LoadFromMemory (data);

		//AssetBundle ab = AssetBundle.LoadFromFile (path);

		UnityEngine.SceneManagement.SceneManager.LoadScene("main");

		yield break;
	}
}