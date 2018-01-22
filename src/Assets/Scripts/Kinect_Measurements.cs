using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Face = Microsoft.Kinect.Face;

public class Kinect_Measurements : MonoBehaviour 
{
	public Material BoneMaterial;
	public GameObject BodySourceManager;

	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
	private Kinect_BodyManager _BodyManager;

    private HashSet<float> hipDepth = new HashSet<float>();
    private float initialHipDepth;

	private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
	{
		{ Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
		{ Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
		{ Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
		{ Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

		{ Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
		{ Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
		{ Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
		{ Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

		{ Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
		{ Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
		{ Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
		{ Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
		{ Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
		{ Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

		{ Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
		{ Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
		{ Kinect.JointType.HandRight, Kinect.JointType.WristRight },
		{ Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
		{ Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
		{ Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

		{ Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
		{ Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
		{ Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
		{ Kinect.JointType.Neck, Kinect.JointType.Head },
	};

	void Update () 
	{
		if (BodySourceManager == null)
		{
            Debug.LogError("No BodySourceManager");
            return;
		}
 
		_BodyManager = BodySourceManager.GetComponent<Kinect_BodyManager>();
		if (_BodyManager == null)
		{
            Debug.LogError("Cannot get script");
            return;
		}

        Kinect.Body[] data = _BodyManager.GetData();

        if (data == null)
		{
            Debug.LogError("No data");
			return;
		}

		List<ulong> trackedIds = new List<ulong>();
		foreach(var body in data)
		{
			if (body == null)
			{
                Debug.LogError("Body not tracked");
				continue;
			}

			if(body.IsTracked)
			{
				trackedIds.Add (body.TrackingId);

			}
		}

		List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

		// First delete untracked bodies
		foreach(ulong trackingId in knownIds)
		{
			if(!trackedIds.Contains(trackingId))
			{
				Destroy(_Bodies[trackingId]);
				_Bodies.Remove(trackingId);
			}
		}

		foreach(var body in data){
			if (body == null)
			{
                Debug.LogError("Body not tracked");
				continue;
			}

            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
				Measurements (body);
			}
		}
	}

	private void Measurements(Kinect.Body body) {
		double height = GetBodyHeight (body) * 100;
		double legLength = GetLegLength (body) * 100;
		double armLength = GetArmLength (body) * 100;
		double shoulderLength = getShoulderLength (body) * 100;
        double hip = getHipLength(body) * 100;

		Debug.Log ("***************************************************************************");
		Debug.Log ("Height: " + height + "\nLeg Length: " + legLength + "\nArm Length: " + armLength + "\nShoulder Length: " + shoulderLength + "\nHip: " + hip);
		Debug.Log ("***************************************************************************");
	}

	private GameObject CreateBodyObject(ulong id)
	{
		Debug.Log ("New Body");
		GameObject body = new GameObject("Body:" + id);

		for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
		{
			GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

			LineRenderer lr = jointObj.AddComponent<LineRenderer>();
			lr.SetVertexCount(2);
			lr.material = BoneMaterial;
			lr.SetWidth(0.05f, 0.05f);

			jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			jointObj.name = jt.ToString();
			jointObj.transform.parent = body.transform;
		}

		return body;
	}

	private double GetBodyHeight(Kinect.Body body) 
	{
		const double HEAD_DIVERGENCE = 0.1;

		/*foreach(KeyValuePair<Kinect.JointType, Kinect.Joint> j in body.Joints) {
			Debug.Log ("IsTracked? " + j.Value.TrackingState + " Key: " + j.Key + " | X: " + j.Value.Position.X + " Y: " + j.Value.Position.Y);
		} */

		var head = body.Joints[Kinect.JointType.Head];
		var neck = body.Joints[Kinect.JointType.SpineShoulder];
		var spine = body.Joints[Kinect.JointType.SpineMid];
		var waist = body.Joints[Kinect.JointType.SpineBase];
		var hipLeft = body.Joints[Kinect.JointType.HipLeft];
		var hipRight = body.Joints[Kinect.JointType.HipRight];
		var kneeLeft = body.Joints[Kinect.JointType.KneeLeft];
		var kneeRight = body.Joints[Kinect.JointType.KneeRight];
		var ankleLeft = body.Joints[Kinect.JointType.AnkleLeft];
		var ankleRight = body.Joints[Kinect.JointType.AnkleRight];
		var footLeft = body.Joints[Kinect.JointType.FootLeft];
		var footRight = body.Joints[Kinect.JointType.FootRight];

		int leftLegTrackedJoints = NumberOfTrackedJoints (hipLeft, kneeLeft, ankleLeft, footLeft);
		int rightLegTrackedJoints = NumberOfTrackedJoints (hipRight, kneeRight, ankleRight, footRight);

		double legLength = leftLegTrackedJoints > rightLegTrackedJoints ? Length(hipLeft, kneeLeft, ankleLeft, footLeft) : Length(hipRight, kneeRight, ankleRight, footRight);

		return Length(head, neck, spine, waist) + legLength + HEAD_DIVERGENCE;
	}

	private double GetLegLength(Kinect.Body body) {

		var hipLeft = body.Joints[Kinect.JointType.HipLeft];
		var hipRight = body.Joints[Kinect.JointType.HipRight];
		var kneeLeft = body.Joints[Kinect.JointType.KneeLeft];
		var kneeRight = body.Joints[Kinect.JointType.KneeRight];
		var ankleLeft = body.Joints[Kinect.JointType.AnkleLeft];
		var ankleRight = body.Joints[Kinect.JointType.AnkleRight];
		var footLeft = body.Joints[Kinect.JointType.FootLeft];
		var footRight = body.Joints[Kinect.JointType.FootRight];

		int leftLegTrackedJoints = NumberOfTrackedJoints (hipLeft, kneeLeft, ankleLeft, footLeft);
		int rightLegTrackedJoints = NumberOfTrackedJoints (hipRight, kneeRight, ankleRight, footRight);

		//int leftLegTrackedJoints = NumberOfTrackedJoints (hipLeft, kneeLeft, ankleLeft);
		//int rightLegTrackedJoints = NumberOfTrackedJoints (hipRight, kneeRight, ankleRight);

		double legLength = leftLegTrackedJoints > rightLegTrackedJoints ? Length(hipLeft, kneeLeft, ankleLeft, footLeft) : Length(hipRight, kneeRight, ankleRight, footRight);
		//double legLength = leftLegTrackedJoints > rightLegTrackedJoints ? Length(hipLeft, kneeLeft, ankleLeft) : Length(hipRight, kneeRight, ankleRight);

		return legLength;
	}

	private double GetArmLength(Kinect.Body body) {
		var rightShoulder = body.Joints[Kinect.JointType.ShoulderRight];
		var leftShoulder = body.Joints[Kinect.JointType.ShoulderLeft];
		var rightElbow = body.Joints[Kinect.JointType.ElbowRight];
		var leftElbow = body.Joints[Kinect.JointType.ElbowLeft];
		var rightWrist = body.Joints[Kinect.JointType.WristRight];
		var leftWrist = body.Joints[Kinect.JointType.WristLeft];
		/*var rightHandTip = body.Joints[Kinect.JointType.HandTipRight];
		var leftHandTip = body.Joints[Kinect.JointType.HandTipLeft]; */

		/*var leftArmTrackedJoints = NumberOfTrackedJoints (leftShoulder, leftElbow, leftWrist, leftHandTip);
		var rightArmTrackedJoints = NumberOfTrackedJoints (rightShoulder, rightElbow, rightWrist, rightHandTip);*/

		var leftArmTrackedJoints = NumberOfTrackedJoints (leftShoulder, leftElbow, leftWrist);
		var rightArmTrackedJoints = NumberOfTrackedJoints (rightShoulder, rightElbow, rightWrist);

		//double armLength = leftArmTrackedJoints > rightArmTrackedJoints ? Length(leftShoulder, leftElbow, leftWrist, leftHandTip) : Length(rightShoulder, rightElbow, rightWrist, rightHandTip);
		double armLength = leftArmTrackedJoints > rightArmTrackedJoints ? Length(leftShoulder, leftElbow, leftWrist) : Length(rightShoulder, rightElbow, rightWrist);

		return armLength;
	}

	private double getShoulderLength (Kinect.Body body) {
		var rightShoulder = body.Joints[Kinect.JointType.ShoulderRight];
		var leftShoulder = body.Joints[Kinect.JointType.ShoulderLeft];

		double shoulderLength = Length (leftShoulder, rightShoulder);

		return shoulderLength;
	}

    private double getHipLength(Kinect.Body body)
    {
        var leftHip = body.Joints[Kinect.JointType.HipLeft];
        var rightHip = body.Joints[Kinect.JointType.HipRight];
        float centerHip = body.Joints[Kinect.JointType.SpineBase].Position.Z;
        var HipPoint = body.Joints[Kinect.JointType.SpineBase].Position;

        Debug.Log("WAIST Z VALUE: " + centerHip);

        if (hipDepth == null)
        {
            hipDepth = new HashSet<float>();
            initialHipDepth = centerHip;
        } else
        {
            hipDepth.Add(centerHip);
        }

        Kinect.DepthSpacePoint depthPoint = _BodyManager._Sensor.CoordinateMapper.MapCameraPointToDepthSpace(HipPoint);

        Debug.Log("Point in depth space: " + depthPoint + " | X: " + depthPoint.X + " Y: " + depthPoint.Y);

        /*if (_BodyManager.depthPixels != null)
        {
            var i = (int)depthPoint.X;
            var j = (int)depthPoint.Y;
            var depthInMM = _BodyManager.depthPixels[i];
        }*/

        //double hipLength = ((Length(leftHip, centerHip) * 2) + (Length(centerHip, rightHip) * 2));

        return 0.0;
    }

    void OnApplicationQuit()
    {
        var angle = 360 / hipDepth.Count;

        Debug.Log ("Number of elements: " + hipDepth.Count);
        Debug.Log("Hipe Depth Values: ");
        foreach (float i in hipDepth)
        {
            Debug.Log(i);
        }
    }

    private static double Length(Kinect.Joint p1, Kinect.Joint p2) {
		return Mathf.Sqrt (
			Mathf.Pow(p1.Position.X - p2.Position.X, 2) +
			Mathf.Pow(p1.Position.Y - p2.Position.Y, 2) +
			Mathf.Pow(p1.Position.Z - p2.Position.Z, 2));
	}

	private static double Length(params Kinect.Joint[] joints) {
		double length = 0;

		for(int i = 0; i < joints.Length - 1; i++) {
			length += Length (joints[i], joints[i+1]);
		}

		return length;
	}

	private int NumberOfTrackedJoints (params Kinect.Joint[] joints) {
		int trackedJoints = 0;

		foreach(var j in joints) {
			if (j.TrackingState == Kinect.TrackingState.Tracked) {
				trackedJoints++;
			}
		}

		return trackedJoints;
	}

	private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
	{
		for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
		{
			Kinect.Joint sourceJoint = body.Joints[jt];
			Kinect.Joint? targetJoint = null;

			if(_BoneMap.ContainsKey(jt))
			{
				targetJoint = body.Joints[_BoneMap[jt]];
			}

			Transform jointObj = bodyObject.transform.Find(jt.ToString());
			jointObj.localPosition = GetVector3FromJoint(sourceJoint);

			LineRenderer lr = jointObj.GetComponent<LineRenderer>();
			if(targetJoint.HasValue)
			{
				lr.SetPosition(0, jointObj.localPosition);
				lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
				lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
			}
			else
			{
				lr.enabled = false;
			}
		}
	}

	private static Color GetColorForState(Kinect.TrackingState state)
	{
		switch (state)
		{
		case Kinect.TrackingState.Tracked:
			return Color.green;

		case Kinect.TrackingState.Inferred:
			return Color.red;

		default:
			return Color.black;
		}
	}

	private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
	{
		return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
	}
}
