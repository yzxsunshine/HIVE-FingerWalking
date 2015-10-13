using UnityEngine;
using System.Collections;

public enum LEVEL_TYPE {
	TRIAL,
	EASY,
	MEDIUM,
	HARD
};

public enum TRAVEL_TYPE {
	WALKING,
	SEGWAY,
	SURFING
};

public enum CONTROL_TYPE {
	JOYSTICK,
	FORCEPAD_GESTURE,
	BODY_DRIVEN
};

public class TrialControl : MonoBehaviour {
	private SegwayPathController segwayPathControl;
	private WalkingTrialControl walkingTrialControl;
	private SurfingTrialControl surfingTrialControl;
	private GameObject character;
	public LEVEL_TYPE levelType;
	public TRAVEL_TYPE travelType;
	public CONTROL_TYPE controlType;

	void Awake () {
		character = GameObject.Find ("Character");
		segwayPathControl = GameObject.Find ("SegwayWayPointsManager").GetComponent<SegwayPathController> ();
		walkingTrialControl = GameObject.Find ("WalkingTrialManager").GetComponent<WalkingTrialControl> ();
		surfingTrialControl = GameObject.Find ("SurfingTrialManager").GetComponent<SurfingTrialControl> ();
	}

	// Use this for initialization
	void Start() {
		GenerateTrial();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GenerateTrial() {
		levelType = LEVEL_TYPE.EASY;//LEVEL_TYPE.TRIAL;
		travelType = TRAVEL_TYPE.SURFING;//TRAVEL_TYPE.WALKING;//TRAVEL_TYPE.SEGWAY;
		controlType = ConfigurationHandler.controllerType;

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

		switch (travelType) {
		case TRAVEL_TYPE.WALKING: 
			walkingTrialControl.SetWalkingPath ((int)levelType, 0, character.transform);
			break;
		case TRAVEL_TYPE.SEGWAY: 
			segwayPathControl.SetSegwayPath ((int)levelType, 0);
			break;
		case TRAVEL_TYPE.SURFING: 
			Vector3 startPts = GameObject.Find ("SegwayWaypoint_0").transform.position;
			Vector3 endPts = GameObject.Find ("SegwayWaypoint_1").transform.position;
			surfingTrialControl.GenerateSamples (startPts, endPts);
			break;
		}
	}
}
