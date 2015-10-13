﻿using UnityEngine;
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
	private GESTURE_TYPE gestureType = GESTURE_TYPE.WALKING;
	public float rightHorizontal;
	public float rightVertical;
	public float leftHorizontal;
	public float leftVertical;

	public JoystickParams joystickParams;
	public bool flipLeftRight = false;
	public bool flipSurfPitch = false;

	private TravelModelInterface travelModelInterface;
	// Use this for initialization
	void Start () {
		travelModelInterface = GetComponent<TravelModelInterface>();
		joystickParams = ConfigurationHandler.joystickParams;
	}
	
	// Update is called once per frame
	void Update () {
		//GetComponentInChildren<LocomotionAnimation> ().vel = moveVel;
		if(Input.GetKey("joystick button 0")) {
			travelModelInterface.SetTargetGestureType (GESTURE_TYPE.WALKING);
			travelModelInterface.SetGestureType (GESTURE_TYPE.WALKING);
			Debug.Log("Walking");
		}
		else if(Input.GetKey("joystick button 1")) {
			travelModelInterface.SetTargetGestureType (GESTURE_TYPE.SEGWAY);
			travelModelInterface.SetGestureType (GESTURE_TYPE.SEGWAY);
			Debug.Log("Segway");
		}
		else if(Input.GetKey("joystick button 2")) {
			travelModelInterface.SetTargetGestureType (GESTURE_TYPE.SURFING);
			travelModelInterface.SetGestureType (GESTURE_TYPE.SURFING);
			Debug.Log("Surfing");
		}
		else if(Input.GetKey("joystick button 6")) {
			flipLeftRight = ! flipLeftRight;
			Debug.Log("Flip left and right");
		}
		else if(Input.GetKey("joystick button 7")) {
			flipSurfPitch = ! flipSurfPitch;
			Debug.Log("Flip surfing pitch");
		}

		if (flipLeftRight) {
			rightHorizontal = Input.GetAxis ("Horizontal") * 1.0f;
			rightVertical = Input.GetAxis ("Vertical") * 1.0f;
			leftHorizontal = Input.GetAxis ("ZHorizontal") * 1.0f;
			leftVertical = Input.GetAxis ("ZVertical") * 1.0f;
		} else {
			rightHorizontal = Input.GetAxis ("ZHorizontal") * 1.0f;
			rightVertical = -Input.GetAxis ("ZVertical") * 1.0f;
			leftHorizontal = Input.GetAxis ("Horizontal") * 1.0f;
			leftVertical = -Input.GetAxis ("Vertical") * 1.0f;
		}
		//Vector3 moveVel = velQueue.GetAvgVelocity(velocity, gestureType);
		//controller.SendMessage("SetVelocity", moveVel);
		//this.GetComponent("HIVEFPSController").SendMessage("SetRotation", rotation);
		//controller.SendMessage("DoStep");
		//Debug.Log(moveVel.ToString());
		Vector3 moveVel = new Vector3();
		Vector3 rotVel = new Vector3 ();

		switch (travelModelInterface.GetGestureType ()) {
		case GESTURE_TYPE.WALKING:
			// right hand joystick control the translation, left hand joystick control rotation
			moveVel = new Vector3 (leftHorizontal, 0, leftVertical) * joystickParams.walkingSpeed;
			rotVel = new Vector3 (0, rightHorizontal, 0) * joystickParams.walkingAngularSpeed;
			this.GetComponent ("HIVEFPSController").SendMessage ("SetWalking");
			break;
		case GESTURE_TYPE.SEGWAY:
			moveVel = Vector3.forward * leftVertical * joystickParams.segwaySpeed;
			rotVel = new Vector3 (0, leftHorizontal, 0) * joystickParams.segwayAngularSpeed;
			this.GetComponent ("HIVEFPSController").SendMessage ("SetSegway");
			break;
		case GESTURE_TYPE.SURFING:
			moveVel = -Vector3.forward * rightVertical * joystickParams.surfingSpeed;

			if (flipSurfPitch) {
				leftVertical = - leftVertical;
			} 
			rotVel = new Vector3(leftVertical * joystickParams.surfingPitchSpeed, leftHorizontal * joystickParams.surfingYawSpeed, 0);
			this.GetComponent("HIVEFPSController").SendMessage("SetSurfing");
			break;
		}

		travelModelInterface.SetVelocity (moveVel, rotVel);
		GetComponentInChildren<LocomotionAnimation> ().vel = moveVel;
	}
}