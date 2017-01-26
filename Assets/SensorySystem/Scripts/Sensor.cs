using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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

    public bool CustomDistanceCalculation = false;
    
    public Vector3 Position, Forward; // position interface

    [Tooltip("Layer mask to ignore when raycasting. Usually the layer where the sensor object belongs to")]
    public LayerMask LayerMask;
    
    // Callbacks
    public delegate void DelegateSignalDetected(SenseLink senseLink);
    public DelegateSignalDetected delegateSignalDetected;
    public delegate Vector3 DelegateDistanceCalculation(Sensor sensor, Signal signal);
    public DelegateDistanceCalculation delegateDistanceCalculation;


    private float maxViewConeDistance;

    private RaycastHit hit;
    private Ray ray;

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
            if (Physics.Raycast(Position, directionToSignal, out hit, Mathf.Infinity, LayerMask.value))
            {
                if (hit.transform.position.Equals(signal.Transform.position)) //hit the signal, nothing in between
                    return new SenseLink(Time.time, signal, maxAwarenessForSignal, true, signal.Sense);
            }
        }

        return null;        
    }

    private Vector3 calculateDistanceFromSignal(Signal signal)
    {
        if (CustomDistanceCalculation)
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
        }
        return DefaultDistanceCalculator.CalculateDistance(this, signal);
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

    public float CalculateCooldownTimePerPhase()
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
        foreach(ViewCone vc in ViewCones)
        {
            if (vc.Range > maxDistance)
                maxDistance = vc.Range;
        }

        maxViewConeDistance = maxDistance;
    }


}