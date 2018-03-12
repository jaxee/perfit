using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

using System.IO;


public class Measurements : MonoBehaviour
{
    const int SCAN_LENGTH = 1000;
    int scan = 0;

    // Body Manager (Joints)
    public GameObject BodySrcManager;
    private BodySourceManager bodyManager;
    private Kinect.Body[] bodies;

    private HashSet<double> heightCalculations = new HashSet<double>();
    private HashSet<double> legCalculations = new HashSet<double>();
    private HashSet<double> armCalculations = new HashSet<double>();

    private bool beginScanOne = false;
    private bool scanOneDone = false;

    private bool beginScanTwo = false;
    private bool scanTwoDone = false;

    // Depth Manager (Z data)
    public GameObject DepthSrcManager;
    private DepthSourceManager depthManager;
    private ushort[] depthData;
    private Texture2D texture;


    void Start()
    {
        if (BodySrcManager == null)
        {
            Debug.LogError("Missing Game Object (Body Source Manager)");
        }
        else
        {
            bodyManager = BodySrcManager.GetComponent<BodySourceManager>();
        }

        if (DepthSrcManager == null)
        {
            Debug.LogError("Missing Game Object (Depth Source Manager)");
        }
        else
        {
            depthManager = DepthSrcManager.GetComponent<DepthSourceManager>();
        }

        Debug.Log("Kinect Initialized. Press Space to Scan");
    }

    void Update()
    {
        if (bodyManager == null)
        {
            Debug.LogError("No BodySourceManager");
            return;
        }

        if (depthManager == null)
        {
            Debug.LogError("No DepthSourceManager");
            return;
        }

        bodies = bodyManager.GetData();

        if (bodies == null)
        {
            return;
        }

        depthData = depthManager.GetData();

        if (depthData == null)
        {
            return;
        }

        texture = depthManager.GetDepthTexture();

        if (texture == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            scan++;

            if (scan == 1)
            {
                Debug.Log("Scanning Front");
                beginScanOne = true;
            }
            if (scan == 2)
            {
                Debug.Log("Scanning Side");
                beginScanTwo = true;
            }
        }

        if (beginScanOne && !scanOneDone && scan == 1)
        {
            int i = 0;

            if (i == 0)
            {
                using (FileStream stream = new FileStream("C:\\DepthData\\depthdatafront.raw", FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        foreach (var dep in depthData)
                        {
                            writer.Write(dep);
                        }

                        writer.Close();
                    }
                }
            }

            while (i < SCAN_LENGTH)
            {
                foreach (var body in bodies)
                {
                    if (body == null)
                    {
                        Debug.LogError("Body not tracked");
                        continue;
                    }

                    if (body.IsTracked)
                    {
                        measureFrontBody(body);
                    }
                }

                i++;
            }

            if (i >= SCAN_LENGTH)
            {
                Debug.Log("Front Scan Ended");

                var finalHeight = averageValues(heightCalculations);
                var finalLeg = averageValues(legCalculations);
                var finalArm = averageValues(armCalculations);

                Debug.Log("----------------------------------------------------------------------------------------------");
                Debug.Log("Measurements");
                Debug.Log("Current Height: " + finalHeight + "\nLeg Length: " + finalLeg + "\nArm Length: " + finalArm);

                scanOneDone = true;
            }
        }

        if (beginScanTwo && !scanTwoDone && scan == 2)
        {
            int y = 0;

            if (y == 0)
            {
                using (FileStream stream2 = new FileStream("C:\\DepthData\\depthdataside.raw", FileMode.Create))
                {
                    using (BinaryWriter writer2 = new BinaryWriter(stream2))
                    {
                        foreach (var dep in depthData)
                        {
                            writer2.Write(dep);
                        }

                        writer2.Close();
                    }
                }
            }

            while (y < SCAN_LENGTH)
            {
                foreach (var body in bodies)
                {
                    if (body == null)
                    {
                        Debug.LogError("Body not tracked");
                        continue;
                    }

                    if (body.IsTracked)
                    {
                        measureSideBody(body);
                    }
                }

                y++;
            }

            if (y >= SCAN_LENGTH)
            {
                Debug.Log("Side Scan Ended");

                var finalHeight = averageValues(heightCalculations);

                Debug.Log("----------------------------------------------------------------------------------------------");
                Debug.Log("Final Measurements");
                Debug.Log("Final Height: " + finalHeight);

                scanTwoDone = true;
            }
        }
    }

