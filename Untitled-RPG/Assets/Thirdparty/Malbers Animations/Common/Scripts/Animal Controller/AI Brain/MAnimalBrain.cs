using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using MalbersAnimations.Events;
using System;
using UnityEngine.AI;

namespace MalbersAnimations.Controller.AI
{
    //[RequireComponent(typeof(MAnimalAIControl))]
    public class MAnimalBrain : MonoBehaviour, IAnimatorListener
    {
        /// <summary>Transform used to raycast Rays to interact with the world</summary>
        [RequiredField, Tooltip("Transform used to raycast Rays to interact with the world")]
        public Transform Eyes;
        /// <summary>Time needed to make a new transition. Necesary to avoid Changing to multiple States in the same frame</summary>
        [Tooltip("Time needed to make a new transition. Necesary to avoid Changing to multiple States in the same frame")]
        public float TransitionCoolDown = 0.2f;
        /// <summary>Last Time the Animal make a new transition</summary>
        private float TransitionLastTime;

        /// <summary>Last Time the Animal  started a transition</summary>
        public float StateLastTime { get; set; }

        [Space]
        /// <summary>Reference AI State for the animal</summary>
        public MAIState currentState;

        /// <summary>Reference of an Empty State</summary>
        public MAIState remainInState;


        [Space, Tooltip("Removes all AI Components when the Animal Dies. (Brain, AiControl, Agent)")]
        public bool RemoveAIOnDeath = true;
        public bool debug = true;

        [Space]
        [Header("Events")]
        public IntEvent OnTaskStarted = new IntEvent();
        public IntEvent OnDecisionSucceded = new IntEvent();
        
       

        /// <summary>Tasks Local Vars (1 Int,1 Bool,1 Float)</summary>
        internal BrainVars[] TasksVars;
        /// <summary>Saves on the a Task that it has finish is stuff</summary>
        internal bool[] TasksDone;
        /// <summary>Decision Local Vars (1 Int,1 Bool,1 Float)</summary>
        internal BrainVars[] DecisionsVars;

        #region Properties
        /// <summary>Reference for the Ai Control Movement</summary>
        public MAnimalAIControl AIMovement { get; set; }

        private MAnimal animal;
        /// <summary>Reference for the Animal</summary>
        public MAnimal Animal
        {
            get
            {
                if (animal == null) animal = GetComponent<MAnimal>();
                   
                return animal;
            }
        }

        /// <summary>Reference for the AnimalStats</summary>
        public Dictionary<int, Stat> AnimalStats { get; set; }

        #region Target References
        /// <summary>Reference for the Current Target the Animal is using</summary>
        public Transform Target { get; set; }


        private MAnimal targetAnimal;
        /// <summary>Reference for the Target the Animal Component</summary>
        public MAnimal TargetAnimal
        {
            get { return targetAnimal; }
            set
            {
                //currentState.RemoveListeners_Decisions(this);
                targetAnimal = value;
            }
        }


        public Vector3 AgentPosition => AIMovement.Agent.transform.position;
        public NavMeshAgent Agent => AIMovement.Agent;
       
        /// <summary>Has the Animal Arrived to the Target Position... [NOT THE DESTINATION POSITION] </summary>
        public bool ArrivedToTarget =>  Target ? (Vector3.Distance(AgentPosition, AIMovement.GetTargetPosition() ) < AIMovement.GetTargetStoppingDistance()) : false;

        public float AgentHeight { get; private set; }
       
        /// <summary>True if the Current Target has Stats</summary>
        public bool TargetHasStats { get; private set; }

        /// <summary>Reference for the Target the Stats Component</summary>
        public Dictionary<int, Stat> TargetStats { get; set; }
        #endregion

        /// <summary>Reference for the Last WayPoint the Animal used</summary>
        public IWayPoint LastWayPoint { get; set; }

        /// <summary>Time Elapsed for the Tasks on an AI State</summary>
        [HideInInspector] public float[] TasksTimeElapsed;// { get; set; }

        /// <summary>Time Elapsed for the State Decisions</summary>
        [HideInInspector] public float[] DecisionsTimeElapsed;// { get; set; }
        #endregion


        void Awake()
        {
            animal = GetComponent<MAnimal>();
            AIMovement = GetComponent<MAnimalAIControl>();
            var AnimalStatscomponent = GetComponent<Stats>();
            if (AnimalStatscomponent) AnimalStats = AnimalStatscomponent.stats_D;

            AgentHeight = transform.lossyScale.y * AIMovement.Agent.height;
        }

        void Start()
        {
            Animal.isPlayer.Value = false; //If is using a brain... disable that he is the main player
            StartNewState(currentState);

            AIMovement.AutoNextTarget = false;

            LastWayPoint = null;

          if (AIMovement.Target)
                SetLastWayPoint(AIMovement.Target);
        }
      

