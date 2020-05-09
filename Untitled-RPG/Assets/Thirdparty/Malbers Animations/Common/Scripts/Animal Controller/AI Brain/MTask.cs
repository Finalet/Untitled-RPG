using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller.AI
{
    public abstract class MTask : BrainBase 
    {
        [Tooltip("ID Used for sending messages to the Brain to see if the Task started")]
        public IntReference MessageID = new IntReference(0);
        //[Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        //public Affected affect = Affected.Self;

        /// <summary>When a AI State starts using this Task, Run this Code</summary>
        public virtual void StartTask(MAnimalBrain brain, int index) { }

        /// <summary>While a AI State is runing using this Task, run thi Code</summary>
        public virtual void UpdateTask(MAnimalBrain brain, int index) { }

        /// <summary>While a AI State Ends, run this Code</summary>
        public virtual void ExitTask(MAnimalBrain brain, int index) { }

        public virtual void OnTargetArrived(MAnimalBrain brain, Transform target, int index) { }

        public virtual void OnPositionArrived(MAnimalBrain brain, Vector3 Position, int index) { }

        public virtual void OnAnimalStateEnter(MAnimalBrain brain, State state, int index) { }
        public virtual void OnAnimalStateExit(MAnimalBrain brain, State state, int index) { }
        public virtual void OnAnimalStanceChange(MAnimalBrain brain, int Stance, int index) { }
        public virtual void OnAnimalModeStart(MAnimalBrain brain, Mode mode, int index) { }
        public virtual void OnAnimalModeEnd(MAnimalBrain brain, Mode mode, int index) { }

        public virtual void OnTargetAnimalStateEnter(MAnimalBrain brain, State state, int index) { }
        public virtual void OnTargetAnimalStateExit(MAnimalBrain brain, State state, int index) { }
        public virtual void OnTargetAnimalStanceChange(MAnimalBrain brain, int Stance, int index) { }
        public virtual void OnTargetAnimalModeStart(MAnimalBrain brain, Mode mode, int index) { }
        public virtual void OnTargetAnimalModeEnd(MAnimalBrain brain, Mode mode, int index) { }
    }

    public abstract class BrainBase : ScriptableObject
    {
        [Space,TextArea(3,10)]
        public string Description = "Type Description Here";
        public virtual void DrawGizmos(MAnimalBrain brain) { }

        //public virtual void OnAnimalStateEnter(MAnimalBrain brain, State state, int index) { }
        //public virtual void OnAnimalStateExit(MAnimalBrain brain, State state, int index) { }
        //public virtual void OnAnimalStanceChange(MAnimalBrain brain, int Stance, int index) { }
        //public virtual void OnAnimalModeStart(MAnimalBrain brain, Mode mode, int index) { }
        //public virtual void OnAnimalModeEnd(MAnimalBrain brain, Mode mode, int index) { }

        //public virtual void OnTargetAnimalStateEnter(MAnimalBrain brain, State state, int index) { }
        //public virtual void OnTargetAnimalStateExit(MAnimalBrain brain, State state, int index) { }
        //public virtual void OnTargetAnimalStanceChange(MAnimalBrain brain, int Stance, int index) { }
        //public virtual void OnTargetAnimalModeStart(MAnimalBrain brain, Mode mode, int index) { }
        //public virtual void OnTargetAnimalModeEnd(MAnimalBrain brain, Mode mode, int index) { }

    }
}