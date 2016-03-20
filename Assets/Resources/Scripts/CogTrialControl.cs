using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CogTrialSequence {
	public int[] sequence; 
	public int mazeID;
	public int nextMazeID;
	public int sequenceID;
	private const int MAZE_OFFSET = 11;
	private const int LANE_OFFSET = 4;
	private const int LANE_START = 33;

	public CogTrialSequence(int mID, int sID) {
		mazeID = mID;
		sequenceID = sID;
		switch (sequenceID) {
		case 0: 
			sequence = new int[12] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 0};
			for (int i = 0; i < 12; i++) {
				sequence[i] = sequence[i] + mazeID * MAZE_OFFSET;
			}
			nextMazeID = mazeID;
			break;
		case 1:
			sequence = new int[10] {1, 10, 0, 1, 2, 3, 4, 10, 1, 0};
			sequence[0] = sequence[0] + mazeID * MAZE_OFFSET;
			sequence[1] = sequence[1] + mazeID * MAZE_OFFSET;
			sequence[7] = sequence[7] + ((mazeID + 1) % 3) * MAZE_OFFSET;
			sequence[8] = sequence[8] + ((mazeID + 1) % 3) * MAZE_OFFSET;
			sequence[9] = sequence[9] + ((mazeID + 1) % 3) * MAZE_OFFSET;
			for (int i = 0; i < 5; i++) {
				sequence[2 + i] = sequence[2 + i] + LANE_START + mazeID * LANE_OFFSET;
				if (sequence[2 + i] > 44) {
					sequence[2 + i] = sequence[2 + i] - 12;
				}
			}
			nextMazeID = (mazeID + 1) % 3;
			break;
		case 2:
			sequence = new int[18] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 1, 2, 3, 4, 10, 1, 0};
			for (int i = 0; i < 10; i++) {
				sequence[i] = sequence[i] + mazeID * MAZE_OFFSET;
			}
			sequence[15] = sequence[15] + ((mazeID + 1) % 3) * MAZE_OFFSET;
			sequence[16] = sequence[16] + ((mazeID + 1) % 3) * MAZE_OFFSET;
			sequence[17] = sequence[17] + ((mazeID + 1) % 3) * MAZE_OFFSET;

			for (int i = 0; i < 5; i++) {
				sequence[10 + i] = sequence[10 + i] + LANE_START + mazeID * LANE_OFFSET;
				if (sequence[10 + i] > 44) {
					sequence[10 + i] = sequence[10 + i] - 12;
				}
			}
			nextMazeID = (mazeID + 1) % 3;
			break;
		case 3:
			sequence = new int[12] {1, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0};
			for (int i = 0; i < 12; i++) {
				sequence[i] = sequence[i] + mazeID * MAZE_OFFSET;
			}
			nextMazeID = mazeID;
			break;
		case 4:
			sequence = new int[14] {1, 10, 0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 1, 0};
			sequence[0] = sequence[0] + mazeID * MAZE_OFFSET;
			sequence[1] = sequence[1] + mazeID * MAZE_OFFSET;
			sequence[11] = sequence[11] + ((mazeID + 2) % 3) * MAZE_OFFSET;
			sequence[12] = sequence[12] + ((mazeID + 2) % 3) * MAZE_OFFSET;
			sequence[13] = sequence[13] + ((mazeID + 2) % 3) * MAZE_OFFSET;
			for (int i = 0; i < 9; i++) {
				sequence[2 + i] = sequence[2 + i] + LANE_START + mazeID * LANE_OFFSET;
				if (sequence[2 + i] > 44) {
					sequence[2 + i] = sequence[2 + i] - 12;
				}
			}
			nextMazeID = (mazeID + 2) % 3;
			break;
		case 5:
			sequence = new int[18] {1, 10, 0, 1, 2, 3, 4, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0};

			sequence[0] = sequence[0] + mazeID * MAZE_OFFSET;
			sequence[1] = sequence[1] + mazeID * MAZE_OFFSET;

			for (int i = 0; i < 11; i++) {
				sequence[7 + i] = sequence[7 + i] + ((mazeID + 1) % 3) * MAZE_OFFSET;
			}

			for (int i = 0; i < 5; i++) {
				sequence[2 + i] = sequence[2 + i] + LANE_START + mazeID * LANE_OFFSET;
				if (sequence[2 + i] > 44) {
					sequence[2 + i] = sequence[2 + i] - 12;
				}
			}
			nextMazeID = (mazeID + 1) % 3;
			break;
		}
	}

}

