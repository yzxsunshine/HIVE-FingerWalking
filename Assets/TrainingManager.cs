using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TrainingManager : MonoBehaviour {
	private int currentStep = 0;
	private int maxTrainingNum = 4;
	private RawImage trainingImage;
	private JoystickGesture gamepadTrainingResponse;
	private TouchPadGesture forcepadTrainingResponse;
	private float trainingTimer = 0.0f;

	private SegwayPathController segwayPathControl;
	private WalkingTrialControl walkingTrialControl;
	private SurfingTrialControl surfingTrialControl;
	private GameObject character;
	public Text modeSwitchText;
	private TRAVEL_TYPE travelType;
	private PlayerStatus playerStatus;
	private float validWalkingTime = 100.0f;
	private float validSegwayTime = 200.0f;
	private int validSegwayCollision = 10;
	private float validSurfingTime = 100.0f;
	private float validSurfingDistance = 20.0f;
	private int walkingTrainingNum = 0;
	private int segwayTrainingNum = 0;
	private int surfingTrainingNum = 0;

	void Awake() {
		segwayPathControl = GameObject.Find ("SegwayWayPointsManager").GetComponent<SegwayPathController> ();
		walkingTrialControl = GameObject.Find ("WalkingTrialManager").GetComponent<WalkingTrialControl> ();
		surfingTrialControl = GameObject.Find ("SurfingTrialManager").GetComponent<SurfingTrialControl> ();
		modeSwitchText = GameObject.Find ("ModeSwitchText").GetComponent<Text> ();
		character = GameObject.Find("Character");
		playerStatus = character.GetComponent<PlayerStatus> ();
	}


	// Use this for initialization
	void Start () {
		//trainingImage = GameObject.Find("TrainingImage").GetComponent<RawImage>();
		gamepadTrainingResponse = GameObject.Find("Character").GetComponent<JoystickGesture>();
		forcepadTrainingResponse = GameObject.Find("Character").GetComponent<TouchPadGesture>();
	}
	
	// Update is called once per frame
	void Update () {
		trainingTimer += Time.deltaTime;
	}

	public void StartTimer() {
		trainingTimer = 0.0f;
	}

	public float GetTimer() {
		return trainingTimer;
	}

	public void StartTraining(Transform targetTransform) {
		walkingTrialControl.SetWalkingPath(0, 0, targetTransform, walkingTrainingNum);
		modeSwitchText.text = "Switch to Walking mode.";
		modeSwitchText.enabled = true;
		currentStep = 0;
		travelType = TRAVEL_TYPE.WALKING;
		character.GetComponent<TravelModelInterface>().SetTargetGestureType (travelType);
		StartTimer();
		walkingTrainingNum++;
	}

	public StoreTransform FinishTrainingTrial (int startPtID, Transform curWayPt, Transform nextWayPt) {
		character.GetComponent<TravelModelInterface>().SetTargetGestureType (TRAVEL_TYPE.RESTING);
		character.GetComponent<TravelModelInterface>().SetGestureType (TRAVEL_TYPE.RESTING);

		if(currentStep % 2 == 0) {
			currentStep++;
		}
		else {
			currentStep++;
			float passedTime = trainingTimer;
			switch(travelType) {
			case TRAVEL_TYPE.WALKING:
				if (passedTime < validWalkingTime)
					travelType = TRAVEL_TYPE.SEGWAY;
				else {
					currentStep--;	// repeat current
				}
				break;
			case TRAVEL_TYPE.SEGWAY:
				if (passedTime < validSegwayTime && playerStatus.GetCollisionNum() < validSegwayCollision)
					travelType = TRAVEL_TYPE.SURFING;
				else {
					currentStep--;	// repeat current
				}
				break;
			case TRAVEL_TYPE.SURFING:
				if (passedTime < validSurfingTime && surfingTrialControl.GetAverageDistance() < validSurfingDistance)
					return null;// all training trial ended; not in training mode anymore
				else {
					currentStep--;	// repeat current
				}
				break;
			}
		}

		StoreTransform targetTransform = null;
		switch(travelType) {
		case TRAVEL_TYPE.WALKING:
			targetTransform = walkingTrialControl.SetWalkingPath(0, 0, curWayPt, walkingTrainingNum);
			walkingTrainingNum++;
			break;
		case TRAVEL_TYPE.SEGWAY:
			targetTransform = segwayPathControl.SetSegwayPath(0, startPtID, 0, segwayTrainingNum);
			segwayTrainingNum++;
			playerStatus.CleanCollision();
			break;
		case TRAVEL_TYPE.SURFING:
			segwayPathControl.OpenAllWayPoints();
			targetTransform = surfingTrialControl.GenerateSamples(curWayPt, nextWayPt, 0, surfingTrainingNum);
			surfingTrainingNum++;
			break;
		}

		return targetTransform;
	}

	public void StartNextTraining() {
		switch(travelType) {
		case TRAVEL_TYPE.WALKING:
			modeSwitchText.text = "Switch to Walking mode.";
			break;
		case TRAVEL_TYPE.SEGWAY:
			modeSwitchText.text = "Switch to Segway mode.";
			playerStatus.CleanCollision();
			break;
		case TRAVEL_TYPE.SURFING:
			modeSwitchText.text = "Switch to Surfing mode.";
			break;
		}
		modeSwitchText.enabled = true;
		character.GetComponent<TravelModelInterface>().SetTargetGestureType (travelType);
		StartTimer();
	}

	public void ShowTrainingImage() {
		if(trainingImage.enabled = false) {
			trainingImage.enabled = true;
			currentStep = 0;
		}
		if(currentStep > maxTrainingNum) {
			trainingImage.enabled = false;
			switch (ConfigurationHandler.controllerType) {
			case CONTROL_TYPE.JOYSTICK:
				gamepadTrainingResponse.SetTrainingResponse(false);
				break;
			case CONTROL_TYPE.FORCEPAD_GESTURE:
				forcepadTrainingResponse.SetTrainingResponse(false);
				break;
			}
			return ;
		}
		string controlTypeStr = "";
		switch (ConfigurationHandler.controllerType) {
		case CONTROL_TYPE.JOYSTICK:
			controlTypeStr = "gamepad";
			gamepadTrainingResponse.SetTrainingResponse(true);
			break;
		case CONTROL_TYPE.FORCEPAD_GESTURE:
			controlTypeStr = "forcepad";
			forcepadTrainingResponse.SetTrainingResponse(true);
			break;
		}
		trainingImage.texture = Resources.Load("training/" + controlTypeStr + "_" + currentStep) as Texture;
		currentStep++;
	}
}
