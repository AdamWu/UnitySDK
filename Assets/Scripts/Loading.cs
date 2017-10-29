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
 
public class Loading : MonoBehaviour
{
	public string sceneName="main";
	private WWW _wwww = null;
	private AsyncOperation _asyncOp = null;
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

		LoadSceneFromAssetBundle (sceneName);
	}

	void Update() {

		if (_asyncOp != null) {
			int progress = (int)(_asyncOp.progress * 100);
			text_progress.text = string.Format("加载场景{0}%", progress);
			slider_progress.value = _asyncOp.progress;
		} else if (_wwww != null) {
			int progress = (int)(_wwww.progress * 100);
			text_progress.text = string.Format("下载资源{0}%", progress);
			slider_progress.value = _wwww.progress;
		}

	}


	void LoadSceneFromAssetBundle(string name) {

		StartCoroutine (_LoadSceneFromAssetBundle (name));
	}
	
 
	//读取一个
	private IEnumerator _LoadSceneFromAssetBundle(string name)
	{
		string path = "";
		#if !UNITY_EDITOR && UNITY_WEBGL
		path = Path.Combine (Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#elif !UNITY_EDITOR && UNITY_ANDROID
		path = Path.Combine ("jar:file://" + Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#else
		path = Path.Combine ("file://" + Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#endif

		path = path + "/" + name + ".unity3d.data";

		Debug.Log ("LoadMain " + path);
		_wwww = new WWW(path);

		yield return _wwww;

		if (_wwww.error != null) {
			Debug.LogError(_wwww.error);
			_wwww.Dispose();
			yield break;
		}

		byte[] data = AES.AESDecrypt (_wwww.bytes);

		byte[] encrypt_data = NativePlugin.Encrypt (data);
		byte[] decrypt_data = NativePlugin.Decrypt (encrypt_data);

		AssetBundle ab = AssetBundle.LoadFromMemory (decrypt_data);

		encrypt_data = null;
		decrypt_data = null;

		data = null;

		_asyncOp =  UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);

		yield break;
	}
}