using UnityEngine;
using System.Collections.Generic;

public class SensorManager : MonoBehaviour
{
    public int FramesDelay;
    private int FramesSinceLastExecution;

    private Dictionary<int, Sensor> sensors;
    private int nextSensorIndex;

    private Dictionary<int, Signal> signals;
    private int nextSignalIndex;


    /// <summary>
    /// Maintains the SenseLinks per Sensor Index. The key corresponds to the key of sensors dictionary.
    /// </summary>
    private Dictionary<int, List<SenseLink>> sensorLinks;

    Signal iterSignal;
    Sensor iterSensor;
    List<SenseLink> iterSenseLinks;

    void Awake()
    {
        nextSensorIndex = 0;
        nextSignalIndex = 0;
        sensors = new Dictionary<int, Sensor>();
        signals = new Dictionary<int, Signal>();
        sensorLinks = new Dictionary<int, List<SenseLink>>();
    }

    void Update()
    {
        if (FramesSinceLastExecution++ >= FramesDelay)
        {
            FramesSinceLastExecution = 0;

            coolDownSensors();

            evaluateSignals();
        }
    }

    private void coolDownSensors()
    {
        // decrease the awareness to implement cooldown
        foreach (int sensorKey in sensorLinks.Keys)
        {
            iterSenseLinks = sensorLinks[sensorKey];

            if (iterSenseLinks != null && iterSenseLinks.Count > 0)
            {
                iterSensor = sensors[sensorKey];
                float coolDownTimePerPhase = iterSensor.CalculateCooldownTimePerPhase();

                foreach (SenseLink iterLink in iterSenseLinks)
                {
                    if (iterLink.TimeLastSensed + coolDownTimePerPhase < Time.time)
                    {
                        iterLink.DecreaseAwareness();
                        iterLink.UpdateTimeLastSensed(Time.time);
                    }
                }

            }

        }
    }

    private void evaluateSignals()
    {
        //for (int signalIndex = 0; signalIndex < signals.Values.Count; signalIndex++)
        foreach(int signalkey in signals.Keys)
        {
            iterSignal = signals[signalkey];
            foreach (int sensorkey in sensors.Keys)
            {
                evaluateIteration(sensorkey);
            }
        }
    }

    private void evaluateIteration(int sensorkey)
    {
        iterSensor = sensors[sensorkey];
        if (iterSensor == null) //in the meantime the gameObject may have died
        {
            sensors.Remove(sensorkey);
            return;
        }

        List<SenseLink> memoryLinks = null;
        try
        {
            memoryLinks = sensorLinks[sensorkey];
        }
        catch
        {
            memoryLinks = new List<SenseLink>();
        }

        SenseLink link = iterSensor.Evaluate(iterSignal);
        if (link != null && iterSensor.CallbackOnSignalDetected != null)
        {
            object[] parameters = new object[1];

            //1. Find the SenseLink in the memory of the particular sensor
            int memoryLinkIndex = memoryLinks.IndexOf(link);

            link.UpdateTimeLastSensed(Time.time);

            // instantly increase the awareness level if the same signal is in higher awareness zone. 
            // Decreasing awareness is done elsewhere based on the cooldown time
            if (memoryLinkIndex != -1 && memoryLinks[memoryLinkIndex].awarenessLevel < link.awarenessLevel)
            {
                //Replace the old one to increase the awareness
                memoryLinks[memoryLinkIndex] = link;
            }
            else if (memoryLinkIndex != -1) //signal detected but with less or equal awareness
            { //Equal is ignored here, less has already been handled by the cooldown mechanism
                //Replace the old one to update the timeLastSensed
                memoryLinks[memoryLinkIndex] = link;
                return;
            }
            else if (memoryLinkIndex == -1) // new signal, add new entry
            {
                memoryLinks.Add(link);
                sensorLinks[sensorkey] = memoryLinks;
            }

            parameters[0] = link;
            iterSensor.CallbackOnSignalDetected.Invoke(iterSensor.callbackScript, parameters);
        }
    }

    public int RegisterSensor(Sensor sensor)
    {
        bool exists = sensors.ContainsValue(sensor);
        if (!exists)
        {
            sensors.Add(nextSensorIndex, sensor);
            sensorLinks.Add(nextSensorIndex, new List<SenseLink>());
            return nextSensorIndex++;
        }
        return -1;
    }

    public void UnregisterSensor(int RegistrationNumber)
    {
        if (IsValidSensor(RegistrationNumber))
        {
            sensors.Remove(RegistrationNumber);
            sensorLinks.Remove(RegistrationNumber);
        }
    }

    private bool IsValidSensor(int RegistrationNumber)
    {
        return RegistrationNumber >= 0 && sensors.ContainsKey(RegistrationNumber);
    }

    public int RegisterSignal(Signal signal)
    {
        bool exists = signals.ContainsValue(signal);
        if (!exists)
        {
            signals.Add(nextSignalIndex, signal);
            return nextSignalIndex++;
        }
        return -1;
    }

    public void UnregisterSignal(int RegistrationNumber)
    {
        if (IsValidSignal(RegistrationNumber))
        {
            signals.Remove(RegistrationNumber);
        }
    }

    private bool IsValidSignal(int RegistrationNumber)
    {
        return RegistrationNumber >= 0 && signals.ContainsKey(RegistrationNumber);
    }
}
