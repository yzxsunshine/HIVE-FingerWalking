using UnityEngine;
using System.Collections;

public class GoalScript : MonoBehaviour {
	private PlayerStatus playerStatus;
	private SegwayPathController segwayPathControl;
	private WalkingTrialControl walkingTrialControl;
	private SurfingTrialControl surfingTrialControl;
	// Use this for initialization
	void Start () {
		playerStatus = GameObject.Find("Character").GetComponent<PlayerStatus>();
		segwayPathControl = GameObject.Find ("SegwayWayPointsManager").GetComponent<SegwayPathController> ();
		walkingTrialControl = GameObject.Find ("WalkingTrialManager").GetComponent<WalkingTrialControl> ();
		surfingTrialControl = GameObject.Find ("SurfingTrialManager").GetComponent<SurfingTrialControl> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter() {
		if(playerStatus.GetGestureType() == TRAVEL_TYPE.SEGWAY) {
			segwayPathControl.ActiveNextWayPoint();
		}
		else if(playerStatus.GetGestureType() == TRAVEL_TYPE.SURFING) {
			surfingTrialControl.CompleteSurfingTrial();
		}
		GameObject.Destroy(this.gameObject);
	}

	public static GameObject CreateGoal(Vector3 position) {
		GameObject goal = Instantiate(Resources.Load("Prefabs/Goal", typeof(GameObject))) as GameObject;
		position.y = 0;
		goal.transform.position = position;
		return goal;
	}
}
