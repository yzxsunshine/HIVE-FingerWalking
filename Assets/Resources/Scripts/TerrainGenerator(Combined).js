//This script is in charge of generating new terrains and destroy old ones
//Terrain Generation also includes the generation of bubbles and clouds within its range

var terrainSize = 2000;		//We use square terrain here, so width = length = 1000;
var terrainHeight = 0;

function OnTriggerEnter(other: Collider) {
	var XLeft = transform.position.x - terrainSize * 0.5;
	var XMiddle = transform.position.x;
	var XRight = transform.position.x + terrainSize * 0.5;
	var ZBottom = transform.position.z - terrainSize * 0.5;
	var ZMiddle = transform.position.z;
	var ZTop = transform.position.z + terrainSize * 0.5;
	
	//The collider came from left
	if(Mathf.Abs(other.transform.position.x - XLeft) < 100) {
		MoveTerrain(Vector3(XMiddle - 2 * terrainSize, terrainHeight, ZMiddle),
						Vector3(XMiddle + terrainSize, terrainHeight, ZMiddle));
		MoveTerrain(Vector3(XMiddle - 2 * terrainSize, terrainHeight, ZMiddle + terrainSize),
						Vector3(XMiddle + terrainSize, terrainHeight, ZMiddle + terrainSize));
		MoveTerrain(Vector3(XMiddle - 2 * terrainSize, terrainHeight, ZMiddle - terrainSize),
						Vector3(XMiddle + terrainSize, terrainHeight, ZMiddle - terrainSize));
	}
	if(Mathf.Abs(other.transform.position.x - XRight) < 100) {
		MoveTerrain(Vector3(XMiddle + 2 * terrainSize, terrainHeight, ZMiddle),
							Vector3(XMiddle - terrainSize, terrainHeight, ZMiddle));
		MoveTerrain(Vector3(XMiddle + 2 * terrainSize, terrainHeight, ZMiddle + terrainSize),
							Vector3(XMiddle - terrainSize, terrainHeight, ZMiddle + terrainSize));
		MoveTerrain(Vector3(XMiddle + 2 * terrainSize, terrainHeight, ZMiddle - terrainSize),
							Vector3(XMiddle - terrainSize, terrainHeight, ZMiddle - terrainSize));
	}
	if(Mathf.Abs(other.transform.position.z - ZTop) < 100) {
		MoveTerrain(Vector3(XMiddle, terrainHeight, ZMiddle + 2 * terrainSize),
							Vector3(XMiddle, terrainHeight, ZMiddle - terrainSize));
		MoveTerrain(Vector3(XMiddle - terrainSize, terrainHeight, ZMiddle  + 2 * terrainSize),
							Vector3(XMiddle - terrainSize, terrainHeight, ZMiddle  - terrainSize));
		MoveTerrain(Vector3(XMiddle + terrainSize, terrainHeight, ZMiddle  + 2 * terrainSize),
							Vector3(XMiddle + terrainSize, terrainHeight, ZMiddle - terrainSize));
	}
	if(Mathf.Abs(other.transform.position.z - ZBottom) < 100) {
		MoveTerrain(Vector3(XMiddle, terrainHeight, ZMiddle - 2 * terrainSize),
							Vector3(XMiddle, terrainHeight, ZMiddle + terrainSize));
		MoveTerrain(Vector3(XMiddle - terrainSize, terrainHeight, ZMiddle  - 2 * terrainSize),
							Vector3(XMiddle - terrainSize, terrainHeight, ZMiddle  + terrainSize));
		MoveTerrain(Vector3(XMiddle + terrainSize, terrainHeight, ZMiddle  - 2 * terrainSize),
							Vector3(XMiddle + terrainSize, terrainHeight, ZMiddle + terrainSize));
	}
}

function MoveTerrain(pos: Vector3, dest: Vector3) {
	var terrains: GameObject[];

	terrains = GameObject.FindGameObjectsWithTag("WorldPrimitive");
	
	for(var terr: GameObject in terrains) {
		if(terr.transform.position == pos) {
		terr.GetComponent("TileRoller").RollTheTile(dest);
		}
	}
}