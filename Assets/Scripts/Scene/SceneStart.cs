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
 
public class SceneStart : MonoBehaviour
{
	public string sceneName="main";

	void Awake() {
		AssetBundleManager.Instance.InitDependenceInfo ();
	}
	
	void Start() {

		SceneLoadManager.Instance.GotoScene (sceneName);
	}
}