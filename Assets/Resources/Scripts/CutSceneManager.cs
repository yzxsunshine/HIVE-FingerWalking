using UnityEngine;
using System.Collections;

public class CutSceneManager : MonoBehaviour {
	RectTransform cutsceneTop;
	RectTransform cutsceneBottom;
	public float maxHeight = 160.0f;
	public float minHeight = 0.0f;
	public float fadeSpeed = 2.0f;
	public float width = 0.0f;
	public bool cutSceneOn = false;
	// Use this for initialization
	void Start () {
		cutsceneTop = GameObject.Find("CutSceneTop").GetComponent<RectTransform> ();
		cutsceneBottom = GameObject.Find("CutSceneBottom").GetComponent<RectTransform> ();
		width = Screen.width;
		maxHeight = Screen.height / 3.6f;
		cutsceneTop.sizeDelta = new Vector2(width, minHeight);
		cutsceneBottom.sizeDelta = new Vector2(width, minHeight);
		//cutSceneOn = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(cutSceneOn && cutsceneTop.sizeDelta.y < maxHeight) {
			cutsceneTop.sizeDelta = new Vector2(width, cutsceneTop.sizeDelta.y + fadeSpeed);
			cutsceneBottom.sizeDelta = new Vector2(width, cutsceneBottom.sizeDelta.y + fadeSpeed);
		}
		if(!cutSceneOn && cutsceneTop.sizeDelta.y > 0) {
			cutsceneTop.sizeDelta = new Vector2(width, cutsceneTop.sizeDelta.y - fadeSpeed);
			cutsceneBottom.sizeDelta = new Vector2(width, cutsceneBottom.sizeDelta.y - fadeSpeed);
		}
	}
}
