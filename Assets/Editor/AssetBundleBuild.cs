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

	[MenuItem("AssetBundle/Build All")]
	private static void BuildBase()
	{
		string PlatformFolder = GetPlatformFolderForAssetBundles (EditorUserBuildSettings.activeBuildTarget);
		string path = Path.Combine(kAssetBundlesOutputPath, PlatformFolder);

		// 清空文件夹
		if (Directory.Exists(path)) {
			Directory.Delete(path, true);
		}

		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}

		AssetBundleManifest manifest =  BuildPipeline.BuildAssetBundles(path, 0, EditorUserBuildSettings.activeBuildTarget);
		if (manifest == null) {
			EditorUtility.DisplayDialog ("警告", "没有需要打包的assetbundle，请先设置assetbundle的名字！", "确定");
			return;
		}

		EncryptFilesRecursively (path);

		Debug.Log("build base ok");
	}


	private static void EncryptFilesRecursively(string path) {
		// files
		string[] files = Directory.GetFiles(path);
		foreach( string file in files) {
			if (Path.GetExtension (file) == ".unity3d") {
				EncryptFile (file);
			}
		}

		// dirs recusively
		string[] dirs = Directory.GetDirectories(path);
		for (int i = 0; i < dirs.Length; i++) {
			EncryptFilesRecursively(dirs[i]);
		}
	}

	private static void EncryptFile(string file) {
		Debug.Log ("encrypt file:" + file);

		FileStream fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite);

		int numBytesToRead = (int)fs.Length;
		int numBytesRead = 0;
		byte[] readByte = new byte[fs.Length];
		//读取字节
		while (numBytesToRead > 0)
		{
			// Read may return anything from 0 to numBytesToRead.
			int n = fs.Read(readByte, numBytesRead, numBytesToRead);

			// Break when the end of the file is reached.
			if (n == 0)
				break;

			numBytesRead += n;
			numBytesToRead -= n;
		}
		fs.Close();
			
		//加密
		byte[] newBuff = AES.AESEncrypt(readByte);

		// 保存
		FileStream cfs = new FileStream(file + ".data", FileMode.Create);
		cfs.Write(newBuff, 0, newBuff.Length);
		newBuff = null;
		cfs.Close();
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