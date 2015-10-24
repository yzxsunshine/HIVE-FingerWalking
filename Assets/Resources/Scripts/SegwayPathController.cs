using UnityEngine;
using System.Collections;

public class SegwayPath {
	public int[] wayPoints;

	public SegwayPath(int[] wayPts) {
		wayPoints = wayPts;
	}
}

public class SegwayPathController : MonoBehaviour {
	private SegwayPath[, , ] pathes;
	private int[] currentWayPts;
	private int currentPosition;
	private GameObject[] wayPointTriggers;
	private ArrowControl arrowControl;
	private GameObject character;
	private PlayerStatus playerStatus;
	private StudyRecorder recorder;
	private float timeStamp;
	private float[] timeStampWayPoints = null;
	private TrialControl trialControl;
	// Use this for initialization
	void Awake () {
		wayPointTriggers = new GameObject[25];
		for (int i = 0; i < 25; i++) {
			string objName = "SegwayWaypoint_" + i;
			wayPointTriggers[i] = GameObject.Find(objName);
			wayPointTriggers[i].GetComponent<BoxCollider>().enabled = false;
		}
		pathes = new SegwayPath[4, 4, 2];
		// trial
		pathes [0, 0, 0] = new SegwayPath(new int[6] {0, 4, 12, 7 ,6, 1});
		pathes [0, 0, 1] = new SegwayPath(new int[6] {0, 5, 15, 10, 11, 3});

		pathes [0, 1, 0] = new SegwayPath(new int[6] {1, 6, 13, 9, 8, 2});
		pathes [0, 1, 1] = new SegwayPath(new int[6] {1, 7, 12, 5, 4, 0});

		pathes [0, 2, 0] = new SegwayPath(new int[6] {2, 8, 14, 11, 10, 3});
		pathes [0, 2, 1] = new SegwayPath(new int[6] {2, 9, 13, 6, 7, 1});

		pathes [0, 3, 0] = new SegwayPath(new int[6] {3, 10, 15, 5, 4, 0});
		pathes [0, 3, 1] = new SegwayPath(new int[6] {3, 11, 14, 8, 9, 2});

		// easy
		pathes [1, 0, 0] = new SegwayPath(new int[7] {0, 5, 4, 12, 7 ,6, 1});
		pathes [1, 0, 1] = new SegwayPath(new int[7] {0, 4, 5, 15, 10, 11, 3});
		
		pathes [1, 1, 0] = new SegwayPath(new int[7] {1, 7, 6, 13, 9, 8, 2});
		pathes [1, 1, 1] = new SegwayPath(new int[7] {1, 6, 7, 12, 5, 4, 0});
		
		pathes [1, 2, 0] = new SegwayPath(new int[7] {2, 9, 8, 14, 11, 10, 3});
		pathes [1, 2, 1] = new SegwayPath(new int[7] {2, 8, 9, 13, 6, 7, 1});
		
		pathes [1, 3, 0] = new SegwayPath(new int[7] {3, 11, 10, 15, 5, 4, 0});
		pathes [1, 3, 1] = new SegwayPath(new int[7] {3, 10, 11, 14, 8, 9, 2});

		// medium
		pathes [2, 0, 0] = new SegwayPath(new int[17] {0, 4, 5, 15, 19, 23, 16, 20, 24, 22, 18, 21, 17, 13, 6, 7, 1});
		pathes [2, 0, 1] = new SegwayPath(new int[17] {0, 5 ,4, 12, 16, 23, 19, 22, 24, 20, 17, 21, 18, 14, 11, 10, 3});
		
		pathes [2, 1, 0] = new SegwayPath(new int[17] {1, 6, 7, 12, 16, 20, 17, 21, 24, 23, 19, 22, 18, 14, 8, 9, 2});
		pathes [2, 1, 1] = new SegwayPath(new int[17] {1, 7, 6, 13, 17, 20, 16, 23, 24, 21, 18, 22, 19, 15, 5, 4, 0});
		
		pathes [2, 2, 0] = new SegwayPath(new int[17] {2, 8, 9, 13, 17, 21, 18, 22, 24, 20, 16, 23, 19, 15, 10, 11, 3});
		pathes [2, 2, 1] = new SegwayPath(new int[17] {2, 9, 8, 14, 18, 21, 17, 20, 24, 22, 19, 23, 16, 12, 7, 6, 1});
		
		pathes [2, 3, 0] = new SegwayPath(new int[17] {3, 10, 11, 14, 18, 22, 19, 23, 24, 21, 17, 20, 16, 12, 4, 5, 0});
		pathes [2, 3, 1] = new SegwayPath(new int[17] {3, 11, 10, 15, 19, 22, 18, 21, 24, 23, 16, 20, 17, 13, 9, 8, 2});

		// hard
		pathes [3, 0, 0] = new SegwayPath(new int[17] {0, 5, 4, 12, 16, 23, 24, 22, 18, 21, 24, 20, 17, 13, 6, 7, 1});
		pathes [3, 0, 1] = new SegwayPath(new int[17] {0, 4, 5, 15, 19, 22, 24, 23, 16, 20, 24, 21, 18, 14, 11, 10, 3});
		
		pathes [3, 1, 0] = new SegwayPath(new int[17] {1, 7, 6, 13, 17, 21, 24, 23, 19, 22, 24, 21, 18, 14, 8, 9, 2});
		pathes [3, 1, 1] = new SegwayPath(new int[17] {1, 6, 7, 12, 16, 23, 24, 20, 17, 21, 24, 22, 19, 15, 5, 4, 0});
		
		pathes [3, 2, 0] = new SegwayPath(new int[17] {2, 9, 8, 14, 18, 22, 24, 20, 16, 23, 24, 22, 19, 15, 10, 11, 3});
		pathes [3, 2, 1] = new SegwayPath(new int[17] {2, 8, 9, 13, 17, 20, 24, 21, 18, 22, 24, 23, 16, 12, 7, 6, 1});
		
		pathes [3, 3, 0] = new SegwayPath(new int[17] {3, 11, 10, 15, 19, 23, 24, 21, 17, 20, 24, 23, 16, 12, 4, 5, 0});
		pathes [3, 3, 1] = new SegwayPath(new int[17] {3, 10, 11, 14, 18, 21, 24, 22, 19, 23, 24, 20, 17, 13, 9, 8, 2});

		currentPosition = 0;
		arrowControl = GameObject.Find ("arrow").GetComponent<ArrowControl> ();
		character = GameObject.Find("Character");
		playerStatus = character.GetComponent<PlayerStatus> ();
		recorder = GameObject.Find ("StudyRecorder").GetComponent<StudyRecorder> ();
	}

