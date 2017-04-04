using UnityEngine;
using System.Collections;
namespace UnitySensorySystem
{
    public class SignalObject : MonoBehaviour
    {

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
        private SensorManagerObject manager;
        private int signalIndex = -1;

        void Start()
        {
            Object obj = FindObjectOfType(typeof(SensorManagerObject));
            if (obj != null)
                manager = (SensorManagerObject)obj;
            resetSignal();
        }

        void resetSignal()
        {
            if (signal != null && signalIndex >= 0)
                manager.UnregisterSignal(signal);

            if (signalType == SenseType.Vision)
                signal = new VisualSignal(transform.position);
            else
                signal = new AudioSignal(AudioSignalRange, AudioSignalAttenuatedByObstacles);

            signalIndex = manager.RegisterSignal(signal);
        }

        void Update()
        {
            if (signal != null)
                signal.SetPosition(transform.position);
        }

        void OnDisable()
        {
            if (manager != null)
                manager.UnregisterSignal(signal);
        }

        public Signal GetSignal()
        {
            return signal;
        }
    }

}