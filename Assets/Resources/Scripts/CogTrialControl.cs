using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CogTrialControl : MonoBehaviour {
	private GameObject character;
	public LEVEL_TYPE levelType;
	public TRAVEL_TYPE travelType;
	public CONTROL_TYPE controlType;
	public CutSceneManager cutSceneManager;
	public Text modeSwitchText;
	private PlayerStatus playerStatus;
	public TravelModelInterface travelModelInterface;
	private HIVEFPSController fpsController;
	private StudyRecorder studyRecorder;

	void Awake() {
		character = GameObject.Find ("Character");
		cutSceneManager = GameObject.Find ("CutSceneManager").GetComponent<CutSceneManager> ();
		modeSwitchText = GameObject.Find ("ModeSwitchText").GetComponent<Text> ();
		playerStatus = character.GetComponent<PlayerStatus> ();
		studyRecorder = GameObject.Find ("StudyRecorder").GetComponent<StudyRecorder> ();
		travelModelInterface = GetComponent<TravelModelInterface> ();
		fpsController = GetComponent<HIVEFPSController> ();
	}
	// Use this for initialization
	void Start () {
		controlType = ConfigurationHandler.controllerType;
		if(controlType != CONTROL_TYPE.FORCEPAD_GESTURE && controlType != CONTROL_TYPE.FORCE_EXTENSION) {
			GameObject.Find("Widget").GetComponent<RawImage>().enabled = false;
		}
		playerStatus.DisableControl ();
		playerStatus.EnableControl(controlType);
		travelModelInterface.SetTargetGestureType (TRAVEL_TYPE.FORCE_EXT);
		travelModelInterface.SetGestureType(TRAVEL_TYPE.FORCE_EXT);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
