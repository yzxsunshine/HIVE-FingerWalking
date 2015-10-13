using UnityEngine;
using System.Collections;
using System.IO;

public class StudyRecorder : MonoBehaviour {
	public int subjectID;
	public int currentTrialID;
	public TRAVEL_TYPE currentTrialTravelType;
	public LEVEL_TYPE currentTrialLevel;
	public StreamWriter currentTrialWriter;
	private string trialDirectory;
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
				Application.Quit();	// don't overwrite existing study
			}
			
		}
		catch (IOException ex)
		{
			Application.Quit();	// don't overwrite existing study
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public StreamWriter GenerateFileWriter(int controller, int level, int travelType) {
		string fileName = trialDirectory + "/subject_" + ConfigurationHandler.subjectID;
		fileName += "_trial_" + controller + "_" + level + "_" + travelType;
		fileName += ".txt";
		if (!File.Exists (fileName)) {
			currentTrialWriter = File.CreateText (fileName);
		} else {
			currentTrialWriter = File.AppendText(fileName);
		}
		return currentTrialWriter;
	}
	

	public void StopTrialFileWriter () {
		currentTrialWriter.Flush ();
		currentTrialWriter.Close ();
		currentTrialWriter = null;
	}

	public bool RecordLine(string line) {
		if (currentTrialWriter != null) {
			currentTrialWriter.WriteLine(line);
			return true;
		}
		return false;
	}
}
