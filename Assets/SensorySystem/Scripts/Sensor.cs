using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class Sensor : MonoBehaviour
{
    public SenseType Sense;
    
    public List<ViewCone> ViewCones;
    
    public bool DrawCones = true;

    public Alertness CurrentAlertnessLevel;

    public float CoolDownSeconds;
    
    // Callbacks
    public MethodInfo CallbackOnSignalDetected;
    public MonoBehaviour callbackScript;

    // Callback names (the actually persistent information)
    [SerializeField]
    public string signalDetectionHandlerMethod;
    [SerializeField]
    public string signalDetectionMonobehaviorHandler;

    private float maxViewConeDistance;

    private RaycastHit hit;
    private Ray ray;

    private int RegistrationNumber;
    private SensorManager sensorManager;

    void Start()
    {
        sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
        RegistrationNumber = sensorManager.RegisterSensor(this);
        recalculateMaxViewConeDistance();

        ResolveCallbacks();
    }

    void OnDisable()
    {
        sensorManager.UnregisterSensor(RegistrationNumber);
    }

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
        Vector3 directionToSignal = signal.Transform.position - transform.position;
        if (directionToSignal.magnitude > maxViewConeDistance + 1)
            return null; // too far to evaluate

        // 2. If it's in range find the maximum awareness for the cones that the signal intersects
        Awareness maxAwarenessForSignal = Awareness.None;
        Awareness temp;
        foreach (ViewCone vc in ViewCones)
        {
            temp = vc.EvaluateSignal(transform.position, transform.forward, signal);
            if ((int)temp > (int)maxAwarenessForSignal)
                maxAwarenessForSignal = temp;
        }

        // 3. If the signal is in a view cone raycast to see if it's visible
        if ((int)maxAwarenessForSignal > (int)Awareness.None)
        {
            if (Physics.Raycast(transform.position, directionToSignal, out hit))
            {
                if (hit.transform.position.Equals(signal.Transform.position)) //hit the signal, nothing in between
                    return new SenseLink(Time.time, signal, maxAwarenessForSignal, true, signal.Sense);
            }
        }

        return null;        
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

    private void recalculateMaxViewConeDistance()
    {
        float maxDistance = 0;
        foreach(ViewCone vc in ViewCones)
        {
            if (vc.Range > maxDistance)
                maxDistance = vc.Range;
        }

        maxViewConeDistance = maxDistance;
    }

    public void ResolveCallbacks()
    {
        if (signalDetectionMonobehaviorHandler != "" && signalDetectionHandlerMethod != "")
        {
            IEnumerable<MonoBehaviour> allCallbacks = gameObject.GetComponents<MonoBehaviour>().
                Where(x => x.name == signalDetectionMonobehaviorHandler || x.GetType().Name == signalDetectionMonobehaviorHandler ||
                x.GetType().BaseType.Name == signalDetectionMonobehaviorHandler);

            if (allCallbacks.Count() <= 0)
            {
                Debug.LogError("Sensor Callback " + signalDetectionMonobehaviorHandler + "." +
                                    signalDetectionHandlerMethod + "() was not resolved!");
            } else {
                callbackScript = allCallbacks.First();

                CallbackOnSignalDetected = callbackScript.GetType().GetMethods().Where(x => x.Name.Equals(signalDetectionHandlerMethod)).First();
            }
        }
    }
}