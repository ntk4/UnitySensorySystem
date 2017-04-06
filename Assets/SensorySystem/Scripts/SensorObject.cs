using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace UnitySensorySystem
{
    public class SensorObject : MonoBehaviour
    {

        public Sensor sensor;

        public SensorManagerObject sensorManager;

        // Callback names (the actually persistent information)
        public string signalDetectionHandlerMethod;
        public string signalDetectionMonobehaviorHandler;
        // Custom Distance Callback names (the actually persistent information)
        public string customDistanceHandlerMethod;
        public string customDistanceMonobehaviorHandler;
        // Custom Line Of Sight Callback names (the actually persistent information)
        public string customLineOfSightHandlerMethod;
        public string customLineOfSightMonobehaviorHandler;

        private static string[] ignoreMethods = new string[] { "Start", "Update", "LateUpdate", "FixedUpdate" };

        void Awake()
        {
            sensorManager = GameObject.FindObjectOfType<SensorManagerObject>();
            sensorManager.RegisterSensor(sensor);
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
            sensorManager.UnregisterSensor(sensor);
        }

        public void ResolveCallbacks()
        {
            sensor.delegateSignalDetected = null;
            sensor.delegateDistanceCalculation = null;
            sensor.delegateLineOfSight = null;

            ResolveCallbacks(gameObject);

            if (sensorManager == null)
                sensorManager = GameObject.FindObjectOfType<SensorManagerObject>();

            if (sensorManager != null)
                ResolveCallbacks(sensorManager.gameObject);

            if (sensor.delegateSignalDetected == null && signalDetectionHandlerMethod != null && !"".Equals(signalDetectionHandlerMethod))
                Debug.LogError("Sensor Callback " + signalDetectionMonobehaviorHandler + "." +
                                    signalDetectionHandlerMethod + "() was not resolved or has been removed!");

            if (sensor.delegateDistanceCalculation == null && customDistanceHandlerMethod != null && !"".Equals(customDistanceHandlerMethod))
                Debug.LogError("Custom Distance Callback " + customDistanceMonobehaviorHandler + "." +
                                    customDistanceHandlerMethod + "() was not resolved or has been removed!");

            if (sensor.delegateLineOfSight == null && customLineOfSightHandlerMethod != null && !"".Equals(customLineOfSightHandlerMethod))
                Debug.LogError("Line Of Sight Callback " + customLineOfSightMonobehaviorHandler + "." +
                                    customLineOfSightHandlerMethod + "() was not resolved or has been removed!");
        }

        private void ResolveCallbacks(GameObject gameObject)
        {
            if (signalDetectionMonobehaviorHandler != "" && signalDetectionHandlerMethod != "")
            {
                IEnumerable<MonoBehaviour> allCallbacks = gameObject.GetComponents<MonoBehaviour>().
                    Where(x => x.name == signalDetectionHandlerMethod || x.GetType().Name == signalDetectionMonobehaviorHandler ||
                    x.GetType().BaseType.Name == signalDetectionMonobehaviorHandler);

                if (allCallbacks != null && allCallbacks.Count() > 0)
                {
                    try
                    { 
                        MonoBehaviour callbackScript = allCallbacks.First();
                        MethodInfo callbackMethod = callbackScript.GetType().GetMethods().Where(x => x.Name.Equals(signalDetectionHandlerMethod)).First();
                        sensor.delegateSignalDetected = (Sensor.DelegateSignalDetected)(
                            Sensor.DelegateSignalDetected.CreateDelegate(typeof(Sensor.DelegateSignalDetected), callbackScript, callbackMethod));
                    }
                    catch
                    {
                        signalDetectionHandlerMethod = "";
                        signalDetectionMonobehaviorHandler = "";
                    }
                }
            }

            if (customDistanceMonobehaviorHandler != "" && customDistanceHandlerMethod != "")
            {
                IEnumerable<MonoBehaviour> allCallbacks = gameObject.GetComponents<MonoBehaviour>().
                    Where(x => x.name == customDistanceHandlerMethod || x.GetType().Name == customDistanceMonobehaviorHandler ||
                    x.GetType().BaseType.Name == customDistanceMonobehaviorHandler);

                if (allCallbacks.Count() > 0)
                {
                    try
                    {
                        MonoBehaviour callbackCustomDistanceScript = allCallbacks.First();
                        MethodInfo CallbackCustomDistance = callbackCustomDistanceScript.GetType().GetMethods().Where(x => x.Name.Equals(customDistanceHandlerMethod)).First();
                        sensor.delegateDistanceCalculation = (Sensor.DelegateDistanceCalculation)(
                            Sensor.DelegateDistanceCalculation.CreateDelegate(typeof(Sensor.DelegateDistanceCalculation), callbackCustomDistanceScript, CallbackCustomDistance));
                    }
                    catch
                    {
                        customDistanceHandlerMethod = "";
                        customDistanceMonobehaviorHandler = "";
                    }
                }
                    
            }

            if (customLineOfSightMonobehaviorHandler != "" && customLineOfSightHandlerMethod != "")
            {
                IEnumerable<MonoBehaviour> allCallbacks = gameObject.GetComponents<MonoBehaviour>().
                    Where(x => x.name == customLineOfSightHandlerMethod || x.GetType().Name == customLineOfSightMonobehaviorHandler ||
                    x.GetType().BaseType.Name == customLineOfSightMonobehaviorHandler);

                if (allCallbacks.Count() > 0)
                {
                    try
                    {
                        MonoBehaviour callbackLineOfSightScript = allCallbacks.First();
                        MethodInfo CallbackLineOfSight = callbackLineOfSightScript.GetType().GetMethods().Where(x => x.Name.Equals(customLineOfSightHandlerMethod)).First();
                        sensor.delegateLineOfSight = (Sensor.DelegateLineOfSight)(
                            Sensor.DelegateLineOfSight.CreateDelegate(typeof(Sensor.DelegateLineOfSight), callbackLineOfSightScript, CallbackLineOfSight));
                    } catch
                    {
                        customLineOfSightHandlerMethod = "";
                        customLineOfSightMonobehaviorHandler = "";
                    }
                }
            }
        }

        public void resolveCallbackMethods(List<MethodInfo> methods, Type parameter)
        {
            resolveCallbackMethods(gameObject, methods, parameter);

            if (sensorManager == null)
                sensorManager = GameObject.FindObjectOfType<SensorManagerObject>();

            if (sensorManager != null)
                resolveCallbackMethods(sensorManager.gameObject, methods, parameter);
        }

        private void resolveCallbackMethods(GameObject gameObject, List<MethodInfo> methods, Type parameter)
        {

            MethodInfo[] temp;
            foreach (MonoBehaviour script in gameObject.GetComponents<MonoBehaviour>())
            {
                if (script == null)
                    continue; //which should never happen

                temp = script.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) // Instance methods, both public and private/protected
                                                                                                  //.Where(x => x.DeclaringType == script.GetType()) // Do not only list methods defined in our own class, they may be in a super class
                .Where(x => x.GetParameters().Length == 1) // Make sure we only get methods with one arguments
                .Where(x => x.GetParameters()[0].ParameterType == parameter) // Make sure we only get methods with SenseLink argument
                .Where(x => !ignoreMethods.Any(n => n == x.Name)) // Don't list methods in the ignoreMethods array (so we can exclude Unity specific methods, etc.)
                                                                  //.Select(x => x)
                .ToArray();

                methods.AddRange(temp);
            }
        }

        public void resolveCallbackMethods(List<MethodInfo> methods, Type parameter, Type returnType)
        {
            resolveCallbackMethods(gameObject, methods, parameter, returnType);

            if (sensorManager == null)
                sensorManager = GameObject.FindObjectOfType<SensorManagerObject>();

            if (sensorManager != null)
                resolveCallbackMethods(sensorManager.gameObject, methods, parameter, returnType);
        }

        // TODO: Should be generalized for n parameters
        public void resolveCallbackMethods(GameObject gameObject, List<MethodInfo> methods, Type parameter, Type returnType)
        {

            MethodInfo[] temp;
            foreach (MonoBehaviour script in gameObject.GetComponents<MonoBehaviour>())
            {
                if (script != null)
                {
                    temp = script.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) // Instance methods, both public and private/protected
                                                                                                      //.Where(x => x.DeclaringType == script.GetType()) // Do not only list methods defined in our own class, they may be in a super class
                    .Where(x => x.GetParameters().Length == 2) // Make sure we only get methods with two arguments
                    .Where(x => x.GetParameters()[0].ParameterType == typeof(Sensor)) // Make sure we only get methods with Sensor first argument
                    .Where(x => x.GetParameters()[1].ParameterType == parameter) // Make sure we only get methods with Signal second argument
                    .Where(x => x.ReturnType == returnType)
                    .Where(x => !ignoreMethods.Any(n => n == x.Name)) // Don't list methods in the ignoreMethods array (so we can exclude Unity specific methods, etc.)
                                                                      //.Select(x => x)
                    .ToArray();

                    methods.AddRange(temp);
                }
            }
        }

    }

}