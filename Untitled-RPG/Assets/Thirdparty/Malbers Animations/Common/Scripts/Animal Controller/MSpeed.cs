using MalbersAnimations.Scriptables;
using System.Collections.Generic;

namespace MalbersAnimations.Controller
{
    [System.Serializable]
    public class MSpeedSet
    {
        public string name;
        public List<StateID> states;
       // public List<StanceID> stances;
        public IntReference StartVerticalIndex;
        public IntReference TopIndex;
        /// <summary> List of Speed Modifiers for the Speed Set</summary>
        public List<MSpeed> Speeds;

        /// <summary> Current Active Index of the SpeedSet</summary>
        public int CurrentIndex { get; set; }

        public MSpeedSet()
        {
            name = "Set Name";
            states = new List<StateID>();
            StartVerticalIndex = new IntReference(1);
            TopIndex = new IntReference(2);
            Speeds = new List<MSpeed>(1)
            {
                new MSpeed("SpeedName",1,4,4) 
            };
        }

        public MSpeed this[int index]
        {
            get => Speeds[index];
            set => Speeds[index] = value;
        }
    }
    [System.Serializable]
    /// <summary>Position, Rotation and Animator Modifiers for the Animals</summary>
    public struct MSpeed
    {
        /// <summary>Default value for an MSpeed</summary>
        public static readonly MSpeed Default = new MSpeed("Default", 1, 4, 4);

        /// <summary>Name of this Speed</summary>
        public string name;

        /// <summary>Name of the Speed converted to HashCode, easier to compare</summary>
        public int nameHash;

        ///// <summary>Name of this Speed</summary>
        //public bool active = false;

        /// <summary>Vertical Mutliplier for the Animator</summary>
        public FloatReference Vertical;

        /// <summary>Add additional speed to the transfrom</summary>
        public FloatReference position;
        /// <summary> Smoothness to change to the Transform speed, higher value more Responsiveness</summary>
        public FloatReference lerpPosition;

        /// <summary>Changes the Animator Speed</summary>
        public FloatReference animator;

        /// <summary> Smoothness to change to the Animator speed, higher value more Responsiveness </summary>
        public FloatReference lerpAnimator;

        /// <summary>Add Aditional Rotation to the Speed</summary>
        public FloatReference rotation;

        /// <summary>Can this Speed Sprint?</summary>
        public bool sprint;

        /// <summary> Smoothness to change to the Rotation speed, higher value more Responsiveness </summary>
        public FloatReference lerpRotation;

        ///// <summary> State on which this Speed Modifier can be used </summary>
        //public int state;



        public MSpeed(MSpeed newSpeed)
        {
            name = newSpeed.name;
            position = newSpeed.position;
            animator = newSpeed.animator;
            lerpPosition = newSpeed.lerpPosition;
            lerpAnimator = newSpeed.lerpAnimator;
            rotation = newSpeed.rotation;
            lerpRotation = newSpeed.lerpRotation;
            Vertical = newSpeed.Vertical;
            nameHash = name.GetHashCode();
            sprint = newSpeed.sprint;
            //active = false;
        }


        public MSpeed(string name, float lerpPos, float lerpanim)
        {
            this.name = name;
            position = 0;
            animator = 1;
            rotation = 0;
            lerpPosition = lerpPos;
            lerpAnimator = lerpanim;
            lerpRotation = 4;
            Vertical = 1;
            nameHash = name.GetHashCode();
            sprint = true;
            //active = false;
        }

        public MSpeed(string name, float vertical, float lerpPos, float lerpanim)
        {
            this.name = name;
            position = 0;
            animator = 1;
            rotation = 0;
            lerpPosition = lerpPos;
            lerpAnimator = lerpanim;
            lerpRotation = 4;
            Vertical = vertical;
            nameHash = name.GetHashCode();
            sprint = true;
            //active = false;
        }


        public MSpeed(string name)
        {
            this.name = name;
            position = 0;
            animator = 1;
            rotation = 0;
            lerpPosition = 4;
            lerpAnimator = 4;
            Vertical = 1;
            lerpRotation = 4;
            nameHash = name.GetHashCode();
            sprint = true;
            //active = false;
        }

        public MSpeed(float lerpPos, float lerpanim)
        {
            name = "SpeedName";
            position = 0;
            animator = 1;
            rotation = 0;
            lerpPosition = lerpPos;
            lerpAnimator = lerpanim;
            sprint = true;
            Vertical = 1;
            lerpRotation = 4;
            nameHash = name.GetHashCode();
        }
    }
}