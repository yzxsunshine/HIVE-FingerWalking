using UnityEngine;
using System.Collections;

public class CollisionEvent : MonoBehaviour {
	private float startTime;
	private PlayerStatus status;
	// Use this for initialization
	void Start () {
		status = GameObject.Find ("Character").GetComponent<PlayerStatus> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	
	void OnTriggerEnter(Collider other) {
		startTime = Time.time;
		status.segwayCollisionNum++;
		status.CollisionEntered();
	}

	
	void OnTriggerExit(Collider other) {
		float duration = Time.time - startTime;
		status.segwayCollisionTime += duration;
		status.CollisionExit();
	}
}
