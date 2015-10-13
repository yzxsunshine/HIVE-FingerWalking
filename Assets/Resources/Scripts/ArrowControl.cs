using UnityEngine;
using System.Collections;

public class ArrowControl : MonoBehaviour {
	public Transform target = null;
	private MeshRenderer renderer = null;
	private PlayerStatus playerStatus;
	// Use this for initialization
	void Start () {
		playerStatus = GameObject.Find ("Character").GetComponent<PlayerStatus> ();
		renderer = GetComponentInChildren<MeshRenderer> ();
		renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (target != null) {
			if (playerStatus.GetGestureType() != GESTURE_TYPE.SURFING) {
				this.transform.forward = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);
			}
			else {
				this.transform.forward = target.position - this.transform.position;
			}
			this.transform.forward.Normalize();
		}
	}

	public void SetTarget(Transform transform) {
		target = transform;
		renderer.enabled = true;
	}

	public void ResetTarget() {
		target = null;
		renderer.enabled = false;
	}
}
