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
	// Use this for initialization
	void Start () {
		lineRenderer = GameObject.Find("PathRenderer").GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
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
				string line;
				while((line = sr.ReadLine()) != null) {
					if (line[0] == '#')
						continue;
					string[] parse = line.Split(new char[]{'\t'});
					if (parse.Length < 18) {
						Debug.Log("Wrong Stream!");
						return;
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

					GameObject obj = Instantiate(Resources.Load("Prefabs/PathPoint", typeof(GameObject))) as GameObject;
					obj.transform.rotation = quat;
					obj.transform.position = position;

					if (collision == "COLLISION_ENTER") {
						GameObject eventObj = Instantiate(Resources.Load("Prefabs/EventObject", typeof(GameObject))) as GameObject;
						eventObj.transform.rotation = quat;
						Vector3 position_above = position;
						position_above.y = 10.0f;
						eventObj.transform.position = position_above;
						Color color = Color.cyan;//new Color(1.0, 0.0, 0.0, 1.0);
						eventObj.GetComponent<Renderer>().material.color = color;
					}

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

					if (wayptTriger == "TRIGGER_WAYPOINT") {
						GameObject eventObj = Instantiate(Resources.Load("Prefabs/EventObject", typeof(GameObject))) as GameObject;
						eventObj.transform.rotation = quat;
						Vector3 position_above = position;
						position_above.y = 20.0f;
						eventObj.transform.position = position_above;
						Color color = Color.yellow;//new Color(1.0, 0.0, 0.0, 1.0);
						eventObj.GetComponent<Renderer>().material.color = color;
					}
				}
			}
		}
	}
}
