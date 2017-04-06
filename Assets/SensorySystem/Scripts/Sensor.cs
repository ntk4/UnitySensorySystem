using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
namespace UnitySensorySystem
{
    [Serializable]
    public class Sensor
    {
        public SenseType Sense;

        //[SerializeField]
        public List<ViewCone> ViewCones = new List<ViewCone>();

        //[SerializeField]
        public bool DrawCones = true;

        public Alertness CurrentAlertnessLevel;

        //[SerializeField]
        public float CoolDownSeconds;

        public Vector3 Position, Forward; // position interface
        private int InstanceID;

        public LineOfSightCheck lineOfSightCheck;

        // Callbacks
        public delegate void DelegateSignalDetected(SenseLink senseLink);
        [SerializeField]
        public DelegateSignalDetected delegateSignalDetected;

        public delegate Vector3 DelegateDistanceCalculation(Sensor sensor, Signal signal);
        [SerializeField]
        public DelegateDistanceCalculation delegateDistanceCalculation;

        public delegate bool DelegateLineOfSight(Sensor sensor, Signal signal, Vector3 directionToSignal);
        [SerializeField]
        public DelegateLineOfSight delegateLineOfSight;

        private float maxViewConeDistance;

        public SenseLink Evaluate(Signal signal)
        {
            if (signal.Sense == SenseType.Vision)
            {
                return EvaluateVision(signal);
            }
            else
            {
                return EvaluateHearing(signal);
            }
        }

        private SenseLink EvaluateVision(Signal signal)
        {
            // 1. if it's further than the largest view cone range + 1, don't even bother to process
            Vector3 directionToSignal = calculateDistanceFromSignal(signal);
            if (directionToSignal.magnitude > maxViewConeDistance + 1)
                return null; // too far to evaluate

            // 2. If it's in range find the maximum awareness for the cones that the signal intersects
            Awareness maxAwarenessForSignal = Awareness.None;
            Awareness temp;
            foreach (ViewCone vc in ViewCones)
            {
                temp = vc.EvaluateSignal(Position, Forward, signal);
                if ((int)temp > (int)maxAwarenessForSignal)
                    maxAwarenessForSignal = temp;
            }

            // 3. If the signal is in a view cone raycast to see if it's visible
            if ((int)maxAwarenessForSignal > (int)Awareness.None)
            {
                Boolean sensed = invokeSelectedLineOfSightAlgorithm(signal, directionToSignal);

                if (sensed)
                    return new SenseLink(Time.time, signal, maxAwarenessForSignal, true, signal.Sense);
            }

            return null;
        }

        private Vector3 calculateDistanceFromSignal(Signal signal)
        {
            if (delegateDistanceCalculation != null)
            {
                object result = delegateDistanceCalculation.Invoke(this, signal);
                if (result != null && result.GetType() == typeof(Vector3))
                {
                    return (Vector3)result;
                }

                Debug.LogWarning("Custom distance callback was not resolved or not called properly. Switching to default");
            }
            return Vector3.zero;
        }

        private bool invokeSelectedLineOfSightAlgorithm(Signal signal, Vector3 directionToSignal)
        {
            if (delegateLineOfSight != null)
            {
                object result = delegateLineOfSight.Invoke(this, signal, directionToSignal);
                if (result != null && result.GetType() == typeof(bool))
                {
                    return (bool)result;
                }

                Debug.LogWarning("Line Of Sight callback was not resolved or not called properly. Switching to default");
            }

            return false;
        }

        private SenseLink EvaluateHearing(Signal signal)
        {
            //TODO
            return new SenseLink(Time.time, signal, Awareness.Low, true, signal.Sense);
        }

        public void AddViewCone()
        {
            changeViewConeCount(ViewCones.Count + 1);
        }

        public void RemoveViewCone(int index)
        {
            if (index >= 0 && index < ViewCones.Count)
            {
                ViewCone coneToRemove = ViewCones[index];
                ViewCones.RemoveAt(index);

                if (coneToRemove.Range == maxViewConeDistance)
                    recalculateMaxViewConeDistance();
            }
        }

        internal float CalculateCooldownTimePerPhase()
        {
            // divide by 3 because we have 3 stages from Awareness.High through Medium, Low to None
            return CoolDownSeconds * 0.33f;
        }

        private void changeViewConeCount(int newValue)
        {
            int oldVal = ViewCones.Count;
            if (newValue > oldVal)
            {
                ViewCone coneToCopy = null;
                if (oldVal != 0 && ViewCones.Count == oldVal)
                    coneToCopy = ViewCones[ViewCones.Count - 1];

                for (int i = oldVal; i < newValue; i++)
                {
                    if (coneToCopy != null)
                        ViewCones.Add(new ViewCone(coneToCopy));
                    else ViewCones.Add(new ViewCone());
                }
            }
            else if (newValue < oldVal)
            {
                if (newValue == 0)
                    ViewCones.Clear();
                else
                    for (int i = 0; i < oldVal - newValue; i++)
                        RemoveViewCone(ViewCones.Count - 1);
            }
        }

        public void recalculateMaxViewConeDistance()
        {
            float maxDistance = 0;
            foreach (ViewCone vc in ViewCones)
            {
                if (vc.Range > maxDistance)
                    maxDistance = vc.Range;
            }

            maxViewConeDistance = maxDistance;
        }

        public int GetInstanceID()
        {
            return InstanceID;
        }

        // Builder methods

        public Sensor SetSense(SenseType sense)
        {
            this.Sense = sense;
            return this;
        }

        public Sensor SetCoolDownSeconds(float CoolDownSeconds)
        {
            this.CoolDownSeconds = CoolDownSeconds;
            return this;
        }

        public Sensor SetDrawCones(Boolean drawCones)
        {
            this.DrawCones = drawCones;
            return this;
        }

        public Sensor SetForward(Vector3 forward)
        {
            this.Forward = forward;
            return this;
        }

        public Sensor SetPosition(Vector3 position)
        {
            this.Position = position;
            return this;
        }

        public Sensor SetDelegateSignalDetected(DelegateSignalDetected delegateSignalDetected)
        {
            this.delegateSignalDetected = delegateSignalDetected;
            return this;
        }

        public Sensor SetDelegateDistanceCalculation(DelegateDistanceCalculation delegateDistanceCalculation)
        {
            this.delegateDistanceCalculation = delegateDistanceCalculation;
            return this;
        }

        public Sensor SetDelegateLineOfSight(DelegateLineOfSight delegateLineOfSight)
        {
            this.delegateLineOfSight = delegateLineOfSight;
            return this;
        }

        public Sensor SetInstanceID(int instanceID)
        {
            this.InstanceID = instanceID;
            return this;
        }

        public Sensor AddViewCone(ViewCone viewCone)
        {
            if (!ViewCones.Contains(viewCone))
                ViewCones.Add(viewCone);
            return this;
        }
    }
}