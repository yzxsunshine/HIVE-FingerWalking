using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;

public class ConfigurationHandler {
	private static XmlDocument xmlDoc;
	private static string CONFIG_FILENAME = "config.xml";
	public static string CONFIG_DIRECTORY = "";
	public static string UIVA_IP_ADDRESS;

	public static int subjectID;
	public static CONTROL_TYPE controllerType;

	public static ConfigurationHandler configurationInstance = new ConfigurationHandler();
	public static ForcePadParams forcePadParams;
	public static JoystickParams joystickParams;
	public static int StartTrialPass;
	public static int StartTrialID;

	public ConfigurationHandler () {
		xmlDoc = new XmlDocument();
		CONFIG_DIRECTORY = Application.dataPath;
		forcePadParams = new ForcePadParams ();
		joystickParams = new JoystickParams ();
		StartTrialPass = 0;
		StartTrialID = -1;
		readXMLConfig (CONFIG_DIRECTORY + "/" + CONFIG_FILENAME);
	}

	private static void readXMLConfig(string filePath) {
		xmlDoc.RemoveAll();
		if(File.Exists(filePath)) {
			xmlDoc.Load(filePath);
		}else {
			Debug.LogError("ERROR: Cannot find game config XML file to load from");
		}
		
		XmlNode devicesNode = xmlDoc.SelectSingleNode("config/devices");
		UIVA_IP_ADDRESS = devicesNode.Attributes.GetNamedItem("uiva_ip_address").Value;

		XmlNode subjectNode = xmlDoc.SelectSingleNode("config/subject");
		subjectID = int.Parse (subjectNode.Attributes.GetNamedItem ("subject_id").Value);
		string controllerTypeStr = subjectNode.Attributes.GetNamedItem ("controller_type").Value;
		if (controllerTypeStr.CompareTo ("joystick") == 0 || controllerTypeStr[0] == 'j') {
			controllerType = CONTROL_TYPE.JOYSTICK;
		}
		else if (controllerTypeStr.CompareTo ("forcepad") == 0 || controllerTypeStr[0] == 'f') {
			controllerType = CONTROL_TYPE.FORCEPAD_GESTURE;
		}
		else if (controllerTypeStr.CompareTo ("body") == 0 || controllerTypeStr[0] == 'b') {
			controllerType = CONTROL_TYPE.BODY_DRIVEN;
		}
		StartTrialPass = int.Parse (subjectNode.Attributes.GetNamedItem ("start_pass").Value);
		StartTrialID = int.Parse (subjectNode.Attributes.GetNamedItem ("start_id").Value);

		XmlNode joystickNode = xmlDoc.SelectSingleNode("config/travel_interfaces/joystick");
		joystickParams.walkingSpeed = float.Parse(joystickNode.Attributes.GetNamedItem ("walking_speed").Value);
		joystickParams.walkingAngularSpeed = float.Parse(joystickNode.Attributes.GetNamedItem ("walking_angular_speed").Value);

		joystickParams.segwaySpeed = float.Parse(joystickNode.Attributes.GetNamedItem ("segway_speed").Value);
		joystickParams.segwayAngularSpeed = float.Parse(joystickNode.Attributes.GetNamedItem ("segway_angular_speed").Value);

		joystickParams.surfingSpeed = float.Parse(joystickNode.Attributes.GetNamedItem ("surfing_speed").Value);
		joystickParams.surfingPitchSpeed = float.Parse(joystickNode.Attributes.GetNamedItem ("surfing_pitch_speed").Value);
		joystickParams.surfingYawSpeed = float.Parse(joystickNode.Attributes.GetNamedItem ("surfing_yaw_speed").Value);

		XmlNode forcepadNode = xmlDoc.SelectSingleNode("config/travel_interfaces/forcepad");
		forcePadParams.minFingerMove = float.Parse(forcepadNode.Attributes.GetNamedItem ("min_finger_move").Value);
		forcePadParams.minFingerSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("min_finger_speed").Value);
		forcePadParams.maxFingerSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("max_finger_speed").Value);
		forcePadParams.walkingSpeedScale = float.Parse(forcepadNode.Attributes.GetNamedItem ("walking_speed_scale").Value);
		forcePadParams.walkingAngularSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("walking_angular_speed").Value);

		forcePadParams.segwaySpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("segway_speed").Value);
		forcePadParams.segwayPressureAngularSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("segway_pressure_angular_speed").Value);
		forcePadParams.segwayAngularSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("segway_angular_speed").Value);
		forcePadParams.segwayOffsetSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("segway_offset_speed").Value);

		forcePadParams.surfingSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("surfing_speed").Value);
		forcePadParams.surfingPitchSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("surfing_pitch_speed").Value);
		forcePadParams.surfingYawSpeed = float.Parse(forcepadNode.Attributes.GetNamedItem ("surfing_yaw_speed").Value);

		Debug.Log(string.Format("All configuration loaded from XML file...{0}", filePath));
	}
}
