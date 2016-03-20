using UnityEngine;
using System.Collections;

public class ConeWayPointScript : MonoBehaviour {
	GameObject character;
	ArrowControl arrowCtrl;
	private PlayerStatus status;
	private CogTrialControl cogTrialControl;
	// Use this for initialization
	void Start () {
		character = GameObject.Find ("Character");
		arrowCtrl = character.GetComponentInChildren<ArrowControl> ();
		status = character.GetComponent<PlayerStatus> ();
		cogTrialControl = character.GetComponent<CogTrialControl> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other) {
		GameObject obj = cogTrialControl.NextStep ();
		if (obj != null)
			arrowCtrl.SetTarget (obj.transform);
		else
			arrowCtrl.ResetTarget ();
	}

}
