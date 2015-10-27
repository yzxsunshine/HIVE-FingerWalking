using UnityEngine;
using System.Collections;

public class HMDCamera : MonoBehaviour {
	private GameObject character = null;
	private float HEAD_HEIGHT = 2.0f;
	private PlayerStatus playerStatus = null;
	// Use this for initialization
	void Start () {
		character = GameObject.Find ("Character");
		playerStatus = character.GetComponent<PlayerStatus> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Vector3 UpdateCamera (float hmdX, float hmdY, float hmdZ) {
		//transform.position = character.transform.position + HEAD_HEIGHT;
		transform.localPosition.Set (0, HEAD_HEIGHT, 0);
		//Quaternion forwardQuat = character.transform.rotation;
		Quaternion hmdQuat = Quaternion.Euler(hmdX, hmdY, hmdZ);
		transform.localRotation = hmdQuat; //forwardQuat * hmdQuat;
		if(playerStatus != null)
			playerStatus.headOrientation = new Vector3(hmdX, hmdY, hmdZ);
		return transform.eulerAngles;
	}
}
