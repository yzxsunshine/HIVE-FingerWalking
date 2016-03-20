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
	public static JoystickSingleModeParams joystickSingleModeParams;
	public static int StartTrialPass;
	public static int StartTrialID;
	public static float FORCE_EXT_LIMIT = 0.3f;

	public ConfigurationHandler () {
		xmlDoc = new XmlDocument();
		CONFIG_DIRECTORY = Application.dataPath;
		forcePadParams = new ForcePadParams ();
		joystickParams = new JoystickParams ();
		joystickSingleModeParams = new JoystickSingleModeParams ();
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
		if (controllerTypeStr.CompareTo ("joystick") == 0) {
			controllerType = CONTROL_TYPE.JOYSTICK;
		}
		else if (controllerTypeStr.CompareTo ("forcepad") == 0) {
			controllerType = CONTROL_TYPE.FORCEPAD_GESTURE;
		}
		else if (controllerTypeStr.CompareTo ("body") == 0) {
			controllerType = CONTROL_TYPE.BODY_DRIVEN;
		}
		else if (controllerTypeStr.CompareTo ("force_extension") == 0) {
			controllerType = CONTROL_TYPE.FORCE_EXTENSION;
		}
		else if (controllerTypeStr.CompareTo ("joystick_single_mode") == 0) {
			controllerType = CONTROL_TYPE.JOYSTICK_SINGLE_MODE;
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

		XmlNode joystickSingleModeNode = xmlDoc.SelectSingleNode("config/travel_interfaces/joystick_single_mode");
		joystickSingleModeParams.minSpeed = float.Parse(joystickSingleModeNode.Attributes.GetNamedItem("min_speed").Value);
		joystickSingleModeParams.maxSpeed = float.Parse(joystickSingleModeNode.Attributes.GetNamedItem("max_speed").Value);
		joystickSingleModeParams.angularSpeed = float.Parse(joystickSingleModeNode.Attributes.GetNamedItem("angular_speed").Value);
		Debug.Log(string.Format("All configuration loaded from XML file...{0}", filePath));
	}
}
