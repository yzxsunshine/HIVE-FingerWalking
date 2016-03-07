//#define DEBUG_DISPLAY

using UnityEngine;
using System.Collections;

public enum COLLISION_STATUS {
	COLLISION_ENTER,
	COLLISION_INSIDE,
	COLLISION_EXIT,
	COLLISION_NONE
};

public enum TRIAL_STATUS {
	TRIAL_START,
	TRIAL_EXECUTING,
	TRIAL_FINISH,
	TRIAL_NOTHING
};

public enum TRAVEL_MODE_STATUS {
	WAIT_SWITCH,
	CORRECT_SWITCH,
	INCORRECT_SWITCH_TO_WALKING,
	INCORRECT_SWITCH_TO_SEGWAY,
	INCORRECT_SWITCH_TO_SURFING,
	NOT_SWITCH,
	IDLE_SWITCH
};

public class PlayerStatus : MonoBehaviour {
	public int segwayCollisionNum = 0;
	public float segwayCollisionTime = 0;
	private TravelModelInterface travelModelInterface;
	private HIVEFPSController fpsController;
	private TrialControl trialControl;
	private CogTrialControl cogTrialControl;
	public float PLAYER_HEIGHT = 1.8f;
	public COLLISION_STATUS collisionStatus = COLLISION_STATUS.COLLISION_NONE;
	public TRIAL_STATUS trialStatus = TRIAL_STATUS.TRIAL_NOTHING;
	public TRAVEL_MODE_STATUS modeStatus = TRAVEL_MODE_STATUS.NOT_SWITCH;
	public bool triggerReset = false;
	public Vector3 headOrientation;
	public float timeStamp;
	public bool triggerWaypoint = false;
	public Vector3 wayPointOnPath;
	// Use this for initialization
	void Awake () {
		travelModelInterface = GetComponent<TravelModelInterface> ();
		if (Application.loadedLevelName == "ve_comples") {
			trialControl = GetComponent<TrialControl> ();
		} else {
			cogTrialControl = GetComponent<CogTrialControl>();
		}
		fpsController = GetComponent<HIVEFPSController>();
		ResetPlayerStatus ();
		headOrientation = Vector3.zero;
		wayPointOnPath = Vector3.zero;
	}

	void Start() {
		timeStamp = 0;
	}

	// Update is called once per frame
	void FixedUpdate () {
		timeStamp += Time.fixedDeltaTime;
	}

	void OnGUI() {
#if DEBUG_DISPLAY
		GUI.Label(new Rect(10, 60, 400, 30), "Number of Collision in this trial : " + segwayCollisionNum);
		GUI.Label(new Rect(10, 100, 400, 30), "Time of stucking in obstacles : " + segwayCollisionTime);
#endif
	}


	public void ResetPlayerStatus() {
		segwayCollisionNum = 0;
		segwayCollisionTime = 0;
	}

	public TRAVEL_TYPE GetGestureType() {
		return travelModelInterface.GetGestureType ();
	}

	public string GetCurrentTransformLine() {
		string line = "" + timeStamp + "\t";
		/*switch (trialStatus) {
		case TRIAL_STATUS.TRIAL_START:
			line += "TRIAL_START";
			trialStatus = TRIAL_STATUS.TRIAL_EXECUTING;
			break;
		case TRIAL_STATUS.TRIAL_EXECUTING:
			line += "TRIAL_EXECUTING";
			break;
		case TRIAL_STATUS.TRIAL_FINISH:
			line += "TRIAL_FINISH";
			trialStatus = TRIAL_STATUS.TRIAL_NOTHING;
			break;
		case TRIAL_STATUS.TRIAL_NOTHING:
			line += "TRIAL_NOTHING";
			break;
		}
		line += "\t";*/

		switch (collisionStatus) {
		case COLLISION_STATUS.COLLISION_ENTER:
			line += "COLLISION_ENTER";
			collisionStatus = COLLISION_STATUS.COLLISION_INSIDE;
			break;
		case COLLISION_STATUS.COLLISION_INSIDE:
			line += "COLLISION_INSIDE";
			break;
		case COLLISION_STATUS.COLLISION_EXIT:
			line += "COLLISION_EXIT";
			collisionStatus = COLLISION_STATUS.COLLISION_NONE;
			break;
		case COLLISION_STATUS.COLLISION_NONE:
			line += "COLLISION_NONE";
			break;
		}
		line += "\t";

		switch (modeStatus) {
		case TRAVEL_MODE_STATUS.WAIT_SWITCH:
			line += "WAIT_SWITCH";
			modeStatus = TRAVEL_MODE_STATUS.NOT_SWITCH;
			break;
		case TRAVEL_MODE_STATUS.CORRECT_SWITCH:
			line += "CORRECT_SWITCH";
			modeStatus = TRAVEL_MODE_STATUS.NOT_SWITCH;
			break;
		case TRAVEL_MODE_STATUS.INCORRECT_SWITCH_TO_WALKING:
			line += "INCORRECT_SWITCH_TO_WALKING";
			modeStatus = TRAVEL_MODE_STATUS.NOT_SWITCH;
			break;
		case TRAVEL_MODE_STATUS.INCORRECT_SWITCH_TO_SEGWAY:
			line += "INCORRECT_SWITCH_TO_SEGWAY";
			modeStatus = TRAVEL_MODE_STATUS.NOT_SWITCH;
			break;
		case TRAVEL_MODE_STATUS.INCORRECT_SWITCH_TO_SURFING:
			line += "INCORRECT_SWITCH_TO_SURFING";
			modeStatus = TRAVEL_MODE_STATUS.NOT_SWITCH;
			break;
		case TRAVEL_MODE_STATUS.NOT_SWITCH:
			line += "NOT_SWITCH";
			break;
		case TRAVEL_MODE_STATUS.IDLE_SWITCH:
			line += "IDLE_SWITCH";
			modeStatus = TRAVEL_MODE_STATUS.NOT_SWITCH;
			break;
		}
		line += "\t";

		if (triggerReset) {
			line += "RESET";
			triggerReset = false;
		}
		else
			line += "NOT_RESET";
		line += "\t";

		if (triggerWaypoint) {
			line += "TRIGGER_WAYPOINT";
			triggerWaypoint = false;
		}
		else
			line += "NOT_WAYPOINT";
		line += "\t";

		line += transform.position.x + "\t" + transform.position.y + "\t" + transform.position.z + "\t";
		line += transform.rotation.x + "\t" + transform.rotation.y + "\t" + transform.rotation.z + "\t" + transform.rotation.w + "\t";
		line += headOrientation.x + "\t" + headOrientation.y + "\t" + headOrientation.z + "\t";
		line += wayPointOnPath.x + "\t" + wayPointOnPath.y + "\t" + wayPointOnPath.z + "\t";
		return line;
	}

