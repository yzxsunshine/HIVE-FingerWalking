using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Tuio;

public class FingerWalkingGesture : MonoBehaviour 
{	
	private int status;
	Dictionary<int, Vector2> fingerTrails = new Dictionary<int, Vector2>();
	Dictionary<int, Vector2> fingerVel = new Dictionary<int, Vector2>();
	public GameObject character = null;
	float speedScale = 0.1f;

	FingerWalkingGesture() {
		status = 0;	// not walking
		//character = GetComponent<First Person Controller>;
	}

	// Use this for initialization

	// a step is from addTouch to removeTouch
	void HandleTouches(Tuio.Touch t)
	{
		switch (t.Status)
		{
		case TouchStatus.Began:
			addTouch(t);
			break;
		case TouchStatus.Ended:
			removeTouch(t);
			break;
		case TouchStatus.Moved:
			updateTouch(t);
			break;
		case TouchStatus.Stationary:
		default:
			break;
		}
	}

	// step start
	void addTouch(Tuio.Touch t)
	{
		//Debug.Log("Add Touch");
		addTrail(t);
		//this.GetComponent("CharacterMotor").SendMessage("SetVelocity", Vector3.zero);
	}

	// step end
	void removeTouch(Tuio.Touch t)
	{
		//Debug.Log("Remove Touch");
		Vector2 diff = t.TouchPoint - fingerTrails[t.TouchId];
		float dist = diff.magnitude;
		status++;
		if(status > 2)
			status = 1;

		if(status == 1)
			Debug.Log("left");
		if(status == 2)
			Debug.Log("right");

		//if(character != null) {
			
			//Debug.Log("Rotate");
		Vector3 move = new Vector3(-diff.x, 0.0f, diff.y) * speedScale;
		Vector3 moveDirection = transform.rotation * move;
		this.GetComponent("CharacterMotor").SendMessage("SetVelocity", moveDirection);

			//moveDirection = transform.rotation * move;
			//Debug.Log("Before Translate: " + transform.position);
			//transform.Translate(move);
			//Debug.Log("moveDirection: " + move);
			//Debug.Log("After Translate: " + transform.position);
			//Debug.Log("Translate");
		//}
		removeTrail(t);

	}
	
	void updateTouch(Tuio.Touch t)
	{
		//Debug.Log("id=" + t.FingerId + "x=" + t.TouchPoint.x + ", y=" + t.TouchPoint.y + ", force = " + t.Properties.Force);
	}

	void addTrail (Tuio.Touch t) 
	{
		fingerTrails.Add(t.TouchId, new Vector2(t.TouchPoint.x, t.TouchPoint.y));
	}

	void removeTrail(Tuio.Touch t)
	{
		fingerTrails.Remove(t.TouchId);
	}
}

