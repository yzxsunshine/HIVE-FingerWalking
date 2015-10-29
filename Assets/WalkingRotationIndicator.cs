using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WalkingRotationIndicator : MonoBehaviour {
	private RawImage leftRotateArea;
	private RawImage rightRotateArea;

	private float leftFlashTimer = 0;
	private float rightFlashTimer = 0;
	private float MAX_FLASH_TIME = 2.0f;

	private CONTROL_TYPE controlType;

	void Awake () {
		leftRotateArea = GameObject.Find("LeftRotateArea").GetComponent<RawImage>();
		rightRotateArea = GameObject.Find("RightRotateArea").GetComponent<RawImage>();
		leftRotateArea.enabled = false;
		rightRotateArea.enabled = false;
		controlType = ConfigurationHandler.controllerType;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(leftFlashTimer > 0) {
			leftFlashTimer -= Time.deltaTime;
			if(leftFlashTimer < 0) {
				leftFlashTimer = 0;
				rightFlashTimer = MAX_FLASH_TIME;
			}
			Color color = leftRotateArea.color;
			color.a = leftFlashTimer / MAX_FLASH_TIME;
			leftRotateArea.color = color;
		}

		if(rightFlashTimer > 0) {
			rightFlashTimer -= Time.deltaTime;
			if(rightFlashTimer < 0) {
				rightFlashTimer = 0;
				leftRotateArea.enabled = false;
				rightRotateArea.enabled = false;
			}
			Color color = rightRotateArea.color;
			color.a = rightFlashTimer / MAX_FLASH_TIME;
			rightRotateArea.color = color;
		}
	}


	public void StartAnimation () {
		switch (controlType) {
		case CONTROL_TYPE.JOYSTICK:

			break;
		case CONTROL_TYPE.FORCEPAD_GESTURE:
			leftFlashTimer = MAX_FLASH_TIME;
			leftRotateArea.enabled = true;
			rightRotateArea.enabled = true;
			break;
		}
	}

}