        void Update() { currentState.Update_State(this); }
    
        public virtual void TransitionToState(MAIState nextState, bool decisionValue, MAIDecision decision)
        {
            if (Time.time - TransitionLastTime >= TransitionCoolDown) //This avoid making transition on the same Frame ****IMPORTANT
            {
                if (nextState != remainInState)
                {
                    TransitionLastTime = Time.time;

                    if (debug)
                        Debug.Log($"Changed from <B>{currentState.name} </B> to <B>{nextState.name}</B> with the decision <b>{decision.name}</b> been <B>{decisionValue}</B>.");

                    InvokeDecisionEvent(decisionValue, decision);

                    StartNewState(nextState);
                }
            }
        }

        private void InvokeDecisionEvent(bool decisionValue, MAIDecision decision)
        {
            if (decision.send == MAIDecision.WSend.SendTrue && decisionValue)
            {
                OnDecisionSucceded.Invoke(decision.MessageID);
            }
            else if (decision.send == MAIDecision.WSend.SendFalse && !decisionValue)
            {
                OnDecisionSucceded.Invoke(decision.MessageID);
            }
        }

        void StartNewState(MAIState newState)
        {
            StateLastTime = Time.time;      //Store the last time the Animal made a transition

            currentState.Finish_Tasks(this);                    //Finish all the Task on the Current State
            currentState.Finish_Decisions(this);                    //Finish all the Decisions on the Current State

            currentState = newState;                            //Set a new State

            PrepareVarsOnNewState();

            currentState.Start_Taks(this);                      //Start all Tasks on the new State
            currentState.Prepare_Decisions(this);                      //Start all Tasks on the new State


            foreach (var tasks in currentState.tasks)         //Invoke the Task Events in case anyone wants to listen
            {
                int taskID = tasks.MessageID;
                if (taskID != 0) OnTaskStarted.Invoke(taskID);
            }
        }


        /// <summary>Prepare all the local variables on the New State before starting new tasks</summary>
        private void PrepareVarsOnNewState()
        {
            TasksVars = new BrainVars[currentState.tasks.Length];                  //Local Variables you can use on your tasks
            TasksTimeElapsed = new float[currentState.tasks.Length];               //Reset all the Tasks    Time elapsed time

            TasksDone = new bool[currentState.tasks.Length];                       //Reset if they are Done

            DecisionsVars = new BrainVars[currentState.transitions.Length];        //Local Variables you can use on your Decisions
            DecisionsTimeElapsed = new float[currentState.transitions.Length];          //Reset all the Decisions Time elapsed time
        }


        public bool IsTaskDone(int TaskIndex)
        {
            return TasksDone[TaskIndex];
        }

        public void TaskDone(int TaskIndex, bool value = true)
        {
            TasksDone[TaskIndex] = value;
        }


        /// <summary> Check if the time elapsed of a task using a duration or CountDown time </summary>
        /// <param name="duration">Duration of the countDown|CoolDown</param>
        /// <param name="index">Index of the Task on the AI State Tasks list</param>
        public bool CheckIfTasksCountDownElapsed(float duration,int index)
        {
            TasksTimeElapsed[index] += Time.deltaTime;
            return TasksTimeElapsed[index] >= duration;
        }

        /// <summary> Check if the time elapsed of a task using a duration or CountDown time </summary>
        /// <param name="duration">Duration of the countDown|CoolDown</param>
        /// <param name="index">Index of the Task on the AI State Tasks list</param>
        public bool CheckIfDecisionsCountDownElapsed(float duration, int index)
        {
            DecisionsTimeElapsed[index] += Time.deltaTime;
            return DecisionsTimeElapsed[index] >= duration;
        }

        /// <summary>Reset the Time elapsed on a Task using its index from the Tasks list</summary>
        /// <param name="Index">Index of the Task on the AI state</param>
        public void ResetTaskTimeElapsed(int Index)
        { TasksTimeElapsed[Index] = 0; }
        
        /// <summary>Reset the Time elapsed on a Decision using its index from the Transition List </summary>
        /// <param name="Index">Index of the Decision on the AI State Transition List</param>
        public void ResetDecisionTimeElapsed(int Index) 
        { DecisionsTimeElapsed[Index] = 0; }