public class CogTrialControl : MonoBehaviour {
	private GameObject character;
	public LEVEL_TYPE levelType;
	public TRAVEL_TYPE travelType;
	public CONTROL_TYPE controlType;
	public CutSceneManager cutSceneManager;
	public Text modeSwitchText;
	private PlayerStatus playerStatus;
	public TravelModelInterface travelModelInterface;
	private HIVEFPSController fpsController;
	private StudyRecorder studyRecorder;

	private int[] mazeIDs = new int[3]{0, 11, 22};
	private int[] laneIDs = new int[3]{33, 37, 41};
	private int currentWayPt = 0;
	private int currentTrialID = 0;
	private int currentStep = 0;
	private int currentMazeID = 0;
	private GameObject[] wayPoints = new GameObject[45];
	private GameObject[] mazes = new GameObject[3];
	private GameObject[] barrierTapes = new GameObject[3];
	private int[, ] wayPtIndexes = new int[6, 6] {	{0, 5, 11, 16, 22, 27},
													{5, 10, 16, 21, 27, 32},
													{1, 10, 11, 21, 22, 32},
													{10, 33, 21, 37, 32, 41},
													{33, 37, 37, 41, 41, 33},
													{33, 41, 37, 33, 41, 37}};
	private int[, ] lattinSquare = new int[,] {	{0,5,1,4,2,3},		// 0 short distance, 1 long distance, 2 combined
												{1,0,2,5,3,4},
												{2,1,3,0,4,5},
												{3,2,4,1,5,0},
												{4,3,5,2,0,1},
												{5,4,0,3,1,2}}; 
	private CogTrialSequence[] trialSequence = new CogTrialSequence[6];

	private StartWayPointCalculator startWayPointCalculator;
	public StoreTransform targetTransform = null;

	private float posCutSceneSpeed = 1.0f;
	private float rotCutSceneSpeed = 0.3f;
	ArrowControl arrowCtrl;

	void Awake() {
		character = GameObject.Find ("Character");
		cutSceneManager = GameObject.Find ("CutSceneManager").GetComponent<CutSceneManager> ();
		modeSwitchText = GameObject.Find ("ModeSwitchText").GetComponent<Text> ();
		playerStatus = character.GetComponent<PlayerStatus> ();
		studyRecorder = GameObject.Find ("StudyRecorder").GetComponent<StudyRecorder> ();
		travelModelInterface = GetComponent<TravelModelInterface> ();
		fpsController = GetComponent<HIVEFPSController> ();
	}
	// Use this for initialization
	void Start () {
		controlType = ConfigurationHandler.controllerType;
		if(controlType != CONTROL_TYPE.FORCEPAD_GESTURE && controlType != CONTROL_TYPE.FORCE_EXTENSION) {
			GameObject.Find("Widget").GetComponent<RawImage>().enabled = false;
		}
		playerStatus.DisableControl ();
		playerStatus.EnableControl(controlType);
		travelModelInterface.SetTargetGestureType (TRAVEL_TYPE.SINGLE_MODE);
		travelModelInterface.SetGestureType(TRAVEL_TYPE.SINGLE_MODE);

		for (int i = 0; i < 45; i++) {
			wayPoints[i] = GameObject.Find("WayPointCone_"+i);
			wayPoints[i].GetComponent<MeshRenderer> ().enabled = false;
			wayPoints[i].GetComponent<ConeWayPointScript>().enabled = false;
		}

		for (int i = 0; i < 3; i++) {
			mazes[i] = GameObject.Find("Maze_"+i);
			barrierTapes[i] = GameObject.Find("NoPassing_"+i);
		}

		int trialID = 0;


		startWayPointCalculator = new StartWayPointCalculator ();
		startWayPointCalculator.SetWayPointTransform (0, wayPoints [0].transform);
		startWayPointCalculator.SetWayPointTransform (1, wayPoints [11].transform);
		startWayPointCalculator.SetWayPointTransform (2, wayPoints [22].transform);
		startWayPointCalculator.SetWayPointTransform (3, null);

		arrowCtrl = character.GetComponentInChildren<ArrowControl> ();

		targetTransform = new StoreTransform ();
		FirstTrial (0);
	}
	
