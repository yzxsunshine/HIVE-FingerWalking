using UnityEngine;
using System.Collections;

public class JoystickParams {
	public float walkingSpeed;
	public float walkingAngularSpeed;
	public float segwaySpeed;
	public float segwayAngularSpeed;
	public float surfingSpeed;
	public float surfingPitchSpeed;
	public float surfingYawSpeed;

	public JoystickParams() {
		walkingSpeed = 10.0f;
		walkingAngularSpeed = 1.0f;
		segwaySpeed = 20.0f;
		segwayAngularSpeed = 1.0f;
		surfingSpeed = 30.0f;
		surfingPitchSpeed = 20.0f;
		surfingYawSpeed = 1.0f;
	}
};

public class JoystickGesture : MonoBehaviour {
	private TRAVEL_TYPE gestureType = TRAVEL_TYPE.WALKING;
	public float rightHorizontal;
	public float rightVertical;
	public float leftHorizontal;
	public float leftVertical;

	public JoystickParams joystickParams;
	public bool flipLeftRight = false;
	public bool flipSurfPitch = false;

	public const float MAX_DOUBLE_CLICK_TIME = 2.0f;
	public float timerDoubleClick;
	private TravelModelInterface travelModelInterface;

	private bool trainingResponse = false;
	public float calibratedLH;
	public float calibratedLV;
	public float calibratedRH;
	public float calibratedRV;
	// Use this for initialization
	void Start () {
		timerDoubleClick = MAX_DOUBLE_CLICK_TIME;
		travelModelInterface = GetComponent<TravelModelInterface>();
		joystickParams = ConfigurationHandler.joystickParams;
		//calibratedLH = 0.0f;
	    //calibratedLV = 0.0f;
	    //calibratedRH = 0.0f;
	    //calibratedRV = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
		
		//GetComponentInChildren<LocomotionAnimation> ().vel = moveVel;
		if(Input.GetKeyDown("joystick button 0")) {
			//if (Application.loadedLevelName == "cognitive") {
				travelModelInterface.SetTargetGestureType (TRAVEL_TYPE.WALKING);
			//}
			travelModelInterface.SetGestureType (TRAVEL_TYPE.WALKING);
			Debug.Log("Walking");
		}
		else if(Input.GetKeyDown("joystick button 1")) {
			//if (Application.loadedLevelName == "cognitive") {
				travelModelInterface.SetTargetGestureType (TRAVEL_TYPE.SEGWAY);
			//}
			travelModelInterface.SetGestureType (TRAVEL_TYPE.SEGWAY);
			Debug.Log("Segway");
		}
		else if(Input.GetKeyDown("joystick button 2")) {
			//if (Application.loadedLevelName == "cognitive") {
			//	travelModelInterface.SetTargetGestureType (TRAVEL_TYPE.SURFING);
			//}
			//travelModelInterface.SetGestureType (TRAVEL_TYPE.SURFING);
			//Debug.Log("Surfing");
		}
		else if(Input.GetKeyDown("joystick button 3")) {
			if (timerDoubleClick > MAX_DOUBLE_CLICK_TIME) {	// too long between two click
				timerDoubleClick = 0;
			}
			else
			{
				travelModelInterface.GetTrialControl().ResetToLatestPoint();
				timerDoubleClick = MAX_DOUBLE_CLICK_TIME;
			}
		}
		else if(Input.GetKeyDown("joystick button 4")) {
			flipLeftRight = ! flipLeftRight;
			Debug.Log("Flip left and right");
		}
		else if(Input.GetKeyDown("joystick button 5")) {
			flipSurfPitch = ! flipSurfPitch;
			Debug.Log("Flip surfing pitch");
		}

		if (flipLeftRight) {
			rightHorizontal = Input.GetAxis ("Horizontal") * 1.0f - calibratedLH;
			rightVertical = Input.GetAxis ("Vertical") * 1.0f - calibratedLV;
			leftHorizontal = Input.GetAxis ("ZHorizontal") * 1.0f - calibratedRH;
			leftVertical = Input.GetAxis ("ZVertical") * 1.0f - calibratedRV;
		} else {
			rightHorizontal = Input.GetAxis ("ZHorizontal") * 1.0f - calibratedRH;
			rightVertical = -Input.GetAxis ("ZVertical") * 1.0f + calibratedRV;
			leftHorizontal = Input.GetAxis ("Horizontal") * 1.0f - calibratedLH;
			leftVertical = -Input.GetAxis ("Vertical") * 1.0f + calibratedLV;
		}
		//Vector3 moveVel = velQueue.GetAvgVelocity(velocity, gestureType);
		//controller.SendMessage("SetVelocity", moveVel);
		//this.GetComponent("HIVEFPSController").SendMessage("SetRotation", rotation);
		//controller.SendMessage("DoStep");
		//Debug.Log(moveVel.ToString());
		Vector3 moveVel = new Vector3();
		Vector3 rotVel = new Vector3 ();

		switch (travelModelInterface.GetGestureType ()) {
		case TRAVEL_TYPE.WALKING:
			// right hand joystick control the translation, left hand joystick control rotation
			moveVel = new Vector3 (leftHorizontal, 0, leftVertical) * joystickParams.walkingSpeed;
			rotVel = new Vector3 (0, rightHorizontal, 0) * joystickParams.walkingAngularSpeed;
			this.GetComponent ("HIVEFPSController").SendMessage ("SetWalking");
			break;
		case TRAVEL_TYPE.SEGWAY:
			moveVel = Vector3.forward * leftVertical * joystickParams.segwaySpeed;
			rotVel = new Vector3 (0, leftHorizontal, 0) * joystickParams.segwayAngularSpeed;
			this.GetComponent ("HIVEFPSController").SendMessage ("SetSegway");
			break;
		case TRAVEL_TYPE.SURFING:
			moveVel = -Vector3.forward * Mathf.Min(rightVertical, 0) * joystickParams.surfingSpeed;

			if (flipSurfPitch) {
				leftVertical = - leftVertical;
			} 
			rotVel = new Vector3(leftVertical * joystickParams.surfingPitchSpeed, leftHorizontal * joystickParams.surfingYawSpeed, 0);
			this.GetComponent("HIVEFPSController").SendMessage("SetSurfing");
			break;
		}

		travelModelInterface.SetVelocity (moveVel, rotVel);
		timerDoubleClick += Time.deltaTime;
	}

	public void Calibrate() {
		calibratedLH = Input.GetAxis ("Horizontal") * 1.0f;
		calibratedLV = Input.GetAxis ("Vertical") * 1.0f;
		calibratedRH = Input.GetAxis ("ZHorizontal") * 1.0f;
		calibratedRV = Input.GetAxis ("ZVertical") * 1.0f;
	}

	public void SetTrainingResponse(bool response) {
		trainingResponse = response;
	}
}
