using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

using System.IO;

public class Measurements_Copy : MonoBehaviour
{

    //SaveManager sm;
    //Save data;

    const int SCAN_LENGTH = 500;
    //int scan = 0;
    public bool startScanning = false;
    public bool startFrontScan = false;
    public bool startSideScan = false;

    private const float INCHES = 0.39370f;

    // Body Manager (Joints)
    public GameObject BodySrcManager;
    private BodySourceManager bodyManager;
    private Kinect.Body[] bodies;

    private HashSet<double> heightCalculations = new HashSet<double>();
    private HashSet<double> legCalculations = new HashSet<double>();
    private HashSet<double> armCalculations = new HashSet<double>();

    private float hipFront = 0.0f;
    private float hipSide = 0.0f;
    private float hipCalculation = 0.0f;

    private float bustFront = 0.0f;
    private float bustSide = 0.0f;
    private float bustCalculation = 0.0f;

    /*private float neckFront = 0.0f;
    private float neckSide = 0.0f;
    private float neckCalculation = 0.0f;*/

    private bool beginScanOne = false;
    private bool scanOneDone = false;

    private bool beginScanTwo = false;
    private bool scanTwoDone = false;

    // Depth Manager (Z data)
    public GameObject DepthSrcManager;
    private DepthSourceManager depthManager;
    private ushort[] depthData;
    private Texture2D texture;

