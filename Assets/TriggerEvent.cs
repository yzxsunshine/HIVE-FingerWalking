using UnityEngine;
using System.Collections;

public class TriggerEvent : MonoBehaviour {
	private GameObject Character;
	private float[] angleList;
	public float selfRotateAngle = 1.0f;
	public float starDistance = 20.0f;
	// Use this for initialization
	void Start () {
		Character = GameObject.Find ("Character");
		angleList = new float[6]{-108, -72, -36, 36, 72, 108};
	}
	// Update is called once per frame
	void Update () {
		transform.Rotate (0, selfRotateAngle, 0);
	}
	
	// 36, 72, 108, -36, -72, -108
	// 1 (sqrt(5) + 1) / 2
	void OnTriggerEnter() {
		GameObject cube = Instantiate(Resources.Load("Prefabs/JackoLantern", typeof(GameObject))) as GameObject;
		Vector3 forward = Character.transform.forward;
		int rand_index = Random.Range (1, 6);

		Vector3 nextDirection = Quaternion.AngleAxis(angleList[rand_index], Vector3.up) * forward;
		cube.transform.position = transform.position + nextDirection * starDistance;
		GameObject.Destroy (gameObject);
	}
}