        /// <summary>Removes the Target on the Animal</summary>
        public void RemoveTarget() { AIMovement.SetTarget(null, false); }
       
        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);
        }

        #region Event Listeners
        void OnEnable()
        {
            AIMovement.OnTargetArrived.AddListener(OnTargetArrived);
            AIMovement.OnTargetPositionArrived.AddListener(OnPositionArrived);
            AIMovement.OnTargetSet.AddListener(OnTargetSet);

            Animal.OnStateChange.AddListener(OnAnimalStateChange);
            Animal.OnStanceChange.AddListener(OnAnimalStanceChange);
            Animal.OnModeStart.AddListener(OnAnimalModeStart);
            Animal.OnModeEnd.AddListener(OnAnimalModeEnd);
        }

        void OnDisable()
        {
            AIMovement.OnTargetArrived.RemoveListener(OnTargetArrived);
            AIMovement.OnTargetPositionArrived.RemoveListener(OnPositionArrived);

            Animal.OnStateChange.RemoveListener(OnAnimalStateChange);
            Animal.OnStanceChange.RemoveListener(OnAnimalStanceChange);
            Animal.OnModeStart.RemoveListener(OnAnimalModeStart);
            Animal.OnModeEnd.RemoveListener(OnAnimalModeEnd);
        }
        #endregion

        #region SelfAnimal Event Listeners
        void OnAnimalStateChange(int state)
        {
            currentState.OnAnimalStateEnter(this, Animal.ActiveState);
            currentState.OnAnimalStateExit(this, Animal.LastState);

            if (RemoveAIOnDeath && state == StateEnum.Death) //meaning this animal has died
            {
                for (int i = 0; i < currentState.tasks.Length; i++)         //Exit the Current Tasks
                    currentState.tasks[i].ExitTask(this, i);

                Destroy(AIMovement.Agent.gameObject);
                Destroy(AIMovement);
                Destroy(this);
            }
        }


        void OnAnimalStanceChange(int stance) { currentState.OnAnimalStanceChange(this, Animal.Stance); }
        void OnAnimalModeStart(int mode) { currentState.OnAnimalModeStart(this, Animal.ActiveMode); }
        void OnAnimalModeEnd(int mode) { currentState.OnAnimalModeEnd(this, Animal.ActiveMode); }
        #endregion

        #region TargetAnimal Event Listeners
        void OnTargetAnimalStateChange(int state)
        {
            currentState.OnTargetAnimalStateEnter(this, Animal.ActiveState);
            currentState.OnTargetAnimalStateExit(this, Animal.LastState);
        }
  
        private void OnTargetArrived(Transform target)
        {
            currentState.OnTargetArrived(this, target);
        }

        private void OnPositionArrived(Vector3 position)
        {
            currentState.OnPositionArrived(this, position);
        }

        private void OnTargetSet(Transform target)
        {
            Target = target;

            if (target)
            {
                TargetAnimal = target.GetComponentInParent<MAnimal>();// ?? target.GetComponentInChildren<MAnimal>();

                TargetStats = null;
                var TargetStatsC = target.GetComponentInParent<Stats>();// ?? target.GetComponentInChildren<Stats>();

                TargetHasStats = TargetStatsC != null;
                if (TargetHasStats) TargetStats = TargetStatsC.stats_D;

               // SetLastWayPoint(target);
            }
        }

        public void SetLastWayPoint(Transform target)
        {
            LastWayPoint = target.GetComponentInParent<IWayPoint>() ?? LastWayPoint; //If not is a waypoint save the last one
        }

        public void SetLastWayPoint()
        {
            if (Target)
                LastWayPoint = Target.GetComponentInParent<IWayPoint>() ?? LastWayPoint; //If not is a waypoint save the last one
        }

        #endregion


#if UNITY_EDITOR


        void Reset()
        {
            remainInState = MalbersTools.GetInstance<MAIState>("Remain in State");
            Animal.isPlayer.Value = false; //Make sure this animal is not the Main Player

            AIMovement = GetComponent<MAnimalAIControl>();

            if (AIMovement)
            {
                AIMovement.AutoInteract = false;
                AIMovement.AutoNextTarget = false;
                AIMovement.UpdateTargetPosition = false;
                AIMovement.MoveAgentOnMovingTarget= false;
                AIMovement.LookAtTargetOnArrival= false;
            }
        }


        void OnDrawGizmos()
        {
            if (isActiveAndEnabled && currentState && Eyes)
            {
                Gizmos.color = currentState.GizmoStateColor;
                Gizmos.DrawWireSphere(Eyes.position, 0.2f);

                if (currentState && debug)
                {
                    foreach (var act in currentState.tasks)
                        act?.DrawGizmos(this);

                    foreach (var tran in currentState.transitions)
                        tran?.decision?.DrawGizmos(this);
                }
            }
        }
#endif 
    }

    public enum Affected { Self, Target }
    public enum ExecuteTask { OnStart, OnUpdate,OnExit }

    public struct BrainVars
    {
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public Vector3 V3Value;
    }
}