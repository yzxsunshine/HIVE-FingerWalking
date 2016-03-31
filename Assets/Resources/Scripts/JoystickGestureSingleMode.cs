using UnityEngine;
using System.Collections;

public class JoystickSingleModeParams {
	public float minSpeed;
	public float angularSpeed;
	public float maxSpeed;
	public float maxAngularSpeed;

	public JoystickSingleModeParams() {
		minSpeed = 10.0f;
		angularSpeed = 1.0f;
		maxAngularSpeed = 2.0f;
		maxSpeed = 20.0f;
	}
};

public class JoystickGestureSingleMode : MonoBehaviour {
	private TRAVEL_TYPE gestureType = TRAVEL_TYPE.WALKING;
	public float rightHorizontal;
	public float rightVertical;
	public float leftHorizontal;
	public float leftVertical;
	public float zValue;

	public JoystickSingleModeParams joystickSingleModeParams;
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
	public float calibratedZ;
	// Use this for initialization
	void Start () {
		timerDoubleClick = MAX_DOUBLE_CLICK_TIME;
		travelModelInterface = GetComponent<TravelModelInterface>();
		joystickSingleModeParams = ConfigurationHandler.joystickSingleModeParams;
		//calibratedLH = 0.0f;
	    //calibratedLV = 0.0f;
	    //calibratedRH = 0.0f;
	    //calibratedRV = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {

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

		zValue = Mathf.Abs(Input.GetAxis ("ZAxis") * 1.0f - calibratedZ);
		//Vector3 moveVel = velQueue.GetAvgVelocity(velocity, gestureType);
		//controller.SendMessage("SetVelocity", moveVel);
		//this.GetComponent("HIVEFPSController").SendMessage("SetRotation", rotation);
		//controller.SendMessage("DoStep");
		//Debug.Log(moveVel.ToString());
		Vector3 moveVel = new Vector3();
		Vector3 rotVel = new Vector3 ();

		moveVel = Vector3.forward * leftVertical * (joystickSingleModeParams.minSpeed + zValue * (joystickSingleModeParams.maxSpeed - joystickSingleModeParams.minSpeed));
		rotVel = new Vector3 (0, rightHorizontal, 0) * (joystickSingleModeParams.angularSpeed + zValue * (joystickSingleModeParams.maxAngularSpeed - joystickSingleModeParams.angularSpeed));
		this.GetComponent ("HIVEFPSController").SendMessage ("SegSingleMode");

		travelModelInterface.SetVelocity (moveVel, rotVel);
		timerDoubleClick += Time.deltaTime;
	}

	public void Calibrate() {
		calibratedLH = Input.GetAxis ("Horizontal") * 1.0f;
		calibratedLV = Input.GetAxis ("Vertical") * 1.0f;
		calibratedRH = Input.GetAxis ("ZHorizontal") * 1.0f;
		calibratedRV = Input.GetAxis ("ZVertical") * 1.0f;
		calibratedZ = Input.GetAxis ("ZAxis") * 1.0f;
	}

	public void SetTrainingResponse(bool response) {
		trainingResponse = response;
	}
}
