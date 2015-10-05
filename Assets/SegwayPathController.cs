using UnityEngine;
using System.Collections;

public class SegwayPath {
	public int[] wayPoints;
}

public class SegwayPathController : MonoBehaviour {
	private SegwayPath[, , ] pathes;
	private int[] currentWayPts;
	// Use this for initialization
	void Start () {
		pathes = new SegwayPath[3, 4, 2];
		// easy
		pathes [0, 0, 0].wayPoints = new int[7] {0, 5, 4, 12, 7 ,6, 1};
		pathes [0, 0, 1].wayPoints = new int[7] {0, 4, 5, 15, 10, 11, 3};

		pathes [0, 1, 0].wayPoints = new int[7] {1, 7, 6, 13, 9, 8, 2};
		pathes [0, 1, 1].wayPoints = new int[7] {1, 6, 7, 12, 5, 4, 0};

		pathes [0, 2, 0].wayPoints = new int[7] {2, 9, 8, 14, 11, 10, 3};
		pathes [0, 2, 1].wayPoints = new int[7] {2, 8, 9, 13, 6, 7, 1};

		pathes [0, 3, 0].wayPoints = new int[7] {3, 11, 10, 15, 5, 4, 0};
		pathes [0, 3, 1].wayPoints = new int[7] {3, 10, 11, 14, 8, 9, 2};

		// medium
		pathes [1, 0, 0].wayPoints = new int[17] {0, 4, 5, 15, 19, 23, 16, 20, 24, 22, 18, 21, 17, 13, 6, 7, 1};
		pathes [1, 0, 1].wayPoints = new int[17] {0, 5 ,4, 12, 16, 23, 19, 22, 24, 20, 17, 21, 18, 14, 11, 10, 3};
		
		pathes [1, 1, 0].wayPoints = new int[17] {1, 6, 7, 12, 16, 20, 17, 21, 24, 23, 19, 22, 18, 14, 8, 9, 2};
		pathes [1, 1, 1].wayPoints = new int[17] {1, 7, 6, 13, 17, 20, 16, 23, 24, 21, 18, 22, 19, 15, 5, 4, 0};
		
		pathes [1, 2, 0].wayPoints = new int[17] {2, 8, 9, 13, 17, 21, 18, 22, 24, 20, 16, 23, 19, 15, 10, 11, 3};
		pathes [1, 2, 1].wayPoints = new int[17] {2, 9, 8, 14, 18, 21, 17, 20, 24, 22, 19, 23, 16, 12, 7, 6, 1};
		
		pathes [1, 3, 0].wayPoints = new int[17] {3, 10, 11, 14, 18, 22, 19, 23, 24, 21, 17, 20, 16, 12, 4, 5, 0};
		pathes [1, 3, 1].wayPoints = new int[17] {3, 11, 10, 15, 19, 22, 18, 21, 24, 23, 16, 20, 17, 13, 9, 8, 2};

		// hard
		pathes [2, 0, 0].wayPoints = new int[17] {0, 5, 4, 12, 16, 23, 24, 22, 18, 21, 24, 20, 17, 13, 6, 7, 1};
		pathes [2, 0, 1].wayPoints = new int[17] {0, 4, 5, 15, 19, 22, 24, 23, 16, 20, 24, 21, 18, 14, 11, 10, 3};
		
		pathes [2, 1, 0].wayPoints = new int[17] {1, 7, 6, 13, 17, 21, 24, 23, 19, 22, 24, 21, 18, 14, 8, 9, 2};
		pathes [2, 1, 1].wayPoints = new int[17] {1, 6, 7, 12, 16, 23, 24, 20, 17, 21, 24, 22, 19, 15, 5, 4, 0};
		
		pathes [2, 2, 0].wayPoints = new int[17] {2, 9, 8, 14, 18, 22, 24, 20, 16, 23, 24, 22, 19, 15, 10, 11, 3};
		pathes [2, 2, 1].wayPoints = new int[17] {2, 8, 9, 13, 17, 20, 24, 21, 18, 22, 24, 23, 16, 12, 7, 6, 1};
		
		pathes [2, 3, 0].wayPoints = new int[17] {3, 11, 10, 15, 19, 23, 24, 21, 17, 20, 24, 23, 16, 12, 4, 5, 0};
		pathes [2, 3, 1].wayPoints = new int[17] {3, 10, 11, 14, 18, 21, 24, 22, 19, 23, 24, 20, 17, 13, 9, 8, 2};
	}

	void SetSegwayPath (int difficulty, int startPt, int LorR) {
		if (difficulty > 2) {
			difficulty = 2;
		}
		startPt = startPt % 4;
		if (LorR <= 0)
			LorR = 0;
		else
			LorR = 1;

		int[] wayPoints = pathes[difficulty, startPt, LorR].wayPoints;
		for (int i=0; i<wayPoints.Length; i++) {
			string objName = "SegwayWaypoint_" + wayPoints[i];
			GameObject obj = GameObject.Find(objName);
			if (i < wayPoints.Length - 1) {
				string blockerName = "BarrierTape-"+wayPoints[i]+"-"+wayPoints[i+1];
				GameObject blocker = GameObject.Find(blockerName);
				BoxCollider[] colliders = blocker.GetComponentsInChildren<BoxCollider>();
				for(int j=0; j<colliders.Length; j++) {
					colliders[j].enabled = false;
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