	void Start() {
		trialControl = character.GetComponent<TrialControl>();
	}

	public GameObject GetWayPointTrigger(int id) {
		return wayPointTriggers[id];
	}

	private void CloseWayPoints() {
		for (int i = 0; i < 25; i++) {
			string objName = "SegwayWaypoint_" + i;
			wayPointTriggers[i] = GameObject.Find(objName);
			for(int j = 0; j < wayPointTriggers[i].transform.childCount; j++) {
				string blockerName = wayPointTriggers[i].transform.GetChild(j).gameObject.name;
				if(blockerName[0] == 'B') {
					BoxCollider[] colliders = wayPointTriggers[i].transform.GetChild(j).gameObject.GetComponentsInChildren<BoxCollider>();
					for(int k=0; k<colliders.Length; k++) {
						colliders[k].enabled = true;
					}
					GameObject tape_1 = GameObject.Find(blockerName + "/barrierTape");
					MeshRenderer[] render_1 = tape_1.GetComponentsInChildren<MeshRenderer>();
					for(int k=0; k<render_1.Length; k++) {
						render_1[k].enabled = true;
					}
				}
			}
			//wayPointTriggers[i].GetComponent<BoxCollider>().enabled = false;
		}
	}

	private void OpenWayPointsAt(int curPos, int forwardNum, int[] wayPts) {
		CloseWayPoints();
		for (int i=curPos; i<wayPts.Length && i < curPos + forwardNum; i++) {
			if (i < wayPts.Length - 1) {
				string blockerName = "BarrierTape-"+wayPts[i]+"-"+wayPts[i+1];
				GameObject blocker = GameObject.Find(blockerName);
				BoxCollider[] colliders = blocker.GetComponentsInChildren<BoxCollider>();
				for(int j=0; j<colliders.Length; j++) {
					colliders[j].enabled = false;
				}
				GameObject tape_1 = GameObject.Find(blockerName + "/barrierTape");
				MeshRenderer[] render_1 = tape_1.GetComponentsInChildren<MeshRenderer>();
				for(int j=0; j<render_1.Length; j++) {
					render_1[j].enabled = false;
				}
			}
			if(i > 0) {
				string blockerName_2 = "BarrierTape-"+wayPts[i]+"-"+wayPts[i-1];
				GameObject blocker_2 = GameObject.Find(blockerName_2);
				BoxCollider[] colliders_2 = blocker_2.GetComponentsInChildren<BoxCollider>();
				for(int j=0; j<colliders_2.Length; j++) {
					colliders_2[j].enabled = false;
				}
				
				GameObject tape_2 = GameObject.Find(blockerName_2 + "/barrierTape");
				MeshRenderer[] render_2 = tape_2.GetComponentsInChildren<MeshRenderer>();
				for(int j=0; j<render_2.Length; j++) {
					render_2[j].enabled = false;
				}
			}
		}
	}
	
