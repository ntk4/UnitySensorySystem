using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

public class SensorObject : MonoBehaviour {

    public Sensor sensor;

    private int RegistrationNumber;

    public SensorManagerObject sensorManager;

    // Callback names (the actually persistent information)
    [SerializeField]
    public string signalDetectionHandlerMethod;
    [SerializeField]
    public string signalDetectionMonobehaviorHandler;
    // Custom Distance Callback names (the actually persistent information)
    //[SerializeField]
    public string customDistanceHandlerMethod;
    //[SerializeField]
    public string customDistanceMonobehaviorHandler;

    void Start()
    {
        sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManagerObject>();
        RegistrationNumber = sensorManager.RegisterSensor(sensor);
        sensor.recalculateMaxViewConeDistance();

        ResolveCallbacks();
    }

    void Update()
    {
        if (sensor != null)
        {
            sensor.Position = transform.position;
            sensor.Forward = transform.forward;
        }
    }

    void OnDisable()
    {
        sensorManager.UnregisterSensor(RegistrationNumber);
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
            }
            else {
                MonoBehaviour callbackScript = allCallbacks.First();
                MethodInfo callbackMethod = callbackScript.GetType().GetMethods().Where(x => x.Name.Equals(signalDetectionHandlerMethod)).First();
                sensor.delegateSignalDetected = (Sensor.DelegateSignalDetected)(
                    Sensor.DelegateSignalDetected.CreateDelegate(typeof(Sensor.DelegateSignalDetected), callbackScript, callbackMethod));
                
            }
        }

        if (customDistanceMonobehaviorHandler != "" && customDistanceHandlerMethod != "")
        {
            IEnumerable<MonoBehaviour> allCallbacks = gameObject.GetComponents<MonoBehaviour>().
                Where(x => x.name == customDistanceMonobehaviorHandler || x.GetType().Name == customDistanceMonobehaviorHandler ||
                x.GetType().BaseType.Name == customDistanceMonobehaviorHandler);

            if (allCallbacks.Count() <= 0)
            {
                Debug.LogError("Custom Distance Callback " + customDistanceMonobehaviorHandler + "." +
                                    customDistanceHandlerMethod + "() was not resolved!");
            }
            else {
                MonoBehaviour callbackCustomDistanceScript = allCallbacks.First();
                MethodInfo CallbackCustomDistance = callbackCustomDistanceScript.GetType().GetMethods().Where(x => x.Name.Equals(customDistanceHandlerMethod)).First();
                sensor.delegateDistanceCalculation = (Sensor.DelegateDistanceCalculation)(
                    Sensor.DelegateDistanceCalculation.CreateDelegate(typeof(Sensor.DelegateDistanceCalculation), callbackCustomDistanceScript, CallbackCustomDistance));
            }
        }
    }
}
