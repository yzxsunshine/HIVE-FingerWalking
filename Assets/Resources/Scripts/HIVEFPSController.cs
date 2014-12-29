using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]

public class HIVEFPSController : MonoBehaviour {
	public float speed = 10.0f;
	public float gravity = 10.0f;
	private float maxWalkingVelocityIncrease = 15.0f;
	private float maxWalkingVelocityDecrease = -1.0f;
	private float maxSegwayVelocityIncrease = 40.0f;
	private float maxSegwayVelocityDecrease = -20.0f;
	private float maxSurfVelocityIncrease = 100.0f;
	private float maxSurfVelocityDecrease = -50.0f;
	public bool canJump = true;
	public float jumpHeight = 2.0f;

	public float frictionCoeff = 1.0f;
	public Vector3 targetVelocity;
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

		switch (gestureType) {
		case GESTURE_TYPE.WALKING:
			// Calculate how fast we should be moving
			//Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			//targetVelocity = transform.TransformDirection(targetVelocity);
			//targetVelocity *= speed;
			
			// Apply a force that attempts to reach our target velocit;
			velocityChange.x = Mathf.Clamp(velocityChange.x, maxWalkingVelocityDecrease, maxWalkingVelocityIncrease);
			velocityChange.z = Mathf.Clamp(velocityChange.z, maxWalkingVelocityDecrease, maxWalkingVelocityIncrease);
			velocityChange.y = 0;
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

			// Jump
			if (canJump && Input.GetButton("Jump")) {
				rigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
			}
			break;

		case GESTURE_TYPE.SEGWAY:
			velocityChange.x = Mathf.Clamp(velocityChange.x, maxSegwayVelocityDecrease, maxSegwayVelocityIncrease);
			velocityChange.z = Mathf.Clamp(velocityChange.z, maxSegwayVelocityDecrease, maxSegwayVelocityIncrease);
			velocityChange.y = 0;
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

			break;
			break;
		case GESTURE_TYPE.SKATEBOARD:
			velocityChange.x = Mathf.Clamp(velocityChange.x, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
			velocityChange.z = Mathf.Clamp(velocityChange.z, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
			velocityChange.y = Mathf.Clamp(velocityChange.y, maxSurfVelocityDecrease, maxSurfVelocityIncrease);
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
		gestureType = GESTURE_TYPE.SKATEBOARD;
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