	public StoreTransform SetSegwayPath (int difficulty, int startPointID, int LorR) {
		if (difficulty > 3) {
			difficulty = 3;
		}
		startPointID = startPointID % 4;
		if (LorR <= 0)
			LorR = 0;
		else
			LorR = 1;

		int[] wayPoints = pathes[difficulty, startPointID, LorR].wayPoints;
		wayPointTriggers[wayPoints[0]].GetComponent<BoxCollider>().enabled = true;
		OpenWayPointsAt(0, 2, wayPoints);
		arrowControl.SetTarget (wayPointTriggers [wayPoints [1]].transform);
		currentWayPts = wayPoints;
		timeStampWayPoints = new float[wayPoints.Length];
		recorder.GenerateFileWriter ((int) playerStatus.GetControlType(), difficulty, (int) TRAVEL_TYPE.SEGWAY);
		timeStamp = 0;
		string instruction = "#Segway Trial Path#";
		recorder.RecordLine(instruction);
		instruction = playerStatus.GetStatusTableHead ();
		recorder.RecordLine(instruction);
		StoreTransform startTransform = new StoreTransform();
		startTransform.position = wayPointTriggers [currentWayPts [0]].transform.position;
		startTransform.forward = wayPointTriggers [currentWayPts [1]].transform.position - startTransform.position;
		startTransform.forward.Normalize();
		currentPosition = 0;
		GoalScript.CreateGoal(wayPointTriggers[wayPoints[wayPoints.Length - 1]].transform.position);
		return startTransform;
	}

	public bool ActiveNextWayPoint () {
		wayPointTriggers [currentWayPts [currentPosition]].GetComponent<BoxCollider> ().enabled = false;
		timeStampWayPoints [currentPosition] = timeStamp;
		if (currentPosition < currentWayPts.Length - 1) {
			OpenWayPointsAt(currentPosition, 2, currentWayPts);
			currentPosition++;
			wayPointTriggers [currentWayPts [currentPosition]].GetComponent<BoxCollider> ().enabled = true;
			arrowControl.SetTarget (wayPointTriggers [currentWayPts [currentPosition]].transform);

			return false;
		} else {
			arrowControl.ResetTarget();
			string instruction = "#Segway Trial Way Points#";
			recorder.RecordLine(instruction);
			instruction = "#TimeStamp#\t" + "#WayPointID#\t" + "#WayPonitX#\t" + "#WayPonitY#\t" + "#WayPonitZ#\t";
			recorder.RecordLine(instruction);

			for (int i=0; i<currentWayPts.Length; i++) {	// dump all waypoints and timestamp to file
				Vector3 wayPtPos = wayPointTriggers[currentWayPts[i]].transform.position;
				string line = "" + timeStampWayPoints[i] 
							+ "\t" + currentWayPts[i]
							+ "\t" + wayPtPos.x
							+ "\t" + wayPtPos.y
							+ "\t" + wayPtPos.z;
				recorder.RecordLine(line);
			}
			currentWayPts = null;
			timeStampWayPoints = null;
			recorder.StopTrialFileWriter();
			trialControl.FinishTrial();
			return true;	// trial ended
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (currentWayPts != null) {
			string line = "" + timeStamp + "\t" + playerStatus.GetCurrentTransformLine();
			recorder.RecordLine(line);
			timeStamp += Time.deltaTime;
		}
	}
}
