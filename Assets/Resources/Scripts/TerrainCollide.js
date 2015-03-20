private var GMScript;

function Start()   {
	GMScript = GameObject.Find("GameManager").GetComponent("GameManager");
}

function Update () {
}

function OnTriggerEnter(other: Collider) {
	GMScript.terrainCollidePenalty();
	other.transform.Translate(0.0, 200.0, 0.0);
}