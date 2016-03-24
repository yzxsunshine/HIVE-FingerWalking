using UnityEngine;
using System.Collections;

public class WallTriggerManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		BoxCollider[] boxes = GetComponentsInChildren<BoxCollider> ();
		foreach (BoxCollider box in boxes) {
			if(box.isTrigger == true && box.gameObject.GetComponent<WallCollisionHandler>() == null) {
				box.gameObject.AddComponent<WallCollisionHandler>();
			}
		}
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in renderers) {
			renderer.enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
