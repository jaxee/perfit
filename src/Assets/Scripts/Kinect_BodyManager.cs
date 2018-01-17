using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class Kinect_BodyManager : MonoBehaviour 
{
	private KinectSensor _Sensor;
	private BodyFrameReader _Reader;
	private Body[] _Data = null;

	public Body[] GetData()
	{
		return _Data;
	}


	void Start () 
	{
		_Sensor = KinectSensor.GetDefault();

		if (_Sensor != null)
		{
			_Reader = _Sensor.BodyFrameSource.OpenReader();

			if (!_Sensor.IsOpen)
			{
				_Sensor.Open();
			}
		}

		Debug.Log ("Initializing Kinect...");
	}

	void Update () 
	{
		if (_Reader != null)
		{
			var frame = _Reader.AcquireLatestFrame();
			if (frame != null)
			{
				if (_Data == null)
				{
					_Data = new Body[_Sensor.BodyFrameSource.BodyCount];
					Debug.Log ("Kinect Ready!");
				}

				frame.GetAndRefreshBodyData(_Data);

				frame.Dispose();
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
