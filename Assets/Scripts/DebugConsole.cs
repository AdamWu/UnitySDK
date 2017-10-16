using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour {

	//public GUIStyle guiStyle;
	
	public class Log {
		public string msg;
		public string stacktrace;
		public LogType type;
		public Log(string msg, string stacktrace, LogType type) {
			this.msg = msg;
			this.stacktrace = stacktrace;
			this.type = type;
		}
	}
	static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>  
        {  
            { LogType.Assert, Color.white },  
            { LogType.Error, Color.red },  
            { LogType.Exception, Color.red },  
            { LogType.Log, Color.white },  
            { LogType.Warning, Color.yellow },  
        };  

	private double 		lastInterval = 0.0;
	private int 		frames = 0;
	private float 		m_fps;
	private float 		m_accumTime = 0;
	private float 		m_fpsUpdateInterval = 0.5f;

	private string 		strFPS;
	private string 		strMem;

	private List<Log> 	m_logs = new List<Log>();
	private const int LogMaxCount = 25;

	void Awake() {
		DontDestroyOnLoad(gameObject);

		Texture2D tex = new Texture2D(128, 128);
		for (int y = 0; y < tex.height; y ++) {
			for (int x = 0; x < tex.width; x ++) {
				tex.SetPixel(x,y, Color.gray);
			}
		}
		//guiStyle.normal.background = tex;
	}

	void Start () {
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
	}

	void HandleLog(string msg, string stacktrace, LogType type) {
		//if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception) {
		m_logs.Add(new Log(msg, stacktrace, type));
		//}
		if (m_logs.Count > LogMaxCount) {
			m_logs.RemoveAt (0);
		}

		if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception) {
			if (m_logs.Count > LogMaxCount - 5) {
				m_logs.RemoveRange (0, 5);
			}
		}
	}

	void OnEnable() {
		Application.logMessageReceived += HandleLog;
	}

	void OnDisable () {
		Application.logMessageReceived -= HandleLog;
	}

	// Update is called once per frame
	void Update () {
		++frames;
		//*
		float timeNow = Time.realtimeSinceStartup;
		if (timeNow - lastInterval > m_fpsUpdateInterval) {
			float fps =  frames /(float) (timeNow - lastInterval);
			float ms = 1000.0f / Mathf.Max(fps, 0.0001f);
			strFPS = string.Format("{0} ms {1}FPS", ms.ToString("f1"), fps.ToString("f2"));
			frames = 0;
			lastInterval = timeNow;
		}
		/*/
		/*
		float dt = Time.deltaTime/Time.timeScale;
		m_accumTime += dt;
		if (m_accumTime >= m_fpsUpdateInterval) {
			m_fps = frames/m_accumTime;
			m_accumTime = 0.0f;
			frames = 0;
			float ms = 1000.0f / Mathf.Max(m_fps, 0.0001f);
			strFPS = string.Format("{0} ms {1}FPS", ms.ToString("f1"), m_fps.ToString("f2"));
		}
		*/
		// system info
		if (Time.frameCount % 30 == 0) {
			strMem = string.Format("memory : {0} MB", System.GC.GetTotalMemory(false) / (1024*1024));
		}
	}


	void OnGUI()  {  	

		GUI.skin.label.normal.textColor = Color.green;
		GUI.skin.label.fontSize = 12;
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUILayout.Label (strFPS);
		GUILayout.Label (strMem);

		foreach (Log log in m_logs) {
			Color showClr = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			if (log.type == LogType.Error || log.type == LogType.Exception) {
				showClr.r = 1.0f;
			} else if (log.type == LogType.Assert) {
				showClr.r = 1.0f;
				showClr.g = 1.0f;
			} else {
				showClr.g = 1.0f;
			}

			GUI.skin.label.normal.textColor = showClr;
			GUI.skin.label.fontSize = 12;
			GUI.skin.label.alignment = TextAnchor.UpperLeft;

			GUILayout.Label (log.msg);
			if (log.type == LogType.Assert || log.type == LogType.Error || log.type == LogType.Exception) {
				GUILayout.Label (log.stacktrace);
			}

		}
	} 
}
