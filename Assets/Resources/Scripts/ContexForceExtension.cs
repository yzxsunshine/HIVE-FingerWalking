using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tuio;

public enum FORCE_FILTER_TYPE {
	AVERAGE,
	EXPONENTIAL,
	GAUSSIAN
}

public class ForceFilter {
	public int QUEUE_SIZE;
	public FORCE_FILTER_TYPE type;
	public float[] weight;

	public ForceFilter(FORCE_FILTER_TYPE t, int size) {
		type = t;
		QUEUE_SIZE = size;
		weight = new float[QUEUE_SIZE];
		float sum = 0.0f;
		switch (type) {
		case FORCE_FILTER_TYPE.AVERAGE:
			for (int i = 0; i < QUEUE_SIZE; i++) {
				weight[i] = 1.0f / QUEUE_SIZE;
			}
			break;
		case FORCE_FILTER_TYPE.EXPONENTIAL:
			for (int i = 0; i < QUEUE_SIZE - 1; i++) {
				weight[i] = Mathf.Pow(0.5f, QUEUE_SIZE - i);
				sum += weight[i];
			}
			weight[QUEUE_SIZE - 1] = 1.0f - sum;
			break;
		case FORCE_FILTER_TYPE.GAUSSIAN:

			break;
		}
	}

	public float GetAverage(Queue<float> queue) {
		float avg = 0.0f;

		int i = 0;
		foreach (float val in queue) {
			avg += val * weight[i];
			i++;
		}

		return avg;
	}
}

public class ContextForceExtParam {
	public float triggerForce = 0.2f;
	public float forwardScale = 1.0f;
	public float rotScale = 1.0f;
	public float forwardSpeed = 0.2f;
	public float rotSpeed = 0.1f;
}

public class ContexForceExtension : MonoBehaviour {
	private Dictionary<int, Vector2> fingerPositions = new Dictionary<int, Vector2>();	// record the trails of each finger
	private Dictionary<int, Vector2> fingerStart = new Dictionary<int, Vector2>();	// record the trails of each finger
	private Dictionary<int, Queue<float>> fingerForce = new Dictionary<int, Queue<float>>();	// forces of each fingers, can be queried by touch id
	private Dictionary<int, Vector2> fingerVel = new Dictionary<int, Vector2>();
	private Queue<int> fingerNumQueue;
	private int fingerNumQueueLength = 2;
	private int curFingerNum = 0;	// number of fingers on track
	ContextForceExtParam contectForceParams = new ContextForceExtParam();
	Rect touchPadRect;
	private float maxContextScale = 100.0f;
	// for 2d widgets
	private Vector2 widgetRatio;
	private Vector2 widgetSize;
	private Vector2 widgetOffset;
	
	private float resolutionCompensateX = 0.0f;
	private float resolutionCompensateY = 0.0f;
	
	private Dictionary<int, GameObject> fingerTips = new Dictionary<int, GameObject>();
	private GameObject baseTip1 = null;
	private GameObject baseTip2 = null;
	private GameObject dashline = null;
	private TravelModelInterface travel_model_interface;
	private ForceFilter forceFilter;
	float timestamp = 0;
	private int currentFingerID = 0;
	private float lastAvgForce = 0.0f;
	private Vector2 lastBase = new Vector2(0, 0);
	private Vector2 contextVec = new Vector2(0, 0);
	private bool isContextForceExt = false;
	// Use this for i                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       nitialization
	void Start () {
		forceFilter = new ForceFilter (FORCE_FILTER_TYPE.EXPONENTIAL, 5);
		fingerNumQueue = new Queue<int>(fingerNumQueueLength);
		for(int i=0; i<fingerNumQueueLength; i++) {
			fingerNumQueue.Enqueue(0);
		}
		
		touchPadRect = new Rect(0, 0, Screen.width, Screen.height);

		resolutionCompensateX = 900.0f / Screen.width;
		resolutionCompensateY = 500.0f / Screen.height;

		RectTransform rectTrans = GameObject.Find("Widget").GetComponent<RectTransform>();
		widgetSize = new Vector2(rectTrans.sizeDelta.x, rectTrans.sizeDelta.y) * 0.9f;
		widgetOffset = new Vector2(rectTrans.sizeDelta.x, rectTrans.sizeDelta.y) * 0.05f;
		widgetRatio = new Vector2(widgetSize.x / Screen.width, widgetSize.y / Screen.height);
		travel_model_interface = GetComponent<TravelModelInterface>();
	}

	void OnGUI() {
		GUI.Label(new Rect(10, 10, 300, 20), "Context Vector: " + contextVec.ToString());
	}
	// Update is called once per frame
	void Update () {
	
	}

	void InitTouches() {	// called every update in TrackingComponentBase, before HandleTouches and FinishTouches
		
	}
	