	// Update is called once per frame
	void Update () {
		if(cutSceneManager.cutSceneOn) {
			Vector3 targetPos = targetTransform.position;
			targetPos.y = playerStatus.PLAYER_HEIGHT;
			Vector3 posVel = targetPos - character.transform.position;
			float angleDiff = Mathf.Acos(Vector3.Dot(targetTransform.forward, character.transform.forward));
			if (posVel.magnitude < 5.0f && angleDiff < 0.4f) {
				// bingo we are in the right place
				character.transform.position = targetPos;
				character.transform.forward = targetTransform.forward;
				cutSceneManager.cutSceneOn = false;

				playerStatus.EnableControl(controlType);
				StartNextTrial();
			}
			else {
				if (angleDiff > 0.4f) {
					character.transform.Rotate(Vector3.up * angleDiff * rotCutSceneSpeed);
					//character.transform.up = Vector3.up;
				}
				if (posVel.magnitude > 5.0f) {
					//character.transform.position = character.transform.position + posVel.normalized * posCutSceneSpeed;
					Vector3 moveVel = character.transform.InverseTransformDirection(posVel);
					travelModelInterface.SetVelocity (moveVel/*posVel.normalized * posCutSceneSpeed*/, Vector3.zero);
				}
			}
		}
	}

	public void SwitchConeStatus(GameObject cone, bool enable, bool visible) {
		cone.GetComponent<ConeWayPointScript>().enabled = enable;
		cone.GetComponent<SphereCollider>().enabled = enable;
		cone.GetComponent<MeshRenderer> ().enabled = visible;
	}

	public void SwitchBarrierTape(GameObject barrierTape, bool close) {
		BoxCollider[] colliders = barrierTape.GetComponentsInChildren<BoxCollider>();
		for(int k=0; k<colliders.Length; k++) {
			colliders[k].enabled = close;
		}
		GameObject tape_1 = GameObject.Find(barrierTape.name + "/barrierTape");
		MeshRenderer[] render_1 = tape_1.GetComponentsInChildren<MeshRenderer>();
		for(int k=0; k<render_1.Length; k++) {
			render_1[k].enabled = close;
		}
	}

	public void OpenCloseBarrierTapes() {
		for (int i = 0; i < 3; i++) {		// close all
			SwitchBarrierTape(barrierTapes [i], true);
		}
		if (trialSequence [currentTrialID].sequenceID == 1 || trialSequence [currentTrialID].sequenceID == 4) {	 // open for segway only tests
			SwitchBarrierTape(barrierTapes [trialSequence [currentTrialID].mazeID], false);
			SwitchBarrierTape(barrierTapes [trialSequence [currentTrialID].nextMazeID], false);
		} else if (trialSequence [currentTrialID].sequenceID == 3 || trialSequence [currentTrialID].sequenceID == 5) {	
			SwitchBarrierTape(barrierTapes [trialSequence [currentTrialID].mazeID], false);
		}
	}

	public void FirstTrial(int sequenceID) {
		Debug.Log("FirstTrial");
		playerStatus.EnableControl(controlType);
		character.transform.up = Vector3.up;
		currentMazeID = startWayPointCalculator.GetClosestWayPointID(character.transform);
		currentWayPt = currentMazeID * 11;
		GenerateAllTrials (0, currentMazeID);
		OpenCloseBarrierTapes ();
		GenerateTrial();
		targetTransform.SetTransform (mazes [currentMazeID].transform);

		currentTrialID = 0;
		cutSceneManager.cutSceneOn = true;
	}

