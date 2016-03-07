using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
	using UnityEditor;
#endif
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class RenderPath : MonoBehaviour {
	private LineRenderer lineRenderer;
	private int stepLength;
	private int subjectID;
	private int controlType;
	private int travelType;   // 0 walking, 1 segway, 2 surfing
	private int level;
	private int pass;

	// Use this for initialization
	void Start () {
		lineRenderer = GameObject.Find("PathRenderer").GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ParseFileName(string filePath)
	{
		int index = filePath.LastIndexOf("\\");
		string fileName = filePath.Substring(index + 1);
		fileName = fileName.Substring(0, fileName.Length - 4);
		char[] seperators = new char[1] {'_'};
		string[] tokens = fileName.Split(seperators);
		subjectID = int.Parse(tokens[1]);
		controlType = int.Parse(tokens[3]);
		travelType = int.Parse(tokens[5]);
		level = int.Parse(tokens[4]);
		pass = int.Parse(tokens[6]);
	}

	void OnGUI () {
		if (GUI.Button(new Rect(10, 10, 200, 40), "Select Trial File")) {
			string filePath = "";
#if UNITY_EDITOR
			filePath = EditorUtility.OpenFilePanel("Trial File",Application.streamingAssetsPath,"txt");
#endif
			if(filePath.Length != 0) {
				GameObject[] objs = GameObject.FindGameObjectsWithTag("PathPoint");
				for(int i=0; i<objs.Length; i++) {
					if(objs[i] != null)
						Destroy(objs[i]);
				}

				objs = GameObject.FindGameObjectsWithTag("EventObject");
				for(int i=0; i<objs.Length; i++) {
					if(objs[i] != null)
						Destroy(objs[i]);
				}

				StreamReader sr = new StreamReader(filePath);
				string[] lines = sr.ReadToEnd().Split(new char[] {'\n'});
				int count = lines.Length;
				string line;
				stepLength = 1;
				int startIndex = 0;
				bool firstCorrectSwitch = true;
				bool waitSwitch = false;
				for (int i = 0; i < count; i += stepLength) {
					line = lines[i];
					if (line[0] == '#')
						continue;
					string[] parse = line.Split(new char[]{'\t'});
					if (parse.Length < 18) {
						Debug.Log("Wrong Stream!");
						continue;
					}
					float timeStamp = float.Parse(parse[0]);
					string collision = parse[1];
					string modeSwitch = parse[2];
					string reset = parse[3];
					string wayptTriger = parse[4];

					Vector3 position = new Vector3();
					position.x = float.Parse(parse[5]);
					position.y = float.Parse(parse[6]);
					position.z = float.Parse(parse[7]);

					Quaternion quat = new Quaternion();
					quat.x = float.Parse(parse[8]);
					quat.y = float.Parse(parse[9]);
					quat.z = float.Parse(parse[10]);
					quat.w = float.Parse(parse[11]);

					Vector3 headRot = new Vector3();
					headRot.x = float.Parse(parse[12]);
					headRot.y = float.Parse(parse[13]);
					headRot.z = float.Parse(parse[14]);

					Vector3 wayPtPos = new Vector3();
					wayPtPos.x = float.Parse(parse[15]);
					wayPtPos.y = float.Parse(parse[16]);
					wayPtPos.z = float.Parse(parse[17]);

					if (modeSwitch == "WAIT_SWITCH")
					{
						firstCorrectSwitch = false;
						waitSwitch = true;
					}
					
					else if (modeSwitch == "CORRECT_SWITCH")
					{
						if (waitSwitch)
						{
							waitSwitch = false;
						}
						else if (firstCorrectSwitch && Mathf.Abs(position.x - wayPtPos.x) < 0.5f && Mathf.Abs(position.z - wayPtPos.z) < 0.5f)
						{
							firstCorrectSwitch = false;
							waitSwitch = false;
						}
					}
					
					else if (modeSwitch == "INCORRECT_SWITCH_TO_WALKING")
					{
						if (travelType == 0)
						{
							waitSwitch = true;
						}
					}
					else if (modeSwitch == "INCORRECT_SWITCH_TO_SEGWAY")
					{
						if (travelType == 1)
						{
							waitSwitch = true;
						}
					}
					else if (modeSwitch == "INCORRECT_SWITCH_TO_SURFING")
					{
						if (travelType == 2)
						{
							waitSwitch = true;
						}
					}

					//if (!waitSwitch)
					//	continue;
					else if (stepLength == 1) {
						stepLength = 10;
						startIndex = i;
					}

					GameObject obj = Instantiate(Resources.Load("Prefabs/triangle", typeof(GameObject))) as GameObject;
					obj.transform.rotation = quat;
					obj.transform.position = position;
					obj.GetComponentInChildren<Renderer>().material.color = new Color((i - startIndex) * 1.0f / (count - startIndex), 0, 0, 1);

					if (collision == "COLLISION_ENTER") {
						//GameObject eventObj = Instantiate(Resources.Load("Prefabs/EventObject", typeof(GameObject))) as GameObject;
						//eventObj.transform.rotation = quat;
						//Vector3 position_above = position;
						//position_above.y = 10.0f;
						//eventObj.transform.position = position_above;
						//Color color = Color.cyan;//new Color(1.0, 0.0, 0.0, 1.0);
						//eventObj.GetComponent<Renderer>().material.color = color;
					}

					/*
					if (modeSwitch == "WAIT_SWITCH") {
						GameObject eventObj = Instantiate(Resources.Load("Prefabs/EventObject", typeof(GameObject))) as GameObject;
						eventObj.transform.rotation = quat;
						Vector3 position_above = position;
						position_above.y = 25.0f;
						eventObj.transform.position = position_above;
						Color color = Color.blue;//new Color(1.0, 0.0, 0.0, 1.0);
						eventObj.GetComponent<Renderer>().material.color = color;
					}
					
					else if (modeSwitch == "CORRECT_SWITCH") {
						GameObject eventObj = Instantiate(Resources.Load("Prefabs/EventObject", typeof(GameObject))) as GameObject;
						eventObj.transform.rotation = quat;
						Vector3 position_above = position;
						position_above.y = 15.0f;
						eventObj.transform.position = position_above;
						Color color = Color.green;//new Color(1.0, 0.0, 0.0, 1.0);
						eventObj.GetComponent<Renderer>().material.color = color;
					}

					else if (modeSwitch == "INCORRECT_SWITCH") {
						GameObject eventObj = Instantiate(Resources.Load("Prefabs/EventObject", typeof(GameObject))) as GameObject;
						eventObj.transform.rotation = quat;
						Vector3 position_above = position;
						position_above.y = 15.0f;
						eventObj.transform.position = position_above;
						Color color = Color.magenta;//new Color(1.0, 0.0, 0.0, 1.0);
						eventObj.GetComponent<Renderer>().material.color = color;
					}

					if (reset == "RESET") {
						GameObject eventObj = Instantiate(Resources.Load("Prefabs/EventObject", typeof(GameObject))) as GameObject;
						eventObj.transform.rotation = quat;
						Vector3 position_above = position;
						position_above.y = 20.0f;
						eventObj.transform.position = position_above;
						Color color = Color.red;//new Color(1.0, 0.0, 0.0, 1.0);
						eventObj.GetComponent<Renderer>().material.color = color;
					}
*/
					if (wayptTriger == "TRIGGER_WAYPOINT") {
						//GameObject eventObj = Instantiate(Resources.Load("Prefabs/EventObject", typeof(GameObject))) as GameObject;
						//eventObj.transform.rotation = quat;
						//Vector3 position_above = position;
						//position_above.y = 20.0f;
						//eventObj.transform.position = position_above;
						//Color color = Color.yellow;//new Color(1.0, 0.0, 0.0, 1.0);
						//eventObj.GetComponent<Renderer>().material.color = color;
					}

				}
			}
		}
	}
}
