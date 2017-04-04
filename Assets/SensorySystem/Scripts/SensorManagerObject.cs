using UnityEngine;
using System.Collections.Generic;

namespace UnitySensorySystem
{
    public class SensorManagerObject : MonoBehaviour
    {
        public SensorManager sensorManager;

        public int FramesDelay;

        void Awake()
        {
            sensorManager = new SensorManager();
            sensorManager.FramesDelay = this.FramesDelay;
        }

        void Update()
        {
            if (sensorManager != null)
                sensorManager.Update();
        }
        public int RegisterSensor(Sensor sensor)
        {
            if (sensorManager != null)
                return sensorManager.RegisterSensor(sensor);
            return -1;
        }

        public void UnregisterSensor(Sensor sensor)
        {
            if (sensorManager != null)
            {
                sensorManager.UnregisterSensor(sensor);
            }
        }

        public int RegisterSignal(Signal signal)
        {
            if (sensorManager != null)
                return sensorManager.RegisterSignal(signal);
            return -1;
        }

        public void UnregisterSignal(Signal signal)
        {
            if (sensorManager != null)
            {
                sensorManager.UnregisterSignal(signal);
            }
        }
    }
}