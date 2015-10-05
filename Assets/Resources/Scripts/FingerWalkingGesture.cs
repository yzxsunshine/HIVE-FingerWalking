using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tuio;


public class FingerWalkingGesture : MonoBehaviour 
{	
	private int status = 0;
	private Dictionary<int, Vector2> fingerPositions = new Dictionary<int, Vector2>();	// record the trails of each finger
	private Dictionary<int, Vector2> fingerStart = new Dictionary<int, Vector2>();	// record the trails of each finger
	private Dictionary<int, float> fingerForce = new Dictionary<int, float>();	// forces of each fingers, can be queried by touch id
	private Dictionary<int, Vector2> fingerVel = new Dictionary<int, Vector2>();	// velocity of each finger trail, which is used to smooth velocity
	private Dictionary<int, List<Tuio.Touch>> fingerTrails = new Dictionary<int, List<Tuio.Touch>>();
	private KeyValuePair<int, float> leftTurnForce = new KeyValuePair<int, float>();
	private KeyValuePair<int, float> rightTurnForce = new KeyValuePair<int, float>();
	private float filterThresh = 0.7f;

	private int curFingerNum = 0;	// number of fingers on track

	private float speedScale = 	0.3f;
	private int timer = 0;
	private int MAX_ITER = 2;
	private StreamWriter sw;


	private Vector3 velocity;
	private Quaternion rotation;

	private VelocityQueue velQueue;
	private VelocityQueue surfingRotVelQueue;
	private VelocityQueue walkingRotVelQueue;

	private Queue<int> fingerNumQueue;
	private int fingerNumQueueLength;

	private float minCharSpeed = 20.0f;
	private float maxCharSpeed = 30.0f;
	private float minFingerMove = 5.0f;
	private float minFingerSpeed = 5.0f;
	private float maxFingerSpeed = 100.0f;	// clamp

	private float SURFINGBaseForwardSpeed = 500.0f;	// pay attention to the x or y axis related parameters, they will be influenced by screen resolution
	private float SURFINGBaseAngularSpeed = 62.8f;
	private float SURFINGBaseYawSpeed = 0.01f;

	private float segwayBaseForwardSpeed = 200.0f;
	private float segwayPressureBaseYawSpeed = 10.0f;
	private float segwayAngleBaseYawSpeed = 0.01f;
	private float segwayDispSpeed = 80.0f;

	private Vector2[] segwayBasePosition = new Vector2[2];
	private Vector2[] restingBasePosition = new Vector2[2];
	private float resolutionCompensateX = 0.0f;
	private float resolutionCompensateY = 0.0f;

	private bool pressureBasedSegwayOn = false;
	private float minMove = 0.02f;
	private float walkingAngleSpeed = 10f;


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
	GESTURE_TYPE gestureType;


	void Start() {
		//string path = @"calib.txt";
		//sw = File.CreateText(path);
		touchPadRect = new Rect(0, 0, Screen.width, Screen.height);
		leftRect = new Rect(0, 0, touchPadRect.width*0.2f, touchPadRect.height);
		rightRect = new Rect(touchPadRect.width*0.8f, 0, touchPadRect.width*0.2f, touchPadRect.height);
		velQueue = new VelocityQueue();
		velQueue.SetQueueSize(10);

		fingerNumQueueLength = 20;
		fingerNumQueue = new Queue<int>(fingerNumQueueLength);
		for(int i=0; i<fingerNumQueueLength; i++) {
			fingerNumQueue.Enqueue(0);
		}

		resolutionCompensateX = 900.0f / Screen.width;
		resolutionCompensateY = 500.0f / Screen.height;
		speedScale = 1.0f / (maxFingerSpeed - minFingerSpeed) * (maxCharSpeed - minCharSpeed);

		RectTransform rectTrans = GameObject.Find("Widget").GetComponent<RectTransform>();
		widgetSize = new Vector2(rectTrans.sizeDelta.x, rectTrans.sizeDelta.y);
		widgetRatio = new Vector2(widgetSize.x / Screen.width, widgetSize.y / Screen.height);

		leftTurnForce = new KeyValuePair<int, float>(-1, 0.0f);
		rightTurnForce = new KeyValuePair<int, float>(-1, 0.0f);

		travel_model_interface = GetComponent<TravelModelInterface>();
	}
	// Use this for initialization

