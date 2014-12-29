using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tuio;

public enum GESTURE_TYPE {
	WALKING,
	SEGWAY,
	SKATEBOARD
};


public class FingerWalkingGesture : MonoBehaviour 
{	
	private int status = 0;
	private Dictionary<int, Vector2> fingerTrails = new Dictionary<int, Vector2>();	// remember the trails of each finger
	private Dictionary<int, float> fingerForce = new Dictionary<int, float>();	// forces of each fingers, can be queried by touch id
	private Dictionary<int, Vector2> fingerVel = new Dictionary<int, Vector2>();	// velocity of each finger trail, which is used to smooth velocity

	private KeyValuePair<int, float> leftTurnForce = new KeyValuePair<int, float>();
	private KeyValuePair<int, float> rightTurnForce = new KeyValuePair<int, float>();

	private float filterThresh = 0.7f;

	private int curFingerNum = 0;	// number of fingers on track

	private float speedScale = 	0.3f;
	private int timer = 0;
	private int MAX_ITER = 2;
	private StreamWriter sw;

	private GESTURE_TYPE gestureType = GESTURE_TYPE.WALKING;

	private Vector3 velocity;
	private Queue<Vector3> velocityQueue;
	private int queueLength;
	private float[] queueWeights;

	private Queue<int> fingerNumQueue;
	private int fingerNumQueueLength;

	private float minCharSpeed = 20.0f;
	private float maxCharSpeed = 30.0f;
	private float minFingerMove = 5.0f;
	private float minFingerSpeed = 20.0f;
	private float maxFingerSpeed = 200.0f;	// clamp

	private float skateboardBaseForwardSpeed = 0.5f;	// pay attention to the x or y axis related parameters, they will be influenced by screen resolution
	private float skateboardBaseAngularSpeed = 62.8f;
	private float skateboardBaseYawSpeed = 0.01f;

	private float segwayBaseForwardSpeed = 50.0f;
	private float segwayPressureBaseYawSpeed = 10.0f;
	private float segwayAngleBaseYawSpeed = 0.01f;

	private Vector2[] segwayBasePosition = new Vector2[2];
	private float resolutionCompensateX = 0.0f;
	private float resolutionCompensateY = 0.0f;

	private bool pressureBasedSegwayOn = false;

	Rect touchPadRect;
	Rect leftRect;
	Rect rightRect;



	void Start() {
		string path = @"calib.txt";
		sw = File.CreateText(path);
		touchPadRect = new Rect(0, 0, Screen.width, Screen.height);
		leftRect = new Rect(0, 0, touchPadRect.width*0.2f, touchPadRect.height);
		rightRect = new Rect(touchPadRect.width*0.8f, 0, touchPadRect.width*0.2f, touchPadRect.height);
		leftTurnForce = new KeyValuePair<int, float>(-1, 0.0f);
		rightTurnForce = new KeyValuePair<int, float>(-1, 0.0f);
		queueLength = 5;
		queueWeights = new float[queueLength];
		velocityQueue = new Queue<Vector3>(queueLength);
		for(int i=0; i<queueLength; i++) {
			velocityQueue.Enqueue(Vector3.zero);
			queueWeights[i] = Mathf.Pow(0.5f, queueLength - i);
		}		
		queueWeights[queueLength - 1] += Mathf.Pow(0.5f, queueLength);

		fingerNumQueueLength = 10;
		fingerNumQueue = new Queue<int>(fingerNumQueueLength);
		for(int i=0; i<fingerNumQueueLength; i++) {
			fingerNumQueue.Enqueue(0);
		}

		resolutionCompensateX = 900.0f / Screen.width;
		resolutionCompensateY = 500.0f / Screen.height;
		speedScale = 1.0f / (maxFingerSpeed - minFingerSpeed) * (maxCharSpeed - minCharSpeed);
	}
	// Use this for initialization

	void InitTouches() {
		velocity = Vector3.zero;
		if (Input.GetKeyDown(KeyCode.LeftShift))
			pressureBasedSegwayOn = !pressureBasedSegwayOn;
	}
	
	void FinishTouches() {

		fingerNumQueue.Dequeue();
		fingerNumQueue.Enqueue(curFingerNum);

		GestureClassfier();

		GestureExcute();
		Vector3 moveVel = GetAvgVelocity(velocity);
		this.GetComponent("HIVEFPSController").SendMessage("SetVelocity", moveVel);
		this.GetComponent("HIVEFPSController").SendMessage("DoStep");
		//Debug.Log(moveVel.ToString());
	}

	Vector3 GetAvgVelocity(Vector3 vel) {
		Vector3 outVel = Vector3.zero;
		velocityQueue.Dequeue();
		velocityQueue.Enqueue(velocity);
		int i=0; 
		foreach(Vector3 v in velocityQueue) {
			outVel += queueWeights[i] * v;
			i++;
		}		
		return outVel;
	}

