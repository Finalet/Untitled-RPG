using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/State")]
    public class MAIState : ScriptableObject
    {
       // public enum EvenType { OnTargetArrived, OnAnimalStateEnter, OnAnimalStateExit, OnAnimalStanceChange, OnAnimalModeStart, OnAnimalModeEnd }

        [FormerlySerializedAs("actions")]
        public MTask[] tasks;
        public MAITransition[] transitions;
        public Color GizmoStateColor = Color.gray;


        public void Update_State(MAnimalBrain brain)
        {
            Update_Tasks(brain);
            Update_Transitions(brain);
        }

        private void Update_Transitions(MAnimalBrain brain)
        {
            for (int i = 0; i < transitions.Length; i++)
            {
                if (this != brain.currentState) return; //BUG BUG BUG FIXed

                var transition = transitions[i];
                var decision = transition.decision;

                if (decision.interval > 0)
                {
                    if (brain.CheckIfDecisionsCountDownElapsed(decision.interval, i))
                    {
                        brain.ResetDecisionTimeElapsed(i);
                        Decide(brain, i, transition);
                    }
                }
                else
                {
                    Decide(brain, i, transition);
                }
            }
        }
        private void Decide(MAnimalBrain brain, int i, MAITransition transition)
        {
            bool decisionSucceded = transition.decision.Decide(brain, i);
            brain.TransitionToState(decisionSucceded ? transition.trueState : transition.falseState, decisionSucceded, transition.decision);
        }

        /// <summary>When a new State starts this method is called for each Tasks</summary>
        public void Start_Taks(MAnimalBrain brain)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].StartTask(brain,i);
        }

        /// <summary>When a new State starts this method is called for each Decisions</summary>
        public void Prepare_Decisions(MAnimalBrain brain)
        {
            for (int i = 0; i < transitions.Length; i++)
                transitions[i].decision.PrepareDecision(brain,i);
        }

        private void Update_Tasks(MAnimalBrain brain)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].UpdateTask(brain,i);
        }

        public void Finish_Tasks(MAnimalBrain brain)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].ExitTask(brain,i);
        } 


        public void Finish_Decisions(MAnimalBrain brain)
        {
            for (int i = 0; i < transitions.Length; i++)
                transitions[i].decision.FinishDecision(brain,i);
        } 
        //public void RemoveListeners_Decisions(MAnimalBrain brain)
        //{
        //    for (int i = 0; i < transitions.Length; i++)
        //        transitions[i].decision.RemoveListeners(brain,i);
        //} 

        #region Target Event Listeners for Tasks 
        public void OnTargetArrived(MAnimalBrain brain, Transform target)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnTargetArrived(brain, target,i);
        }


        public void OnAnimalEventDecisionListen(MAnimalBrain brain, MAnimal animal, int index)
        {
            transitions[index].decision.OnAnimalEventDecisionListen(brain, animal, index);
        }


        public void OnPositionArrived(MAnimalBrain brain, Vector3 Position)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnPositionArrived(brain, Position,i); 
        }
        #endregion

        #region Self Animal Listen Events
        public void OnAnimalStateEnter(MAnimalBrain brain, State state)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnAnimalStateEnter(brain, state,i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnAnimalStateEnter(brain, state, i);
        }

        public void OnAnimalStateExit(MAnimalBrain brain, State state)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnAnimalStateExit(brain, state,i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnAnimalStateExit(brain, state, i);
        }

        public void OnAnimalStanceChange(MAnimalBrain brain, int Stance)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnAnimalStanceChange(brain, Stance,i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnAnimalStanceChange(brain, Stance, i);
        }

       
        public void OnAnimalModeStart(MAnimalBrain brain, Mode mode)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnAnimalModeStart(brain, mode,i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnAnimalModeStart(brain, mode, i);
        }

        public void OnAnimalModeEnd(MAnimalBrain brain, Mode mode)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnAnimalModeEnd(brain, mode,i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnAnimalModeEnd(brain, mode, i);
        }
        #endregion

        #region Target Animal Listen Events
        public void OnTargetAnimalStateEnter(MAnimalBrain brain, State state)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnTargetAnimalStateEnter(brain, state, i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnTargetAnimalStateEnter(brain, state, i);
        }

        public void OnTargetAnimalStateExit(MAnimalBrain brain, State state)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnTargetAnimalStateExit(brain, state, i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnTargetAnimalStateExit(brain, state, i);
        }

        public void OnTargetAnimalStanceChange(MAnimalBrain brain, int Stance)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnTargetAnimalStanceChange(brain, Stance, i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnTargetAnimalStanceChange(brain, Stance, i);
        }


        public void OnTargetAnimalModeStart(MAnimalBrain brain, Mode mode)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnTargetAnimalModeStart(brain, mode, i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnTargetAnimalModeStart(brain, mode, i);
        }

        public void OnTargetAnimalModeEnd(MAnimalBrain brain, Mode mode)
        {
            for (int i = 0; i < tasks.Length; i++)
                tasks[i].OnTargetAnimalModeEnd(brain, mode, i);

            //for (int i = 0; i < transitions.Length; i++)
            //    transitions[i].decision.OnTargetAnimalModeEnd(brain, mode, i);
        }
        #endregion

    }

    [System.Serializable]
    public class MAITransition
    {
        public MAIDecision decision;
        public MAIState trueState;
        public MAIState falseState;
    }
}
