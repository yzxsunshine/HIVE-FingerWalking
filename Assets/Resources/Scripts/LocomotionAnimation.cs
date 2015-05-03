using UnityEngine;
using System.Collections;

public class LocomotionAnimation : MonoBehaviour {
	public Animator animator = null;
	public float speed;
	public Vector3 vel = new Vector3(0, 0, 0);
	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		speed = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (animator) {
			speed = Vector3.Magnitude(vel);
			animator.SetFloat("t_speed", speed);
		}
		transform.localPosition = new Vector3(0, transform.localPosition.y, -0.25f);
	}
}
