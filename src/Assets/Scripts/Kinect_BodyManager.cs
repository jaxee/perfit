using UnityEngine;
using System.Collections;
using System.Linq;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using System;

public class Kinect_BodyManager : MonoBehaviour
{
    public KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private DepthFrameReader _DepthReader;
    private Body[] _Data = null;

    private FrameDescription depthFrameDescription = null;
    public byte[] depthPixels = null;
    private const int MapDepthToByte = 8000 / 256;

    public Body[] GetData()
    {
        return _Data;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null) {

            //Reset(_Sensor);

            _Reader = _Sensor.BodyFrameSource.OpenReader();
            _DepthReader = _Sensor.DepthFrameSource.OpenReader();

            depthFrameDescription = _Sensor.DepthFrameSource.FrameDescription;
            depthPixels = new byte[depthFrameDescription.Width * depthFrameDescription.Height];

            if (!_Sensor.IsOpen) {
                _Sensor.Open();

                Debug.Log("Initializing Kinect...");
            }
        }
        else
        {
            Debug.LogError("No sensor data");
        }

    }

    /*private void Reset(KinectSensor s)
    {
        _Reader.Dispose();
        _DepthReader.Dispose();

        _Reader = null;
        _DepthReader = null;
    }*/

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();

            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                    Debug.Log("Kinect Ready!");
                }

                frame.GetAndRefreshBodyData(_Data);

                frame.Dispose();
                frame = null;
            }
        } else
        {
            Debug.LogError("Reader has no data");
        }

        if (_DepthReader != null)
        {
            DepthFrame depthFrame = _DepthReader.AcquireLatestFrame();

            if (depthFrame != null)
            {
                KinectBuffer depthBuffer = depthFrame.LockImageBuffer();

                if ((depthFrameDescription.Width * depthFrameDescription.Height) == (depthBuffer.Capacity / depthFrameDescription.BytesPerPixel))
                {
                    ushort maxDepth = depthFrame.DepthMaxReliableDistance;

                    //ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Capacity, depthFrame.DepthMinReliableDistance, maxDepth);
                }

                depthFrame.Dispose();
                depthFrame = null;
            }

            
        } else
        {
            Debug.LogError("Depth Reader has no data");
        }

    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_DepthReader != null)
        {
            _DepthReader.Dispose();
            _DepthReader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }

    /*private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
    {
        // depth frame data is a 16 bit value
        ushort* frameData = (ushort*)depthFrameData;

        // convert depth to a visual representation
        for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
        {
            // Get the depth for this pixel
            ushort depth = frameData[i];

            // To convert to a byte, we're mapping the depth value to the byte range.
            // Values outside the reliable depth range are mapped to 0 (black).
            depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            //Debug.Log("Pixel" + depthPixels[i]);
        }
    }*/
}