	public string GetStatusTableHead () {
		string line = "#TimeStamp#\t" + "#Collision Event#\t" + "#Mode Switch Event#\t" + "#Reset Event#\t" + "#Waypoint Event#\t" + "#PositionX#\t" + "#PositionY#\t" + "#PositionZ#\t";
		line += "#OrientationX#\t" + "#OrientationY#\t" + "#OrientationZ#\t" + "#OrientationW#\t";
		line += "#HeadOrientationX#\t" + "#HeadOrientationY#\t" + "#HeadOrientationZ#\t";
		line += "#WayPointX#\t" + "#WayPointY#\t" + "#WayPointZ#\t";
		return line;
	}

	public CONTROL_TYPE GetControlType() {
		if (Application.loadedLevelName == "ve_comples") {
			return trialControl.controlType;
		} else {
			return cogTrialControl.controlType;
		}
	}

	public void DisableControl() {
		GetComponent<TouchPadGesture>().enabled = false;
		GetComponent<JoystickGesture>().enabled = false;
		GetComponent<ContexForceExtension>().enabled = false;
		travelModelInterface.DisableMove();
	}

	public void EnableControl(CONTROL_TYPE controlType) {
		switch (controlType) {
		case CONTROL_TYPE.JOYSTICK:
			GetComponent<TouchPadGesture>().enabled = false;
			GetComponent<JoystickGesture>().enabled = true;
			GetComponent<ContexForceExtension>().enabled = false;
			break;
		case CONTROL_TYPE.FORCEPAD_GESTURE:
			GetComponent<TouchPadGesture>().enabled = true;
			GetComponent<JoystickGesture>().enabled = false;
			GetComponent<ContexForceExtension>().enabled = false;
			break;
		case CONTROL_TYPE.BODY_DRIVEN:
			GetComponent<TouchPadGesture>().enabled = false;
			GetComponent<JoystickGesture>().enabled = false;
			GetComponent<ContexForceExtension>().enabled = false;
			break;
		case CONTROL_TYPE.FORCE_EXTENSION:
			GetComponent<TouchPadGesture>().enabled = false;
			GetComponent<JoystickGesture>().enabled = false;
			GetComponent<ContexForceExtension>().enabled = true;
			break;
		}
		travelModelInterface.EnableMove();
		//fpsController.EnableMove();
	}

	public void CollisionEntered() {
		collisionStatus = COLLISION_STATUS.COLLISION_ENTER;
	}

	public void CollisionExit() {
		collisionStatus = COLLISION_STATUS.COLLISION_EXIT;
	}

	public void WaitSwitch() {
		modeStatus = TRAVEL_MODE_STATUS.WAIT_SWITCH;
	}

	public void CorrectSwitch() {
		modeStatus = TRAVEL_MODE_STATUS.CORRECT_SWITCH;
	}

	public void IncorrectSwitchToWalking() {
		modeStatus = TRAVEL_MODE_STATUS.INCORRECT_SWITCH_TO_WALKING;
	}

	public void IncorrectSwitchToSegway() {
		modeStatus = TRAVEL_MODE_STATUS.INCORRECT_SWITCH_TO_SEGWAY;
	}

	public void IncorrectSwitchToSurfing() {
		modeStatus = TRAVEL_MODE_STATUS.INCORRECT_SWITCH_TO_SURFING;
	}

	public void IdleSwitch() {
		modeStatus = TRAVEL_MODE_STATUS.IDLE_SWITCH;
	}

	public void Reset() {
		triggerReset = true;
	}

	public void TrigerWayPoint() {
		triggerWaypoint = true;
	}

	public void SetWayPoint(Vector3 wp) {
		wayPointOnPath = wp;
	}

	public void CleanCollision() {
		segwayCollisionNum = 0;
		segwayCollisionTime = 0;
	}

	public int GetCollisionNum() {
		return segwayCollisionNum;
	}

	public void CalibrateControlDevice(CONTROL_TYPE controlType) {
		if (controlType == CONTROL_TYPE.JOYSTICK) {
			GetComponent<JoystickGesture>().Calibrate();
		}
	}
}
