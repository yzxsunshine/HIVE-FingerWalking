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
	private float timeStamp;
	// Use this for initialization
	void Awake () {
		character = GameObject.Find("Character");
		playerStatus = character.GetComponent<PlayerStatus> ();
		recorder = GameObject.Find ("StudyRecorder").GetComponent<StudyRecorder> ();
	}

	void Start() {
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (wayPoints != null) {
			Vector3 currentPos = character.transform.position;
			float totalDistance = (endPt - startPt).magnitude;
			float passedDistance = (currentPos - startPt).magnitude;
			int passedSphereNum = Mathf.FloorToInt(passedDistance / totalDistance * wayPointNum);
			for (int i = 0; i < passedSphereNum; i++) {
				//wayPoints[i].GetComponent<Renderer>().enabled = false;
				GameObject.Destroy(wayPoints[i]);
			}
			for (int i = passedSphereNum; i < wayPointNum; i++) {
				float alpha = 1.0f * (wayPointNum - i + passedSphereNum - 1) / wayPointNum;
				wayPoints[i].GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.8f * alpha * alpha * alpha);
			}
			string line = "" + timeStamp + "\t" + playerStatus.GetCurrentTransformLine();
			recorder.RecordLine(line);
			if (passedSphereNum >= wayPointNum) {
				wayPoints = null;
				recorder.StopTrialFileWriter();
			}
			timeStamp += Time.deltaTime;
		}
	}

	public void GenerateSamples(Vector3 startPoint, Vector3 endPoint) {
		startPt = startPoint;
		endPt = endPoint;
		wayPoints = new GameObject[wayPointNum];
		float distance = (endPt - startPt).magnitude;
		Vector3 direction = (endPt - startPt).normalized;
		float halfSampleNum = wayPointNum * 0.5f;
		float arcDegree = tangentAngle * 2;
		float radius = distance / 2 / Mathf.Sin (tangentAngle);
		float deltaDegree = arcDegree / wayPointNum;
		Vector3 midPoint = (startPt + endPt) / 2;
		midPoint.y = -Mathf.Cos(- arcDegree / 2) * radius;
		for (int i=0; i<wayPointNum; i++) {
			float degree = (i - halfSampleNum) * deltaDegree;

			float xozDistance = Mathf.Sin(degree) * radius;
			float x_offset = midPoint.x + xozDistance / distance * direction.x * distance;
			float z_offset = midPoint.z + xozDistance / distance * direction.z * distance;
			float y_offset = Mathf.Cos(degree) * radius;
			wayPoints[i] = Instantiate(Resources.Load("Prefabs/WayPointSphere", typeof(GameObject))) as GameObject;
			wayPoints[i].transform.position = new Vector3(x_offset, y_offset + midPoint.y + startPt.y, z_offset);
			wayPoints[i].GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, (wayPointNum - i - 1) * 0.8f / wayPointNum);
		}
		recorder.GenerateFileWriter ((int) playerStatus.GetControlType(), 0, (int) TRAVEL_TYPE.SURFING);
		timeStamp = 0;
		string instruction = "#Surfing Trial Path#";
		recorder.RecordLine(instruction);
		instruction = playerStatus.GetStatusTableHead ();
		recorder.RecordLine(instruction);
	}
}
