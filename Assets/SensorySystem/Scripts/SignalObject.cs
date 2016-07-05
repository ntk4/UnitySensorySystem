using UnityEngine;
using System.Collections;

public class SignalObject : MonoBehaviour {

    public SenseType SignalType
    {
        get { return SignalType; }
        set
        {
            if (signalType != value)
            {
                SignalType = value;
                resetSignal();
            }
        }
    }
    [SerializeField]
    private SenseType signalType;

    public float AudioSignalRange = 0.0f;
    public bool AudioSignalAttenuatedByObstacles = false;

    private Signal signal;
    private SensorManager manager;
    private int signalIndex = -1;

    void Start()
    {
        Object obj = FindObjectOfType(typeof(SensorManager));
        if (obj != null)
            manager = (SensorManager)obj;
        resetSignal();
    }

    void resetSignal()
    {
        if (signal != null && signalIndex >= 0)
            manager.UnregisterSignal(signalIndex);

        if (signalType == SenseType.Vision)
            signal = new VisionSignal(transform);
        else
            signal = new AudioSignal(AudioSignalRange, AudioSignalAttenuatedByObstacles);

        signalIndex = manager.RegisterSignal(signal);
    }

    void OnDisable()
    {
        if (manager != null)
            manager.UnregisterSignal(signalIndex);
    }

    public Signal GetSignal()
    {
        return signal;
    }
}
