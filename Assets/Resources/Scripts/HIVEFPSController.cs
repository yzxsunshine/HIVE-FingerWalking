using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]

public class HIVEFPSController : MonoBehaviour {
	public float speed = 10.0f;
	public float gravity = 10.0f;
	public float maxWalkingVelocityIncrease = 6.0f;
	public float maxWalkingVelocityDecrease = -1.0f;
	public float maxSegwayVelocityIncrease = 40.0f;
	public float maxSegwayVelocityDecrease = -5.0f;
	public float maxSurfVelocityIncrease = 100.0f;
	public float maxSurfVelocityDecrease = -10.0f;

	private float maxInertiaChange = 0.0f;
	public bool canJump = true;
	public float jumpHeight = 2.0f;

	public float frictionCoeff = 1.0f;
	public Vector3 targetVelocity;
	public Quaternion targetRotation;
	public GESTURE_TYPE gestureType;


	private bool grounded = false;
	void Awake ()
	{
		rigidbody.freezeRotation = true;
		rigidbody.useGravity = false;
	}
	
	void DoStep () {
		Vector3 velocity = rigidbody.velocity;
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
			mag = Mathf.Clamp(curMag - prevMag, maxWalkingVelocityDecrease, maxWalkingVelocityIncrease);
			velocityChange.Normalize();
			velocityChange = velocityChange * Mathf.Abs(mag);
			velocityChange.y = 0;
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

			// Jump
			if (canJump && Input.GetButton("Jump")) {
				rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
			}
			break;

		case GESTURE_TYPE.SEGWAY:
			mag = Mathf.Clamp(curMag - prevMag, maxSegwayVelocityDecrease, maxSegwayVelocityIncrease);
			velocityChange.Normalize();
			velocityChange = velocityChange * Mathf.Abs(mag);
			velocityChange.y = -gravity;
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

			break;
		case GESTURE_TYPE.SURFING:
			mag = Mathf.Clamp(curMag - prevMag, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
			velocityChange.Normalize();
			velocityChange = velocityChange * Mathf.Abs(mag);
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
			break;
		case GESTURE_TYPE.NOTHING: 
			mag = Mathf.Clamp(curMag - prevMag, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
			velocityChange.Normalize();
			velocityChange = velocityChange * Mathf.Abs(mag);
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
			break;
		}



		// We apply gravity manually for more tuning control
		rigidbody.AddForce(new Vector3 (0, -gravity * rigidbody.mass, 0));
		
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
