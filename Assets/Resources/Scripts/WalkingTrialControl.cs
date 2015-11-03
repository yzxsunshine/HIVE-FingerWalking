using UnityEngine;
using System.Collections;

public class WalkingPathGenerator {
	public int [,] paths;
	public int pathLength;
	public WalkingPathGenerator(int length) {
		pathLength = length;
		paths = new int[2, length];
	}
}

public class WalkingPath {
	public Vector3[] wayPoints;
	static public float segmentDistance = 36.0f / 20.0f;
	static public float[] angleList = new float[3]{72, 108, 144};
	public static WalkingPathGenerator[] pathGenerators = new WalkingPathGenerator[4];

	public WalkingPath(int difficulty, int LorR) {
		int pathLength = pathGenerators[difficulty].pathLength + 1;
		wayPoints = new Vector3[pathLength];
		wayPoints [0] = Vector3.zero;
		Vector3 forward = Vector3.forward;
		wayPoints [1] = wayPoints [0] + forward * segmentDistance;
		for (int i=2; i<pathLength; i++) {
			Vector3 nextDirection = new Vector3();
			nextDirection = Quaternion.AngleAxis (Mathf.Pow (-1, pathGenerators[difficulty].paths[1, i - 1] + LorR) * angleList [pathGenerators[difficulty].paths[0, i - 1]], Vector3.up) * forward;
			forward = nextDirection;
			wayPoints [i] = wayPoints [i - 1] + nextDirection * segmentDistance;
		}
	}

	public static void SetWalkingPathGenerators() {
		pathGenerators[0] = new WalkingPathGenerator(5);
		pathGenerators[0].paths[0, 0] = 0;	pathGenerators[0].paths[1, 0] = 0;
		pathGenerators[0].paths[0, 1] = 0;	pathGenerators[0].paths[1, 1] = 0;
		pathGenerators[0].paths[0, 2] = 0;	pathGenerators[0].paths[1, 2] = 0;
		pathGenerators[0].paths[0, 3] = 0;	pathGenerators[0].paths[1, 3] = 0;
		pathGenerators[0].paths[0, 4] = 0;	pathGenerators[0].paths[1, 4] = 0;
		
		pathGenerators[1] = new WalkingPathGenerator(7);
		pathGenerators[1].paths[0, 0] = 0;	pathGenerators[1].paths[1, 0] = 0;
		pathGenerators[1].paths[0, 1] = 0;	pathGenerators[1].paths[1, 1] = 0;
		pathGenerators[1].paths[0, 2] = 0;	pathGenerators[1].paths[1, 2] = 0;
		pathGenerators[1].paths[0, 3] = 1;	pathGenerators[1].paths[1, 3] = 0;
		pathGenerators[1].paths[0, 4] = 1;	pathGenerators[1].paths[1, 4] = 0;
		pathGenerators[1].paths[0, 5] = 1;	pathGenerators[1].paths[1, 5] = 0;
		pathGenerators[1].paths[0, 6] = 0;	pathGenerators[1].paths[1, 6] = 0;
		
		pathGenerators[2] = new WalkingPathGenerator(7);
		pathGenerators[2].paths[0, 0] = 0;	pathGenerators[2].paths[1, 0] = 0;
		pathGenerators[2].paths[0, 1] = 0;	pathGenerators[2].paths[1, 1] = 0;
		pathGenerators[2].paths[0, 2] = 1;	pathGenerators[2].paths[1, 2] = 0;
		pathGenerators[2].paths[0, 3] = 1;	pathGenerators[2].paths[1, 3] = 0;
		pathGenerators[2].paths[0, 4] = 1;	pathGenerators[2].paths[1, 4] = 0;
		pathGenerators[2].paths[0, 5] = 2;	pathGenerators[2].paths[1, 5] = 0;
		pathGenerators[2].paths[0, 6] = 2;	pathGenerators[2].paths[1, 6] = 0;
		
		pathGenerators[3] = new WalkingPathGenerator(9);
		pathGenerators[3].paths[0, 0] = 0;	pathGenerators[3].paths[1, 0] = 0;
		pathGenerators[3].paths[0, 1] = 1;	pathGenerators[3].paths[1, 1] = 0;
		pathGenerators[3].paths[0, 2] = 2;	pathGenerators[3].paths[1, 2] = 0;
		pathGenerators[3].paths[0, 3] = 2;	pathGenerators[3].paths[1, 3] = 0;
		pathGenerators[3].paths[0, 4] = 2;	pathGenerators[3].paths[1, 4] = 0;
		pathGenerators[3].paths[0, 5] = 2;	pathGenerators[3].paths[1, 5] = 0;
		pathGenerators[3].paths[0, 6] = 2;	pathGenerators[3].paths[1, 6] = 0;
		pathGenerators[3].paths[0, 7] = 2;	pathGenerators[3].paths[1, 7] = 0;
		pathGenerators[3].paths[0, 8] = 2;	pathGenerators[3].paths[1, 8] = 0;
	}
}


