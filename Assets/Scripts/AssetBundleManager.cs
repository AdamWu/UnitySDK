using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;


public delegate void LoadAssetBundleAsyncDelegate(AssetBundle ab);

public delegate void HandleDownloadFinish(WWW www);
public delegate void HandleDownloadCallback();

public class AssetBundleManager : Singleton<AssetBundleManager> {

	private class LoadedAssetBundle {
		public AssetBundle assetBundle;
		public int refCount;
		public LoadedAssetBundle(AssetBundle assetBundle) {
			this.assetBundle = assetBundle;
			refCount = 1;
		}
		public void Unload() {
			assetBundle.Unload (false);
		}
	}

	void Awake() {
		// download path for platforms
		s_BaseDownloadingURL += AssetBundleLoader.GetPlatformFolderForAssetBundles();
	}


	// assetbundles
	static Dictionary<string, string[]> s_AssetBundleDependencies = new Dictionary<string, string[]>();
	static Dictionary<string, LoadedAssetBundle> s_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

	#region assetbundle downloading
	static string s_BaseDownloadingURL = Config.CdnUrl;

	static Queue<string> s_ToDownloadAssetBundles = new Queue<string>();
	static Dictionary<string, WWW> s_DownloadingWWWs = new Dictionary<string, WWW>();

	static HandleDownloadCallback m_callback;
	public void SetDownloadCallback(HandleDownloadCallback callback) { m_callback = callback; }

	public static int GetToDownloadAssetBundleNum() {
		return s_ToDownloadAssetBundles.Count;
	}
	public static void AddDownloadAssetBundle(string assetBundleName) {
		s_ToDownloadAssetBundles.Enqueue(assetBundleName);
	}
	public static int GetDownloadingWWWNum() {
		return s_DownloadingWWWs.Count;
	}
	#endregion

	public void InitDependenceInfo() {
		Debug.Log ("InitDependenceInfo");

		StartCoroutine (_LoadDependenceInfo ());
	} 
	IEnumerator _LoadDependenceInfo() {

		string filename = Path.Combine(AssetBundleLoader.STREAMING_ASSET_PATH, AssetBundleLoader.GetPlatformFolderForAssetBundles());
		WWW www = new WWW(filename);
		yield return www;

		if (www.error != null) {
			Debug.LogWarning(www.error);
			www.Dispose();
			yield break;
		} 

		AssetBundle assetBundle = AssetBundle.LoadFromMemory (www.bytes);
		AssetBundleManifest manifest = assetBundle.LoadAsset ("assetbundlemanifest") as AssetBundleManifest;
		string[] assetBundleNames = manifest.GetAllAssetBundles ();
		foreach (string assetBundleName in assetBundleNames) {
			string[] dependencies = manifest.GetAllDependencies(assetBundleName);
			s_AssetBundleDependencies.Add (assetBundleName, dependencies);
		}
		www.Dispose();
		assetBundle.Unload (true);

	}

	// load assetbuddle 
	// ref++
	public static AssetBundle LoadAssetBundle(string assetBundleName) {
		assetBundleName = assetBundleName.ToLower ();
		assetBundleName += ".unity3d";
		if (s_AssetBundleDependencies.ContainsKey(assetBundleName)) {
			string[] dependencies = s_AssetBundleDependencies [assetBundleName];
			foreach (string dependency in dependencies) {
				if (s_LoadedAssetBundles.ContainsKey (dependency)) {
					s_LoadedAssetBundles [dependency].refCount++; 
				} else {
					// 加载
					//string filename = Path.Combine(Application.persistentDataPath, dependency);
					string filename = Path.Combine(Application.streamingAssetsPath, dependency);
					AssetBundle ab = AssetBundle.LoadFromFile(filename);
					if (ab) {
						Debug.LogFormat("AssetBundle(Dependency) loaded : {0}", dependency);
						LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle (ab);
						s_LoadedAssetBundles.Add (dependency, loadedAssetBundle);
						continue;
					}
				}
			}
		}

		if (s_LoadedAssetBundles.ContainsKey (assetBundleName)) {

			s_LoadedAssetBundles [assetBundleName].refCount++;

			return s_LoadedAssetBundles [assetBundleName].assetBundle;
		} else {
			// 加载
			//string filename = Path.Combine(Application.persistentDataPath, assetBundleName);
			string filename = Path.Combine(Application.streamingAssetsPath, AssetBundleLoader.GetPlatformFolderForAssetBundles());
			filename = Path.Combine(filename, assetBundleName);
			AssetBundle ab = AssetBundle.LoadFromFile(filename);
			if (ab) {
				Debug.LogFormat("AssetBundle loaded : {0}", assetBundleName);
				LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle (ab);
				s_LoadedAssetBundles.Add (assetBundleName, loadedAssetBundle);
				return s_LoadedAssetBundles [assetBundleName].assetBundle;
			}
		
			return null;
		}
	}

