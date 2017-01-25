using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class SensorObject : MonoBehaviour {

    public Sensor sensor;

    private int RegistrationNumber;

    public SensorManager sensorManager;

    void Start()
    {
        sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
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
        if (sensor.signalDetectionMonobehaviorHandler != "" && sensor.signalDetectionHandlerMethod != "")
        {
            IEnumerable<MonoBehaviour> allCallbacks = gameObject.GetComponents<MonoBehaviour>().
                Where(x => x.name == sensor.signalDetectionMonobehaviorHandler || x.GetType().Name == sensor.signalDetectionMonobehaviorHandler ||
                x.GetType().BaseType.Name == sensor.signalDetectionMonobehaviorHandler);

            if (allCallbacks.Count() <= 0)
            {
                Debug.LogError("Sensor Callback " + sensor.signalDetectionMonobehaviorHandler + "." +
                                    sensor.signalDetectionHandlerMethod + "() was not resolved!");
            }
            else {
                sensor.callbackScript = allCallbacks.First();

                sensor.CallbackOnSignalDetected = sensor.callbackScript.GetType().GetMethods().Where(x => x.Name.Equals(sensor.signalDetectionHandlerMethod)).First();
            }
        }

        if (sensor.customDistanceMonobehaviorHandler != "" && sensor.customDistanceHandlerMethod != "")
        {
            IEnumerable<MonoBehaviour> allCallbacks = gameObject.GetComponents<MonoBehaviour>().
                Where(x => x.name == sensor.customDistanceMonobehaviorHandler || x.GetType().Name == sensor.customDistanceMonobehaviorHandler ||
                x.GetType().BaseType.Name == sensor.customDistanceMonobehaviorHandler);

            if (allCallbacks.Count() <= 0)
            {
                Debug.LogError("Custom Distance Callback " + sensor.customDistanceMonobehaviorHandler + "." +
                                    sensor.customDistanceHandlerMethod + "() was not resolved!");
            }
            else {
                sensor.callbackCustomDistanceScript = allCallbacks.First();

                sensor.CallbackCustomDistance = sensor.callbackCustomDistanceScript.GetType().GetMethods().Where(x => x.Name.Equals(sensor.customDistanceHandlerMethod)).First();
            }
        }
    }
}