	public void StartNextTrial() {
		//int [] wayPoints;
		//trialSequence [currentStep];
		if (controlType == CONTROL_TYPE.JOYSTICK_SINGLE_MODE) {
			character.GetComponent<TravelModelInterface>().SetTargetGestureType (TRAVEL_TYPE.SINGLE_MODE);
			character.GetComponent<TravelModelInterface>().SetGestureType(TRAVEL_TYPE.SINGLE_MODE);
		}
		playerStatus.EnableControl(controlType);
		currentStep = 0;
		OpenCloseBarrierTapes ();
		arrowCtrl.SetTarget (wayPoints[trialSequence [currentTrialID].sequence[0]].transform);
		string str = "";
		for (int i = 0; i < trialSequence[currentTrialID].sequence.Length; i++) {
			str += trialSequence[currentTrialID].sequence[i] + ", ";
		}
		Debug.Log (str);
	}

	public void GenerateAllTrials(int trialID, int mazeID) {
		Debug.Log("GenerateAllTrials");
		//playerStatus.EnableControl(controlType);
		int curMazeTmp = mazeID;
		for (int i = 0; i < 6; i++) {
			trialSequence[i] = new CogTrialSequence(curMazeTmp, lattinSquare[trialID, i]);
			curMazeTmp = trialSequence[i].nextMazeID;
		}

	}

	public void GenerateTrial() {
		for (int i = 0; i < trialSequence[currentTrialID].sequence.Length; i++) {
			int wayPtID = trialSequence[currentTrialID].sequence[i];
			Debug.Log("wayPtID = " + wayPtID);
			SwitchConeStatus(wayPoints[wayPtID], false, false);
		}
		SwitchConeStatus(wayPoints [trialSequence [currentTrialID].sequence[0]], true, false);
	}

	public void FinishTrial () {
		currentStep = 0;
		currentTrialID++;
		if (currentTrialID < trialSequence.Length) {
			character.transform.up = Vector3.up;
			currentMazeID = startWayPointCalculator.GetClosestWayPointID (character.transform);
			currentWayPt = currentMazeID * 11;
			targetTransform.SetTransform (mazes [currentMazeID].transform);
			GenerateTrial ();
			cutSceneManager.cutSceneOn = true;
		} else {
			Application.Quit ();
		}
	}

	public GameObject NextStep() {
		currentStep++;
		if (currentStep >= trialSequence [currentTrialID].sequence.Length) {
			FinishTrial();
			return null;
		}
		int wayPtID = trialSequence [currentTrialID].sequence [currentStep];
		int prevWayPtID = trialSequence [currentTrialID].sequence [currentStep - 1];
		SwitchConeStatus (wayPoints [wayPtID], true, false);
		SwitchConeStatus (wayPoints [prevWayPtID], false, false);
		if (currentStep == 5) {
			if (trialSequence [currentTrialID].sequenceID == 3 || trialSequence [currentTrialID].sequenceID == 5) {	
				SwitchBarrierTape(barrierTapes [trialSequence [currentTrialID].mazeID], true);
			}
			else if (trialSequence [currentTrialID].sequenceID == 0 || trialSequence [currentTrialID].sequenceID == 2) {	
				SwitchBarrierTape(barrierTapes [trialSequence [currentTrialID].nextMazeID], false);
			}

			if (trialSequence [currentTrialID].sequenceID == 0 || trialSequence [currentTrialID].sequenceID == 3) {
				SwitchConeStatus(wayPoints[trialSequence [currentTrialID].sequence[trialSequence [currentTrialID].sequence.Length - 1]], true, true);
			}
		}
		if (currentStep >= 5) {
			int lastID = trialSequence [currentTrialID].sequence [trialSequence [currentTrialID].sequence.Length - 1];
			SwitchConeStatus (wayPoints [lastID], true, true);

		}

		return wayPoints[trialSequence [currentTrialID].sequence[currentStep]];
	}
}
