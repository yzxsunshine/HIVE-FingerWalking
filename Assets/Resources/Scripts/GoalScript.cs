using UnityEngine;
using System.Collections;

public class GoalScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter() {
		GameObject.Destroy(this.gameObject);
	}

	public static GameObject CreateGoal(Vector3 position) {
		GameObject goal = Instantiate(Resources.Load("Prefabs/Goal", typeof(GameObject))) as GameObject;
		position.y = 0;
		goal.transform.position = position;
		return goal;
	}
}