	// a step is from addTouch to removeTouch
	void HandleTouches(Tuio.Touch t)
	{
		switch (t.Status)
		{
		case TouchStatus.Began:
			addTouch(t);
			break;
		case TouchStatus.Ended:
			removeTouch(t);
			break;
		case TouchStatus.Moved:
			updateTouch(t);
			break;
		case TouchStatus.Stationary:
			updateTouch(t);
			break;
		default:
			break;
		}
	}

	// step start
	void addTouch(Tuio.Touch t)
	{
		//Debug.Log("Add Touch");
		if(leftRect.Contains(t.TouchPoint)) {
			leftTurnForce = new KeyValuePair<int, float>(t.TouchId, Mathf.Abs((float) t.Properties.Force));
			Debug.Log("add id=" + t.FingerId + ", turn left=" + ", value=" + leftTurnForce.Value);
		}
		else if(rightRect.Contains(t.TouchPoint)) {
			rightTurnForce = new KeyValuePair<int, float>(t.TouchId, Mathf.Abs((float) t.Properties.Force));
			Debug.Log("add id=" + t.FingerId + ", turn right" + ", value=" + rightTurnForce.Value);
		}
		else {
			addTrail(t);
			curFingerNum++;
		}
	}

	// step end
	void removeTouch(Tuio.Touch t)
	{
		//Debug.Log("Remove Touch");
		if(t.TouchId == leftTurnForce.Key) {
			leftTurnForce = new KeyValuePair<int, float>(-1, 0.0f);
			Debug.Log("remove id=" + t.FingerId + ", turn left");
		}
		else if(t.TouchId == rightTurnForce.Key) {
			rightTurnForce = new KeyValuePair<int, float>(-1, 0.0f);
		}
		else {
			status++;
			if(status > 2)
				status = 1;

			if(status == 1)
				Debug.Log("left");
			if(status == 2)
				Debug.Log("right");
			removeTrail(t);
			curFingerNum--;

		}
	}
	
	void updateTouch(Tuio.Touch t)
	{
		// the range of touch pad is {x, y} 0 <= x <= 900 ; 0 <= y <= 500
		/**
		 * 		UIST 2012  logo Synaptics
		 *        0, 0  -------   900, 0
		 *		   |				|
		 *		   |				|
		 *		   |				|
		 *		  0, 500 -------  900, 500
		 *		Student Innovation Contest
		 */		
		if(t.TouchId == leftTurnForce.Key) {
			leftTurnForce = new KeyValuePair<int, float>(t.TouchId, Mathf.Abs((float) t.Properties.Force));
		}
		else if(t.TouchId == rightTurnForce.Key) {
			rightTurnForce = new KeyValuePair<int, float>(t.TouchId, Mathf.Abs((float) t.Properties.Force));
		}
		else if(fingerTrails.ContainsKey(t.TouchId)) {
			Vector2 diff = t.TouchPoint - fingerTrails[t.TouchId];
			fingerVel[t.TouchId] = diff;
			fingerTrails[t.TouchId] = t.TouchPoint;
			fingerForce[t.TouchId] = (float) t.Properties.Force;
			Debug.Log("id = " + t.TouchId + ", force = " + t.Properties.Force);
		}
	}

	void OnGUI() {
		/*var iter = fingerTrails.GetEnumerator();
		GUI.Label(new Rect(10, 10, 100, 50), fingerTrails.Count.ToString());
		int i = 0;
		while(iter.MoveNext()) {
			GUI.Label(new Rect(10, 60 + i * 50, 100, 50), iter.Current.Value.ToString());
			i++;
		}*/
		if(!pressureBasedSegwayOn)
			GUI.Label(new Rect(10, 10, 100, 50), "Pressure Based Segway Off");
		else 
			GUI.Label(new Rect(10, 10, 100, 50), "Pressure Based Segway On");
	}

	void addTrail (Tuio.Touch t) 
	{
		fingerTrails.Add(t.TouchId, new Vector2(t.TouchPoint.x, t.TouchPoint.y));
		fingerForce.Add(t.TouchId, (float)t.Properties.Force);
		fingerVel.Add (t.TouchId, Vector2.zero);
	}

	void removeTrail(Tuio.Touch t)
	{
		fingerTrails.Remove(t.TouchId);
		fingerForce.Remove(t.TouchId);
		fingerVel.Remove(t.TouchId);
	}

	void GestureClassfier() {
		int twoFingerFrames = 0;
		foreach(int num in fingerNumQueue) {
			if(num == 2)
				twoFingerFrames++;
		}

		if(leftTurnForce.Key > 0 || rightTurnForce.Key > 0) {
			gestureType = GESTURE_TYPE.WALKING;
		}
		else if(twoFingerFrames >= fingerNumQueueLength * 0.8) {
			Vector2 diff = new Vector2(0, 0);
			foreach(var key in fingerTrails.Keys) {
				diff = fingerTrails[key] - diff;
			}
			if(Mathf.Abs(diff.x) > Mathf.Abs(diff.y)) {
				if(gestureType != GESTURE_TYPE.SEGWAY) {
					int i = 0;
					if(diff.x < 0)
						i = 1;	// make sure the left finger is 0, and right finger is 1
					foreach(var key in fingerTrails.Keys) {
						segwayBasePosition[i] = fingerTrails[key];
						i = (i + 1) % 2;
					}
				}
				gestureType = GESTURE_TYPE.SEGWAY;
				Debug.Log("Segway");
			}
			else{
				gestureType = GESTURE_TYPE.SKATEBOARD;
				Debug.Log("Skate Board");
			}
		}
		else if (curFingerNum > 0){
			gestureType = GESTURE_TYPE.WALKING;
		}
		else {
			// do nothing
		}
	}

