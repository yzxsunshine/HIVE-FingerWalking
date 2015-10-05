using UnityEngine;
using System.Collections;

public class JoystickGesture : MonoBehaviour {
	private GESTURE_TYPE gestureType = GESTURE_TYPE.WALKING;
	public float right_horizontal;
	public float right_vertical;
	public float left_horizontal;
	public float left_vertical;

	private float walking_speed = 10.0f;
	private float walking_angle_speed = 1.0f;
	private float segway_speed = 20.0f;
	private float segway_angle_speed = 1.0f;
	private float surfing_speed = 30.0f;
	private float surfing_pitch_speed = 20.0f;
	private float surfing_yaw_speed = 1.0f;
	private bool flip_left_right = false;
	private bool flip_surf_pitch = false;

	private TravelModelInterface travel_model_interface;
	// Use this for initialization
	void Start () {
		travel_model_interface = GetComponent<TravelModelInterface>();
	}
	
	// Update is called once per frame
	void Update () {
		//GetComponentInChildren<LocomotionAnimation> ().vel = moveVel;
		if(Input.GetKey("joystick button 0")) {
			travel_model_interface.SetTargetGestureType (GESTURE_TYPE.WALKING);
			travel_model_interface.SetGestureType (GESTURE_TYPE.WALKING);
			Debug.Log("Walking");
		}
		else if(Input.GetKey("joystick button 1")) {
			travel_model_interface.SetTargetGestureType (GESTURE_TYPE.SEGWAY);
			travel_model_interface.SetGestureType (GESTURE_TYPE.SEGWAY);
			Debug.Log("Segway");
		}
		else if(Input.GetKey("joystick button 2")) {
			travel_model_interface.SetTargetGestureType (GESTURE_TYPE.SURFING);
			travel_model_interface.SetGestureType (GESTURE_TYPE.SURFING);
			Debug.Log("Surfing");
		}
		else if(Input.GetKey("joystick button 6")) {
			flip_left_right = ! flip_left_right;
			Debug.Log("Flip left and right");
		}
		else if(Input.GetKey("joystick button 7")) {
			flip_surf_pitch = ! flip_surf_pitch;
			Debug.Log("Flip surfing pitch");
		}

		if (flip_left_right) {
			right_horizontal = Input.GetAxis ("Horizontal") * 1.0f;
			right_vertical = Input.GetAxis ("Vertical") * 1.0f;
			left_horizontal = Input.GetAxis ("ZHorizontal") * 1.0f;
			left_vertical = Input.GetAxis ("ZVertical") * 1.0f;
		} else {
			right_horizontal = Input.GetAxis ("ZHorizontal") * 1.0f;
			right_vertical = -Input.GetAxis ("ZVertical") * 1.0f;
			left_horizontal = Input.GetAxis ("Horizontal") * 1.0f;
			left_vertical = -Input.GetAxis ("Vertical") * 1.0f;
		}
		//Vector3 moveVel = velQueue.GetAvgVelocity(velocity, gestureType);
		//controller.SendMessage("SetVelocity", moveVel);
		//this.GetComponent("HIVEFPSController").SendMessage("SetRotation", rotation);
		//controller.SendMessage("DoStep");
		//Debug.Log(moveVel.ToString());
		Vector3 moveVel = new Vector3();
		Vector3 rotVel = new Vector3 ();

		switch (travel_model_interface.GetGestureType ()) {
		case GESTURE_TYPE.WALKING:
			// right hand joystick control the translation, left hand joystick control rotation
			moveVel = new Vector3 (left_horizontal, 0, left_vertical) * walking_speed;
			rotVel = new Vector3 (0, right_horizontal, 0) * walking_angle_speed;
			this.GetComponent ("HIVEFPSController").SendMessage ("SetWalking");
			break;
		case GESTURE_TYPE.SEGWAY:
			moveVel = Vector3.forward * left_vertical * segway_speed;
			rotVel = new Vector3 (0, left_horizontal, 0) * segway_angle_speed;
			this.GetComponent ("HIVEFPSController").SendMessage ("SetSegway");
			break;
		case GESTURE_TYPE.SURFING:
			moveVel = -Vector3.forward * right_vertical * surfing_speed;

			if (flip_surf_pitch) {
				left_vertical = - left_vertical;
			} 
			rotVel = new Vector3(left_vertical * surfing_pitch_speed, left_horizontal * surfing_yaw_speed, 0);
			this.GetComponent("HIVEFPSController").SendMessage("SetSurfing");
			break;
		}

		travel_model_interface.SetVelocity (moveVel, rotVel);
		GetComponentInChildren<LocomotionAnimation> ().vel = moveVel;
	}
}
