using UnityEngine;
using System.Collections;

public class SurfingPathBell {
	/*
	*  \frac{1}{1+|\frac{x-c}{a}|^{2b}}
	*
	**/
	private Vector3 startPt;
	private Vector3 endPt;
	private Vector3 center;
	private Vector3 direction;
	private float distance;
	private float factorA;
	private float factorB;
	private float maxHeight;
	public SurfingPathBell(Vector3 sp, Vector3 ep, float a, float b, float height) {
		startPt = sp;
		endPt = ep;
		center = (sp + ep) / 2;
		direction = endPt - startPt;
		distance = direction.magnitude;
		direction.Normalize();
		factorA = a;
		maxHeight = height;
		factorB = b;
	}

	public float GetYValue(float p) {
		return maxHeight / (1.0f + Mathf.Pow((p - 0.5f) / factorA, 2 * factorB));
	}

	public Vector3 GetPoint(float p) {
		Vector3 point = startPt + p * distance * direction; 
		point.y = GetYValue(p);
		return point;
	}

	public Vector3 GetPoint (Vector3 pt) {
		float passedDistance = Vector3.Dot(pt - startPt, direction);
		return GetPoint(passedDistance / distance);
	}
};

public class SurfingTrialControl : MonoBehaviour {
	private float tangentAngle = 20 * Mathf.PI / 180;
	private int wayPointNum = 20;
	private GameObject[] wayPoints = null;
	private Vector3 startPt;
	private Vector3 endPt;
	private GameObject character;
	private PlayerStatus playerStatus;
	private StudyRecorder recorder;
	private ArrowControl arrowControl;
	private float timeStamp;
	private TrialControl trialControl;
	private float yawPeriod = 1.0f;
	private float pitchPeriod = 1.0f;
	private float yawAmplification = 1.0f;
	private float pitchAmplification = 1.0f;
	private SurfingPathBell surfingPath = null;
	private float averageDistance = -1;
	private float sumDistance = 0;
	private int frameNum = 0;
	// Use this for initialization
	void Awake () {
		character = GameObject.Find("Character");
		playerStatus = character.GetComponent<PlayerStatus> ();
		recorder = GameObject.Find ("StudyRecorder").GetComponent<StudyRecorder> ();
		arrowControl = GameObject.Find ("arrow").GetComponent<ArrowControl> ();
	}

	void Start() {
		trialControl = character.GetComponent<TrialControl>();
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (wayPoints != null) {
			Vector3 currentPos = character.transform.position;
			float totalDistance = (endPt - startPt).magnitude;
			Vector3 targetDirection = (endPt - startPt);
			targetDirection.Normalize();
			float passedDistance = Vector3.Dot(currentPos - startPt, targetDirection);
			int passedSphereNum = Mathf.Max(Mathf.FloorToInt(passedDistance / totalDistance * wayPointNum), 0);

			Vector3 closestPt = GetClosetPointOnPath(currentPos);
			sumDistance += (currentPos - closestPt).magnitude;
			frameNum++;

			for (int i = 0; i < passedSphereNum; i++) {
				//wayPoints[i].GetComponent<Renderer>().enabled = false;
				GameObject.Destroy(wayPoints[i]);
			}
			for (int i = passedSphereNum; i < wayPointNum; i++) {
				if(wayPoints[i] != null) {
					float alpha = 1.0f * (wayPointNum - i + passedSphereNum - 1) / wayPointNum;
					Color color = wayPoints[i].GetComponent<Renderer>().material.color;
					color.a = 0.8f * alpha * alpha * alpha;
					wayPoints[i].GetComponent<Renderer>().material.color = color;
				}
			}
			//playerStatus.TrigerWayPoint();
			playerStatus.SetWayPoint(GetClosetPointOnPath(currentPos));

			string line = playerStatus.GetCurrentTransformLine();
			recorder.RecordLine(line);
			if (passedSphereNum >= wayPointNum) {
				CompleteSurfingTrial();
			}
			timeStamp += Time.deltaTime;
		}
	}

	public void CompleteSurfingTrial() {
		if(wayPoints == null)
			return;
		for (int i = 0; i < wayPointNum; i++) {
			if(wayPoints[i] != null) {
				GameObject.Destroy(wayPoints[i]);
			}
		}
		wayPoints = null;
		surfingPath = null;
		averageDistance = sumDistance / frameNum;
		recorder.StopTrialFileWriter();
		trialControl.FinishTrial();

		return;	// average distance
	}

