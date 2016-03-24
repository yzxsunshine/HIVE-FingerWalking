using UnityEngine;
using System.Collections;
using System.IO;

public class StudyRecorder : MonoBehaviour {
	public int subjectID;
	public int currentTrialID;
	public TRAVEL_TYPE currentTrialTravelType;
	public LEVEL_TYPE currentTrialLevel;
	public StreamWriter currentTrialWriter;
	public StreamWriter contextSwitchWriter;
	private string trialDirectory;
	private bool ready = false;

	// Use this for initialization
	void Start () {
		string directoryName = "subject_" + ConfigurationHandler.subjectID;
		trialDirectory = Application.dataPath + "/" + directoryName;
		try
		{
			if (!Directory.Exists(trialDirectory))
			{
				Directory.CreateDirectory(trialDirectory);
			}
			else
			{

				//Application.Quit();	// don't overwrite existing study
			}
			
		}
		catch (IOException ex)
		{
			Application.Quit();	// don't overwrite existing study
		}

		string fileName = trialDirectory + "/subject_" + ConfigurationHandler.subjectID;
		fileName += "_context_switch.txt";
		if (File.Exists (fileName)) {
			File.Delete(fileName);
		} 
		contextSwitchWriter = File.CreateText (fileName);
		string line = "#Time To Switch#\t#Num of Incorrect Switch#\t#Target Mode#\t#Current Mode#";
		contextSwitchWriter.WriteLine(line);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public StreamWriter GenerateFileWriter(int controller, int level, int travelType, int numPasses) {
		string fileName = trialDirectory + "/subject_" + ConfigurationHandler.subjectID;
		fileName += "_trial_" + controller + "_" + level + "_" + travelType + "_" + numPasses;
		fileName += ".txt";
		if (File.Exists (fileName)) {
			File.Delete(fileName);
		} 
		currentTrialWriter = File.CreateText (fileName);
		ready = true;
		return currentTrialWriter;
	}

	public StreamWriter GenerateCogTrialFileWriter(int controller, int sequenceID, int travelType) {
		string fileName = trialDirectory + "/subject_" + ConfigurationHandler.subjectID;
		fileName += "_trial_" + controller + "_" + sequenceID + "_" + travelType;
		fileName += ".txt";
		if (File.Exists (fileName)) {
			File.Delete(fileName);
		} 
		currentTrialWriter = File.CreateText (fileName);
		ready = true;
		return currentTrialWriter;
	}


	public bool IsReady() {
		return ready;
	}

	public void StopTrialFileWriter () {
		currentTrialWriter.Flush ();
		currentTrialWriter.Close ();
		currentTrialWriter = null;
		ready = false;
	}

	public bool RecordLine(string line) {
		if (currentTrialWriter != null) {
			currentTrialWriter.WriteLine(line);
			return true;
		}
		return false;
	}

	public void StopContextSwitchRecorder () {
		contextSwitchWriter.Flush ();
		contextSwitchWriter.Close ();
		contextSwitchWriter = null;
	}

	public bool RecordContextSwitch(float timeStamp, int errorSwitchNum, TRAVEL_TYPE targetMode, TRAVEL_TYPE currentMode) {
		string line = "" + timeStamp + "\t" + errorSwitchNum + "\t";
		switch (targetMode) {
		case TRAVEL_TYPE.WALKING:
			line += "Walking";
			break;
		case TRAVEL_TYPE.SEGWAY:
			line += "Segway";
			break;
		case TRAVEL_TYPE.SURFING:
			line += "Surfing";
			break;
		}
		line += "\t";
		switch (currentMode) {
		case TRAVEL_TYPE.WALKING:
			line += "Walking";
			break;
		case TRAVEL_TYPE.SEGWAY:
			line += "Segway";
			break;
		case TRAVEL_TYPE.SURFING:
			line += "Surfing";
			break;
		}
		if (contextSwitchWriter != null) {
			contextSwitchWriter.WriteLine(line);
			return true;
		}
		return false;
	}
}
