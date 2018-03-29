using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class DepthSourceManager : MonoBehaviour
{   
    public KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private ushort[] _Data;
    private byte[] _RawData;

    public ushort[] GetData()
    {
        return _Data;
    }

    private Texture2D _Texture;
    public Texture2D GetDepthTexture()
    {
        return _Texture;
    }

    void Start () 
    {
        /*_Sensor = KinectSensor.GetDefault();
        
		if (_Sensor != null) {
			_Reader = _Sensor.DepthFrameSource.OpenReader ();
			_Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];

			_RawData = new byte[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels * 3];
			_Texture = new Texture2D (_Sensor.DepthFrameSource.FrameDescription.Width, _Sensor.DepthFrameSource.FrameDescription.Height, TextureFormat.RGB24, false);
		} else {
			Debug.LogError ("Kinect not found - Please check connection");
		}*/
    }

	public void StartDepthSourceManager () {
		_Sensor = KinectSensor.GetDefault();

		if (_Sensor != null) {
			_Reader = _Sensor.DepthFrameSource.OpenReader ();
			_Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];

			_RawData = new byte[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels * 3];
			_Texture = new Texture2D (_Sensor.DepthFrameSource.FrameDescription.Width, _Sensor.DepthFrameSource.FrameDescription.Height, TextureFormat.RGB24, false);
		} else {
			Debug.LogError ("Kinect not found - Please check connection");
		}

        Debug.Log("Initializing Body Source Manager");
    }
    
    void Update () 
    {
		if (_Reader != null) {
			var frame = _Reader.AcquireLatestFrame ();
			if (frame != null) {
				frame.CopyFrameDataToArray (_Data);


				for (int i = 0; i < _Data.Length; ++i) {
					_RawData [3 * i + 0] = (byte)(_Data [i] / 256);
					_RawData [3 * i + 1] = (byte)(_Data [i] % 256);
					_RawData [3 * i + 2] = 0;
				}

				_Texture.LoadRawTextureData (_RawData);
				_Texture.Apply ();

				frame.Dispose ();
				frame = null;
			}
		}
    }


    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
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
}
