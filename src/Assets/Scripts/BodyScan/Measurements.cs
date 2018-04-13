using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

using System.IO;

public class Measurements : MonoBehaviour
{

    //SaveManager sm;
    //Save data;

    const int SCAN_LENGTH = 500;
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
    private BodyscanSave.Body data; 
    void Start()
    {
        bodyData = FindObjectOfType<BodyscanSave>();
        data = new BodyscanSave.Body();
 
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

                data.Height = CmToInches(finalHeight);

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
                Debug.Log("Hip Calculation | front: " + hipFront + " side: " + hipSide);
                Debug.Log("CENTIMETERS | Hip: " + hipCalculation);

                Debug.Log("\n");
                Debug.Log("Bust Calculation | front: " + bustFront + " side: " + bustSide);
                Debug.Log("CENTIMETERS | Bust: " + bustCalculation);

                Debug.Log("\n");
                
                data.Hip = CmToInches(hipCalculation);
                data.Bust = CmToInches(bustCalculation);
                bodyData.saveManager.Save("bodyScan", data);
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

        hipFront = EllipseRadiusHip(hip, "Hip front");
        bustFront = EllipseRadiusBust(waist, "Bust front");

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

        hipSide = EllipseRadiusHip(hip, "Hip side");
        hipCalculation = EllipseCircumference((float)hipFront, (float)hipSide) * 100;

        bustSide = EllipseRadiusBust(waist, "Bust side");
        bustCalculation = EllipseCircumference((float)bustFront, (float)bustSide) * 100;
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

    private float EllipseRadiusHip(Kinect.Joint middle, string measurement)
    {
        bool debug = false;
        bool leftFound = false;
        bool rightFound = false;

        Kinect.CameraSpacePoint middle_cameraspace = middle.Position;

        Kinect.DepthSpacePoint centreDepthPoint = bodyManager._Sensor.CoordinateMapper.MapCameraPointToDepthSpace(middle.Position);
        Kinect.DepthSpacePoint rightDepthPoint = centreDepthPoint;
        Kinect.DepthSpacePoint leftDepthPoint = centreDepthPoint;

        int depthFrameHeight = depthManager.GetHeightOfFrame();

        for (int i = 0; i < 70; i++)
        {
            if (texture.GetPixel(((int)rightDepthPoint.X + i), (int)rightDepthPoint.Y).grayscale == 0)
            {
                if (debug)
                {
                    Debug.Log(measurement + " right depth point: (" + ((int)rightDepthPoint.X + i) + ", " + (int)rightDepthPoint.Y + ")");
                }

                rightDepthPoint.X = (int)rightDepthPoint.X + i;
                rightFound = true;
                break;
            }
        }

        for (int j = 0; j < 70; j++)
        {
            if (texture.GetPixel(((int)leftDepthPoint.X - j), (int)leftDepthPoint.Y).grayscale == 0)
            {
                if (debug)
                {
                    Debug.Log(measurement + " left depth point: (" + ((int)leftDepthPoint.X - j) + ", " + (int)leftDepthPoint.Y + ")");
                }

                leftDepthPoint.X = (int)leftDepthPoint.X - j;
                leftFound = true;
                break;
            }
        }

        if (debug)
        {
            Debug.Log(measurement + " centre depth point: (" + centreDepthPoint.X + ", " + centreDepthPoint.Y + ")");
            Debug.Log("\n");
        }

        ushort depth_left = depthData[((int)leftDepthPoint.Y * depthFrameHeight) + (int)leftDepthPoint.X];
        Kinect.CameraSpacePoint left_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(leftDepthPoint, depth_left);

        ushort depth_right = depthData[((int)rightDepthPoint.Y * depthFrameHeight) + (int)rightDepthPoint.X];
        Kinect.CameraSpacePoint right_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(rightDepthPoint, depth_right);

        if (debug)
        {
            Debug.Log("Depth indices | left: " + depth_left + " right: " + depth_right);
            Debug.Log("\n");
        }

        if (depth_left == 0)
        {
            for (int t = 0; t < 50; t++)
            {
                depth_left = depthData[(((int)leftDepthPoint.Y * depthFrameHeight) + (int)leftDepthPoint.X) + t];

                if (depth_left != 0)
                {
                    left_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(leftDepthPoint, depth_left);
                    break;
                }
            }
        }

        if (depth_right == 0)
        {
            for (int r = 0; r < 50; r++)
            {
                depth_right = depthData[(((int)rightDepthPoint.Y * depthFrameHeight) + (int)rightDepthPoint.X) - r];

                if (depth_right != 0)
                {
                    right_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(rightDepthPoint, depth_right);
                    break;
                }
            }
        }

        if (debug)
        {
            Debug.Log(measurement + " middle camera space | x: " + middle_cameraspace.X + " y: " + middle_cameraspace.Y + " z: " + middle_cameraspace.Z);
            Debug.Log(measurement + " left camera space point | X: " + left_newcameraspace.X + " Y: " + left_newcameraspace.Y + " Z: " + left_newcameraspace.Z);
            Debug.Log(measurement + " right camera space point | X: " + right_newcameraspace.X + " Y: " + right_newcameraspace.Y + " Z: " + right_newcameraspace.Z);
            Debug.Log("\n");
        }

        if ((Length(left_newcameraspace.X, middle_cameraspace.X) >= Length(right_newcameraspace.X, middle_cameraspace.X)) && rightFound == true)
        {
            return Length(right_newcameraspace.X, middle_cameraspace.X);
        }
        else if ((Length(left_newcameraspace.X, middle_cameraspace.X) < Length(right_newcameraspace.X, middle_cameraspace.X)) && leftFound == true)
        {
            return Length(left_newcameraspace.X, middle_cameraspace.X);
        } else
        {
            if (rightFound == false)
            {
                return Length(left_newcameraspace.X, middle_cameraspace.X);
            } else if (leftFound == false)
            {
                return Length(right_newcameraspace.X, middle_cameraspace.X);
            } else
            {
                Debug.LogError(measurement + " no good data");
            }
        }

        return 0.0f;
    }

    private float EllipseRadiusBust(Kinect.Joint mid, string meas)
    {
        bool deb = false;
        bool lFound = false;
        bool rFound = false;

        Kinect.CameraSpacePoint m_cameraspace = mid.Position;

        Kinect.DepthSpacePoint cDepthPoint = bodyManager._Sensor.CoordinateMapper.MapCameraPointToDepthSpace(mid.Position);
        Kinect.DepthSpacePoint rDepthPoint = cDepthPoint;
        Kinect.DepthSpacePoint lDepthPoint = cDepthPoint;

        int dFrameHeight = depthManager.GetHeightOfFrame();

        for (int x = 0; x < 70; x++)
        {
            if (texture.GetPixel(((int)rDepthPoint.X + x), (int)rDepthPoint.Y).grayscale == 0)
            {
                if (deb)
                {
                    Debug.Log(meas + " right depth point: (" + ((int)rDepthPoint.X + x) + ", " + (int)rDepthPoint.Y + ")");
                }

                rDepthPoint.X = (int)rDepthPoint.X + x;
                rFound = true;
                break;
            }
        }

        for (int y = 0; y < 70; y++)
        {
            if (texture.GetPixel(((int)lDepthPoint.X - y), (int)lDepthPoint.Y).grayscale == 0)
            {
                if (deb)
                {
                    Debug.Log(meas + " left depth point: (" + ((int)lDepthPoint.X - y) + ", " + (int)lDepthPoint.Y + ")");
                }

                lDepthPoint.X = (int)lDepthPoint.X - y;
                lFound = true;
                break;
            }
        }
         
        if (deb)
        {
            Debug.Log(meas + " centre depth point: (" + cDepthPoint.X + ", " + cDepthPoint.Y + ")");
            Debug.Log("\n");
        }

        ushort depth_l = depthData[((int)lDepthPoint.Y * dFrameHeight) + (int)lDepthPoint.X];
        Kinect.CameraSpacePoint l_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(lDepthPoint, depth_l);

        ushort depth_r = depthData[((int)rDepthPoint.Y * dFrameHeight) + (int)rDepthPoint.X];
        Kinect.CameraSpacePoint r_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(rDepthPoint, depth_r);

        ushort depth_m = 0;
        Kinect.CameraSpacePoint c_newcameraspace = new Kinect.CameraSpacePoint();

        if (deb)
        {
            Debug.Log("Depth indices | left: " + depth_l + " right: " + depth_r);
            Debug.Log("\n");
        }

        if (depth_l == 0 && lFound)
        {
            for (int w = 0; w < 50; w++)
            {
                depth_l = depthData[(((int)lDepthPoint.Y * dFrameHeight) + (int)lDepthPoint.X) + w];

                if (depth_l != 0)
                {
                    l_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(lDepthPoint, depth_l);
                    break;
                }
            }
        }

        if (depth_r == 0 && rFound)
        {
            for (int e = 0; e < 50; e++)
            {
                depth_r = depthData[(((int)rDepthPoint.Y * dFrameHeight) + (int)rDepthPoint.X) - e];

                if (depth_r != 0)
                {
                    r_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(rDepthPoint, depth_r);
                    break;
                }
            }
        }

        if (deb)
        {
            Debug.Log(meas + " middle camera space | x: " + m_cameraspace.X + " y: " + m_cameraspace.Y + " z: " + m_cameraspace.Z);
            Debug.Log(meas + " left camera space point | X: " + l_newcameraspace.X + " Y: " + l_newcameraspace.Y + " Z: " + l_newcameraspace.Z);
            Debug.Log(meas + " right camera space point | X: " + r_newcameraspace.X + " Y: " + r_newcameraspace.Y + " Z: " + r_newcameraspace.Z);
            Debug.Log("\n");
        }

        if (rFound && lFound)
        {
            if (Mathf.Abs((Mathf.Abs(lDepthPoint.X - cDepthPoint.X) - Mathf.Abs(rDepthPoint.X - cDepthPoint.X))) > 10)
            {
                if (deb)
                {
                    Debug.Log("New centre point");
                    Debug.Log("\n");
                }

                cDepthPoint.X = (rDepthPoint.X + lDepthPoint.X) / 2;

                depth_m = depthData[((int)cDepthPoint.Y * dFrameHeight) + (int)cDepthPoint.X];
                c_newcameraspace = depthManager._Sensor.CoordinateMapper.MapDepthPointToCameraSpace(cDepthPoint, depth_m);
            }
        }

        if (depth_m != 0)
        {
            if ((Length(l_newcameraspace.X, c_newcameraspace.X) >= Length(r_newcameraspace.X, c_newcameraspace.X)) && rFound == true)
            {
                return Length(r_newcameraspace.X, c_newcameraspace.X);
            }
            else if ((Length(l_newcameraspace.X, c_newcameraspace.X) < Length(r_newcameraspace.X, c_newcameraspace.X)) && lFound == true)
            {
                return Length(l_newcameraspace.X, c_newcameraspace.X);
            } else
            {
                if (rFound == false)
                {
                    return Length(l_newcameraspace.X, c_newcameraspace.X);
                } else if (lFound == false)
                {
                    return Length(r_newcameraspace.X, c_newcameraspace.X);
                } else
                {
                    Debug.LogError(meas + " no good data");
                }
            }
        }
        else
        {
            if ((Length(l_newcameraspace.X, m_cameraspace.X) >= Length(r_newcameraspace.X, m_cameraspace.X)) && rFound == true)
            {
                return Length(r_newcameraspace.X, m_cameraspace.X);
            }
            else if ((Length(l_newcameraspace.X, m_cameraspace.X) < Length(r_newcameraspace.X, m_cameraspace.X)) && lFound == true)
            {
                return Length(l_newcameraspace.X, m_cameraspace.X);
            }
            else
            {
                if (rFound == false)
                {
                    return Length(l_newcameraspace.X, m_cameraspace.X);
                }
                else if (lFound == false)
                {
                    return Length(r_newcameraspace.X, m_cameraspace.X);
                }
                else
                {
                    Debug.LogError(meas + " no good data");
                }
            }
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

    private static float Length(float p1, float p2)
    {
        return Mathf.Abs(p1 - p2);
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