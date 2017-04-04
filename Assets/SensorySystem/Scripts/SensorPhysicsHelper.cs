﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnitySensorySystem
{
    public class SensorPhysicsHelper : MonoBehaviour
    {

        private RaycastHit hit; //TODO: avoid statics!
        private Ray ray;

        public bool SingleRaycast(Sensor sensor, Signal signal, Vector3 sensorDirectionToSignal)
        {
            if (Physics.Raycast(sensor.Position, sensorDirectionToSignal, out hit))
                return hit.transform.position.Equals(signal.Position); //hit the signal, nothing in between
            return false;
        }


        public bool CompleteRaycast(Sensor sensor, Signal signal, Vector3 sensorDirectionToSignal)
        {
            RaycastHit[] hits = Physics.RaycastAll(sensor.Position, sensorDirectionToSignal);
            return (hits != null && hits.Count() > 0 && hits.Select(x => x.transform.position == signal.Position).Count() > 0);
        }
    }
}
