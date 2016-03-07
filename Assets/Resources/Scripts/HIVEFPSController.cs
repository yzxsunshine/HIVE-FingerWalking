using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]

public class HIVEFPSController : MonoBehaviour {
	public float speed = 10.0f;
	public float gravity = 10.0f;
	public float maxWalkingVelocityIncrease = 1.0f;
	public float maxWalkingVelocityDecrease = -0.3f;
	public float maxSegwayVelocityIncrease = 5.0f;
	public float maxSegwayVelocityDecrease = -2.0f;
	public float maxSurfVelocityIncrease = 10.0f;
	public float maxSurfVelocityDecrease = -5.0f;
	public float maxResetVelocityIncrease = 5.0f;
	public float maxResetVelocityDecrease = -5.0f;

	private float maxInertiaChange = 0.0f;
	public bool canJump = true;
	public float jumpHeight = 2.0f;

	public float frictionCoeff = 1.0f;
	public Vector3 targetVelocity;
	public Vector3 targetPosition;
	public Quaternion targetRotation;
	public TRAVEL_TYPE gestureType;
	public int pos_or_vel = 1;
	private bool hasControl = true;

	private bool grounded = false;
	void Awake ()
	{
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
	}

	public void Reset () {

	}

	public void DoStep () {
		Vector3 velocity = GetComponent<Rigidbody>().velocity;
		Vector3 velocityChange = (targetVelocity - velocity);
		float prevMag = velocity.magnitude;
		float curMag = targetVelocity.magnitude;
		float mag = 0.0f;
		//rigidbody.rotation = targetRotation;
		if(!hasControl) {
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			return;
		}
		switch (gestureType) {
		case TRAVEL_TYPE.WALKING:
// Calculate how fast we should be moving
//Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
//targetVelocity = transform.TransformDirection(targetVelocity);
//targetVelocity *= speed;

// Apply a force that attempts to reach our target velocit;
			mag = Mathf.Clamp (curMag - prevMag, maxWalkingVelocityDecrease, maxWalkingVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			velocityChange.y = 0;//-gravity;
			GetComponent<Rigidbody> ().AddForce (velocityChange, ForceMode.VelocityChange);

// Jump
			if (canJump && Input.GetButton ("Jump")) {
				GetComponent<Rigidbody> ().velocity = new Vector3 (velocity.x, CalculateJumpVerticalSpeed (), velocity.z);
			}
			break;

		case TRAVEL_TYPE.SEGWAY:
			mag = Mathf.Clamp (curMag - prevMag, maxSegwayVelocityDecrease, maxSegwayVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			velocityChange.y = 0;// -gravity;
			GetComponent<Rigidbody> ().AddForce (velocityChange, ForceMode.VelocityChange);

			break;
		case TRAVEL_TYPE.SURFING:
			mag = Mathf.Clamp (curMag - prevMag, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			GetComponent<Rigidbody> ().AddForce (velocityChange, ForceMode.VelocityChange);
			break;
		case TRAVEL_TYPE.FORCE_EXT:
			mag = Mathf.Clamp (curMag - prevMag, maxSegwayVelocityDecrease, maxSegwayVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			velocityChange.y = 0;// -gravity;
			GetComponent<Rigidbody> ().AddForce (velocityChange, ForceMode.VelocityChange);
			
			break;
		case TRAVEL_TYPE.NOTHING: 
			mag = Mathf.Clamp (curMag - prevMag, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			GetComponent<Rigidbody> ().AddForce (velocityChange, ForceMode.VelocityChange);
			break;
		case TRAVEL_TYPE.RESET:
			mag = Mathf.Clamp (curMag, maxResetVelocityDecrease, maxResetVelocityIncrease);
			velocityChange.Normalize ();
			velocityChange = velocityChange * Mathf.Abs (mag);
			GetComponent<Rigidbody> ().AddForce (velocityChange, ForceMode.VelocityChange);
			break;
		default:
			break;
		}

		// We apply gravity manually for more tuning control
		GetComponent<Rigidbody>().AddForce(new Vector3 (0, -gravity * GetComponent<Rigidbody>().mass, 0));
		
		grounded = false;
	}
	
	public void OnCollisionStay () {
		grounded = true;    
	}
	
	public float CalculateJumpVerticalSpeed () {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * jumpHeight * gravity);
	}

	public void SetVelocity (Vector3 vel) {
		targetVelocity = vel;
	}

	public void SetRotation (Quaternion q) {
		targetRotation = q;
	}

	public void SetGravity (float g) {
		gravity = g;
	}

	public void SetFly () {
		grounded = false;
		gravity = 0.0f;
	}

	public void SetGround () {
		grounded = true;
		gravity = 10.0f;
	}

	public void SetSurfing () {
		gestureType = TRAVEL_TYPE.SURFING;
		grounded = false;
		gravity = 0.0f;
	}
	
	public void SetWalking () {
		gestureType = TRAVEL_TYPE.WALKING;
		grounded = true;
		gravity = 10.0f;
	}

	public void SetSegway () {
		gestureType = TRAVEL_TYPE.SEGWAY;
		grounded = true;
		gravity = 10.0f;
	}

	public TRAVEL_TYPE SetReset () {
		TRAVEL_TYPE prevType = gestureType;
		gestureType = TRAVEL_TYPE.RESET;
		grounded = true;
		gravity = 10.0f;
		return prevType;
	}

	public void EnableMove() {
		hasControl = true;
	}

	public void DisableMove () {
		hasControl = false;
	}

}
