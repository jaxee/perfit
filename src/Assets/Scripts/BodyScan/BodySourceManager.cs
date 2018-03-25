using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class BodySourceManager : MonoBehaviour 
{
    public KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;
    
    public Body[] GetData()
    {
        return _Data;
    }
    

    void Start () 
    {
        /*_Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();
            
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
		} else {
			Debug.LogError ("Kinect not connected");
		}*/

		Debug.Log ("Initializing Body Source Manager");
    }

	public void StartBodySourceManager() {
		_Sensor = KinectSensor.GetDefault();

		if (_Sensor != null)
		{
			_Reader = _Sensor.BodyFrameSource.OpenReader();

			if (!_Sensor.IsOpen)
			{
				_Sensor.Open();
			}
		} else {
			Debug.LogError ("Kinect not connected");
		}
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
