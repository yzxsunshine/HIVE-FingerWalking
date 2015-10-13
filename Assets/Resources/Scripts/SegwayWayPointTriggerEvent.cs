using UnityEngine;
using System.Collections;

public class SegwayWayPointTriggerEvent : MonoBehaviour {
	private SegwayPathController segwayPathController;
	// Use this for initialization
	void Start () {
		segwayPathController = GameObject.Find ("SegwayWayPointsManager").GetComponent<SegwayPathController> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		segwayPathController.ActiveNextWayPoint ();
	}
}
