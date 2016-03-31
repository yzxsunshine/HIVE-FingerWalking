/////////////////////////////////////////////////////////////////////////////////////////
//
// Author: Jia Wang
//			WPI CS HIVE
//			wangjia@wpi.edu
// 
// Date: 08/30/2010
// (revision) 02/07/2011 (OMG! I am so lazy?)
//
// Description:
//This script manages multiple devices across the whole game lifetime
// It manages two BPacks, the TactaCage system, and a Wii Fit balance board
// 
// ---> BPack data and WiiFit data are extracted from the UIVA (Unity Indie VRPN adapter)
//       framework. Compared to UWIndie and BPack COM communication, life is much better now.
// 
//       The IP address of the machine which runs UIVA_Server has to be specified (localhost if on the same machine)
//        	public String UIVA_IP_Address = "localhost";
//       
//       Follow these steps to make it work:
// 		(1) Start BPack VRPN server (set vrpn.cfg and update the COM port name as well as multiple BPacks)
//       (2) Start WiiFit VRPN server (set vrpn.cfg and pay attention to any Wiimote connected to the same machine)
//       (3) Start UIVA_Server (set UIVA_Server.cfg and give the VRPN name of each device)
//       (4) Write C# script in Unity to request device data:
// 		
//		WiiFit: GetWiiFitRawData(int which, out double tl, out double tr, out double bl, out double br, out String buttons);
//				 GetWiiFitGravityData(int which, out double weight, out double gravX, out double gravY, out String buttons);
//       BPack: GetBPackRawData(int which, out double accelX, out double accelY, out double accelZ);
//                 GetBPackTiltData(int which, out double pitch, out double roll);
//		An overloaded function exists for each of the above to set the default 'which' to 1, which means there is only one such device.
//
// ---> To set data to TactaCage wind system, we are using TBCSharp (DLL file).
// 		It will be supported by UIVA soon so everything will be abstracted.
//					 SetWindSpeed(leftFans, rightFans, centerFans)
// 		rightFanSpeed, leftFanSpeed, and centerFanSpeed are just variables to make debug easier
//
//////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
//using System.IO.Ports;
//using TBCSharp;	//Wind system library (TBCSharp.dll) for wind simulation

public class DevicesManager : MonoBehaviour {
	enum DataSmoothAlgorithm { MOV_AVG, SIM_EXP, LIN_EXP, NONE};
	
	//-------------------------------------------------Setup UIVA ---------------------------------------- //
	UIVA_Client uiva;
	
	//----------------------------------------------Sensor data recording----------------------------------------//
	public bool devsConnected = false;

	//-----------------------------------------HMD (SpacePoint Fusion) sensor related--------------------//
	private double[] hmd_quat = new double[4] {0.0, 0.0, 0.0, 0.0};
	private string hmd_buttons = "";
	public Vector3 hmd_euler;
	private Vector3 calib_euler;
	private HMDCamera camScript;
	//-----------------------------------------------TactaCage related------------------------------------//	
	/* Wind System */
	/*TactaBoard tb;
	private byte[] rightFans = new byte[3] { 0x06, 0x08, 0x09 };		//Right side fans
	private byte[] leftFans = new byte[3] { 0x04, 0x0A, 0x0B };		//Left side fans
	private byte[] centerFans = new byte[1] {0x05}; 						//Center fan
	public float rightFanSpeed = 0.0f;
	public float leftFanSpeed = 0.0f;
	public float centerFanSpeed = 0.0f;
	*/
	
	//-------------------------------------------Functions-------------------------------------------------//     
	/// Initialization of BPack, WiiFit and Wind System
	void Awake() {
		uiva = new UIVA_Client(ConfigurationHandler.UIVA_IP_ADDRESS);
		
		//tb = new TactaBoard(Configuration.TACTA_BOARD_COM_PORT, 0);		//Init wind system
		//tb.Open();
		
		//DontDestroyOnLoad(this);
		devsConnected = true;
	}

	void Start() {
		camScript = camScript = GameObject.Find("Main Camera").GetComponent<HMDCamera>();
		CalibrateCamera(); // we should remove it after the test
	
	}
	/// Update is called once per frame
	void Update () {
        //uiva.GetWiimoteTiltData(out pitch_ABPack, out roll_ABPack, out wmButtons);
		//pitch_ABPack = -pitch_ABPack;
		//uiva.GetWiiFitGravityData(out wfWeight, out wfPitch, out wfRoll, out wfButton);
		
		//Smooth board data from either BPack or WiiFit to guarantee smoother camera
		//DataSmoothing();

		uiva.GetSpacePointFusionData(ref hmd_quat, out hmd_buttons);
		ComputeHMDEulerAngles();
	}
	
	void ComputeHMDEulerAngles()
	{		
		Quaternion latest = new Quaternion((float)hmd_quat[0], (float)hmd_quat[1], 
					(float)hmd_quat[2], (float)hmd_quat[3]);
		Vector3 cam = latest.eulerAngles;
		
		float GX = -cam.y;
		float GY = -cam.z;
		float GZ = -cam.x;
			
		if(GX>180.0f) GX-=360.0f;
		if(GX<-180.0f) GX+=360.0f;
			
		if(GZ>180.0f) GZ-=360.0f;
		if(GZ<-180.0f) GZ+=360.0f;

		float xRad = Mathf.Sin(Mathf.Deg2Rad*(GY+90.0f));
		float zRad = Mathf.Sin(Mathf.Deg2Rad*(GY+180.0f));
		float resultantX = (GX*xRad)-(GZ*zRad);
		float resultantZ = (GX*zRad)+(GZ*xRad);
		
		if (resultantX < -360.0f)
			resultantX += 360.0f;
		if (resultantX > 360.0f)
			resultantX -= 360.0f;
			
		if (resultantZ < -360.0f)
			resultantZ += 360.0f;
		if (resultantZ > 360.0f)
			resultantZ -= 360.0f;
		
		//transform.eulerAngles = new Vector3(resultantX, -GY, resultantZ);
		hmd_euler = new Vector3(-resultantZ, -GY-90.0f, resultantX);
		
		//Find the camera script and call its UpdateCamera() function
			//if the current level is a study level, we will get is camera and update it later
			//Camera myCam = ;
        camScript.UpdateCamera(hmd_euler.x - calib_euler.x, hmd_euler.y - calib_euler.y,
		                       hmd_euler.z - calib_euler.z);
	}
	
	public void CalibrateCamera()
	{
		//Set the current orientation values of SpacePoint Fusion sensor as the forward direction (zero reference)
		//uiva.GetSpacePointFusionData(ref hmd_quat, out hmd_buttons);
		//ComputeHMDEulerAngles();
		
		//hmd_euler now hold the latest orientation
		//calib_euler = hmd_euler;
	}
	
	public Vector3 GetHMDCalibEuler() {
		return calib_euler;
	}
	
	public void SetHMDCalibEuler(Vector3 eulerFromFile) {
		calib_euler = eulerFromFile;
	}
		
	
	void DisconnectAll()
	{
		uiva.Disconnect();
	}
}