    private void measureFrontBody(Kinect.Body body)
    {
        var head = body.Joints[Kinect.JointType.Head];
        var neck = body.Joints[Kinect.JointType.SpineShoulder];
        var waist = body.Joints[Kinect.JointType.SpineMid];
        var rightShoulder = body.Joints[Kinect.JointType.ShoulderRight];
        var leftShoulder = body.Joints[Kinect.JointType.ShoulderLeft];
        var rightElbow = body.Joints[Kinect.JointType.ElbowRight];
        var leftElbow = body.Joints[Kinect.JointType.ElbowLeft];
        var rightWrist = body.Joints[Kinect.JointType.WristRight];
        var leftWrist = body.Joints[Kinect.JointType.WristLeft];
        var hip = body.Joints[Kinect.JointType.SpineBase];
        var hipLeft = body.Joints[Kinect.JointType.HipLeft];
        var hipRight = body.Joints[Kinect.JointType.HipRight];
        var kneeLeft = body.Joints[Kinect.JointType.KneeLeft];
        var kneeRight = body.Joints[Kinect.JointType.KneeRight];
        var ankleLeft = body.Joints[Kinect.JointType.AnkleLeft];
        var ankleRight = body.Joints[Kinect.JointType.AnkleRight];
        var footLeft = body.Joints[Kinect.JointType.FootLeft];
        var footRight = body.Joints[Kinect.JointType.FootRight];

        double height = BodyHeight(head, neck, waist, hip, hipLeft, hipRight, kneeLeft, kneeRight, ankleLeft, ankleRight, footLeft, footRight) * 100;
        double legLength = LegLength(hipLeft, hipRight, kneeLeft, kneeRight, ankleLeft, ankleRight, footLeft, footRight) * 100;
        double armLength = ArmLength(rightShoulder, leftShoulder, rightElbow, leftElbow, rightWrist, leftWrist) * 100;

        FrontHipMeasurement(hip, hipRight, hipLeft);

        heightCalculations.Add(height);
        legCalculations.Add(legLength);
        armCalculations.Add(armLength);

        /*Debug.Log("***************************************************************************");
        Debug.Log("Height: " + height + "\nLeg Length: " + legLength + "\nArm Length: " + armLength);
        Debug.Log("***************************************************************************");*/
    }

    private void measureSideBody(Kinect.Body body)
    {
        var head = body.Joints[Kinect.JointType.Head];
        var neck = body.Joints[Kinect.JointType.SpineShoulder];
        var waist = body.Joints[Kinect.JointType.SpineMid];
        var rightShoulder = body.Joints[Kinect.JointType.ShoulderRight];
        var leftShoulder = body.Joints[Kinect.JointType.ShoulderLeft];
        var rightElbow = body.Joints[Kinect.JointType.ElbowRight];
        var leftElbow = body.Joints[Kinect.JointType.ElbowLeft];
        var rightWrist = body.Joints[Kinect.JointType.WristRight];
        var leftWrist = body.Joints[Kinect.JointType.WristLeft];
        var hip = body.Joints[Kinect.JointType.SpineBase];
        var hipLeft = body.Joints[Kinect.JointType.HipLeft];
        var hipRight = body.Joints[Kinect.JointType.HipRight];
        var kneeLeft = body.Joints[Kinect.JointType.KneeLeft];
        var kneeRight = body.Joints[Kinect.JointType.KneeRight];
        var ankleLeft = body.Joints[Kinect.JointType.AnkleLeft];
        var ankleRight = body.Joints[Kinect.JointType.AnkleRight];
        var footLeft = body.Joints[Kinect.JointType.FootLeft];
        var footRight = body.Joints[Kinect.JointType.FootRight];

        double height = BodyHeight(head, neck, waist, hip, hipLeft, hipRight, kneeLeft, kneeRight, ankleLeft, ankleRight, footLeft, footRight) * 100;

        heightCalculations.Add(height);

        SideHipMeasurement(hip);
    }

    private double BodyHeight(params Kinect.Joint[] joints)
    {
        const double HEAD_DIVERGENCE = 0.1;

        int leftLegTrackedJoints = NumberOfTrackedJoints(joints[4], joints[6], joints[8], joints[10]);
        int rightLegTrackedJoints = NumberOfTrackedJoints(joints[5], joints[7], joints[9], joints[11]);

        double legLength = leftLegTrackedJoints > rightLegTrackedJoints ?
            Length(joints[4], joints[6], joints[8], joints[10]) :
            Length(joints[5], joints[7], joints[9], joints[11]);

        return Length(joints[0], joints[1], joints[2], joints[3]) + legLength + HEAD_DIVERGENCE;
    }

    private double LegLength(params Kinect.Joint[] joints)
    {
        int leftLegTrackedJoints = NumberOfTrackedJoints(joints[0], joints[2], joints[4], joints[6]);
        int rightLegTrackedJoitns = NumberOfTrackedJoints(joints[1], joints[3], joints[5], joints[7]);

        double legLength = leftLegTrackedJoints > rightLegTrackedJoitns ?
            Length(joints[0], joints[2], joints[4], joints[6]) :
            Length(joints[1], joints[3], joints[5], joints[7]);

        return legLength;
    }

