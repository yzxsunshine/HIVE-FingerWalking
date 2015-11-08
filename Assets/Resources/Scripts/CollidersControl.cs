using UnityEngine;
using System.Collections;

public class CollidersControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject[] walls = GameObject.FindGameObjectsWithTag("TriggerWall");
		for (int i=0; i<walls.Length; i++) {
			GameObject trigger = Instantiate<GameObject>(walls[i]) as GameObject;
			trigger.transform.parent = walls[i].transform.parent;

			trigger.transform.transform.position = walls[i].transform.position;
			trigger.transform.localPosition = walls[i].transform.localPosition;

			trigger.transform.rotation = walls[i].transform.rotation;
			trigger.transform.localRotation = walls[i].transform.localRotation;

			trigger.transform.localScale = walls[i].transform.localScale;


			//trigger.transform.localScale = new Vector3(walls[i].transform.localScale.x * transform.localScale.x
			//                                           , walls[i].transform.localScale.y * transform.localScale.y
			//                                           , walls[i].transform.localScale.z * transform.localScale.z);
			BoxCollider boxCollider = trigger.GetComponent<BoxCollider>();
			if (boxCollider != null) {
				boxCollider.size = new Vector3(1.2f, 1.0f, 1.0f);
				boxCollider.isTrigger = true;
			}

			CapsuleCollider cylinderCollider = trigger.GetComponent<CapsuleCollider>();
			if (cylinderCollider != null) {
				cylinderCollider.radius = 0.6f;
				cylinderCollider.isTrigger = true;
			}
			trigger.AddComponent<CollisionEvent>();
			trigger.GetComponent<MeshRenderer>().enabled = false;
			walls[i].GetComponent<MeshRenderer>().enabled = false;

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DisableAllBlocker() {
		GameObject[] walls = GameObject.FindGameObjectsWithTag("TriggerWall");
		for (int i=0; i<walls.Length; i++) {
			BoxCollider boxCollider = walls[i].GetComponent<BoxCollider>();
			if (boxCollider != null && boxCollider.isTrigger == false) {
				boxCollider.enabled = false;
			}
			
			CapsuleCollider cylinderCollider = walls[i].GetComponent<CapsuleCollider>();
			if (cylinderCollider != null && cylinderCollider.isTrigger == false) {
				cylinderCollider.enabled = false;
			}
		}
	}

	public void EnableAllBlocker() {
		GameObject[] walls = GameObject.FindGameObjectsWithTag("TriggerWall");
		for (int i=0; i<walls.Length; i++) {
			BoxCollider boxCollider = walls[i].GetComponent<BoxCollider>();
			if (boxCollider != null && boxCollider.isTrigger == false) {
				boxCollider.enabled = true;
			}
			
			CapsuleCollider cylinderCollider = walls[i].GetComponent<CapsuleCollider>();
			if (cylinderCollider != null && cylinderCollider.isTrigger == false) {
				cylinderCollider.enabled = true;
			}
		}
	}
}
