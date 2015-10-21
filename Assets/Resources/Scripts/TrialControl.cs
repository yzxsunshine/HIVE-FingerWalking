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
	RESTING
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
	public Transform targetTransform;
	private TrialSequenceGenerator trialGenerator;

	private float posCutSceneSpeed = 1.0f;
	private float rotCutSceneSpeed = 0.01f;
	public StartWayPointCalculator startWayPointCalculator;
	public int currentStartWayPointID = 0;
	void Awake () {
		character = GameObject.Find ("Character");
		segwayPathControl = GameObject.Find ("SegwayWayPointsManager").GetComponent<SegwayPathController> ();
		walkingTrialControl = GameObject.Find ("WalkingTrialManager").GetComponent<WalkingTrialControl> ();
		surfingTrialControl = GameObject.Find ("SurfingTrialManager").GetComponent<SurfingTrialControl> ();
		cutSceneManager = GameObject.Find ("CutSceneManager").GetComponent<CutSceneManager> ();
		modeSwitchText = GameObject.Find ("ModeSwitchText").GetComponent<Text> ();
		trialGenerator = new TrialSequenceGenerator();
		startWayPointCalculator = new StartWayPointCalculator();
	}

	// Use this for initialization
	void Start() {
		targetTransform = character.transform;
		controlType = ConfigurationHandler.controllerType;
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
			Vector3 posVel = targetTransform.position - character.transform.position;
			Quaternion rotVel = new Quaternion();
			rotVel.SetFromToRotation(character.transform.forward, targetTransform.forward);
			Vector3 rotDiff = targetTransform.forward - character.transform.forward;
			if (posVel.magnitude < Mathf.Epsilon && rotDiff.magnitude < Mathf.Epsilon) {
				// bingo we are in the right place
				cutSceneManager.cutSceneOn = false;
			}
			else {
				character.transform.position = character.transform.position + posVel.normalized * posCutSceneSpeed;
				character.transform.Rotate(rotVel.eulerAngles * rotCutSceneSpeed);
			}
		}
	}

	public Transform GenerateTrial() {
		switch (controlType) {
		case CONTROL_TYPE.JOYSTICK:
			character.GetComponent<TouchPadGesture>().enabled = false;
			character.GetComponent<JoystickGesture>().enabled = true;
			break;
		case CONTROL_TYPE.FORCEPAD_GESTURE:
			character.GetComponent<TouchPadGesture>().enabled = true;
			character.GetComponent<JoystickGesture>().enabled = false;
			break;
		case CONTROL_TYPE.BODY_DRIVEN:
			character.GetComponent<TouchPadGesture>().enabled = false;
			character.GetComponent<JoystickGesture>().enabled = false;
			break;
		}

		switch (trialSequence[currentTrialID].mode) {
		case TRAVEL_TYPE.WALKING: 
			targetTransform = walkingTrialControl.SetWalkingPath ((int)trialSequence[currentTrialID].level, 0, character.transform);
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
		currentTrialID++;
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
		modeSwitchText.text = "This trial is complete. Please switch to " + modeStr + " mode to start next trial";
		modeSwitchText.enabled = true;
		character.GetComponent<TravelModelInterface>().SetTargetGestureType (trialSequence[currentTrialID].mode);
	}

	public void FirstTrial() {
		currentTrialID = 0;
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
		modeSwitchText.text = "Please switch to " + modeStr + " mode to start trial";
		modeSwitchText.enabled = true;
		character.GetComponent<TravelModelInterface>().SetTargetGestureType (trialSequence[currentTrialID].mode);
	}

	public void StartNextTrial() {
		cutSceneManager.cutSceneOn = true;
		GenerateTrial();
	}
}
