using System;
using UnityEngine;

namespace UnitySensorySystem
{
    // Level of global alertness of a Sensor. Can be affected by the number of signals received and their awareness levels
    public enum Alertness
    {
        None,
        Low,
        Medium,
        High
    }

    // Characterizes the level of awareness resolved by a Sensor when it evaluates a specific signal.
    // Depends on the criteria of the Evaluate() method, e.g. distance and current alertness level
    public enum Awareness
    {
        None,
        Low,
        Medium,
        High
    }

    // The types of sense (modalities) supported by the Sensory System
    public enum SenseType
    {
        Vision,
        Hearing
    }

    public enum LineOfSightCheck
    {
        NoCheck,
        SingleRaycast,
        CompleteRaycast,
        Custom
    }

    // A ViewCone used for sensing using the SenseType.Vision. A Sensor may have one or more ViewCones
    [Serializable]
    public class ViewCone
    {
        //[Tooltip("Field Of View Angle around the center of the Sense. The center may be the forward direction or not, depends on the AngleFromForwardVector")]
        public int FoVAngle
        {
            get { return fovAngle; }
            set { fovAngle = value; }
        }
        [SerializeField]
        private int fovAngle;

        public float Range
        {
            get { return range; }
            set { range = value; }
        }
        [SerializeField]
        private float range;

        public int HorizontalOffset
        {
            get { return horizontalOffset; }
            set { horizontalOffset = value; }
        }
        [SerializeField]
        private int horizontalOffset;

        public int RecognitionDelayFrames
        {
            get { return delayFrames; }
            set { delayFrames = value; }
        }
        [SerializeField]
        private int delayFrames;

        public Awareness AwarenessLevel
        {
            get { return awarenessLevel; }
            set { awarenessLevel = value; }
        }
        [SerializeField]
        private Awareness awarenessLevel;
        [SerializeField]
        public Color SceneColor;
        [SerializeField]
        public bool DrawCone;

        public ViewCone()
        {
            SceneColor = Color.red;
            SceneColor.a = 0.6f;
            DrawCone = true;
        }

        public ViewCone(ViewCone coneToCopy)
        {
            this.FoVAngle = coneToCopy.FoVAngle;
            this.Range = coneToCopy.Range;
            this.HorizontalOffset = coneToCopy.HorizontalOffset;
            this.SceneColor = coneToCopy.SceneColor;
            this.awarenessLevel = coneToCopy.awarenessLevel;
        }

        public Awareness EvaluateSignal(Vector3 conePosition, Vector3 coneForward, Signal signal)
        {
            if ((signal.Position - conePosition).magnitude < range) //within range
            {
                if (horizontalOffset > 0)
                    coneForward = Quaternion.Euler(0, horizontalOffset, 0) * coneForward;

                if (Vector3.Angle(coneForward, signal.Position - conePosition) < fovAngle / 2)
                {
                    return awarenessLevel;
                }
            }
            return Awareness.None;
        }
    }

    public abstract class Signal
    {
        public Vector3 Position
        {
            get { return position; }
        }
        protected Vector3 position;

        public SenseType Sense;

        public float Intensity;

        private int InstanceID;

        public override bool Equals(object obj)
        {
            return obj is Signal && obj.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            if (InstanceID != 0)
                result *= InstanceID;
            return result;
        }

        public int GetInstanceID()
        {
            return InstanceID;
        }

        //Builder methods
        public Signal SetSense(SenseType sense)
        {
            this.Sense = sense;
            return this;
        }

        public Signal SetIntensity(float intensity)
        {
            this.Intensity = intensity;
            return this;
        }

        public Signal SetPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }

        public Signal SetInstanceID(int instanceID)
        {
            this.InstanceID = instanceID;
            return this;
        }
    }

    public class VisualSignal : Signal
    {
        public VisualSignal()
        {
            this.Intensity = 1;
        }

        public VisualSignal(Vector3 position)
        {
            this.Intensity = 1;
            this.SetPosition(position);
        }

    }

    public class AudioSignal : Signal
    {
        public float Range;

        public bool AttenuatedByObstacles;

        public AudioSignal(float range, bool attenuatedByObstacles)
        {
            this.Range = range;
            this.AttenuatedByObstacles = attenuatedByObstacles;
        }

        //Builder methods
        public AudioSignal()
        {
            this.AttenuatedByObstacles = false;
        }

        public AudioSignal SetAttenuatedByObstacles(bool attenuated)
        {
            this.AttenuatedByObstacles = attenuated;
            return this;
        }

        public AudioSignal SetRange(float range)
        {
            this.Range = range;
            return this;
        }
    }

    public class SenseLink
    {
        public float TimeLastSensed;

        public Awareness awarenessLevel;

        public bool FirstHand;

        public SenseType Sense
        {
            get { return sense; }
        }
        private SenseType sense;

        public Signal Signal
        {
            get { return signal; }
        }
        private Signal signal;

        public SenseLink(float time, Signal signal, Awareness awareness, bool firstHand, SenseType senseType)
        {
            this.TimeLastSensed = time;
            this.signal = signal;
            this.awarenessLevel = awareness;
            this.FirstHand = firstHand;
            this.sense = senseType;
        }

        public void IncreaseAwareness()
        {
            switch (awarenessLevel)
            {
                case Awareness.Low:
                    awarenessLevel = Awareness.Medium;
                    break;
                case Awareness.Medium:
                    awarenessLevel = Awareness.High;
                    break;
                case Awareness.High:
                    break;
            }
            // When increasing awareness the time also has to be reset to avoid premature cooldown
            TimeLastSensed = Time.time;
        }

        public void DecreaseAwareness()
        {
            switch (awarenessLevel)
            {
                case Awareness.Low:
                    awarenessLevel = Awareness.None;
                    break;
                case Awareness.Medium:
                    awarenessLevel = Awareness.Low;
                    break;
                case Awareness.High:
                    awarenessLevel = Awareness.Medium;
                    break;
            }
        }

        public static SenseLink InvalidLink()
        {
            return new SenseLink(0, null, Awareness.None, false, SenseType.Vision);
        }

        public void UpdateTimeLastSensed(float time)
        {
            TimeLastSensed = time;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SenseLink))
                return false;
            SenseLink other = (SenseLink)obj;

            return FirstHand == other.FirstHand && signal == other.signal;// && awarenessLevel == other.awarenessLevel;
        }

        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            if (signal != null)
                result *= signal.GetHashCode();
            result *= FirstHand ? 1 : 2;
            //result *= awarenessLevel.GetHashCode();
            return result;
        }

    }

}
