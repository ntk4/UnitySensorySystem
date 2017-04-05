using UnityEngine;
using System.Collections;

namespace UnitySensorySystem
{
    public class SensorHandler : MonoBehaviour
    {

        public void ReactToSignal(SenseLink senseLink)
        {
            Debug.Log("I see you! " + GetLevel(senseLink.awarenessLevel));
        }

        private string GetLevel(Awareness awareness)
        {
            if (awareness == Awareness.None)
                return "";
            else
                return awareness.ToString() + " awareness";
        }

        public Vector3 CalculateDistance(Sensor sensor, Signal signal)
        {
            return new Vector3(100, 100, 100);
        }
    }

}