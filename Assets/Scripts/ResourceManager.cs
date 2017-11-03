using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;


public delegate void LoadAssetAsyncDelegate(Object obj);

public class ResourceManager : Singleton<ResourceManager> {

	public void Init() {
		Debug.Log("ResourceManager:Init");
	}

	public Object LoadAsset(string assetBundleName, string assetName) {
		AssetBundle assetBundle = AssetBundleManager.LoadAssetBundle(assetBundleName);
		if (assetBundle != null) {
			Object asset = assetBundle.LoadAsset(assetName);
			return asset;
		}
		return null;
	}

	public void LoadAssetAsync(string assetBundleName, string assetName, LoadAssetAsyncDelegate callback) {
		AssetBundleManager.Instance.LoadAssetBundleAsync(assetBundleName, delegate(AssetBundle assetBundle) {
			if (assetBundle != null) {
				Object asset = assetBundle.LoadAsset(assetName);
				callback(asset);
			}
		});
	}

	public void UnloadAsset(string assetBundleName) {
		AssetBundleManager.UnloadAssetBundle(assetBundleName);
	}

	public static long GetFileSize(string filename) {
		if (!File.Exists(filename)) {
			Debug.LogFormat("GetFileSize: {0} not Exist!", filename);
			return 0;
		}

		FileStream fs = new FileStream(filename, FileMode.Open);
		long length = fs.Length;
		fs.Close();
		return length;
	}
	public static string GetFileHash(string filename) {
		if (!File.Exists(filename)) {
			Debug.LogFormat("GetFileHash: {0} not Exist!", filename);
			return null;
		}

		FileStream fs = new FileStream(filename, FileMode.Open);
		byte[] data = new byte[fs.Length];
		fs.Read (data, 0, (int)fs.Length);
		Hash128 hash = Hash128.Parse (data.ToString ());
		fs.Close();
		return hash.ToString();
	}
	public static string GetFileMD5(string filename) {
		if (!File.Exists(filename)) {
			Debug.LogFormat("GetFileMD5: {0} not Exist!", filename);
			return null;
		}

		FileStream fs = new FileStream(filename, FileMode.Open);
		byte[] data = new byte[fs.Length];
		fs.Read (data, 0, (int)fs.Length);
		fs.Close();
		MD5 md5 = new MD5CryptoServiceProvider ();
		byte[] result = md5.ComputeHash (data);
		string filemd5 = "";
		foreach (byte b in result) {
			filemd5 += System.Convert.ToString (b, 16);
		}
		return filemd5;
	}
}