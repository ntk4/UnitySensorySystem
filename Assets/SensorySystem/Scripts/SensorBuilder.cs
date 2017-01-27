using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorBuilder {

    private SensorManager sensorMgr;

    public static SensorBuilder BeginSensorSystem()
    {
        return new SensorBuilder();
    }

    public SensorBuilder CreateSensorManager()
    {
        if (sensorMgr != null)
            cleanSensorManager(sensorMgr);
        sensorMgr = new SensorManager();
        return this;
    }

    private void cleanSensorManager(SensorManager sensorMgr)
    {
        //TODO: properly unregister all sensors & signals
        sensorMgr = null;
    }

    public SensorBuilder AddSensor(Sensor sensor)
    {
        if (sensorMgr != null)
            sensorMgr.RegisterSensor(sensor);
        return this;
    }

    public Sensor CreateSensor()
    {
        return new Sensor();
    }

    public SensorBuilder AddSignal(Signal signal)
    {
        if (sensorMgr != null)
            sensorMgr.RegisterSignal(signal);
        return this;
    }

    public VisualSignal CreateVisionSignal(Transform transform)
    {
        return new VisualSignal();
    }

    public AudioSignal CreateAudioSignal()
    {
        return new AudioSignal();
    }

    public SensorManager build()
    {
        return sensorMgr;
    }
}
