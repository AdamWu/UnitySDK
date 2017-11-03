using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AssetBundleLoader {

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
	
    // WWW加载时StreamingAsset各平台路径
    public static readonly string STREAMING_ASSET_PATH =
		#if !UNITY_EDITOR && UNITY_WEBGL
		Path.Combine (Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#elif !UNITY_EDITOR && UNITY_ANDROID
		Path.Combine (Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#else
		Path.Combine ("file://" + Application.streamingAssetsPath, GetPlatformFolderForAssetBundles ());
		#endif

	/*
    // www协程实现
    static IEnumerator LoadFromWWWCoroutine(string path, Action<AssetBundle> callback)
    {
        WWW www = new WWW(path);
        yield return www;
        callback(www.assetBundle);
        www.Dispose();
        www = null;
    }

    //开启协程
    static void LoadAssetbundleAsync(string finalPath, Action<AssetBundle> callback)
    {
		StartCoroutine(LoadFromWWWCoroutine(finalPath, callback));
    }

    // 从StreamingAssetsPath异步加载
    public static AssetBundle LoadFromStreamingAssetsPathAsync(string assetbundle) {
        return LoadAssetbundleAsync(STREAMING_ASSET_PATH + "/" + assetbundle);
    }

    // PersistantDataPath异步加载
    public static AssetBundle LoadFromPersistantDataPathAsync(string assetbundle) {
        return LoadAssetbundleAsync(Application.persistentDataPath+ "/" + assetbundle);
	}
	*/
}