	void GestureExcute() {
		Vector2[] fingers = new Vector2[2];
		float[] forces = new float[2];
		int i=0;
		float forceDiff = 0.0f;
		float yawDiff = 0.0f;
		float speedDiff = 0.0f;
		foreach (var pos in fingerTrails) {
			fingers[i] = pos.Value;
			forces[i] = fingerForce[pos.Key];
			i++;
		}
		switch(gestureType) {
		case GESTURE_TYPE.WALKING:
			if(leftTurnForce.Key >= 0 && rightTurnForce.Key >= 0) {

			}
			else if(leftTurnForce.Key >= 0) {
				this.transform.Rotate(0.0f, -5.0f * leftTurnForce.Value, 0.0f);
			}
			else if(rightTurnForce.Key >= 0) {
				this.transform.Rotate(0.0f, 5.0f * rightTurnForce.Value, 0.0f);
			}

			Vector2 diff = Vector2.zero;
			foreach (var vel in fingerVel) {
				diff += vel.Value;
			}

			Vector3 move = new Vector3(-diff.x, 0.0f, diff.y);
			move.Normalize();
			if(diff.magnitude < minFingerMove) {
				move = Vector3.zero;
			}
			else if(diff.magnitude < minFingerSpeed) {
				
				move = move * minCharSpeed;
			}
			else if(diff.magnitude > maxFingerSpeed) {
				move = move * maxCharSpeed;
			}
			else {
				move = move * ((diff.magnitude - minFingerSpeed) * speedScale + minCharSpeed);
			}
			
			//move.Normalize();
			//move = move * speedScale;
			Vector3 moveDirection = transform.rotation * move;
			velocity = moveDirection;
			this.GetComponent("HIVEFPSController").SendMessage("SetWalking");
			break;
		case GESTURE_TYPE.SEGWAY:
			int left = 0;
			int right = 1;
			
			if(fingers[0].x > fingers[1].x) {
				left = 1;
				right = 0;
			}
			forceDiff = (forces[right] - forces[left]);// * skateboardBaseAngularSpeed;
			forceDiff = forceDiff * segwayPressureBaseYawSpeed;
			yawDiff = (fingers[right].y - fingers[left].y);// * skateboardBaseYawSpeed;
			yawDiff = yawDiff * resolutionCompensateY * segwayAngleBaseYawSpeed;
			speedDiff = (forces[right] + forces[left]) / 2;// * skateboardBaseYawSpeed;
			speedDiff = speedDiff * segwayBaseForwardSpeed;

			Vector3 segwayMove = Vector3.forward * speedDiff;
			string direction = "forward";
			if(fingers[right].y - segwayBasePosition[1].y + fingers[left].y - segwayBasePosition[0].y > 0) {
				segwayMove = -segwayMove;
				direction = "backward";
			}
			if(!pressureBasedSegwayOn)
				transform.Rotate(Vector3.up, yawDiff);
			else
				transform.Rotate(Vector3.up, forceDiff);
			velocity = transform.rotation * segwayMove;
			Debug.Log("Segway Speed = " + speedDiff +", Yaw Speed = " + yawDiff + ", " + direction);
			this.GetComponent("HIVEFPSController").SendMessage("SetSegway");
			break;
		case GESTURE_TYPE.SKATEBOARD:
			int front = 0;
			int back = 1;

			if(fingers[0].y > fingers[1].y) {
				front = 1;
				back = 0;
			}
			forceDiff = (forces[front] - forces[back]);// * skateboardBaseAngularSpeed;
			forceDiff = forceDiff * skateboardBaseAngularSpeed;
			yawDiff = (fingers[front].x - fingers[back].x);// * skateboardBaseYawSpeed;
			yawDiff = yawDiff * resolutionCompensateX * skateboardBaseYawSpeed;
			speedDiff = (fingers[back].y - fingers[front].y);// * skateboardBaseYawSpeed;
			speedDiff = speedDiff * resolutionCompensateY * skateboardBaseForwardSpeed;
			Vector3 boardMove = Vector3.forward * speedDiff;

			//rotationY += forceDiff;
			Debug.Log("Skate Board Angular Speed = " + forceDiff +", Yaw Speed = " + yawDiff);

			//transform.Rotate(Vector3.right, forceDiff - transform.eulerAngles.x);
			transform.Rotate(Vector3.up, yawDiff);
			transform.localEulerAngles = new Vector3(forceDiff, transform.localEulerAngles.y, 0);
			velocity = transform.rotation * boardMove;
			this.GetComponent("HIVEFPSController").SendMessage("SetSurfing");
			break;
		}

	}
}

