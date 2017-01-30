using UnityEngine;
using System.Collections.Generic;

public class SensorManager
{
    private int FramesSinceLastExecution;

    private List<Sensor> sensors = new List<Sensor>();
    private int nextSensorIndex = 0;

    private List<Signal> signals = new List<Signal>();
    private int nextSignalIndex = 0;


    /// <summary>
    /// Maintains the SenseLinks per Sensor Index. The key corresponds to the key of sensors dictionary.
    /// </summary>
    private Dictionary<Sensor, List<SenseLink>> sensorLinks = new Dictionary<Sensor, List<SenseLink>>();
    
    List<SenseLink> iterSenseLinks;

    public void Update(int FramesDelay)
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
        foreach (Sensor sensor in sensorLinks.Keys)
        {
            iterSenseLinks = sensorLinks[sensor];

            if (iterSenseLinks != null && iterSenseLinks.Count > 0)
            {
                //iterSensor = sensors.[sensor];
                float coolDownTimePerPhase = sensor.CalculateCooldownTimePerPhase();

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
        foreach(Signal signal in signals)
        {
            //iterSignal = signals[signalkey];
            foreach (Sensor sensor in sensors)
            {
                evaluateIteration(sensor, signal);
            }
        }
    }

    private void evaluateIteration(Sensor iterSensor, Signal signal)
    {
        if (iterSensor == null) //in the meantime the gameObject may have died
        {
            sensors.Remove(iterSensor);
            return;
        }

        List<SenseLink> memoryLinks = null;
        try
        {
            memoryLinks = sensorLinks[iterSensor];
        }
        catch
        {
            memoryLinks = new List<SenseLink>();
        }

        SenseLink link = iterSensor.Evaluate(signal);
        if (link != null && iterSensor.delegateSignalDetected != null)
        {
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
                sensorLinks[iterSensor] = memoryLinks;
            }
            
            iterSensor.delegateSignalDetected.Invoke(link);
        }
    }

    public int RegisterSensor(Sensor sensor)
    {
        bool exists = sensors.Contains(sensor);
        if (!exists)
        {
            //TODO: let sensor resolve its own ID with an independent authority, not SensorManager
            sensor.SetInstanceID(nextSensorIndex); 
            sensors.Add(sensor);
            sensorLinks.Add(sensor, new List<SenseLink>());
            return nextSensorIndex++;
        }
        return -1;
    }

    public void UnregisterSensor(Sensor sensor)
    {
        if (IsValidSensor(sensor))
        {
            sensors.Remove(sensor);
            sensorLinks.Remove(sensor);
        }
    }

    private bool IsValidSensor(Sensor sensor)
    {
        return sensor != null && sensors.Contains(sensor);
    }

    public int RegisterSignal(Signal signal)
    {
        bool exists = signals.Contains(signal);
        if (!exists)
        {
            //TODO: let signal resolve its own ID with an independent authority, not SensorManager
            signal.SetInstanceID(nextSignalIndex);
            signals.Add(signal);
            return nextSignalIndex++;
        }
        return -1;
    }

    public void UnregisterSignal(Signal signal)
    {
        if (IsValidSignal(signal))
        {
            signals.Remove(signal);
        }
    }

    private bool IsValidSignal(Signal signal)
    {
        return signal != null && signals.Contains(signal);
    }
}
