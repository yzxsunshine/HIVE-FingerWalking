using UnityEngine;
using System.Collections;

public class PathIndicatorSegment {
	public Transform startPt;
	public Transform endPt;
	public Vector3 center;
	public float radius;

	public void Init(Transform s, Transform e) {
		startPt = s;
		endPt = e;
		center = Vector3.zero;
		radius = 0.0f;
	}

	public void InitCurve(Transform s, Transform e, Vector3 c, float r) {
		startPt = s;
		endPt = e;
		center = c;
		radius = r;
	}
}

public class PathGenerator : MonoBehaviour {
	public float stepLength = 10.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void CleanPath() {
		GameObject[] objs = GameObject.FindGameObjectsWithTag("PathTriangle");
		foreach (GameObject obj in objs) {
			GameObject.Destroy(obj);
		}
	}

	public void GeneratePath(PathIndicatorSegment[] segs) {
		CleanPath ();
		foreach (PathIndicatorSegment seg in segs) {
			if (seg == null) {
				break;
			}
			if (seg.center.x == 0 && seg.center.y == 0 && seg.center.z == 0) {
				Vector3 direction = seg.endPt.position - seg.startPt.position;
				float distance = direction.magnitude;
				int steps = (int) Mathf.Ceil(distance / stepLength) + 1;
				float unitStepLen = distance / stepLength;
				for (int i = 0; i <= steps; i++) {
					GameObject obj = Instantiate(Resources.Load("Prefabs/Plane", typeof(GameObject))) as GameObject;
					obj.transform.position = seg.startPt.position + direction.normalized * Mathf.Min(unitStepLen * i, distance);
					obj.transform.parent = this.transform;
				}
			}
			else {
				Vector3 direction = seg.endPt.position - seg.startPt.position;
				float distance = direction.magnitude;
				Vector3 startDir = seg.startPt.position - seg.center;
				startDir.y = 0;
				Vector3 endDir = seg.endPt.position - seg.center;
				endDir.y = 0;
				float radius = startDir.magnitude;
				float baseTheta = Mathf.Acos(Vector3.Dot(startDir.normalized, new Vector3(1, 0, 0)));
				float endTheta = Mathf.Acos(Vector3.Dot(endDir.normalized, new Vector3(1, 0, 0)));
				float theta = endTheta - baseTheta;
				float length = Mathf.Abs(theta * radius);
				int steps = (int) Mathf.Ceil(length / stepLength) + 1;
				float unitAngleInc = theta / steps;
				for (int i = 0; i <= steps; i++) {
					GameObject obj = Instantiate(Resources.Load("Prefabs/Plane", typeof(GameObject))) as GameObject;
					float curAngle = baseTheta + unitAngleInc * i;
					if (baseTheta < endTheta)
						curAngle = -curAngle;
					Vector3 newPos = new Vector3(Mathf.Cos(curAngle) * radius, seg.startPt.position.y, Mathf.Sin(curAngle) * radius) + seg.center;
					newPos.y = 0;
					obj.transform.position = newPos;
					obj.transform.parent = this.transform;
				}
			}
		}
	}
}
