using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tuio;

public class ForcePadParams {
	public float minFingerMove;
	public float minFingerSpeed;
	public float maxFingerSpeed;	// clamp
	public float walkingAngularSpeed;
	
	public float surfingSpeed;	// pay attention to the x or y axis related parameters, they will be influenced by screen resolution
	public float surfingPitchSpeed;
	public float surfingYawSpeed;
	
	public float segwaySpeed;
	public float segwayPressureAngularSpeed;
	public float segwayAngularSpeed;
	public float segwayOffsetSpeed;

	public ForcePadParams() {
		minFingerMove = 5.0f;
		minFingerSpeed = 2.0f;
		maxFingerSpeed = 10.0f;
		walkingAngularSpeed = 10f;
		
		surfingSpeed = 100.0f;	
		surfingPitchSpeed = 62.8f;
		surfingYawSpeed = 0.01f;
		
		segwaySpeed = 50.0f;
		segwayPressureAngularSpeed = 10.0f;
		segwayAngularSpeed = 0.01f;
		segwayOffsetSpeed = 80.0f;
	}
};


public class TouchPadGesture : MonoBehaviour {
	private Dictionary<int, Vector2> fingerPositions = new Dictionary<int, Vector2>();	// record the trails of each finger
	private Dictionary<int, Vector2> fingerStart = new Dictionary<int, Vector2>();	// record the trails of each finger
	private Dictionary<int, float> fingerForce = new Dictionary<int, float>();	// forces of each fingers, can be queried by touch id
	private Dictionary<int, Vector2> fingerVel = new Dictionary<int, Vector2>();	// velocity of each finger trail, which is used to smooth velocity
	private Dictionary<int, List<Tuio.Touch>> fingerTrails = new Dictionary<int, List<Tuio.Touch>>();
	private KeyValuePair<int, float> leftTurnForce = new KeyValuePair<int, float>();
	private KeyValuePair<int, float> rightTurnForce = new KeyValuePair<int, float>();

	private int curFingerNum = 0;	// number of fingers on track
	
	private float speedScale = 	0.3f;
	private int timer = 0;
	private int MAX_ITER = 2;

	// for 2d widgets
	private Vector2 widgetRatio;
	private Vector2 widgetSize;
	
	private Dictionary<int, GameObject> fingerTips = new Dictionary<int, GameObject>();
	private GameObject baseTip1 = null;
	private GameObject baseTip2 = null;
	private GameObject dashline = null;
	private TravelModelInterface travel_model_interface;
	
	//private Dictionary<int, GameObject> fingerTrailRender = new Dictionary<int, GameObject>();
	
	Rect touchPadRect;
	Rect leftRect;
	Rect rightRect;

	private Queue<int> fingerNumQueue;
	private int fingerNumQueueLength = 5;


	private Vector2[] segwayBasePosition = new Vector2[2];
	private float resolutionCompensateX = 0.0f;
	private float resolutionCompensateY = 0.0f;

	private Vector3 moveVel;
	private Vector3 rotVel;

	public ForcePadParams forcePadParams;


	// Use this for initialization
	void Start () {
		fingerNumQueue = new Queue<int>(fingerNumQueueLength);
		for(int i=0; i<fingerNumQueueLength; i++) {
			fingerNumQueue.Enqueue(0);
		}

		touchPadRect = new Rect(0, 0, Screen.width, Screen.height);
		leftRect = new Rect(0, 0, touchPadRect.width*0.2f, touchPadRect.height);
		rightRect = new Rect(touchPadRect.width*0.8f, 0, touchPadRect.width*0.2f, touchPadRect.height);

		resolutionCompensateX = 900.0f / Screen.width;
		resolutionCompensateY = 500.0f / Screen.height;

		// 2d touch pad UI gadget
		RectTransform rectTrans = GameObject.Find("Widget").GetComponent<RectTransform>();
		widgetSize = new Vector2(rectTrans.sizeDelta.x, rectTrans.sizeDelta.y);
		widgetRatio = new Vector2(widgetSize.x / Screen.width, widgetSize.y / Screen.height);
		travel_model_interface = GetComponent<TravelModelInterface>();
		forcePadParams = ConfigurationHandler.forcePadParams;
	}