    private BodyscanSave bodyData;
    void Start()
    {
        bodyData = FindObjectOfType<BodyscanSave>();

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

        if (startScanning)
        {
            if (startFrontScan)
            {
                beginScanOne = true;
            }
            if (startSideScan)
            {
                beginScanTwo = true;
            }

            startScanning = false;
            startFrontScan = false;
            startSideScan = false;
        }

        if (beginScanOne && !scanOneDone)
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
                float finalHeight = averageValues(heightCalculations);
                var finalLeg = averageValues(legCalculations);
                var finalArm = averageValues(armCalculations);

                Debug.Log("\n");

                Debug.Log("CENTIMETERS | Height: " + finalHeight + "\nLeg Length: " + finalLeg + "\nArm Length: " + finalArm);
                //Debug.Log("INCHES | Height: " + CmToInches(finalHeight) + "\nLeg Length: " + CmToInches(finalLeg) + "\nArm Length: " + CmToInches(finalArm));

                bodyData.Height = CmToInches(finalHeight);

                scanOneDone = true;
            }
        }

        if (beginScanTwo && !scanTwoDone)
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
                Debug.Log("\n\n");
                Debug.Log("CENTIMETERS | Hip: " + hipCalculation);
                //Debug.Log("INCHES | Hip: " + CmToInches(hipCalculation));

                Debug.Log("\n");

                //Debug.Log("CENTIMETERS | Bust: " + bustCalculation);
                //Debug.Log("INCHES | Bust: " + CmToInches(bustCalculation));

                //Debug.Log("\n");

                //Debug.Log("CENTIMETERS | Neck: " + neckCalculation);
                //Debug.Log("INCHES | Neck: " + CmToInches(neckCalculation));

                //data.hip = CmToInches(hipCalculation);
                //data.bust = CmToInches(bustCalculation);

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

        hipFront = EllipseRadius(hip, hipRight, hipLeft, "Hip front");
        //bustFront = EllipseRadius(waist, "Bust front");
        //neckFront = EllipseRadius(neck, "Neck front");

        heightCalculations.Add(height);
        legCalculations.Add(legLength);
        armCalculations.Add(armLength);
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

        hipSide = EllipseRadius(hip, "Hip side");
        hipCalculation = EllipseCircumference((float)hipFront, (float)hipSide) * 100;

        /*bustSide = EllipseRadius(waist, "Bust side");
        bustCalculation = EllipseCircumference((float)bustFront, (float)bustSide) * 100; */

        //neckSide = EllipseRadius(neck, "Neck side");
        //neckCalculation = EllipseCircumference((float)neckFront, (float)neckSide) * 100;
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

    private float ArmLength(params Kinect.Joint[] joints)
    {
        int leftArmTrackedJoints = NumberOfTrackedJoints(joints[1], joints[3], joints[5]);
        int rightArmTrackedJoints = NumberOfTrackedJoints(joints[0], joints[2], joints[4]);

        float armLength = leftArmTrackedJoints > rightArmTrackedJoints ?
            Length(joints[1], joints[3], joints[5]) :
            Length(joints[0], joints[2], joints[4]);

        return armLength;
    }

    // Front Calculation
    private float EllipseRadius(Kinect.Joint middle, Kinect.Joint left, Kinect.Joint right, string measurement)
    {
        bool debug = false;

        Kinect.DepthSpacePoint centreDepthPoint = bodyManager._Sensor.CoordinateMapper.MapCameraPointToDepthSpace(middle.Position);
        Kinect.DepthSpacePoint rightDepthPoint = centreDepthPoint;
        Kinect.DepthSpacePoint leftDepthPoint = centreDepthPoint;

        for (int i = 0; i < 100; i++)
        {
            if (texture.GetPixel(((int)rightDepthPoint.X + i), (int)rightDepthPoint.Y).grayscale == 0)
            {
                if (debug)
                {
                    Debug.Log(measurement + " right depth point: (" + ((int)rightDepthPoint.X + i) + ", " + (int)rightDepthPoint.Y + ")");
                }
                rightDepthPoint.X = rightDepthPoint.X + i;
                break;
            }
        }

        for (int i = 0; i < 100; i++)
        {
            if (texture.GetPixel(((int)leftDepthPoint.X - i), (int)leftDepthPoint.Y).grayscale == 0)
            {
                if (debug)
                {
                    Debug.Log(measurement + " left depth point: (" + ((int)leftDepthPoint.X - i) + ", " + (int)leftDepthPoint.Y + ")");
                }
                leftDepthPoint.X = leftDepthPoint.X - i;
                break;
            }
        }

        if (debug)
        {
            Debug.Log(measurement + " centre depth point: (" + centreDepthPoint.X + ", " + centreDepthPoint.Y + ")");
        }

        int depthFrameHeight = depthManager.GetHeightOfFrame();

        ushort depthAThl = depthData[(int)leftDepthPoint.Y * (depthFrameHeight + ((int)leftDepthPoint.X))];
        ushort depthAThr = depthData[(int)rightDepthPoint.Y * (depthFrameHeight + ((int)rightDepthPoint.X))];

        if (depthAThl == 0 && depthAThr == 0)
        {
            Debug.LogError(measurement + " both depths 0");
        }
        else if (depthAThr == 0)
        {
            Kinect.CameraSpacePoint leftCameraSpace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(leftDepthPoint, depthAThl);
            leftCameraSpace.Z = left.Position.Z;
            leftCameraSpace.Y = left.Position.Y;

            if (debug)
            {
                Debug.Log(measurement + " " + Length(leftCameraSpace, middle.Position));
            }

            if (debug)
            {
                Debug.Log(measurement + " depth space points | my left(x): " + leftCameraSpace.X + " original centre(x): " + middle.Position.X);
                Debug.Log(measurement + " depth space points | my left(y): " + leftCameraSpace.Y + " original centre(y): " + middle.Position.Y);
                Debug.Log(measurement + " depth space points | my left(z): " + leftCameraSpace.Z + " original centre(z): " + middle.Position.Z);
                Debug.Log("\n");
            }

            if (Length(leftCameraSpace, middle.Position) > 0.25)
            {
                Debug.Log("X | (" + leftCameraSpace.X + ", " + middle.Position.X);
            }

            Debug.Log(measurement + " " + Length(leftCameraSpace, middle.Position));

            return Length(leftCameraSpace, middle.Position);
        }
        else
        {
            Kinect.CameraSpacePoint rightCameraSpace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(rightDepthPoint, depthAThr);
            rightCameraSpace.Z = right.Position.Z;
            rightCameraSpace.Y = right.Position.Y;

            if (debug)
            {
                Debug.Log(measurement + " " + Length(middle.Position, rightCameraSpace));
            }

            if (debug)
            {
                Debug.Log(measurement + " depth space points | original centre(x): " + middle.Position.X + " my right(x): " + rightCameraSpace.X);
                Debug.Log(measurement + " depth space points | original centre(y): " + middle.Position.Y + " my right(y): " + rightCameraSpace.Y);
                Debug.Log(measurement + " depth space points | original centre(z): " + middle.Position.Z + " my right(z): " + rightCameraSpace.Z);
                Debug.Log("\n");
            }

            if (Length(middle.Position, rightCameraSpace) > 0.25)
            {
                Debug.Log("X | (" + rightCameraSpace.X + ", " + middle.Position.X);
            }

            Debug.Log(measurement + " " + Length(middle.Position, rightCameraSpace));

            return Length(middle.Position, rightCameraSpace);
        }

        return 0.0f;
    } 

    // Side Calculation
    private float EllipseRadius(Kinect.Joint middle, string measurement)
    {
        bool debug = true;

        Kinect.DepthSpacePoint centreDepthPoint = bodyManager._Sensor.CoordinateMapper.MapCameraPointToDepthSpace(middle.Position);
        Kinect.DepthSpacePoint rightDepthPoint = centreDepthPoint;
        Kinect.DepthSpacePoint leftDepthPoint = centreDepthPoint;

        for (int i = 0; i < 100; i++)
        {
            if (texture.GetPixel(((int)rightDepthPoint.X + i), (int)rightDepthPoint.Y).grayscale == 0)
            {
                if (debug)
                {
                    Debug.Log(measurement + " right depth point: (" + ((int)rightDepthPoint.X + i) + ", " + (int)rightDepthPoint.Y + ")");
                }
                rightDepthPoint.X = rightDepthPoint.X + i;
                break;
            }
        }

        for (int i = 0; i < 100; i++)
        {
            if (texture.GetPixel(((int)leftDepthPoint.X - i), (int)leftDepthPoint.Y).grayscale == 0)
            {
                if (debug)
                {
                    Debug.Log(measurement + " left depth point: (" + ((int)leftDepthPoint.X - i) + ", " + (int)leftDepthPoint.Y + ")");
                }
                leftDepthPoint.X = leftDepthPoint.X - i;
                break;
            }
        }

        if (Mathf.Abs(leftDepthPoint.X - centreDepthPoint.X) - Mathf.Abs(rightDepthPoint.X - centreDepthPoint.X) < 10)
        {
            if (debug)
            {
                Debug.Log("New centre point");
            }

            centreDepthPoint.X = (rightDepthPoint.X + leftDepthPoint.X) / 2;
        }

        if (debug)
        {
            Debug.Log(measurement + " centre depth point: (" + centreDepthPoint.X + ", " + centreDepthPoint.Y + ")");
        }

        int depthFrameHeight = depthManager.GetHeightOfFrame();

        ushort depthAThl = depthData[(int)leftDepthPoint.Y * (depthFrameHeight + ((int)leftDepthPoint.X))];
        ushort depthAThr = depthData[(int)rightDepthPoint.Y * (depthFrameHeight + ((int)rightDepthPoint.X))];
        ushort depthATC = depthData[(int)centreDepthPoint.Y * (depthFrameHeight + ((int)centreDepthPoint.X))];

        if (depthAThl == 0 && depthAThr == 0)
        {
            Debug.LogError(measurement + " both depths 0");
        }
        else if (depthAThl != 0 && depthAThr != 0)
        {
            Kinect.CameraSpacePoint leftCameraSpace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(leftDepthPoint, depthAThl);
            leftCameraSpace.Z = middle.Position.Z;
            leftCameraSpace.Y = middle.Position.Y;

            Kinect.CameraSpacePoint rightCameraSpace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(rightDepthPoint, depthAThr);
            rightCameraSpace.Z = middle.Position.Z;
            rightCameraSpace.Y = middle.Position.Y;

            Kinect.CameraSpacePoint centreCameraSpace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(centreDepthPoint, depthATC);
            centreCameraSpace.Z = middle.Position.Z;
            centreCameraSpace.Y = middle.Position.Y;

            float leftDistance = 0;
            float rightDistance = 0;

            if (depthATC == 0)
            {
                leftDistance = Length(leftCameraSpace, middle.Position);
                rightDistance = Length(rightCameraSpace, middle.Position);
            }
            else
            {
                if (debug)
                {
                    Debug.Log("Original Camera Space Point Centre | x: " + middle.Position.X + " y: " + middle.Position.Y + " z: " + middle.Position.Z);
                    Debug.Log("New Camera Space Point Centre | x: " + centreCameraSpace.X + " y: " + centreCameraSpace.Y + " z: " + centreCameraSpace.Z);
                }

                leftDistance = Length(leftCameraSpace, centreCameraSpace);
                rightDistance = Length(rightCameraSpace, centreCameraSpace);
            }

            if (leftDistance > rightDistance)
            {
                if (debug)
                {
                    Debug.Log(measurement + " " + rightDistance + "(right)");
                }

                return rightDistance;
            }

            if (debug)
            {
                Debug.Log(measurement + " " + leftDistance + "(left)");
                Debug.Log("\n");
            }

            return leftDistance;

        } else if (depthAThr == 0)
        {
            Kinect.CameraSpacePoint leftCameraSpace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(leftDepthPoint, depthAThl);
            leftCameraSpace.Z = middle.Position.Z;
            leftCameraSpace.Y = middle.Position.Y;

            if (debug)
            {
                Debug.Log(measurement + " " + Length(leftCameraSpace, middle.Position) + " (left)");
            }

            if (debug)
            {
                Debug.Log(measurement + " depth space points | my left(x): " + leftCameraSpace.X + " original centre(x): " + middle.Position.X);
                Debug.Log(measurement + " depth space points | my left(y): " + leftCameraSpace.Y + " original centre(y): " + middle.Position.Y);
                Debug.Log(measurement + " depth space points | my left(z): " + leftCameraSpace.Z + " original centre(z): " + middle.Position.Z);
                Debug.Log("\n");
            }

            return Length(leftCameraSpace, middle.Position);
        }
        else
        {
            Kinect.CameraSpacePoint rightCameraSpace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(rightDepthPoint, depthAThr);
            rightCameraSpace.Z = middle.Position.Z;
            rightCameraSpace.Y = middle.Position.Y;

            if (debug)
            {
                Debug.Log(measurement + " " + Length(middle.Position, rightCameraSpace) + " (right)");
            }

            if (debug)
            {
                Debug.Log(measurement + " depth space points | original centre(x): " + middle.Position.X + " my right(x): " + rightCameraSpace.X);
                Debug.Log(measurement + " depth space points | original centre(y): " + middle.Position.Y + " my right(y): " + rightCameraSpace.Y);
                Debug.Log(measurement + " depth space points | original centre(z): " + middle.Position.Z + " my right(z): " + rightCameraSpace.Z);
                Debug.Log("\n");
            }

            return Length(middle.Position, rightCameraSpace);
        }

        return 0.0f;
    }

    public int GetNumberOfBodies()
    {
        bodies = bodyManager.GetData();

        if (bodies == null)
        {
            return 0;
        }
        return bodies.Length;
    }

    public bool isHeadAndToesTracked()
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
                var head = body.Joints[Kinect.JointType.Head];
                var leftFoot = body.Joints[Kinect.JointType.FootLeft];
                var rightFoot = body.Joints[Kinect.JointType.FootRight];

                if (head.TrackingState == Kinect.TrackingState.Tracked && (leftFoot.TrackingState == Kinect.TrackingState.Tracked || rightFoot.TrackingState == Kinect.TrackingState.Tracked))
                {
                    return true;
                }
                else
                {
                    Debug.LogError("Head: " + head.TrackingState + " | Left Foot: " + leftFoot.TrackingState + " | Right Foot: " + rightFoot.TrackingState);
                    return false;
                }
            }
        }

        return false;
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

    private static float Length(Kinect.Joint p1, Kinect.Joint p2)
    {
        return Mathf.Sqrt(
            Mathf.Pow(p1.Position.X - p2.Position.X, 2) +
            Mathf.Pow(p1.Position.Y - p2.Position.Y, 2) +
            Mathf.Pow(p1.Position.Z - p2.Position.Z, 2));
    }

    private static float Length(params Kinect.Joint[] joints)
    {
        float length = 0;

        for (int i = 0; i < joints.Length - 1; i++)
        {
            length += Length(joints[i], joints[i + 1]);
        }

        return length;
    }

    private static float Length(Kinect.CameraSpacePoint p1, Kinect.CameraSpacePoint p2)
    {
        return Mathf.Sqrt(
            Mathf.Pow(p1.X - p2.X, 2) +
            Mathf.Pow(p1.Y - p2.Y, 2) +
            Mathf.Pow(p1.Z - p2.Z, 2));
    }

    private static float Length(Kinect.CameraSpacePoint p1, Kinect.DepthSpacePoint p2)
    {
        return Mathf.Sqrt(
            Mathf.Pow(p1.X - p2.X, 2) +
            Mathf.Pow(p1.Y - p2.Y, 2));
    }

    private static float Length(params Kinect.CameraSpacePoint[] pt)
    {
        float length = 0;

        for (int i = 0; i < pt.Length - 1; i++)
        {
            length += Length(pt[i], pt[i + 1]);
        }

        return length;
    }

    private float averageValues(HashSet<double> vals)
    {
        var size = vals.Count;
        float valsAdded = 0.0f;

        foreach (float v in vals)
        {
            valsAdded += v;
        }

        return valsAdded / size;
    }

    private float EllipseCircumference(float a, float b)
    {
        return Mathf.PI * (3 * (a + b) - Mathf.Sqrt((3 * a + b) * (a + 3 * b)));
    }

    private float CmToInches(float cm)
    {
        return cm * INCHES;
    }
}