	void InitTouches() {	// called every update in TrackingComponentBase, before HandleTouches and FinishTouches
		velocity = Vector3.zero;
		if (Input.GetKeyDown(KeyCode.LeftShift))
			pressureBasedSegwayOn = !pressureBasedSegwayOn;
	}
	
	void FinishTouches() {	// called every update in TrackingComponentBase, after HandleTouches
		fingerNumQueue.Dequeue();
		fingerNumQueue.Enqueue(curFingerNum);

		GestureClassfier();

		GestureExcute();
		Vector3 moveVel = velQueue.GetAvgVelocity(velocity, gestureType);

		//Debug.Log(moveVel.ToString());
		travel_model_interface.SetVelocity (moveVel, new Vector3(0, 0, 0));
		GetComponentInChildren<LocomotionAnimation> ().vel = moveVel;


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
			if (travel_model_interface.GetGestureType() == GESTURE_TYPE.WALKING) {
				status++;
				if (status > 2)
					status = 1;

				if (status == 1)
					Debug.Log ("left");
				if (status == 2)
					Debug.Log ("right");
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

	void OnGUI() {
		/*var iter = fingerPositions.GetEnumerator();
		GUI.Label(new Rect(10, 10, 100, 50), fingerPositions.Count.ToString());
		int i = 0;
		while(iter.MoveNext()) {
			GUI.Label(new Rect(10, 60 + i * 50, 100, 50), iter.Current.Value.ToString());
			i++;
		}*/
		if(!pressureBasedSegwayOn)
			GUI.Label(new Rect(10, 10, 100, 50), "Pressure Based Segway Off");
		else 
			GUI.Label(new Rect(10, 10, 100, 50), "Pressure Based Segway On");

		GUI.Label(new Rect(10, 60, 400, 30), "Max In(De)crease Walking Speed: " + travel_model_interface.GetController().maxWalkingVelocityIncrease.ToString() + ", " + travel_model_interface.GetController().maxWalkingVelocityDecrease.ToString());
		GUI.Label(new Rect(10, 100, 400, 30), "Max In(De)crease Segway Speed: " + travel_model_interface.GetController().maxSegwayVelocityIncrease.ToString() + ", " + travel_model_interface.GetController().maxSegwayVelocityDecrease.ToString());
		GUI.Label(new Rect(10, 140, 400, 30), "Max In(De)crease Surfing Speed: " + travel_model_interface.GetController().maxSurfVelocityIncrease.ToString() + ", " + travel_model_interface.GetController().maxSurfVelocityDecrease.ToString());
		GUI.Label(new Rect(10, 180, 400, 30), "Walking Angular Speed: " + walkingAngleSpeed.ToString());
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
		GESTURE_TYPE curGestureType = travel_model_interface.GetGestureType();
		if(leftTurnForce.Key > 0 || rightTurnForce.Key > 0) {
				curGestureType = GESTURE_TYPE.WALKING;
			}
		else if(curFingerNum == 0) {
				curGestureType = GESTURE_TYPE.NOTHING;
				Debug.Log("Do Nothing");
			}
		else if(twoFingerFrames >= fingerNumQueueLength * 0.8) {
			Vector2 diff = new Vector2(0, 0);
			foreach(var key in fingerPositions.Keys) {
				diff = fingerPositions[key] - diff;
			}

			curGestureType = GESTURE_TYPE.RESTING; 
			int i = 0;
			if(diff.x < 0)
				i = 1;	// make sure the left finger is 0, and right finger is 1
			if(travel_model_interface.GetGestureType() != GESTURE_TYPE.RESTING) {	// set resting pose
				foreach(var key in fingerPositions.Keys) {
					restingBasePosition[i] = fingerPositions[key];
					i = (i + 1) % 2;
				}
			}

			if (Mathf.Abs(diff.y) / Mathf.Abs(diff.x) > Mathf.Tan(Mathf.PI * 0.4f) ) {	// add more constraints for triggring the first segway
				if(travel_model_interface.GetGestureType() != GESTURE_TYPE.SURFING) {
					curGestureType = GESTURE_TYPE.SURFING;
					Debug.Log("Skate Board");
				}
				else {
					curGestureType = GESTURE_TYPE.SURFING;
					Debug.Log("Skate Board");
				}
			}
			else {
				float[] dist = {0.0f, 0.0f};
				foreach(var key in fingerPositions.Keys) {
					dist[i] += Mathf.Abs(restingBasePosition[i].x - fingerPositions[key].x);
					dist[i] += Mathf.Abs(restingBasePosition[i].y - fingerPositions[key].y);
					dist[i] = dist[i] / Screen.height;
					i = (i + 1) % 2;
				}

				if(dist[0] > minMove && dist[1] > minMove && Mathf.Abs(diff.x) > Mathf.Abs(diff.y) ) {
					if(travel_model_interface.GetGestureType() != GESTURE_TYPE.SEGWAY) {
						if (Mathf.Abs(diff.y) / Mathf.Abs(diff.x) < Mathf.Tan(Mathf.PI * 0.1f) ) {	// add more constraints for triggring the first segway
							foreach(var key in fingerPositions.Keys) {
								segwayBasePosition[i] = fingerPositions[key];
								i = (i + 1) % 2;
							}
							curGestureType = GESTURE_TYPE.SEGWAY;
							Debug.Log("Segway");
						}
						else {
							curGestureType = travel_model_interface.GetGestureType();
						}
					}
					else {
						curGestureType = GESTURE_TYPE.SEGWAY;
						Debug.Log("Segway");
					}
				}
				else if(travel_model_interface.GetGestureType() == GESTURE_TYPE.SEGWAY || travel_model_interface.GetGestureType() == GESTURE_TYPE.SURFING){
					curGestureType = travel_model_interface.GetGestureType();
				}
			}

		}
		else if (curFingerNum > 0){
			curGestureType = GESTURE_TYPE.WALKING;
		}
		else {
			// do nothing
			curGestureType = GESTURE_TYPE.NOTHING;
		}

		// start a new metaphor, the last one is SURFING, reset states
		if(travel_model_interface.GetGestureType() != curGestureType && travel_model_interface.GetGestureType() == GESTURE_TYPE.SURFING) {
			Vector3 forward = transform.forward;
			Vector3 up = new Vector3(0, 1.0f, 0);
			Vector3 xAxis = Vector3.Cross(transform.up, forward);
			transform.up = up;
			forward = Vector3.Cross(xAxis, up);
			transform.forward = forward;
		}
		// start a new metaphor, the last one is segway, destroy segway drawing
		else if(travel_model_interface.GetGestureType() != curGestureType && travel_model_interface.GetGestureType() == GESTURE_TYPE.SEGWAY) {	// last one is SEGWAY, remove baseline
			if(baseTip1 != null) {
				Destroy(baseTip1);
				baseTip1 = null;
			}

			if(baseTip2 != null) {
				Destroy(baseTip2);
				baseTip2 = null;
			}

			if(dashline != null) {
				Destroy(dashline);
				dashline = null;
			}
		}
		// start a new metaphor, the new one is segway, set initial position
		else if(gestureType != curGestureType && curGestureType == GESTURE_TYPE.SEGWAY) {
			baseTip1 = Instantiate(Resources.Load("Prefabs/base_finger_tip", typeof(GameObject))) as GameObject;
			RectTransform rectTrans1 = baseTip1.GetComponent<RectTransform>();
			rectTrans1.anchoredPosition = new Vector2(0, 0);
			rectTrans1.localPosition = TransformToWidget(segwayBasePosition[0]);
			
			baseTip1.transform.parent = GameObject.Find("Canvas").transform;
			
			baseTip2 = Instantiate(Resources.Load("Prefabs/base_finger_tip", typeof(GameObject))) as GameObject;
			RectTransform rectTrans2 = baseTip2.GetComponent<RectTransform>();
			rectTrans2.anchoredPosition = new Vector2(0, 0);
			rectTrans2.localPosition = TransformToWidget(segwayBasePosition[1]);
			
			baseTip2.transform.parent = GameObject.Find("Canvas").transform;
			
			dashline = Instantiate(Resources.Load("Prefabs/dashline", typeof(GameObject))) as GameObject;
			RectTransform rectTrans3 = dashline.GetComponent<RectTransform>();
			rectTrans3.anchoredPosition = new Vector2(0, 0);
			Vector2 baselineCenter = (segwayBasePosition[0]+segwayBasePosition[1])/2;
			//baselineCenter.x = Screen.width - widgetSize.x * 3/ 2;
			rectTrans3.localPosition = TransformToWidget(baselineCenter);
			
			dashline.transform.parent = GameObject.Find("Canvas").transform;
		}
		else if (gestureType != curGestureType && curGestureType == GESTURE_TYPE.SURFING) {
			surfingRotVelQueue = new VelocityQueue();
			surfingRotVelQueue.SetQueueSize(20);
		}
		else if (gestureType != curGestureType && curGestureType == GESTURE_TYPE.WALKING) {
			walkingRotVelQueue = new VelocityQueue();
			walkingRotVelQueue.SetQueueSize(20);
		}

		// start a new metaphor, the new one is walking, create the trail
		gestureType = curGestureType;
	}

	void GestureExcute() {
		Vector2[] fingers = new Vector2[2];
		float[] forces = new float[2];
		int i=0;
		float forceDiff = 0.0f;
		float yawDiff = 0.0f;
		float speedDiff = 0.0f;
		float avgDisplacement = 0.0f;
		foreach (var pos in fingerPositions) {
			fingers[i] = pos.Value;
			forces[i] = fingerForce[pos.Key];
			i++;
		}
		switch(gestureType) {
		case GESTURE_TYPE.WALKING:
			if(leftTurnForce.Key >= 0 && rightTurnForce.Key >= 0) {
			}
			else if(leftTurnForce.Key >= 0) {
				this.transform.Rotate(0.0f, -walkingAngleSpeed * leftTurnForce.Value, 0.0f);
			}
			else if(rightTurnForce.Key >= 0) {
				this.transform.Rotate(0.0f, walkingAngleSpeed * rightTurnForce.Value, 0.0f);
			}
			Vector2 diff = Vector2.zero;
			foreach (var vel in fingerVel) {
				diff += vel.Value;
			}

			Vector3 move = new Vector3(-diff.x, 0.0f, diff.y);
			move.Normalize();
			if(diff.magnitude < minFingerMove) {
				// tapping
				move = Vector3.zero;
				/*Vector2 newStep = new Vector2();
				Vector2 oldStep = new Vector2();
				float stepDist = 0.0f;
				bool hasStep = false;
				if (fingerTrails.Count > 1) {
					foreach (var trail in fingerTrails) {
						if(trail.Value.Count == 1) {	// new step
							newStep = trail.Value.ElementAt(0).TouchPoint;
							hasStep = true;
							break;
						}
					}
					if(hasStep) {
						foreach (var trail in fingerTrails) {
							int numElem = trail.Value.Count;
							Vector2 curStep = trail.Value.ElementAt(numElem - 1).TouchPoint;
							float curDist = Mathf.Abs(curStep.x - newStep.x) + Mathf.Abs(curStep.y - newStep.y);
							if(curDist > stepDist) {
								stepDist = curDist;
								oldStep = curStep;
							}
						}
						move = newStep - oldStep;
						move = new Vector3(move.x, 0.0f, -move.y);
						velocity = transform.TransformDirection(move);//Vector3.zero;
						//transform.Translate(move.x * 0.03f, 0.0f, -move.y*0.03f);
					}

				}*/
			}
			else {
				if(diff.magnitude < minFingerSpeed) {	
					move = move * minCharSpeed;
				}
				else if(diff.magnitude > maxFingerSpeed) {
					move = move * maxCharSpeed;
				}
				else {
					move = move * diff.magnitude;//((diff.magnitude - minFingerSpeed) * speedScale + minCharSpeed);
				}
				
				velocity = transform.TransformDirection(move);
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
		case GESTURE_TYPE.SEGWAY:
			int left = 0;
			int right = 1;
			
			if(fingers[0].x > fingers[1].x) {
				left = 1;
				right = 0;
			}
			forceDiff = (forces[right] - forces[left]);// * SURFINGBaseAngularSpeed;
			forceDiff = forceDiff * segwayPressureBaseYawSpeed;
			yawDiff = (fingers[right].y - fingers[left].y);// * SURFINGBaseYawSpeed;
			yawDiff = yawDiff * resolutionCompensateY * segwayAngleBaseYawSpeed;

			speedDiff = (forces[right] + forces[left]) / 2;// * SURFINGBaseYawSpeed;
			speedDiff = speedDiff * segwayBaseForwardSpeed;
			float avgBase = (segwayBasePosition[0].y + segwayBasePosition[1].y) / 2;
			avgDisplacement = Mathf.Abs((fingers[right].y + fingers[left].y) / 2 - avgBase);
			avgDisplacement = avgDisplacement / (Screen.height * 0.5f);
			avgDisplacement = avgDisplacement * avgDisplacement * segwayDispSpeed;

			Vector3 segwayMove = Vector3.forward * (speedDiff + avgDisplacement);
			string direction = "forward";
			if(fingers[right].y - segwayBasePosition[1].y + fingers[left].y - segwayBasePosition[0].y > 0) {
				segwayMove = -segwayMove;
				direction = "backward";
			}
			if(!pressureBasedSegwayOn)
				transform.Rotate(Vector3.up, yawDiff);
			else
				transform.Rotate(Vector3.up, forceDiff);
			//velocity = transform.rotation * segwayMove;
			velocity = transform.TransformDirection(segwayMove);
			Debug.Log("Segway Speed = " + speedDiff +", Yaw Speed = " + yawDiff + ", " + direction);
			this.GetComponent("HIVEFPSController").SendMessage("SetSegway");
			break;
		case GESTURE_TYPE.SURFING:
			int front = 0;
			int back = 1;

			if(fingers[0].y > fingers[1].y) {
				front = 1;
				back = 0;
			}
			forceDiff = (forces[front] - forces[back]);// * SURFINGBaseAngularSpeed;
			forceDiff = forceDiff * SURFINGBaseAngularSpeed;
			yawDiff = (fingers[front].x - fingers[back].x);// * SURFINGBaseYawSpeed;
			yawDiff = yawDiff * resolutionCompensateX * SURFINGBaseYawSpeed;
			speedDiff = (fingers[back].y - fingers[front].y) / (float)Screen.height;// * SURFINGBaseYawSpeed;
			speedDiff = speedDiff * speedDiff * SURFINGBaseForwardSpeed;  // parabolic increasing speed
			Vector3 boardMove = Vector3.forward * speedDiff;

			//rotationY += forceDiff;
			Debug.Log("Skate Board Angular Speed = " + forceDiff +", Yaw Speed = " + yawDiff);
			Vector3 avgVel = surfingRotVelQueue.GetAvgVelocity(new Vector3(forceDiff, yawDiff, 0), gestureType);
			//transform.Rotate(Vector3.right, forceDiff - transform.eulerAngles.x);
			transform.Rotate(Vector3.up, avgVel.y);
			//transform.localEulerAngles = new Vector3(forceDiff, transform.localEulerAngles.y, transform.localEulerAngles.z);
			//transform.localEulerAngles = new Vector3(forceDiff, transform.localEulerAngles.y, 0);
			transform.eulerAngles = new Vector3(avgVel.x, transform.eulerAngles.y, 0);
			//velocity = transform.rotation * boardMove;
			velocity = transform.TransformDirection(boardMove);
			this.GetComponent("HIVEFPSController").SendMessage("SetSurfing");
			break;
		}
		rotation = transform.rotation;
	}

	Vector3 TransformToWidget(Vector2 pt) {
		Vector3 pos = new Vector3(pt.x * widgetRatio.x, (Screen.height - pt.y) * widgetRatio.y, 0);
		pos.x = Screen.width - widgetSize.x + pos.x;
		pos.y = Screen.height - widgetSize.y + pos.y;
		return pos;
	}


}

