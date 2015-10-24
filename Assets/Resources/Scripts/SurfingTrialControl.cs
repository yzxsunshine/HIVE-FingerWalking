using UnityEngine;
using System.Collections;

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
			string line = "" + timeStamp + "\t" + playerStatus.GetCurrentTransformLine();
			recorder.RecordLine(line);
			if (passedSphereNum >= wayPointNum) {
				wayPoints = null;
				recorder.StopTrialFileWriter();
				trialControl.FinishTrial();
			}
			timeStamp += Time.deltaTime;
		}
	}

	public StoreTransform GenerateSamples(Transform startPoint, Transform endPoint) {
		startPt = startPoint.position;
		endPt = endPoint.position;
		wayPoints = new GameObject[wayPointNum];
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
		}
		recorder.GenerateFileWriter ((int) playerStatus.GetControlType(), 0, (int) TRAVEL_TYPE.SURFING);
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
		return startTransform;
	}
}