	void InitTouches() {	// called every update in TrackingComponentBase, before HandleTouches and FinishTouches

	}
	
	void FinishTouches() {	// called every update in TrackingComponentBase, after HandleTouches
		if (fingerNumQueue != null) {
			fingerNumQueue.Dequeue ();
			fingerNumQueue.Enqueue (curFingerNum);
		
			GestureClassfier ();
		
			GestureExecute ();
			//Vector3 moveVel = velQueue.GetAvgVelocity(velocity, gestureType);
		
			//Debug.Log(moveVel.ToString());
			travel_model_interface.SetVelocity (moveVel, rotVel);
			GetComponentInChildren<LocomotionAnimation> ().vel = moveVel;
		
		}
	}
	
	
	
	// a step is from addTouch to removeTouch
	void HandleTouches(Tuio.Touch t) { // called every update in TrackingComponentBase, between InitTouches and FinishTouches
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
		
		if (leftRect.Contains (t.TouchPoint)) {
			leftTurnForce = new KeyValuePair<int, float> (t.TouchId, Mathf.Abs ((float)t.Properties.Force));
			Debug.Log ("add id=" + t.FingerId + ", turn left=" + ", value=" + leftTurnForce.Value);
		} else if (rightRect.Contains (t.TouchPoint)) {
			rightTurnForce = new KeyValuePair<int, float> (t.TouchId, Mathf.Abs ((float)t.Properties.Force));
			Debug.Log ("add id=" + t.FingerId + ", turn right" + ", value=" + rightTurnForce.Value);
		} else {
			addTrail (t);
			curFingerNum++;
		}
		//GameObject trail = Instantiate(Resources.Load("Prefabs/FingerTrail", typeof(GameObject))) as GameObject;
		//fingerTrailRender.Add(t.TouchId, trail);
		
		GameObject tip = Instantiate(Resources.Load("Prefabs/finger_tip", typeof(GameObject))) as GameObject;
		tip.transform.parent = GameObject.Find("Canvas").transform;
		RectTransform rectTrans = tip.GetComponent<RectTransform>();
		//rectTrans.anchoredPosition = new Vector2(0, 0);
		Vector3 pos = TransformToWidget(t.TouchPoint);
		pos.z = 1;
		rectTrans.localPosition = pos;
		rectTrans.localRotation = Quaternion.identity;
		rectTrans.localScale = new Vector3(1, 1, 1);
		//tip.transform.parent = GameObject.Find("Canvas").transform;
		fingerTips.Add(t.TouchId, tip);
		List<Tuio.Touch> trail = new List<Tuio.Touch> ();
		trail.Add(t);
		fingerTrails.Add(t.TouchId, trail);
	}
	