public class WalkingTrialControl : MonoBehaviour {
	private float[] angleList;
	private GameObject character;
	private WalkingPath[, ] pathes;
	private int currentWayPoint;
	private ArrowControl arrowControl;
	private Transform[] currentWayPts;
	private Vector3[] currentWayPtPositions;
	private PlayerStatus playerStatus;
	private StudyRecorder recorder;
	private float timeStamp;
	private float[] timeStampWayPoints = null;
	private TrialControl trialControl;
	// Use this for initialization
	void Awake () {
		WalkingPath.SetWalkingPathGenerators();
		pathes = new WalkingPath[4, 2];

		for (int i=0; i<4; i++) {
			for (int j=0; j<2; j++) {
				pathes [i, j] = new WalkingPath (i, j);
			}
		}
		character = GameObject.Find ("Character");

		arrowControl = GameObject.Find ("arrow").GetComponent<ArrowControl> ();
		currentWayPoint = 0;
		playerStatus = character.GetComponent<PlayerStatus> ();
		recorder = GameObject.Find ("StudyRecorder").GetComponent<StudyRecorder> ();
	}

	// Use this for initialization
	void Start () {
		trialControl = character.GetComponent<TrialControl>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (currentWayPts != null) {
			string line = playerStatus.GetCurrentTransformLine();
			recorder.RecordLine(line);
			timeStamp += Time.deltaTime;
		}
	}

	public Transform CreateWayPointGameObject(Vector3 wayPoint) { 
		GameObject obj = Instantiate(Resources.Load("Prefabs/JackoLantern", typeof(GameObject))) as GameObject;
		obj.transform.position = wayPoint;
		return obj.transform;
	}

	public StoreTransform SetWalkingPath(int difficulty, int LorR, Transform characterTransform, int numPasses) {
		if(pathes [difficulty, LorR] == null)
			pathes [difficulty, LorR] = new WalkingPath(difficulty, LorR);
		Vector3[] wayPointsPositions = pathes [difficulty, LorR].wayPoints;
		currentWayPts = new Transform[wayPointsPositions.Length];
		currentWayPtPositions = new Vector3[wayPointsPositions.Length];
		currentWayPts [0] = characterTransform;
		currentWayPtPositions [0] = characterTransform.position;
		for (int i=1; i<wayPointsPositions.Length; i++) {
			Vector3 wayPoint = characterTransform.localToWorldMatrix.MultiplyPoint (wayPointsPositions [i]);
			wayPoint.y = playerStatus.PLAYER_HEIGHT;

			currentWayPtPositions[i] = wayPoint;
		}
		currentWayPoint = 1;
		currentWayPts[currentWayPoint] = CreateWayPointGameObject(currentWayPtPositions[currentWayPoint]);
		arrowControl.SetTarget (currentWayPts[currentWayPoint]);
		timeStampWayPoints = new float[currentWayPts.Length];
		recorder.GenerateFileWriter ((int) playerStatus.GetControlType(), difficulty, (int) TRAVEL_TYPE.WALKING, numPasses);
		timeStamp = 0;
		string instruction = "#Walking Trial Path#";
		recorder.RecordLine(instruction);
		instruction = playerStatus.GetStatusTableHead ();
		recorder.RecordLine(instruction);
		StoreTransform startTransform = new StoreTransform();
		startTransform.position = characterTransform.position;
		startTransform.forward = characterTransform.forward;
		startTransform.forward.Normalize();
		return startTransform;
	}

	public bool ActiveNextWayPoint () {
		playerStatus.TrigerWayPoint();
		playerStatus.SetWayPoint(currentWayPtPositions[currentWayPoint]);
		timeStampWayPoints [currentWayPoint] = timeStamp;
		if (currentWayPoint < currentWayPts.Length - 1) {
			currentWayPoint++;
			currentWayPts[currentWayPoint] = CreateWayPointGameObject(currentWayPtPositions[currentWayPoint]);
			arrowControl.SetTarget (currentWayPts [currentWayPoint]);
			return false;
		} else {
			arrowControl.ResetTarget();
			/*string instruction = "#Walking Trial Way Points#";
			recorder.RecordLine(instruction);
			instruction = "#TimeStamp#\t" + "#WayPonitX#\t" + "#WayPonitY#\t" + "#WayPonitZ#\t";
			instruction += "#OrientationX#\t" + "#OrientationY#\t" + "#OrientationZ#\t" + "#OrientationW#\t";
			recorder.RecordLine(instruction);
			
			for (int i=0; i<currentWayPts.Length; i++) {	// dump all waypoints and timestamp to file
				Vector3 wayPtPos = currentWayPtPositions[i];
				string line = "" + timeStampWayPoints[i] 
							+ "\t" + wayPtPos.x
							+ "\t" + wayPtPos.y
							+ "\t" + wayPtPos.z
							+ "\t" + 0//currentWayPts[i].rotation.x
							+ "\t" + 0//currentWayPts[i].rotation.y
							+ "\t" + 0//currentWayPts[i].rotation.z
							+ "\t" + 0;//currentWayPts[i].rotation.w;
				recorder.RecordLine(line);
			}*/
			currentWayPts = null;
			timeStampWayPoints = null;
			recorder.StopTrialFileWriter();
			trialControl.FinishTrial();
			return true;	// trial ended
		}
	}
}
