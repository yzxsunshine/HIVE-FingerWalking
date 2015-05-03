using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]

public class HIVEFPSController : MonoBehaviour {
	public float speed = 10.0f;
	public float gravity = 10.0f;
	public float maxWalkingVelocityIncrease = 6.0f;
	public float maxWalkingVelocityDecrease = -1.0f;
	public float maxSegwayVelocityIncrease = 160.0f;
	public float maxSegwayVelocityDecrease = -5.0f;
	public float maxSurfVelocityIncrease = 500.0f;
	public float maxSurfVelocityDecrease = -10.0f;

	private float maxInertiaChange = 0.0f;
	public bool canJump = true;
	public float jumpHeight = 2.0f;

	public float frictionCoeff = 1.0f;
	public Vector3 targetVelocity;
	public Vector3 targetPosition;
	public Quaternion targetRotation;
	public GESTURE_TYPE gestureType;
	public int pos_or_vel = 1;

	private bool grounded = false;
	void Awake ()
	{
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
	}
	
	void DoStep () {
		Vector3 velocity = GetComponent<Rigidbody>().velocity;
		Vector3 velocityChange = (targetVelocity - velocity);
		float prevMag = velocity.magnitude;
		float curMag = targetVelocity.magnitude;
		float mag = 0.0f;
		//rigidbody.rotation = targetRotation;
		switch (gestureType) {
		case GESTURE_TYPE.WALKING:
// Calculate how fast we should be moving
//Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
//targetVelocity = transform.TransformDirection(targetVelocity);
//targetVelocity *= speed;

// Apply a force that attempts to reach our target velocit;
			mag = Mathf.Clamp (curMag - prevMag, maxWalkingVelocityDecrease, maxWalkingVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			velocityChange.y = 0;
			GetComponent<Rigidbody>().AddForce (velocityChange, ForceMode.VelocityChange);

// Jump
			if (canJump && Input.GetButton ("Jump")) {
					GetComponent<Rigidbody>().velocity = new Vector3 (velocity.x, CalculateJumpVerticalSpeed (), velocity.z);
			}
			break;

		case GESTURE_TYPE.SEGWAY:
			mag = Mathf.Clamp (curMag - prevMag, maxSegwayVelocityDecrease, maxSegwayVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			velocityChange.y = -gravity;
			GetComponent<Rigidbody>().AddForce (velocityChange, ForceMode.VelocityChange);

			break;
		case GESTURE_TYPE.SURFING:
			mag = Mathf.Clamp (curMag - prevMag, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			GetComponent<Rigidbody>().AddForce (velocityChange, ForceMode.VelocityChange);
			break;
		case GESTURE_TYPE.NOTHING: 
			mag = Mathf.Clamp (curMag - prevMag, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			GetComponent<Rigidbody>().AddForce (velocityChange, ForceMode.VelocityChange);
			break;
		}
	


		// We apply gravity manually for more tuning control
		GetComponent<Rigidbody>().AddForce(new Vector3 (0, -gravity * GetComponent<Rigidbody>().mass, 0));
		
		grounded = false;
	}
	
	void OnCollisionStay () {
		grounded = true;    
	}
	
	float CalculateJumpVerticalSpeed () {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * jumpHeight * gravity);
	}

	void SetVelocity (Vector3 vel) {
		targetVelocity = vel;
	}

	void SetRotation (Quaternion q) {
		targetRotation = q;
	}

	void SetGravity (float g) {
		gravity = g;
	}

	void SetFly () {
		grounded = false;
		gravity = 0.0f;
	}

	void SetGround () {
		grounded = true;
		gravity = 10.0f;
	}

	void SetSurfing () {
		gestureType = GESTURE_TYPE.SURFING;
		grounded = false;
		gravity = 0.0f;
	}
	
	void SetWalking () {
		gestureType = GESTURE_TYPE.WALKING;
		grounded = true;
		gravity = 10.0f;
	}

	void SetSegway () {
		gestureType = GESTURE_TYPE.SEGWAY;
		grounded = true;
		gravity = 10.0f;
	}

}
