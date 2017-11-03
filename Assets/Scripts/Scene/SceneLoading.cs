using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.Runtime.InteropServices;
 
public class SceneLoading : MonoBehaviour
{
	private Text text_progress;
	private Slider slider_progress;
	
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
		text_progress = GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ();
		slider_progress = GameObject.Find ("Canvas/Panel/Slider").GetComponent<Slider> ();
	}
	
	void Start() {

		byte[] data = Encoding.Default.GetBytes ("test12345678");
		byte[] encrypt_data = NativePlugin.Encrypt (data);
		string str_encrypt = Encoding.Default.GetString (encrypt_data);
		Debug.Log ("encrypt -> " + str_encrypt);
		string str_decrypt = Encoding.Default.GetString (NativePlugin.Decrypt (encrypt_data));
		Debug.Log ("decrypt -> " + str_decrypt);

		LoadSceneFromAssetBundle (SceneLoadManager.Instance.LoadSceneName);
	}

	void SetProgress(string str, float progress) {
		int value = (int)(progress * 100);
		text_progress.text = string.Format("{0}{1}%", str, value);
		slider_progress.value = progress;
	}


	void LoadSceneFromAssetBundle(string name) {

		StartCoroutine (_LoadSceneFromAssetBundle (name));
	}
	
 
	//读取一个
	private IEnumerator _LoadSceneFromAssetBundle(string name)
	{

		yield return new WaitForEndOfFrame ();

		string path = "";
		#if !UNITY_EDITOR && UNITY_WEBGL
		path = Path.Combine (Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#elif !UNITY_EDITOR && UNITY_ANDROID
		path = Path.Combine (Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#else
		path = Path.Combine ("file://" + Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#endif

		path = path + "/" + name + ".unity3d.data";

		Debug.Log ("LoadSceneFromAssetBundle " + path);
		WWW www = new WWW(path);

		while (!www.isDone) {
			Debug.Log ("download " + www.progress);
			SetProgress ("下载资源", www.progress);
			yield return new WaitForEndOfFrame ();
		}

		if (www.error != null) {
			Debug.LogError(www.error);
			www.Dispose();
			yield break;
		}

		byte[] data = AES.AESDecrypt (www.bytes);
		AssetBundle assetBundle = AssetBundle.LoadFromMemory (data);
		www.Dispose();
		data = null;

		AsyncOperation asyncOp =  UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);
		asyncOp.allowSceneActivation = false;
		while (asyncOp.progress < 0.9f) {
			Debug.Log ("load scene " + asyncOp.progress);
			SetProgress ("加载场景", asyncOp.progress);
			yield return new WaitForEndOfFrame ();
		}
		SetProgress ("加载场景", 1);
		yield return new WaitForEndOfFrame ();
		asyncOp.allowSceneActivation = true;

		assetBundle.Unload (false);
		SceneLoadManager.Instance.LoadSceneComplete();

		yield break;
	}
}