	// step end
	void removeTouch(Tuio.Touch t)
	{
		//Debug.Log("Remove Touch");
		if (t.TouchId == leftTurnForce.Key) {
			leftTurnForce = new KeyValuePair<int, float> (-1, 0.0f);
			Debug.Log ("remove id=" + t.FingerId + ", turn left");
		} else if (t.TouchId == rightTurnForce.Key) {
			rightTurnForce = new KeyValuePair<int, float> (-1, 0.0f);
		} else {
			if (travel_model_interface.GetGestureType() == TRAVEL_TYPE.WALKING) {
			}
			removeTrail (t);
			curFingerNum--;
		}
		Destroy(fingerTips[t.TouchId]);
		fingerTips.Remove(t.TouchId);
		//Destroy(fingerTrailRender[t.TouchId]);
		//fingerTrailRender.Remove(t.TouchId);
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
		else if(fingerPositions.ContainsKey(t.TouchId)) {
			Vector2 diff = t.TouchPoint - fingerPositions[t.TouchId];
			fingerVel[t.TouchId] = diff;
			fingerPositions[t.TouchId] = t.TouchPoint;
			fingerForce[t.TouchId] = (float) t.Properties.Force;
			fingerTrails[t.TouchId].Add(t);
			Debug.Log("id = " + t.TouchId + ", force = " + t.Properties.Force);
		}
		
		RectTransform rectTrans = fingerTips[t.TouchId].GetComponent<RectTransform>();
		rectTrans.anchoredPosition = new Vector2(0, 0);
		Vector3 pos = TransformToWidget(t.TouchPoint);//TransformToWidget(t.TouchPoint);
		//pos.y = Screen.height - pos.y;
		//pos.x = Screen.width - pos.x;
		rectTrans.position = pos;
		//rectTrans.localPosition = pos;
		
		RawImage img = fingerTips[t.TouchId].GetComponent<RawImage>();
		float force = Mathf.Clamp(((float)t.Properties.Force), 0, 1);
		img.color = new Color(1.0f, 0.9f * (1-force), 0.9f * (1-force));
		
		
	}

	Vector3 TransformToWidget(Vector2 pt) {
		Vector3 pos = new Vector3(pt.x * widgetRatio.x, (Screen.height - pt.y) * widgetRatio.y, 0);
		pos.x = Screen.width - widgetSize.x + pos.x;
		pos.y = Screen.height - widgetSize.y + pos.y;
		return pos;
	}

	void addTrail (Tuio.Touch t) 
	{
		fingerPositions.Add(t.TouchId, new Vector2(t.TouchPoint.x, t.TouchPoint.y));
		fingerForce.Add(t.TouchId, (float)t.Properties.Force);
		fingerVel.Add (t.TouchId, Vector2.zero);
		fingerStart.Add(t.TouchId, new Vector2(t.TouchPoint.x, t.TouchPoint.y));
	}
	
	void removeTrail(Tuio.Touch t)
	{
		fingerPositions.Remove(t.TouchId);
		fingerForce.Remove(t.TouchId);
		fingerVel.Remove(t.TouchId);
		fingerStart.Remove(t.TouchId);
		// remember to save the record here in future
		fingerTrails.Remove (t.TouchId);
	}
	
	void GestureClassfier() {
		int twoFingerFrames = 0;
		foreach(int num in fingerNumQueue) {
			if(num == 2)
				twoFingerFrames++;
		}
		TRAVEL_TYPE curGestureType = travel_model_interface.GetGestureType();
		if(leftTurnForce.Key >= 0 || rightTurnForce.Key >= 0) {
			curGestureType = TRAVEL_TYPE.WALKING;
		}
		else if(curFingerNum == 0) {
			curGestureType = TRAVEL_TYPE.NOTHING;
			//Debug.Log("Do Nothing");
		}
		else if(twoFingerFrames >= fingerNumQueueLength * 0.8) {
			Vector2 diff = new Vector2(0, 0);
			foreach(var key in fingerPositions.Keys) {
				diff = fingerPositions[key] - diff;
			}
			
			curGestureType = TRAVEL_TYPE.RESTING; 
			int i = 0;
			if(diff.x < 0)
				i = 1;	// make sure the left finger is 0, and right finger is 1
			if (Mathf.Abs(diff.y) / Mathf.Abs(diff.x) > Mathf.Tan(Mathf.PI * 0.4f) ) {	// add more constraints for triggring the first segway
				if(travel_model_interface.GetGestureType() != TRAVEL_TYPE.SURFING) {
					curGestureType = TRAVEL_TYPE.SURFING;
					Debug.Log("Skate Board");
				}
				else {
					curGestureType = TRAVEL_TYPE.SURFING;
					Debug.Log("Skate Board");
				}
			}
			else {
				if(Mathf.Abs(diff.x) > Mathf.Abs(diff.y) ) {
					if(travel_model_interface.GetGestureType() != TRAVEL_TYPE.SEGWAY) {
						if (Mathf.Abs(diff.y) / Mathf.Abs(diff.x) < Mathf.Tan(Mathf.PI * 0.1f) ) {	// add more constraints for triggring the first segway
							foreach(var key in fingerPositions.Keys) {
								segwayBasePosition[i] = fingerPositions[key];
								i = (i + 1) % 2;
							}
							curGestureType = TRAVEL_TYPE.SEGWAY;
							Debug.Log("Segway");
						}
						else {
							curGestureType = travel_model_interface.GetGestureType();
						}
					}
					else {
						curGestureType = TRAVEL_TYPE.SEGWAY;
						Debug.Log("Segway");
					}
				}
				else if(travel_model_interface.GetGestureType() == TRAVEL_TYPE.SEGWAY || travel_model_interface.GetGestureType() == TRAVEL_TYPE.SURFING){
					curGestureType = travel_model_interface.GetGestureType();
				}
			}
		}
		else if (curFingerNum > 0){
			curGestureType = TRAVEL_TYPE.WALKING;
		}
		else {
			// do nothing
			curGestureType = TRAVEL_TYPE.NOTHING;
		}
		travel_model_interface.SetTargetGestureType (curGestureType);
		travel_model_interface.SetGestureType (curGestureType);

	}

