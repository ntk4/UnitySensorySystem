using UnityEngine;
using System.Collections;

public class SensorHandler : MonoBehaviour {

    public void ReactToSignal(SenseLink senseLink)
    {
        Debug.Log("I see you! " + GetLevel(senseLink.awarenessLevel));
    }


    private string GetLevel(Awareness awareness)
    {
        if (awareness == Awareness.None)
            return "";
        else
            return awareness.ToString() + " awareness";
    }
}
