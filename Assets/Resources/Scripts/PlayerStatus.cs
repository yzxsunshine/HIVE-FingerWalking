//#define DEBUG_DISPLAY

using UnityEngine;
using System.Collections;

public class PlayerStatus : MonoBehaviour {
	public int segwayCollisionNum = 0;
	public float segwayCollisionTime = 0;
	private TravelModelInterface travelModelInterface;
	private HIVEFPSController fpsController;
	private TrialControl trialControl;
	public float PLAYER_HEIGHT = 1.8f;
	// Use this for initialization
	void Awake () {
		travelModelInterface = GetComponent<TravelModelInterface> ();
		trialControl = GetComponent<TrialControl> ();
		fpsController = GetComponent<HIVEFPSController>();
		ResetPlayerStatus ();
	}

	void Start() {
	
	}

	// Update is called once per frame
	void Update () {
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
		string line = "" + transform.position.x + "\t" + transform.position.y + "\t" + transform.position.z + "\t";
		line += transform.rotation.x + "\t" + transform.rotation.y + "\t" + transform.rotation.z + "\t" + transform.rotation.w;
		return line;
	}

	public string GetStatusTableHead () {
		string line = "#TimeStamp#\t" + "#PositionX#\t" + "#PositionY#\t" + "#PositionZ#\t";
		line += "#OrientationX#\t" + "#OrientationY#\t" + "#OrientationZ#\t" + "#OrientationW#\t";
		return line;
	}

	public CONTROL_TYPE GetControlType() {
		return trialControl.controlType;
	}

	public void DisableControl() {
		GetComponent<TouchPadGesture>().enabled = false;
		GetComponent<JoystickGesture>().enabled = false;
		travelModelInterface.DisableMove();
	}

	public void EnableControl(CONTROL_TYPE controlType) {
		switch (controlType) {
		case CONTROL_TYPE.JOYSTICK:
			GetComponent<TouchPadGesture>().enabled = false;
			GetComponent<JoystickGesture>().enabled = true;
			break;
		case CONTROL_TYPE.FORCEPAD_GESTURE:
			GetComponent<TouchPadGesture>().enabled = true;
			GetComponent<JoystickGesture>().enabled = false;
			break;
		case CONTROL_TYPE.BODY_DRIVEN:
			GetComponent<TouchPadGesture>().enabled = false;
			GetComponent<JoystickGesture>().enabled = false;
			break;
		}
		travelModelInterface.EnableMove();
		//fpsController.EnableMove();
	}
}
