using UnityEngine;
using System.Collections;

public class TrialSequenceGenerator {
	public static int [,] lattinSquare = new int[9, 9] {{0,8,1,7,2,6,3,5,4},
														{1,0,2,8,3,7,4,6,5},
														{2,1,3,0,4,8,5,7,6},
														{3,2,4,1,5,0,6,8,7},
														{4,3,5,2,6,1,7,0,8},
														{5,4,6,3,7,2,8,1,0},
														{6,5,7,4,8,3,0,2,1},
														{7,6,8,5,0,4,1,3,2},
														{8,7,0,6,1,5,2,4,3}} ;
	public TrialSequenceGenerator() {

	}

	public TrialType[] GenerateByLattinSquare(int subjectID) {
		int lattinID = subjectID % 9;
		TrialType[] trials = new TrialType[9];
		for (int i=0; i<9; i++) {
			int difficultLevel = lattinSquare[lattinID, i] % 3;
			int travelType = lattinSquare[lattinID, i] / 3;
			trials[i] = new TrialType();
			switch (difficultLevel) {
			case 0:
				trials[i].level = LEVEL_TYPE.EASY;
				break;
			case 1:
				trials[i].level = LEVEL_TYPE.MEDIUM;
				break;
			case 2:
				trials[i].level = LEVEL_TYPE.HARD;
				break;
			}

			switch (travelType) {
			case 0:
				trials[i].mode = TRAVEL_TYPE.WALKING;
				break;
			case 1:
				trials[i].mode = TRAVEL_TYPE.SEGWAY;
				break;
			case 2:
				trials[i].mode = TRAVEL_TYPE.SURFING;
				break;
			}
		}
		return trials;
	}
}