    private double ArmLength(params Kinect.Joint[] joints)
    {
        int leftArmTrackedJoints = NumberOfTrackedJoints(joints[1], joints[3], joints[5]);
        int rightArmTrackedJoints = NumberOfTrackedJoints(joints[0], joints[2], joints[4]);

        double armLength = leftArmTrackedJoints > rightArmTrackedJoints ?
            Length(joints[1], joints[3], joints[5]) :
            Length(joints[0], joints[2], joints[4]);

        return armLength;
    }

    private double FrontHipMeasurement(Kinect.Joint hc, Kinect.Joint hr, Kinect.Joint hl)
    {
        Kinect.DepthSpacePoint hipDepthPointC = bodyManager._Sensor.CoordinateMapper.MapCameraPointToDepthSpace(hc.Position);
        Kinect.DepthSpacePoint hrPoint = hipDepthPointC;
        Kinect.DepthSpacePoint hlPoint = hipDepthPointC;


        for (int i = 0; i < 100; i++)
        {
            if (texture.GetPixel(((int)hrPoint.X + i), (int)hrPoint.Y).grayscale == 0)
            {
                //Debug.Log("Right hip point: (" + ((int)hrPoint.X + i) + ", " + (int)hrPoint.Y + ")");
                hrPoint.X = hrPoint.X + i;
                break;
            }
        }

        for (int i = 0; i < 100; i++)
        {
            if (texture.GetPixel(((int)hlPoint.X - i), (int)hlPoint.Y).grayscale == 0)
            {
                //Debug.Log("Left hip point: (" + ((int)hlPoint.X - i) + ", " + (int)hlPoint.Y + ")");
                hlPoint.X = hlPoint.X - i;
                break;
            }
        }

        Debug.Log("Final front hip positions are: (left) " + hlPoint.X + " | (centre) " + hipDepthPointC.X + " | (right) " + hrPoint.X + "||| y is: " + hipDepthPointC.Y);

        return 0.0;
    }

    private double SideHipMeasurement(Kinect.Joint hc)
    {
        Kinect.DepthSpacePoint hipDepthPointC = bodyManager._Sensor.CoordinateMapper.MapCameraPointToDepthSpace(hc.Position);
        Kinect.DepthSpacePoint hrPoint = hipDepthPointC;
        Kinect.DepthSpacePoint hlPoint = hipDepthPointC;

        for (int i = 0; i < 100; i++)
        {
            if (texture.GetPixel(((int)hrPoint.X + i), (int)hrPoint.Y).grayscale == 0)
            {
                //Debug.Log("Right hip point: (" + ((int)hrPoint.X + i) + ", " + (int)hrPoint.Y + ")");
                hrPoint.X = hrPoint.X + i;
                break;
            }
        }

        for (int i = 0; i < 100; i++)
        {
            if (texture.GetPixel(((int)hlPoint.X - i), (int)hlPoint.Y).grayscale == 0)
            {
                //Debug.Log("Left hip point: (" + ((int)hlPoint.X - i) + ", " + (int)hlPoint.Y + ")");
                hlPoint.X = hlPoint.X - i;
                break;
            }
        }

        Debug.Log("Final side hip positions are: (left) " + hlPoint.X + " | (centre) " + hipDepthPointC.X + " | (right) " + hrPoint.X + "||| y is: " + hipDepthPointC.Y);

        //Here
        //Kinect.CameraSpacePoint[] hrFinalPoint = depthManager._Sensor.CoordinateMapper.MapDepthFrameToCameraSpace(depthData, hrPoint);
        //Kinect.CameraSpacePoint[] hrFinalPoint;


        //Debug.Log("Texture Info: Width " + texture.width + " | " + texture.height + " | " + texture.GetPixel((int)hipDepthPointC.X, (int)hipDepthPointC.Y).grayscale + " | " + texture.GetPixels32());

        return 0.0;
    }


    // HELPERS

    private int NumberOfTrackedJoints(params Kinect.Joint[] joints)
    {
        int trackedJoints = 0;

        foreach (var j in joints)
        {
            if (j.TrackingState == Kinect.TrackingState.Tracked)
            {
                trackedJoints++;
            }
        }

        return trackedJoints;
    }

    private static double Length(Kinect.Joint p1, Kinect.Joint p2)
    {
        return Mathf.Sqrt(
            Mathf.Pow(p1.Position.X - p2.Position.X, 2) +
            Mathf.Pow(p1.Position.Y - p2.Position.Y, 2) +
            Mathf.Pow(p1.Position.Z - p2.Position.Z, 2));
    }

    private static double Length(params Kinect.Joint[] joints)
    {
        double length = 0;

        for (int i = 0; i < joints.Length - 1; i++)
        {
            length += Length(joints[i], joints[i + 1]);
        }

        return length;
    }

    private double averageValues(HashSet<double> vals)
    {
        var size = vals.Count;
        double valsAdded = 0.0;

        foreach (double v in vals)
        {
            valsAdded += v;
        }

        return valsAdded / size;
    }
}
