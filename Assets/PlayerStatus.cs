#define DEBUG_DISPLAY

using UnityEngine;
using System.Collections;

public class PlayerStatus : MonoBehaviour {
	public int segwayCollisionNum = 0;
	public float segwayCollisionTime = 0;

	// Use this for initialization
	void Start () {
		ResetPlayerStatus ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnGUI() {
#if DEBUG_DISPLAY
		GUI.Label(new Rect(10, 60, 400, 30), "Number of Collision in this trial : " + segwayCollisionNum);
		GUI.Label(new Rect(10, 100, 400, 30), "Time of stucking in obstacles : " + segwayCollisionTime);
#endif
	}


	public void ResetPlayerStatus() {
		segwayCollisionNum = 0;
		segwayCollisionTime = 0;
	}
}
