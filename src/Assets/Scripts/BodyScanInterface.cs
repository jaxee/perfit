using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Windows.Kinect;

public class BodyScanInterface : MonoBehaviour {
	public KinectSensor _Sensor;
	public int deviceCount = 0;

	public BodySourceManager bdySrcMgr;
	public DepthSourceManager dpthSrcMgr;

	public Measurements measurementsScpt;

    public GameObject placementUIGroup;
	public GameObject placementInstructions;
	public GameObject placementSideStepsUI;
	public GameObject placementStepsFrontSelected;
	public GameObject placementStepsSideSelected;

	public GameObject frontPlacementUI;
	public GameObject frontPlacementPreInstructions;
	public GameObject frontPlacementDuringInstructions;

	public GameObject sidePlacementUI;
	public GameObject sidePlacementPreInstructions;
	public GameObject sidePlacementDuringInstructions;

	public GameObject finishScanUI;

	public InputField firstName;
	public InputField lastName;
	public InputField email;
	public InputField password;

	public Text placeInstOne;
	public Text placeInstTwo;
	public Text placeInstThree;
	public Text placeInstFour;

    public GameObject chosenThree;
    public GameObject chosenFour;

	private string first_name, last_name, user_email, user_password = null;

	// Use this for initialization
	void Start () {
		//if (KinectSensor.GetDefault ().IsAvailable) {
			bdySrcMgr = GameObject.FindGameObjectWithTag ("BodySourceManager").GetComponent<BodySourceManager> ();
			dpthSrcMgr = GameObject.FindGameObjectWithTag ("DepthSourceManager").GetComponent<DepthSourceManager> ();
		//} else {
		//	Debug.LogError ("Kinect is not connected");
		//}

		measurementsScpt = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Measurements>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SaveInformation() {
		Debug.Log ("The values that you entered are: ");
		first_name = firstName.text;
		last_name = lastName.text;
		user_email = email.text;
		user_password = password.text;

		Debug.Log (first_name + " " + last_name + " | " + user_email + " " + user_password);
	}

	public void StartBodyScan() {
		Debug.Log ("Start body scan");

		bdySrcMgr.StartBodySourceManager ();
		dpthSrcMgr.StartDepthSourceManager ();

		StartCoroutine (WaitForPosition ());
	}

	IEnumerator WaitForPosition() {
        placementUIGroup.gameObject.SetActive(true);

        yield return new WaitForSeconds(5);

        placeInstOne.gameObject.SetActive(false);
        placeInstTwo.gameObject.SetActive(true);

        Debug.Log ("waiting done");
		bool isNotTracked = true;

        Debug.Log("Bodies:" + measurementsScpt.GetNumberOfBodies());
		if (measurementsScpt.GetNumberOfBodies () > 0) {
			Debug.Log ("Body found");

			while (isNotTracked) {
				if (measurementsScpt.isHeadAndToesTracked ()) {
                    placementInstructions.gameObject.SetActive (false);
					placementSideStepsUI.gameObject.SetActive (true);

					StartCoroutine (WaitForPositionFront ());
					isNotTracked = false;
				} else {
					placeInstTwo.gameObject.SetActive (false);

					if (measurementsScpt.GetNumberOfBodies () > 0) {
						placeInstThree.gameObject.SetActive (true);
					} else {
						placeInstFour.gameObject.SetActive (true);
					}
				}
			}
		} else {
			Debug.LogError ("Could not find person in front of camera");
		}

	}

	IEnumerator WaitForPositionFront() {
		frontPlacementUI.gameObject.SetActive (true);

		yield return new WaitForSeconds (5);

		frontPlacementPreInstructions.gameObject.SetActive (false);
		frontPlacementDuringInstructions.gameObject.SetActive (true);

		StartCoroutine (ScanPositionFront ());

	}

	IEnumerator ScanPositionFront() {
		measurementsScpt.startScanning = true;
		measurementsScpt.startFrontScan = true;

		yield return new WaitForSeconds(5);

		frontPlacementUI.gameObject.SetActive (false);
		placementStepsFrontSelected.gameObject.SetActive (false);

		StartCoroutine (WaitForPositionSide ());
	}

	IEnumerator WaitForPositionSide() {
		sidePlacementUI.gameObject.SetActive (true);
		placementStepsSideSelected.gameObject.SetActive (true);

		yield return new WaitForSeconds (5);

		sidePlacementPreInstructions.gameObject.SetActive (false);
		sidePlacementDuringInstructions.gameObject.SetActive (true);

		StartCoroutine (ScanPositionSide());
	}

	IEnumerator ScanPositionSide() {
		measurementsScpt.startScanning = true;
		measurementsScpt.startSideScan = true;

		yield return new WaitForSeconds (5);

		placementUIGroup.gameObject.SetActive (false);
        chosenThree.gameObject.SetActive(false);
        chosenFour.gameObject.SetActive(true);
		finishScanUI.gameObject.SetActive (true);
	}
}