	public Vector3 GetEndPoint() {
		return endPt;
	}

	public Vector3 GetClosetPointOnPath (Vector3 pos) {
		/*float totalDistance = (endPt - startPt).magnitude;
		Vector3 targetDirection = (endPt - startPt);
		targetDirection.Normalize();
		float passedDistance = Vector3.Dot(pos - startPt, targetDirection);
		Vector3 closestPt = startPt + targetDirection * passedDistance;
		return closestPt;
		*/
		if(surfingPath != null)
			return surfingPath.GetPoint(pos);
		return Vector3.zero;
	}

	public StoreTransform GenerateSamples(Transform startPoint, Transform endPoint, int difficultyLevel) {
		startPt = startPoint.position;
		endPt = endPoint.position;
		wayPoints = new GameObject[wayPointNum];
		surfingPath = new SurfingPathBell(startPt, endPt, 0.2f, 1.0f, 50);
		for (int i=0; i<wayPointNum; i++) {

			wayPoints[i] = Instantiate(Resources.Load("Prefabs/WayPointSphere", typeof(GameObject))) as GameObject;
			wayPoints[i].transform.position = surfingPath.GetPoint((i + 1) * 1.0f / wayPointNum);
		}
		/*
		float distance = (endPt - startPt).magnitude;
		Vector3 direction = (endPt - startPt).normalized;
		float halfSampleNum = wayPointNum * 0.5f;
		float arcDegree = tangentAngle * 2;
		float radius = distance / 2 / Mathf.Sin (tangentAngle);
		float deltaDegree = arcDegree / wayPointNum;
		Vector3 midPoint = (startPt + endPt) / 2;
		midPoint.y = -Mathf.Cos(- arcDegree / 2) * radius;

		float deltaDegreePitch = pitchPeriod * Mathf.PI * 2 / wayPointNum;
		float deltaDegreeYaw = yawPeriod * Mathf.PI * 2 / wayPointNum;
		float angleBetweenZ = Mathf.Acos(Vector3.Dot(direction, Vector3.forward));
		for (int i=0; i<wayPointNum; i++) {
			float degree = (i - halfSampleNum) * deltaDegree;
			float xozDistance = Mathf.Sin(degree) * radius;
			float perturbYaw = Mathf.Cos(1.0f * (i + 1) / wayPointNum * 2 * Mathf.PI * yawPeriod) * yawAmplification;
			float perturbPitch = Mathf.Cos(1.0f * (i + 1) / wayPointNum * 2 * Mathf.PI * pitchPeriod) * pitchAmplification;
			float x_offset = midPoint.x + xozDistance * direction.x + Mathf.Cos(angleBetweenZ) * perturbYaw;
			float z_offset = midPoint.z + xozDistance * direction.z + Mathf.Sin(angleBetweenZ) * perturbYaw;
			float y_offset = Mathf.Cos(degree) * radius + perturbPitch;
			wayPoints[i] = Instantiate(Resources.Load("Prefabs/WayPointSphere", typeof(GameObject))) as GameObject;
			wayPoints[i].transform.position = new Vector3(x_offset, y_offset + midPoint.y + startPt.y, z_offset);
			//wayPoints[i].GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, (wayPointNum - i - 1) * 0.8f / wayPointNum);
		}*/
		recorder.GenerateFileWriter ((int) playerStatus.GetControlType(), difficultyLevel, (int) TRAVEL_TYPE.SURFING);
		timeStamp = 0;
		string instruction = "#Surfing Trial Path#";
		recorder.RecordLine(instruction);
		instruction = playerStatus.GetStatusTableHead ();
		recorder.RecordLine(instruction);

		StoreTransform startTransform = new StoreTransform();
		startTransform.position = startPt;
		startTransform.forward = endPt - startPt;
		startTransform.forward.Normalize();
		arrowControl.SetTarget (GoalScript.CreateGoal(endPt).transform);
		averageDistance = -1;
		sumDistance = 0;
		frameNum = 0;
		return startTransform;
	}

	public float GetAverageDistance() {
		if (averageDistance < 0) {
			Debug.Log("Data not avaliable yet!");
		}
		return averageDistance;
	}
}