	void FinishTouches() {	// called every update in TrackingComponentBase, after HandleTouches
		if (this.enabled == false) 
			return;
		if (fingerNumQueue != null) {
			fingerNumQueue.Dequeue ();
			fingerNumQueue.Enqueue (curFingerNum);
		}
		if (curFingerNum == 1 && fingerForce.Count > 0) {
			float avgForce = forceFilter.GetAverage (fingerForce [currentFingerID]);
			if (lastAvgForce >= contectForceParams.triggerForce 
			    && avgForce < contectForceParams.triggerForce) {
				isContextForceExt = false;

			} else if (lastAvgForce < contectForceParams.triggerForce 
			           && avgForce >= contectForceParams.triggerForce) {
				lastBase = fingerPositions [currentFingerID];
				isContextForceExt = true;
			}
			lastAvgForce = avgForce;
			Vector3 moveVel = new Vector3(0, 0, 0);
			Vector3 rotVel = new Vector3(0, 0, 0);
			if(isContextForceExt) {
				contextVec = fingerPositions [currentFingerID] - fingerStart[currentFingerID];//lastBase;
				moveVel = Vector3.forward * contextVec.y * contectForceParams.forwardSpeed;
				rotVel.y = DataSmooth.SmoothQuadratic(contextVec.x / maxContextScale) * maxContextScale * contectForceParams.rotSpeed;
			}
			else {
				moveVel = Vector3.forward * fingerVel[currentFingerID].y * contectForceParams.forwardScale;
				rotVel.y = DataSmooth.SmoothQuadratic(fingerVel[currentFingerID].x / maxContextScale) * maxContextScale * contectForceParams.rotScale;
			}
			Debug.Log("rotVel: " + rotVel.ToString());
			travel_model_interface.SetVelocity(-moveVel, rotVel);
		} else {
			travel_model_interface.SetVelocity(Vector3.zero, Vector3.zero);
		}

	}
	
	
	
	// a step is from addTouch to removeTouch
	void HandleTouches(Tuio.Touch t) { // called every update in TrackingComponentBase, between InitTouches and FinishTouches
		if (this.enabled == false) 
			return;
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
		float force = Mathf.Clamp(Mathf.Abs ((float)t.Properties.Force), 0, 1);
		//Debug.Log("Add Touch");
		addTrail (t);
		curFingerNum++;
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
		//tip.transform.parent = GameObject.Find("Canvas").transform;

	}

	// step end
	void removeTouch(Tuio.Touch t)
	{
		//Debug.Log("Remove Touch");

		removeTrail (t);
		curFingerNum--;
		if(curFingerNum < 0)
			curFingerNum = 0;
		Debug.Log("Remove Touch id=" + t.FingerId + ", remain figers: " + curFingerNum);
		Destroy(fingerTips[t.TouchId]);
		fingerTips.Remove(t.TouchId);
		contextVec.x = 0;
		contextVec.y = 0;
	}

	void updateTouch(Tuio.Touch t)
	{float force = Mathf.Clamp(Mathf.Abs ((float)t.Properties.Force), 0, 1);
		if(fingerPositions.ContainsKey(t.TouchId)) {
			Vector2 diff = t.TouchPoint - fingerPositions[t.TouchId];
			fingerVel[t.TouchId] = diff;
			fingerPositions[t.TouchId] = t.TouchPoint;
			fingerForce[t.TouchId].Dequeue();
			fingerForce[t.TouchId].Enqueue(force);
		}
		currentFingerID = t.TouchId;
		RectTransform rectTrans = fingerTips[t.TouchId].GetComponent<RectTransform>();
		rectTrans.anchoredPosition = new Vector2(0, 0);
		Vector3 pos = TransformToWidget(t.TouchPoint);//TransformToWidget(t.TouchPoint);
		//pos.y = Screen.height - pos.y;
		//pos.x = Screen.width - pos.x;
		rectTrans.position = pos;
		//rectTrans.localPosition = pos;
		
		RawImage img = fingerTips[t.TouchId].GetComponent<RawImage>();
		img.color = new Color(1.0f, 0.9f * (1-force), 0.9f * (1-force));
		
		Debug.Log("timestamp = " + (Time.time - timestamp) + ", id = " + t.TouchId + ", force = " + t.Properties.Force);
		timestamp = Time.time;
	}
	
	Vector3 TransformToWidget(Vector2 pt) {
		Vector3 pos = new Vector3(-widgetOffset.x + pt.x * widgetRatio.x, -widgetOffset.y + (Screen.height - pt.y) * widgetRatio.y, 0);
		pos.x = Screen.width - widgetSize.x + pos.x;
		pos.y = Screen.height - widgetSize.y + pos.y;
		return pos;
	}
	
	void addTrail (Tuio.Touch t) 
	{
		float force = Mathf.Clamp(Mathf.Abs ((float)t.Properties.Force), 0, 1);
		fingerPositions.Add(t.TouchId, new Vector2(t.TouchPoint.x, t.TouchPoint.y));
		Queue<float> forceQueue = new Queue<float> (forceFilter.QUEUE_SIZE);
		for (int i = 0; i < forceFilter.QUEUE_SIZE-1; i++){
			forceQueue.Enqueue(0);
		}
		forceQueue.Enqueue (force);
		fingerForce.Add(t.TouchId, forceQueue);
		fingerVel.Add (t.TouchId, Vector2.zero);
		fingerStart.Add(t.TouchId, new Vector2(t.TouchPoint.x, t.TouchPoint.y));
	}
	
	void removeTrail(Tuio.Touch t)
	{
		fingerPositions.Remove(t.TouchId);
		fingerForce.Remove(t.TouchId);
		fingerStart.Remove(t.TouchId);
		fingerVel.Remove(t.TouchId);
		// remember to save the record here in future
	}
}

