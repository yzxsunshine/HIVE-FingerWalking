using UnityEngine;
using System.Collections;

public class WalkingPath {
	public Vector3[] wayPoints;
	static public float segmentDistance = 50.0f;
	static public float[] angleList = new float[3]{72, 108, 144};
	public WalkingPath(int pathLength, int difficulty, int LorR) {
		wayPoints = new Vector3[pathLength];
		wayPoints [0] = Vector3.zero;
		Vector3 forward = Vector3.forward;
		wayPoints [1] = wayPoints [0] + forward * segmentDistance;
		for (int i=2; i<pathLength; i++) {
			Vector3 nextDirection = new Vector3();
			switch (difficulty) {
			case 0:
				nextDirection = Quaternion.AngleAxis (Mathf.Pow (-1, LorR) * angleList [0], Vector3.up) * forward;
				break;
			case 1:
				nextDirection = Quaternion.AngleAxis (Mathf.Pow (-1, LorR + Mathf.FloorToInt(i * 2 / pathLength)) * angleList [difficulty - 1], Vector3.up) * forward;
				break;
			case 2:
				nextDirection = Quaternion.AngleAxis (Mathf.Pow (-1, LorR + Mathf.FloorToInt(i * 2 / pathLength)) * angleList [difficulty - 1], Vector3.up) * forward;
				break;
			case 3:
				nextDirection = Quaternion.AngleAxis (Mathf.Pow (-1, i + LorR) * angleList [difficulty - 1], Vector3.up) * forward;
				break;
			}
			forward = nextDirection;
			wayPoints [i] = wayPoints [i - 1] + nextDirection * segmentDistance;
		}
	}
}


public class WalkingTrialControl : MonoBehaviour {
	private float[] angleList;
	private GameObject character;
	private WalkingPath[, ] pathes;
	private int currentWayPoint;
	private ArrowControl arrowControl;
	private Transform[] currentWayPts;
	private PlayerStatus playerStatus;
	private StudyRecorder recorder;
	private float timeStamp;
	private float[] timeStampWayPoints = null;
	// Use this for initialization
	void Awake () {
		character = GameObject.Find ("Character");

		pathes = new WalkingPath[4, 2];

		for (int i=0; i<4; i++) {
			for (int j=0; j<2; j++) {
				pathes [i, j] = new WalkingPath (5, i, j);
			}
		}
		arrowControl = GameObject.Find ("arrow").GetComponent<ArrowControl> ();
		currentWayPoint = 0;
		playerStatus = character.GetComponent<PlayerStatus> ();
		recorder = GameObject.Find ("StudyRecorder").GetComponent<StudyRecorder> ();
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (currentWayPts != null) {
			string line = "" + timeStamp + "\t" + playerStatus.GetCurrentTransformLine();
			recorder.RecordLine(line);
			timeStamp += Time.deltaTime;
		}
	}

	public void SetWalkingPath(int difficulty, int LorR, Transform characterTransform) {
		Vector3[] wayPointsPositions = pathes [difficulty, LorR].wayPoints;
		currentWayPts = new Transform[wayPointsPositions.Length];
		currentWayPts [0] = characterTransform;
		for (int i=1; i<wayPointsPositions.Length; i++) {
			Vector3 wayPoint = characterTransform.localToWorldMatrix.MultiplyPoint (wayPointsPositions [i]);
			GameObject obj = Instantiate(Resources.Load("Prefabs/JackoLantern", typeof(GameObject))) as GameObject;
			obj.transform.position = wayPoint;
			currentWayPts[i] = obj.transform;
		}
		currentWayPoint = 1;
		arrowControl.SetTarget (currentWayPts[currentWayPoint]);
		timeStampWayPoints = new float[currentWayPts.Length];
		timeStampWayPoints [0] = 0.0f;
		recorder.GenerateFileWriter ((int) playerStatus.GetControlType(), difficulty, (int) TRAVEL_TYPE.WALKING);
		timeStamp = 0;
		string instruction = "#Walking Trial Path#";
		recorder.RecordLine(instruction);
		instruction = playerStatus.GetStatusTableHead ();
		recorder.RecordLine(instruction);
	}

	public bool ActiveNextWayPoint () {
		timeStampWayPoints [currentWayPoint + 1] = timeStamp;
		if (currentWayPoint < currentWayPts.Length - 1) {
			currentWayPoint++;
			arrowControl.SetTarget (currentWayPts [currentWayPoint]);
			return false;
		} else {
			arrowControl.ResetTarget();
			string instruction = "#Segway Trial Way Points#";
			recorder.RecordLine(instruction);
			instruction = "#TimeStamp#\t" + "#WayPonitX#\t" + "#WayPonitY#\t" + "#WayPonitZ#\t";
			instruction += "#OrientationX#\t" + "#OrientationY#\t" + "#OrientationZ#\t" + "#OrientationW#\t";
			recorder.RecordLine(instruction);
			
			for (int i=0; i<currentWayPts.Length; i++) {	// dump all waypoints and timestamp to file
				Vector3 wayPtPos = currentWayPts[i].position;
				string line = "" + timeStampWayPoints[i] 
							+ "\t" + wayPtPos.x
							+ "\t" + wayPtPos.y
							+ "\t" + wayPtPos.z
							+ "\t" + currentWayPts[i].rotation.x
							+ "\t" + currentWayPts[i].rotation.y
							+ "\t" + currentWayPts[i].rotation.z
							+ "\t" + currentWayPts[i].rotation.w;
				recorder.RecordLine(line);
			}
			currentWayPts = null;
			timeStampWayPoints = null;
			recorder.StopTrialFileWriter();
			return true;	// trial ended
		}
	}
}
