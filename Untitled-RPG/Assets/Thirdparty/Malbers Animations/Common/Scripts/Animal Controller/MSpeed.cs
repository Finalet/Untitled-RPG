using MalbersAnimations.Scriptables;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MalbersAnimations.Controller
{
    [System.Serializable]
    public class MSpeedSet : IComparable,IComparer
    {
        public string name;
        public List<StateID> states;
        public List<StanceID> stances;
        public IntReference StartVerticalIndex;
        public IntReference TopIndex;
        public FloatReference BackSpeedMult = new FloatReference(0.5f);
        /// <summary> List of Speed Modifiers for the Speed Set</summary>
        public List<MSpeed> Speeds;

        /// <summary>THis Speed Set has no Stances
        public bool HasStances => stances != null && stances.Count > 0;
      // public bool HasStates => states != null && states.Count > 0;

        /// <summary> Current Active Index of the SpeedSet</summary>
        public int CurrentIndex { get; set; }

        public MSpeedSet()
        {
            name = "Set Name";
            states = new List<StateID>();
            StartVerticalIndex = new IntReference(1);
            TopIndex = new IntReference(2);
            Speeds = new List<MSpeed>(1) { new MSpeed("SpeedName", 1, 4, 4) };
        }

        public MSpeed this[int index]
        {
            get => Speeds[index];
            set => Speeds[index] = value;
        }

        public MSpeed this[string name] => Speeds.Find(x => x.name == name);
       


        public bool HasStance(int stance)
        {
            if (!HasStances) return true;
            else  return stances.Find(s => s.ID == stance);
        }

        public int Compare(object x, object y)
        {
            bool XHas = (x as MSpeedSet).HasStances;
            bool YHas = (y as MSpeedSet).HasStances;

            if (XHas && YHas)
                return 0;
            else if (XHas && !YHas)
                return 1;
            else return -1;
        }

        public int CompareTo(object obj)
        {
            bool XHas = (obj as MSpeedSet).HasStances;
            bool YHas = HasStances;

            if (XHas && YHas)
                return 0;
            else if (XHas && !YHas)
                return 1;
            else return -1;
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

        /// <summary> Smoothness to change to the Animator Vertical speed, higher value more Responsiveness</summary>
        public FloatReference lerpPosAnim;


        /// <summary>Add Aditional Rotation to the Speed</summary>
        public FloatReference rotation;
 
        /// <summary> Smoothness to change to the Rotation speed, higher value more Responsiveness </summary>
        public FloatReference lerpRotation;

        /// <summary> Smoothness to change to the Animator Vertical speed, higher value more Responsiveness</summary>
        public FloatReference lerpRotAnim;

        /// <summary>Changes the Animator Speed</summary>
        public FloatReference animator;

        /// <summary> Smoothness to change to the Animator speed, higher value more Responsiveness </summary>
        public FloatReference lerpAnimator;

        /// <summary>Strafe Stored Velocity</summary>
        public FloatReference strafeSpeed;


        /// <summary> Smoothness to change to the Rotation speed, higher value more Responsiveness </summary>
        public FloatReference lerpStrafe;


        public MSpeed(MSpeed newSpeed)
        {
            name = newSpeed.name;

            position = newSpeed.position;
            lerpPosition = newSpeed.lerpPosition;
            lerpPosAnim = newSpeed.lerpPosAnim;

            rotation = newSpeed.rotation;
            lerpRotation = newSpeed.lerpRotation;
            lerpRotAnim = newSpeed.lerpRotAnim;

            animator = newSpeed.animator;
            lerpAnimator = newSpeed.lerpAnimator;
            Vertical = newSpeed.Vertical;
            strafeSpeed = newSpeed.strafeSpeed;
            strafeSpeed = newSpeed.strafeSpeed;
            lerpStrafe = newSpeed.lerpStrafe;

            nameHash = name.GetHashCode();
        }


        public MSpeed(string name, float lerpPos, float lerpanim)
        {
            this.name = name;
            Vertical = 1;

            position = 0;
            lerpPosition = lerpPos;
            lerpPosAnim = 4;

            rotation = 0;
            strafeSpeed = 0;
            lerpRotation = 4;
            lerpRotAnim = 4;
            lerpStrafe = 4;

            animator = 1;
            lerpAnimator = lerpanim;
            nameHash = name.GetHashCode();
        }

        public MSpeed(string name, float vertical, float lerpPos, float lerpanim)
        {
            this.name = name;
            Vertical = vertical;

            position = 0;
            lerpPosition = lerpPos;
            lerpPosAnim = 4;

            rotation = 0;
            strafeSpeed = 0;
            lerpRotation = 4;
            lerpRotAnim = 4;
            lerpStrafe = 4;


            animator = 1;
            lerpAnimator = lerpanim;

            nameHash = name.GetHashCode();
        }


        public MSpeed(string name)
        {
            this.name = name;
            Vertical = 1;
            
            position = 0;
            lerpPosition = 4;
            lerpPosAnim = 4;


            rotation = 0;
            strafeSpeed = 0;

            lerpRotation = 4;
            lerpRotAnim = 4;
            lerpStrafe = 4;


            animator = 1;
            lerpAnimator = 4;

            nameHash = name.GetHashCode();
        }
    }
}