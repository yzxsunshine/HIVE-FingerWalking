using UnityEngine;
using System.Collections;

public class TriggerEvent : MonoBehaviour {
	private GameObject Character;
	public float selfRotateAngle = 0.6f;
	private WalkingTrialControl walkingTrialControl;
	// Use this for initialization
	void Start () {
		Character = GameObject.Find ("Character");
		walkingTrialControl = GameObject.Find ("WalkingTrialManager").GetComponent<WalkingTrialControl> ();
	}
	// Update is called once per frame
	void Update () {
		transform.Rotate (0, selfRotateAngle, 0);
	}
	
	// 36, 72, 108, -36, -72, -108
	// 1 (sqrt(5) + 1) / 2
	void OnTriggerEnter() {
		walkingTrialControl.ActiveNextWayPoint ();

		GameObject.Destroy (this.gameObject);
	}
}