	// load assetbuddle Async
	// ref++
	public void LoadAssetBundleAsync(string assetBundleName, LoadAssetBundleAsyncDelegate callback) {
		StartCoroutine (_LoadAssetBundleAsync (assetBundleName, callback));
	}
	IEnumerator _LoadAssetBundleAsync(string assetBundleName, LoadAssetBundleAsyncDelegate callback) {
		assetBundleName = assetBundleName.ToLower ();
		assetBundleName += ".unity3d";
		if (s_AssetBundleDependencies.ContainsKey(assetBundleName)) {
			string[] dependencies = s_AssetBundleDependencies [assetBundleName];
			foreach (string dependency in dependencies) {
				if (s_LoadedAssetBundles.ContainsKey (dependency)) {
					s_LoadedAssetBundles [dependency].refCount++; 
				} else {
					// 加载
					//string filename = Path.Combine(Application.persistentDataPath, dependency);
					string filename = Path.Combine(AssetBundleLoader.STREAMING_ASSET_PATH, dependency);
					WWW www = new WWW(filename);
					yield return www;

					if (www.error == null) {
						AssetBundle ab = AssetBundle.LoadFromMemory (www.bytes);
						Debug.LogFormat("AssetBundle(Dependency) loaded : {0}", dependency);
						LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle (ab);
						s_LoadedAssetBundles.Add (dependency, loadedAssetBundle);
						www.Dispose();
						continue;
					} else {
						Debug.LogWarning(www.error);
						www.Dispose();
						yield break;
					}
				}
			}
		}

		if (s_LoadedAssetBundles.ContainsKey (assetBundleName)) {

			s_LoadedAssetBundles [assetBundleName].refCount++;

			callback (s_LoadedAssetBundles [assetBundleName].assetBundle);
		} else {
			// 加载
			//string filename = Path.Combine(Application.persistentDataPath, assetBundleName);
			string filename = Path.Combine(AssetBundleLoader.STREAMING_ASSET_PATH, assetBundleName);
			WWW www = new WWW(filename);
			yield return www;

			if (www.error == null) {
				AssetBundle ab = AssetBundle.LoadFromMemory (www.bytes);
				Debug.LogFormat("AssetBundle loaded : {0}", assetBundleName);
				LoadedAssetBundle loadedAssetBundle = new LoadedAssetBundle (ab);
				s_LoadedAssetBundles.Add (assetBundleName, loadedAssetBundle);
				callback (s_LoadedAssetBundles [assetBundleName].assetBundle);
				www.Dispose();
			} else {
				Debug.LogWarning(www.error);
				www.Dispose();
				callback (null);
			}
		}
	}

	
	public static void UnloadAssetBundle(string assetBundleName) {
		assetBundleName = assetBundleName.ToLower ();
		assetBundleName += ".unity3d";
		if (s_AssetBundleDependencies.ContainsKey(assetBundleName)) {
			string[] dependencies = s_AssetBundleDependencies [assetBundleName];
			foreach (string dependency in dependencies) {
				if (s_LoadedAssetBundles.ContainsKey (dependency)) {
					s_LoadedAssetBundles [dependency].refCount--;
					if (s_LoadedAssetBundles [dependency].refCount == 0) {
						s_LoadedAssetBundles [dependency].Unload ();
						s_LoadedAssetBundles.Remove (dependency);
						Debug.LogFormat("AssetBundle unloaded : {0}", dependency);
					}
				}
			}
		}

		if (s_LoadedAssetBundles.ContainsKey (assetBundleName)) {

			s_LoadedAssetBundles [assetBundleName].refCount--;

			if (s_LoadedAssetBundles [assetBundleName].refCount == 0) {
				s_LoadedAssetBundles [assetBundleName].Unload ();
				s_LoadedAssetBundles.Remove (assetBundleName);
				Debug.LogFormat("AssetBundle unloaded : {0}", assetBundleName);
			}
		}
	}
		


	#region assetbundle download
	private void DownloadAssetBundle(string assetBundleName) {
		Debug.Log("DownloadAssetBundle " + assetBundleName);
		StartCoroutine( _DownloadAssetBundle(s_BaseDownloadingURL+assetBundleName, assetBundleName, delegate (WWW www){

			// write to local 
			WriteToLocal(assetBundleName, www.bytes);
		}
		));
	}

 	IEnumerator _DownloadAssetBundle(string url, string assetBundleName, HandleDownloadFinish handler) {

		Debug.Log("start downloading " + url);

		WWW www = new WWW(url);
		s_DownloadingWWWs.Add(assetBundleName, www);

		yield return www;

		if (www.error != null) {
			Debug.LogError("downloading error! " + www.error);
		} else {
			if (www.isDone) {
				if (handler != null) {
					handler(www);
				}
			}
		}

		// destroy
		s_DownloadingWWWs.Remove(assetBundleName);
		www.Dispose();
	}

	void Update() {
		if (s_DownloadingWWWs.Count < 5) {
			if (s_ToDownloadAssetBundles.Count > 0) {
				string assetBundleName = s_ToDownloadAssetBundles.Dequeue();
				DownloadAssetBundle(assetBundleName);

				if (m_callback != null) m_callback();
			}
		}
	}

	private void WriteToLocal(string name, byte [] data) {
		Debug.Log("WriteToLocal " + name);
		string filename = Path.Combine(Application.persistentDataPath, name);
		if (!File.Exists(filename)) {
			string path = Path.GetDirectoryName(filename);
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
		}

		FileStream file = new FileStream(filename, FileMode.Create);
		file.Write(data, 0, data.Length);
		file.Close();
	}
	#endregion 
}