	void GestureExecute() {
		Vector2[] fingers = new Vector2[2];
		float[] forces = new float[2];
		int i=0;
		float forceDiff = 0.0f;
		float yawDiff = 0.0f;
		float speedDiff = 0.0f;
		float avgDisplacement = 0.0f;
		moveVel = new Vector3 (0, 0, 0);
		rotVel = new Vector3 (0, 0, 0);
		foreach (var pos in fingerPositions) {
			fingers[i] = pos.Value;
			forces[i] = fingerForce[pos.Key];
			i++;
		}
		switch(travel_model_interface.GetGestureType()) {
		case TRAVEL_TYPE.WALKING:
			if(leftTurnForce.Key > 0 && rightTurnForce.Key > 0) {
			}
			else if(leftTurnForce.Key >= 0) {
				rotVel = new Vector3(0, -forcePadParams.walkingAngularSpeed * leftTurnForce.Value, 0);
			}
			else if(rightTurnForce.Key >= 0) {
				rotVel = new Vector3(0, forcePadParams.walkingAngularSpeed * rightTurnForce.Value, 0);
			}
			Vector2 diff = Vector2.zero;
			foreach (var vel in fingerVel) {
				diff += vel.Value;
			}
			
			Vector3 move = new Vector3(-diff.x, 0.0f, diff.y);
			move.Normalize();
			if(diff.magnitude < forcePadParams.minFingerMove) {
				// tapping
				move = Vector3.zero;
			}
			else {	// Clamping
				if(diff.magnitude < forcePadParams.minFingerSpeed) {	
					move = move * forcePadParams.minFingerSpeed;
				}
				else if(diff.magnitude > forcePadParams.maxFingerSpeed) {
					move = move * forcePadParams.maxFingerSpeed;
				}
				else {
					move = move * diff.magnitude;//((diff.magnitude - minFingerSpeed) * speedScale + minCharSpeed);
				}
				moveVel = move;
				//Vector3 walkingRot = walkingRotVelQueue.GetAvgVelocity(new Vector3(walkingAngleSpeed * move.x, 0, 0), gestureType);
				//this.transform.Rotate(0.0f, walkingRot.x, 0.0f);
				//velocity = transform.TransformDirection(move.z * new Vector3(0, 0, 1));
			}
			this.GetComponent("HIVEFPSController").SendMessage("SetWalking");
			
			// set trail
			/*foreach(var obj in fingerTrailRender) {
				GameObject trail = obj.Value;
				Vector3 baseCorner = new Vector3(Screen.width - widgetSize.x, Screen.height - widgetSize.y, -1.0f);
				Vector3 curFinger = new Vector3(fingerPositions[obj.Key].x * widgetRatio.x, (Screen.height - fingerPositions[obj.Key].y) * widgetRatio.y, 0);
				Vector3 pastFinger = new Vector3(fingerStart[obj.Key].x * widgetRatio.x, (Screen.height - fingerStart[obj.Key].y) * widgetRatio.y, 0);
				trail.GetComponent<LineRenderer>().SetPosition(0, baseCorner + curFinger);
				trail.GetComponent<LineRenderer>().SetPosition(1, baseCorner + pastFinger);
			}	*/
			break;
		case TRAVEL_TYPE.SEGWAY:
			int left = 0;
			int right = 1;
			
			if(fingers[0].x > fingers[1].x) {
				left = 1;
				right = 0;
			}
			forceDiff = (forces[right] - forces[left]);// * SURFINGBaseAngularSpeed;
			forceDiff = forceDiff * forcePadParams.segwayPressureAngularSpeed;
			yawDiff = (fingers[right].y - fingers[left].y);// * SURFINGBaseYawSpeed;
			yawDiff = yawDiff * resolutionCompensateY * forcePadParams.segwayAngularSpeed;
			
			speedDiff = (forces[right] + forces[left]) / 2;// * SURFINGBaseYawSpeed;
			speedDiff = speedDiff * forcePadParams.segwaySpeed;
			float avgBase = (segwayBasePosition[0].y + segwayBasePosition[1].y) / 2;
			avgDisplacement = Mathf.Abs((fingers[right].y + fingers[left].y) / 2 - avgBase);
			avgDisplacement = avgDisplacement / (Screen.height * 0.5f);
			avgDisplacement = avgDisplacement * avgDisplacement * forcePadParams.segwayOffsetSpeed;
			
			Vector3 segwayMove = Vector3.forward * (speedDiff + avgDisplacement);
			string direction = "forward";
			if(fingers[right].y - segwayBasePosition[1].y + fingers[left].y - segwayBasePosition[0].y > 0) {
				segwayMove = -segwayMove;
				direction = "backward";
			}
			if (!travel_model_interface.IsPressureSegway()) {
				rotVel = new Vector3(0, yawDiff, 0);
			}
			else {
				rotVel = new Vector3(0, forceDiff, 0);
			}
			//velocity = transform.rotation * segwayMove;
			moveVel = segwayMove;
			Debug.Log("Segway Speed = " + speedDiff +", Yaw Speed = " + yawDiff + ", " + direction);
			this.GetComponent("HIVEFPSController").SendMessage("SetSegway");
			break;
		case TRAVEL_TYPE.SURFING:
			int front = 0;
			int back = 1;
			
			if(fingers[0].y > fingers[1].y) {
				front = 1;
				back = 0;
			}
			forceDiff = (forces[front] - forces[back]);// * SURFINGBaseAngularSpeed;
			forceDiff = forceDiff * forcePadParams.surfingPitchSpeed;
			yawDiff = (fingers[front].x - fingers[back].x);// * SURFINGBaseYawSpeed;
			yawDiff = yawDiff * resolutionCompensateX * forcePadParams.surfingYawSpeed;
			speedDiff = (fingers[back].y - fingers[front].y) / (float)Screen.height;// * SURFINGBaseYawSpeed;
			speedDiff = speedDiff * speedDiff * forcePadParams.surfingSpeed;  // parabolic increasing speed
			Vector3 boardMove = Vector3.forward * speedDiff;
			
			//rotationY += forceDiff;
			Debug.Log("Skate Board Angular Speed = " + forceDiff +", Yaw Speed = " + yawDiff);
			rotVel = new Vector3(forceDiff, yawDiff, 0);
			//transform.Rotate(Vector3.right, forceDiff - transform.eulerAngles.x);

			//velocity = transform.rotation * boardMove;

			moveVel = boardMove;
			this.GetComponent("HIVEFPSController").SendMessage("SetSurfing");
			break;
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
