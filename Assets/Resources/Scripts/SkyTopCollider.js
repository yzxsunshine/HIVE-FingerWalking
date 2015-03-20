
function Update () {
}

function OnTriggerEnter(other: Collider) {
	other.transform.Translate(Vector3(0.0, -7.0, 0.0));
}

