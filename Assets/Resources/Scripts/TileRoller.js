//This script rolls the terrain from behind the player to the front of her
//So the game creates an fake infinite world

var visited: boolean = false;

function RollTheTile(dest: Vector3) {
	transform.position = dest;
	visited = false;
}