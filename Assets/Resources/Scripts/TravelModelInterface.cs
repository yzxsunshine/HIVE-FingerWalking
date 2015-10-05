using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GESTURE_TYPE {
	NOTHING,
	WALKING,
	SEGWAY,
	SURFING,
	RESTING
};

public class VelocityQueue {
	public Queue<Vector3> queue;
	public int size;
	public float[] weights;
	public float[] weights_surfing;

	public void SetQueueSize(int s) {
		size = s;
		weights = new float[size];
		weights_surfing = new float[size];
		queue = new Queue<Vector3>(size);
		float variant_ratio = 0.1f;
		for(int i=0; i<size; i++) {
			queue.Enqueue(Vector3.zero);
			weights[i] = Mathf.Pow(0.5f, size - i);
			weights_surfing[i] = weights[i] * variant_ratio + (1 - variant_ratio) / size;
		}		
		weights[size - 1] += Mathf.Pow(0.5f, size);
		weights_surfing[size - 1] += Mathf.Pow(0.5f, size) * variant_ratio;
	}
	
	public Vector3 GetAvgVelocity(Vector3 vel, GESTURE_TYPE type) {
		Vector3 outVel = Vector3.zero;
		queue.Dequeue();
		queue.Enqueue(vel);
		int i=0; 
		foreach(Vector3 v in queue) {
			if (type == GESTURE_TYPE.WALKING || type == GESTURE_TYPE.SEGWAY) {
				outVel += weights[i] * v;
			}
			else if (type == GESTURE_TYPE.SURFING) {
				outVel += weights_surfing[i] * v;
			}
			i++;
		}		
		return outVel;
	}
};


public class TravelModelInterface : MonoBehaviour  {
	private GESTURE_TYPE gestureType;
	private GESTURE_TYPE targetGestureType;
	private HIVEFPSController controller;

	public Vector3 velocity;
	public Vector3 rotation;
	
	private VelocityQueue velQueue;
	private VelocityQueue surfingRotVelQueue;
	private VelocityQueue walkingRotVelQueue;
	private VelocityQueue segwayRotVelQueue;
	private bool pressureBasedSegwayOn = false;
	// Use this for initialization
	void Start () {
		controller = GetComponent<HIVEFPSController>();
		gestureType = GESTURE_TYPE.WALKING;
		walkingRotVelQueue = new VelocityQueue();
		walkingRotVelQueue.SetQueueSize(20);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Z)) {
			//minFingerSpeed -= 10.0f;
			controller.maxWalkingVelocityIncrease -= 1.0f;
		}
		else if(Input.GetKeyDown(KeyCode.X)) {
			controller.maxWalkingVelocityIncrease += 1.0f;
		}
		else if(Input.GetKeyDown(KeyCode.A)) {
			controller.maxWalkingVelocityDecrease -= 0.2f;
		}
		else if(Input.GetKeyDown(KeyCode.S)) {
			controller.maxWalkingVelocityDecrease += 0.2f;
		}
		else if(Input.GetKeyDown(KeyCode.C)) {
			//minFingerSpeed -= 10.0f;
			controller.maxSegwayVelocityIncrease -= 1.0f;
		}
		else if(Input.GetKeyDown(KeyCode.V)) {
			controller.maxSegwayVelocityIncrease += 1.0f;
		}
		else if(Input.GetKeyDown(KeyCode.D)) {
			controller.maxSegwayVelocityDecrease -= 0.2f;
		}
		else if(Input.GetKeyDown(KeyCode.F)) {
			controller.maxSegwayVelocityDecrease += 0.2f;
		}
		else if(Input.GetKeyDown(KeyCode.B)) {
			//minFingerSpeed -= 10.0f;
			controller.maxSurfVelocityIncrease -= 1.0f;
		}
		else if(Input.GetKeyDown(KeyCode.N)) {
			controller.maxSurfVelocityIncrease += 1.0f;
		}
		else if(Input.GetKeyDown(KeyCode.G)) {
			controller.maxSurfVelocityDecrease -= 0.2f;
		}
		else if(Input.GetKeyDown(KeyCode.H)) {
			controller.maxSurfVelocityDecrease += 0.2f;
		}
		else if (Input.GetKeyDown(KeyCode.LeftShift))
			pressureBasedSegwayOn = !pressureBasedSegwayOn;
		//else if(Input.GetKeyDown(KeyCode.Q)) {
	//		walkingAngleSpeed -= 0.005f;
	//	}
	//	else if(Input.GetKeyDown(KeyCode.W)) {
	//		walkingAngleSpeed += 0.005f;
	//	}
	}

	public void SetVelocity(Vector3 moveVel, Vector3 rotVel) {
		Vector3 avgVel;
		switch (gestureType) {
		case GESTURE_TYPE.WALKING:
			avgVel = walkingRotVelQueue.GetAvgVelocity(rotVel, gestureType);
			transform.Rotate(avgVel);
			break;
		case GESTURE_TYPE.SEGWAY:
			avgVel = segwayRotVelQueue.GetAvgVelocity(rotVel, gestureType);
			if(!pressureBasedSegwayOn)
				transform.Rotate(Vector3.up, avgVel.y);
			else
				transform.Rotate(Vector3.up, avgVel.y);
			break;
		case GESTURE_TYPE.SURFING:
			avgVel = surfingRotVelQueue.GetAvgVelocity(rotVel, gestureType);
			transform.Rotate(Vector3.up, avgVel.y);
			transform.eulerAngles = new Vector3(avgVel.x, transform.eulerAngles.y, 0);
			break;
		}
	
		Vector3 transVel = transform.TransformDirection (moveVel);
		controller.SetVelocity(transVel);
		//this.GetComponent("HIVEFPSController").SendMessage("SetRotation", rotation);
		controller.SendMessage("DoStep");
		velocity = moveVel;
		rotation = rotVel;
	}

	public void SetGestureType (GESTURE_TYPE gesture) {
		if (gestureType != gesture) {

			// start a new metaphor, the last one is segway, destroy segway drawing
			/*else if(gestureType == GESTURE_TYPE.SEGWAY) {	// last one is SEGWAY, remove baseline
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
			else if(gesture == GESTURE_TYPE.SEGWAY) {
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
			}*/
			if (gestureType == GESTURE_TYPE.SURFING) {	// surfing finished, correct view
				Vector3 forward = transform.forward;
				Vector3 up = new Vector3(0, 1.0f, 0);
				Vector3 xAxis = Vector3.Cross(transform.up, forward);
				transform.up = up;
				forward = Vector3.Cross(xAxis, up);
				transform.forward = forward;
			}

			if (gesture == GESTURE_TYPE.SURFING) {
				surfingRotVelQueue = new VelocityQueue();
				surfingRotVelQueue.SetQueueSize(20);
			}
			else if (gesture == GESTURE_TYPE.SEGWAY) {
				segwayRotVelQueue = new VelocityQueue();
				segwayRotVelQueue.SetQueueSize(20);
			}
			else if (gesture == GESTURE_TYPE.WALKING) {
				walkingRotVelQueue = new VelocityQueue();
				walkingRotVelQueue.SetQueueSize(20);
			}
			gestureType = gesture;
		}
	}

	public GESTURE_TYPE GetGestureType () {
		return gestureType;
	}

	public void SetTargetGestureType (GESTURE_TYPE gesture) {
		targetGestureType = gesture;
	}
	
	public GESTURE_TYPE GetTargetGestureType () {
		return targetGestureType;
	}

	public HIVEFPSController GetController () {
		return controller;
	}

	public bool IsPressureSegway() {
		return pressureBasedSegwayOn;
	}
}
