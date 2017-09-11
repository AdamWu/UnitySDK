using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;  
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class AssetBundleBuild : Editor 
{
	const string kAssetBundlesOutputPath = "Assets/StreamingAssets/";

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

	[MenuItem("AssetBundle/Build All")]
	private static void BuildBase()
	{
		string PlatformFolder = GetPlatformFolderForAssetBundles (EditorUserBuildSettings.activeBuildTarget);
		string path = Path.Combine(kAssetBundlesOutputPath, PlatformFolder);

		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}

		AssetBundleManifest manifest =  BuildPipeline.BuildAssetBundles(path, 0, EditorUserBuildSettings.activeBuildTarget);
		if (manifest == null) {
			EditorUtility.DisplayDialog ("警告", "没有需要打包的assetbundle，请先设置assetbundle的名字！", "确定");
			return;
		}

		Debug.Log("build base ok");
	}

	private static void DeleteEmptyFolder(string path) {
		foreach( string dir in Directory.GetDirectories(path)){
			DeleteEmptyFolder(dir);
		}
	
		string[] paths =  Directory.GetDirectories(path);
		string[] files =  Directory.GetFiles(path);
		if (files.Length == 0 && paths.Length == 0) {
			Directory.Delete(path);
		}
	}
}