using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum LEVEL_TYPE {
	TRIAL,
	EASY,
	MEDIUM,
	HARD
};

public enum TRAVEL_TYPE {
	WALKING,
	SEGWAY,
	SURFING,
	NOTHING,
	RESTING,
	RESET
};

public enum CONTROL_TYPE {
	JOYSTICK,
	FORCEPAD_GESTURE,
	BODY_DRIVEN
};

public class TrialType {
	public TRAVEL_TYPE mode;
	public LEVEL_TYPE level;
	public int LeftOrRight;
};

public class StoreTransform {
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 scale;
	public Vector3 forward;

	public void SetTransform(Transform t) {
		position = t.position;
		rotation = t.rotation;
		scale = t.localScale;
		forward = t.forward;
	}
}

public class StartWayPointCalculator {
	public Transform[] startWayPoints;
	public StartWayPointCalculator () {
		startWayPoints = new Transform[4];
	}

	public void SetWayPointTransform (int id, Transform transform) {
		startWayPoints[id] = transform;
	}

	public int GetClosestWayPointID (Transform transform) {
		int minID = 0;
		float minDis = (startWayPoints[0].transform.position - transform.position).magnitude;
		for(int i=1; i<4; i++) {
			float distance = (startWayPoints[i].transform.position - transform.position).magnitude;
			if (distance < minDis) {
				minDis = distance;
				minID = i;
			}
		}
		return minID;
	}

	public Transform GetTransformByID(int id) {
		return startWayPoints[id];
	}
};

public class TrialControl : MonoBehaviour {
	private SegwayPathController segwayPathControl;
	private WalkingTrialControl walkingTrialControl;
	private SurfingTrialControl surfingTrialControl;
	private GameObject character;
	public LEVEL_TYPE levelType;
	public TRAVEL_TYPE travelType;
	public CONTROL_TYPE controlType;
	public CutSceneManager cutSceneManager;
	public Text modeSwitchText;
	public TrialType[] trialSequence;
	public int currentTrialID;
	public StoreTransform targetTransform;
	private TrialSequenceGenerator trialGenerator;
	private HIVEFPSController fpsController;
	private StudyRecorder studyRecorder;

	private float posCutSceneSpeed = 1.0f;
	private float rotCutSceneSpeed = 0.3f;
	public StartWayPointCalculator startWayPointCalculator;
	public int currentStartWayPointID = 0;
	private PlayerStatus playerStatus;
	public TravelModelInterface travelModelInterface;

	private TRAVEL_TYPE typeBeforeReset;
	void Awake () {
		character = GameObject.Find ("Character");
		segwayPathControl = GameObject.Find ("SegwayWayPointsManager").GetComponent<SegwayPathController> ();
		walkingTrialControl = GameObject.Find ("WalkingTrialManager").GetComponent<WalkingTrialControl> ();
		surfingTrialControl = GameObject.Find ("SurfingTrialManager").GetComponent<SurfingTrialControl> ();
		cutSceneManager = GameObject.Find ("CutSceneManager").GetComponent<CutSceneManager> ();
		modeSwitchText = GameObject.Find ("ModeSwitchText").GetComponent<Text> ();
		playerStatus = character.GetComponent<PlayerStatus>();
		trialGenerator = new TrialSequenceGenerator();
		startWayPointCalculator = new StartWayPointCalculator();
		studyRecorder = GameObject.Find ("StudyRecorder").GetComponent<StudyRecorder> ();
		travelModelInterface = GetComponent<TravelModelInterface>();
		fpsController = GetComponent<HIVEFPSController>();
	}

