using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    public abstract class MAIDecision : BrainBase
    {
        public enum WSend { SendTrue,SendFalse}

        [Space, Tooltip("ID Used for sending messages to the Brain to see if the Decision was TRUE or FALSE")]
        public IntReference MessageID = new IntReference(0);
        [Tooltip("What to send if a Decision is successful")]
        public WSend send = WSend.SendTrue;

        /// <summary>Execute the Decide method every x Seconds</summary>
        [Tooltip("Execute the Decide method every x Seconds to improve performance")]
        public FloatReference interval = new FloatReference(0.2f);

        /// <summary>Decides which State to take on the Transition based on the Return value</summary>
        /// <param name="brain">The Animal using the Decision</param>
        /// <param name="Index">Index of the Transition on the AI State</param>
        public abstract bool Decide(MAnimalBrain brain, int Index);


        /// <summary>Prepares a Decision the an AI State Starts (OPTIONAL)</summary>
        /// <param name="brain">The Animal using the Decision</param>
        /// <param name="Index">Index of the Transition on the AI State</param>
        public virtual void PrepareDecision(MAnimalBrain brain, int Index) { }

        public virtual void FinishDecision(MAnimalBrain brain, int Index) { }

     //   public virtual void RemoveListeners(MAnimalBrain brain, int Index) { }

        public virtual void OnAnimalEventDecisionListen(MAnimalBrain brain, MAnimal animal, int Index) { }

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
