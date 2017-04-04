using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnitySensorySystem
{
    internal class PhysicsHelper
    {
        public delegate bool DelSingleRaycast(Vector3 Position, Vector3 directionToSignal, Signal signal);
        //public DelSingleRaycast SingleRaycastDelegate = SingleRaycast;
        public delegate bool DelCompleteRaycast(Vector3 Position, Vector3 directionToSignal, Signal signal);

        private RaycastHit hit; //TODO: avoid statics!
        private Ray ray;

        public bool SingleRaycast(Vector3 Position, Vector3 directionToSignal, Signal signal)
        {
            if (Physics.Raycast(Position, directionToSignal, out hit))
                return hit.transform.position.Equals(signal.Position); //hit the signal, nothing in between
            return false;
        }


        public bool CompleteRaycast(Vector3 Position, Vector3 directionToSignal, Signal signal)
        {
            RaycastHit[] hits = Physics.RaycastAll(Position, directionToSignal);
            return (hits != null && hits.Count() > 0 && hits.Select(x => x.transform.position == signal.Position).Count() > 0);
        }
    }
}