	// Use this for initialization
	void Start() {
		targetTransform = new StoreTransform();
		targetTransform.SetTransform(character.transform);
		controlType = ConfigurationHandler.controllerType;
		if(controlType != CONTROL_TYPE.FORCEPAD_GESTURE) {
			GameObject.Find("Widget").GetComponent<RawImage>().enabled = false;
		}
		trialSequence = trialGenerator.GenerateByLattinSquare(ConfigurationHandler.subjectID);
		for (int i=0; i<4; i++) {
			startWayPointCalculator.SetWayPointTransform(i, segwayPathControl.GetWayPointTrigger(i).transform);
		}
		currentStartWayPointID = startWayPointCalculator.GetClosestWayPointID(character.transform);
		//GenerateTrial();
		FirstTrial();
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
				switch(typeBeforeReset) {
				case TRAVEL_TYPE.WALKING: 
					fpsController.SetWalking();
					break;
				case TRAVEL_TYPE.SEGWAY: 
					fpsController.SetSegway();
					break;
				case TRAVEL_TYPE.SURFING: 
					fpsController.SetSurfing();
					break;
				}
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

	public StoreTransform GenerateTrial() {
		Debug.Log("GenerateTrial");
		//playerStatus.EnableControl(controlType);

		switch (trialSequence[currentTrialID].mode) {
		case TRAVEL_TYPE.WALKING: 
			Transform nearestWayPt = startWayPointCalculator.GetTransformByID(currentStartWayPointID);
			targetTransform = walkingTrialControl.SetWalkingPath ((int)trialSequence[currentTrialID].level, 0, nearestWayPt);
			break;
		case TRAVEL_TYPE.SEGWAY: 
			targetTransform = segwayPathControl.SetSegwayPath ((int)trialSequence[currentTrialID].level, currentStartWayPointID, 0);
			break;
		case TRAVEL_TYPE.SURFING: 
			Transform startPts = startWayPointCalculator.GetTransformByID(currentStartWayPointID);
			Transform endPts = startWayPointCalculator.GetTransformByID((currentStartWayPointID + 1) % 4);
			targetTransform = surfingTrialControl.GenerateSamples (startPts, endPts);
			break;
		}
		return targetTransform;
	}

	public void FinishTrial() {
		Debug.Log("FinishTrial");
		currentTrialID++;
		modeSwitchText.text = "Trial COMPLETE!\n Preparing for next trial.";
		modeSwitchText.enabled = true;
		travelModelInterface.SetVelocity (Vector3.zero, Vector3.zero);
		if(currentTrialID < trialSequence.Length) {
			//
			currentStartWayPointID = startWayPointCalculator.GetClosestWayPointID(character.transform);
			GenerateTrial();
			character.transform.up = Vector3.up;
			cutSceneManager.cutSceneOn = true;
			playerStatus.DisableControl();
			typeBeforeReset = fpsController.SetReset();
		}
		else {
			modeSwitchText.text = "All the trials are finished! Thank you!";
			modeSwitchText.enabled = true;
			studyRecorder.StopContextSwitchRecorder();
		}
	}

	public void FirstTrial() {
		Debug.Log("FirstTrial");
		playerStatus.EnableControl(controlType);
		currentTrialID = 0;
		character.transform.up = Vector3.up;
		cutSceneManager.cutSceneOn = true;
		currentStartWayPointID = startWayPointCalculator.GetClosestWayPointID(character.transform);
		GenerateTrial();
		/*string modeStr = "";
		switch (trialSequence[currentTrialID].mode) {
		case TRAVEL_TYPE.WALKING:
			modeStr = "Walking";
			break;
		case TRAVEL_TYPE.SEGWAY:
			modeStr = "Segway";
			break;
		case TRAVEL_TYPE.SURFING:
			modeStr = "Surfing";
			break;
		}
		modeSwitchText.text = "Please switch to " + modeStr + " mode to start trial";
		modeSwitchText.enabled = true;*/
		//character.GetComponent<TravelModelInterface>().SetTargetGestureType (trialSequence[currentTrialID].mode);
	}

	public void StartNextTrial() {
		Debug.Log("StartNextTrial");
		string modeStr = "";
		switch (trialSequence[currentTrialID].mode) {
		case TRAVEL_TYPE.WALKING:
			modeStr = "Walking";
			break;
		case TRAVEL_TYPE.SEGWAY:
			modeStr = "Segway";
			break;
		case TRAVEL_TYPE.SURFING:
			modeStr = "Surfing";
			break;
		}
		modeSwitchText.text = "Switch to " + modeStr + " mode.";
		modeSwitchText.enabled = true;
		character.GetComponent<TravelModelInterface>().SetTargetGestureType (trialSequence[currentTrialID].mode);
		playerStatus.EnableControl(controlType);
		//character.GetComponent<TravelModelInterface>().SetTargetGestureType (TRAVEL_TYPE.RESTING);


		//playerStatus.DisableControl();
	}
}
