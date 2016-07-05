using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Sensor))]
public class SensorEditor : Editor
{
    static List<MethodInfo> methods;
    static string[] ignoreMethods = new string[] { "Start", "Update", "LateUpdate", "FixedUpdate" };

    public override void OnInspectorGUI()
    {
        Sensor sensor = (Sensor)target;

        sensor.Sense = (SenseType)EditorGUILayout.EnumPopup("Sense type", sensor.Sense);
        sensor.CoolDownSeconds = EditorGUILayout.FloatField("Full Cooldown in seconds", sensor.CoolDownSeconds);
        AddCallbackGUI(sensor);

        EditorGUILayout.Space();

        if (sensor.Sense == SenseType.Vision)
        {
            bool val = EditorGUILayout.Toggle("Preview View Cones", sensor.DrawCones);
            if (val != sensor.DrawCones)
            {
                Undo.RecordObject(sensor, "Preview cones");
                sensor.DrawCones = val;
                MarkSceneDirty();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Add new View Cone"))
            {
                Undo.RecordObject(sensor, "Add new View Cone");
                sensor.AddViewCone();
                MarkSceneDirty();
                SceneView.RepaintAll();
            }
            if (sensor.ViewCones != null)
            {
                for (int i = 0; i < sensor.ViewCones.Count; i++)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("View Cone " + (i + 1), EditorStyles.boldLabel);

                    int value = EditorGUILayout.IntSlider("FoV (degrees)", sensor.ViewCones[i].FoVAngle, 0, 180);
                    if (value != sensor.ViewCones[i].FoVAngle)
                    {
                        Undo.RecordObject(sensor, "Change View Cone");
                        MarkSceneDirty();
                        sensor.ViewCones[i].FoVAngle = value;
                        SceneView.RepaintAll();
                    }

                    value = EditorGUILayout.IntSlider("Range of sight", (int)sensor.ViewCones[i].Range, 0, 15);
                    if (value != sensor.ViewCones[i].Range)
                    {
                        Undo.RecordObject(sensor, "Change View Cone");
                        MarkSceneDirty();
                        sensor.ViewCones[i].Range = value;
                        SceneView.RepaintAll();
                    }

                    Awareness aw = (Awareness)EditorGUILayout.EnumPopup("Awareness Level", sensor.ViewCones[i].AwarenessLevel);
                    if (aw != sensor.ViewCones[i].AwarenessLevel)
                    {
                        Undo.RecordObject(sensor, "Change View Cone");
                        MarkSceneDirty();
                        sensor.ViewCones[i].AwarenessLevel = aw;
                        SceneView.RepaintAll();
                    }

                    value = EditorGUILayout.IntSlider("Horizontal Offset", sensor.ViewCones[i].HorizontalOffset, 0, 360);
                    if (value != sensor.ViewCones[i].HorizontalOffset)
                    {
                        Undo.RecordObject(sensor, "Change View Cone");
                        MarkSceneDirty();
                        sensor.ViewCones[i].HorizontalOffset = value;
                        SceneView.RepaintAll();
                    }

                    value = EditorGUILayout.IntSlider("Recognition delay", sensor.ViewCones[i].RecognitionDelayFrames, 0, 60);
                    if (value != sensor.ViewCones[i].RecognitionDelayFrames)
                    {
                        Undo.RecordObject(sensor, "Change Recognition delay");
                        MarkSceneDirty();
                        sensor.ViewCones[i].RecognitionDelayFrames = value;
                        SceneView.RepaintAll();
                    }

                    Color col = EditorGUILayout.ColorField("Scene color", sensor.ViewCones[i].SceneColor);
                    if (col != sensor.ViewCones[i].SceneColor)
                    {
                        Undo.RecordObject(sensor, "Change View Cone");
                        MarkSceneDirty();
                        sensor.ViewCones[i].SceneColor = col;
                        SceneView.RepaintAll();
                    }

                    bool drawCone = EditorGUILayout.Toggle("Draw", sensor.ViewCones[i].DrawCone);
                    if (drawCone != sensor.ViewCones[i].DrawCone)
                    {
                        Undo.RecordObject(sensor, (drawCone ? "Enable" : "Disable") + " Draw Cone");
                        sensor.ViewCones[i].DrawCone = drawCone;
                        MarkSceneDirty();
                        SceneView.RepaintAll();
                    }


                    if (GUILayout.Button("Remove"))
                    {
                        Undo.RecordObject(sensor, "Remove View Cone");
                        sensor.RemoveViewCone(i);
                        MarkSceneDirty();
                        SceneView.RepaintAll();
                    }
                }
            }
        }
        
    }

    private void AddCallbackGUI(Sensor sensor)
    {
        //if (methods == null)
        resolveMethods(sensor);

        if (sensor != null)
        {
            int index;

            try
            { // resolve the index of the already selected method, if any
                index = methods
                    .Select((v, i) => new { Method = v, Index = i })
                    .First(x => x.Method == sensor.CallbackOnSignalDetected)
                    .Index;
            }
            catch
            { 
                //fallback, use the first
                index = 0;
            }

            if (methods != null && methods.Count > 0)
            {
                int val = EditorGUILayout.Popup("Signal handler", index, 
                    methods.Select(x => x.DeclaringType.Name + "." + x.Name).ToArray());

                if (methods[val] != sensor.CallbackOnSignalDetected)
                {
                    //Undo.RecordObject(sensor, "Set Callback");
                    //MarkSceneDirty();
                    sensor.CallbackOnSignalDetected = methods[val];
                }

                if (sensor.CallbackOnSignalDetected != null)
                {
                    sensor.signalDetectionHandlerMethod = methods[val].Name;
                    sensor.signalDetectionMonobehaviorHandler = methods[val].DeclaringType.Name;
                }
            }
        }
    }

    private void resolveMethods(Sensor sensor)
    {
        methods = new List<MethodInfo>();

        MethodInfo[] temp;
        foreach (MonoBehaviour script in sensor.gameObject.GetComponents<MonoBehaviour>())
        {
            temp = script.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) // Instance methods, both public and private/protected
            //.Where(x => x.DeclaringType == script.GetType()) // Do not only list methods defined in our own class, they may be in a super class
            .Where(x => x.GetParameters().Length == 1) // Make sure we only get methods with one arguments
            .Where(x => x.GetParameters()[0].ParameterType == typeof(SenseLink)) // Make sure we only get methods with SenseLink argument
            .Where(x => !ignoreMethods.Any(n => n == x.Name)) // Don't list methods in the ignoreMethods array (so we can exclude Unity specific methods, etc.)
            //.Select(x => x)
            .ToArray();

            methods.AddRange(temp);
        }
    }

    void OnSceneGUI()
    {
        Sensor sensor = (Sensor)target;

        if (!sensor.DrawCones || !sensor.enabled)
            return;

        if (sensor.ViewCones == null)
            return;

        for (int i = 0; i < sensor.ViewCones.Count; i++)
        {
            ViewCone vc = sensor.ViewCones[i];
            if (vc.DrawCone)
            {
                Handles.color = vc.SceneColor;
                Handles.DrawSolidArc(sensor.transform.position, sensor.transform.up,
                        Quaternion.Euler(0, -vc.FoVAngle * 0.5f + vc.HorizontalOffset, 0) * sensor.transform.forward,
                        vc.FoVAngle, vc.Range);
            }
        }
    }

    private void MarkSceneDirty()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

}