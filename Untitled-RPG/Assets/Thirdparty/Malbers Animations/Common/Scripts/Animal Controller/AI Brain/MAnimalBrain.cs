using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Events;
using UnityEngine.AI;
using MalbersAnimations.Scriptables;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller.AI
{
    [AddComponentMenu("Malbers/Animal Controller/AI/Animal Brain")]
    public class MAnimalBrain : MonoBehaviour, IAnimatorListener
    {
        /// <summary>Reference for the Ai Control Movement</summary>
        [RequiredField] public MAnimalAIControl AIMovement;
        /// <summary>Transform used to raycast Rays to interact with the world</summary>
        [RequiredField, Tooltip("Transform used to raycast Rays to interact with the world")]
        public Transform Eyes;
        /// <summary>Time needed to make a new transition. Necesary to avoid Changing to multiple States in the same frame</summary>
        [Tooltip("Time needed to make a new transition. Necessary to avoid Changing to multiple States in the same frame")]
        public FloatReference TransitionCoolDown = new FloatReference(0.2f);

        /// <summary>Reference AI State for the animal</summary>
        public MAIState currentState;

        ///// <summary>Reference of an Empty State</summary>
        public MAIState remainInState;

        [Tooltip("Removes all AI Components when the Animal Dies. (Brain, AiControl, Agent)")]
        public bool RemoveAIOnDeath = true;
        public bool debug = false;


        public IntEvent OnTaskStarted = new IntEvent();
        public IntEvent OnDecisionSucceeded = new IntEvent();
        public IntEvent OnAIStateChanged = new IntEvent();


        /// <summary>Last Time the Animal make a new transition</summary>
        private float TransitionLastTime;

        /// <summary>Last Time the Animal  started a transition</summary>
        public float StateLastTime { get; set; }

        /// <summary>Tasks Local Vars (1 Int,1 Bool,1 Float)</summary>
        internal BrainVars[] TasksVars;
        /// <summary>Saves on the a Task that it has finish is stuff</summary>
        internal bool[] TasksDone;
        /// <summary>Current Decision Results</summary>
        internal bool[] DecisionResult;
        /// <summary>Store if a Task has Started</summary>
        internal bool[] TasksStarted;
        /// <summary>Decision Local Vars to store values on Prepare Decision</summary>
        internal BrainVars[] DecisionsVars;

        #region Properties


        /// <summary>Reference for the Animal</summary>
        public MAnimal Animal => AIMovement.animal;

        /// <summary>Reference for the AnimalStats</summary>
        public Dictionary<int, Stat> AnimalStats { get; set; }

        #region Target References
        /// <summary>Reference for the Current Target the Animal is using</summary>
        public Transform Target { get; set; }
        //{ 
        //    get => target; 
        //    set 
        //    {
        //    target = value;
        //    }
        //}
        //private Transform target;

        /// <summary>Reference for the Target the Animal Component</summary>
        public MAnimal TargetAnimal { get; set; }

        public Vector3 AgentPosition => AIMovement.Agent.transform.position;
        public NavMeshAgent Agent => AIMovement.Agent;

        /// <summary>Has the Animal Arrived to the Target Position... [NOT THE DESTINATION POSITION] </summary>
        public bool ArrivedToTarget => Target ? (Vector3.Distance(AgentPosition, AIMovement.GetTargetPosition()) < AIMovement.GetTargetStoppingDistance()) : false;

        public float AgentHeight { get; private set; }

        /// <summary>True if the Current Target has Stats</summary>
        public bool TargetHasStats { get; private set; }

        /// <summary>Reference for the Target the Stats Component</summary>
        public Dictionary<int, Stat> TargetStats { get; set; }
        #endregion

        /// <summary>Reference for the Last WayPoint the Animal used</summary>
        public IWayPoint LastWayPoint { get; set; }

        /// <summary>Time Elapsed for the Tasks on an AI State</summary>
        [HideInInspector] public float[] TasksTime;// { get; set; }

        /// <summary>Time Elapsed for the State Decisions</summary>
        [HideInInspector] public float[] DecisionsTime;// { get; set; }

        #endregion


        void Awake()
        {
            if (AIMovement == null) AIMovement = Animal.FindComponent<MAnimalAIControl>();
            var AnimalStatscomponent = Animal.FindComponent<Stats>();
            if (AnimalStatscomponent) AnimalStats = AnimalStatscomponent.stats_D;

            AgentHeight = transform.lossyScale.y * AIMovement.Agent.height;
        }

        public void StartBrain()
        {
            Animal.isPlayer.Value = false; //If is using a brain... disable that he is the main player
            AIMovement.StartAgent();

            if (currentState)
            {
                for (int i = 0; i < currentState.tasks.Length; i++)
                {
                    if (currentState.tasks[i] == null)
                    {
                        Debug.LogError($"The [{currentState.name}] AI State has an Empty Task. AI States can't have empty Tasks. {Animal.name} brain is Disabled", currentState);
                        enabled = false;
                        return;
                    };

                }

                Debuging($"<color=white> Setting First AI State <B>[{currentState.name}]</B> </color>");
                StartNewState(currentState);
            }
            else
            {
                enabled = false;
                return;
            }
            AIMovement.AutoNextTarget = false;

            LastWayPoint = null;

            if (AIMovement.Target)
                SetLastWayPoint(AIMovement.Target);



        }


        void Update()
        {
            if (currentState != null) currentState.Update_State(this);
        }

        public virtual void TransitionToState(MAIState nextState, bool decisionValue, MAIDecision decision, int Index)
        {
            if (MTools.ElapsedTime(TransitionLastTime, TransitionCoolDown)) //This avoid making transition on the same Frame ****IMPORTANT
            {
                if (nextState != null && nextState != remainInState && nextState != currentState) //Do not transition to itself!
                {
                    TransitionLastTime = Time.time;

                    decision.FinishDecision(this, Index);


                    Debuging($"<color=white>Changed AI State from <B>[{currentState.name}]</B> to <B>[{nextState.name}]</B>. Decision: <b>[{decision.name}]</b> = <B>[{decisionValue}]</B>.</color>");

                    InvokeDecisionEvent(decisionValue, decision);

                    StartNewState(nextState);
                }
            }
        }

        protected virtual void Debuging(string Log) { if (debug) Debug.Log($"<B>{Animal.name}:</B> " + Log); }

        private void InvokeDecisionEvent(bool decisionValue, MAIDecision decision)
        {
            if (decision.send == MAIDecision.WSend.SendTrue && decisionValue)
            {
                OnDecisionSucceeded.Invoke(decision.DecisionID);
            }
            else if (decision.send == MAIDecision.WSend.SendFalse && !decisionValue)
            {
                OnDecisionSucceeded.Invoke(decision.DecisionID);
            }
        }

        public virtual void StartNewState(MAIState newState)
        {
            StateLastTime = Time.time;      //Store the last time the Animal made a transition


            if (currentState != null && currentState != newState)
            {
                currentState.Finish_Tasks(this);                 //Finish all the Task on the Current State
             // currentState.Finish_Decisions(this);             //Finish all the Decisions on the Current State
            }

            currentState = newState;                            //Set a new State

            ResetVarsOnNewState();

            OnAIStateChanged.Invoke(currentState.ID);
            currentState.Start_AIState(this);                      //Start all Tasks on the new State
            currentState.Prepare_Decisions(this);               //Start all Tasks on the new State
        }


        /// <summary>Prepare all the local variables on the New State before starting new tasks</summary>
        private void ResetVarsOnNewState()
        {
            var tasks = currentState.tasks.Length > 0 ? currentState.tasks.Length : 1;
            var transitions = currentState.transitions.Length > 0 ? currentState.transitions.Length : 1;

            TasksVars = new BrainVars[tasks];                //Local Variables you can use on your tasks
            TasksTime = new float[tasks];                    //Reset all the Tasks    Time elapsed time

            TasksDone = new bool[tasks];                     //Reset if they are Done
            TasksStarted = new bool[tasks];                  //Reset if they tasks are started

            DecisionsVars = new BrainVars[transitions];      //Local Variables you can use on your Decisions
            DecisionsTime = new float[transitions];          //Reset all the Decisions Time elapsed time
            DecisionResult = new bool[transitions];          //Reset if they tasks are started
        }


        public bool IsTaskDone(int TaskIndex) => TasksDone[TaskIndex];

        public void TaskDone(int TaskIndex, bool value = true) //If the first task is done then go and do the next one
        {
            TasksDone[TaskIndex] = value;
            if (TaskIndex + 1 < currentState.tasks.Length && currentState.tasks[TaskIndex + 1].WaitForPreviousTask) //Start the next task that needs to wait for the previus one
            {
                currentState.StartTaks(this, TaskIndex + 1);
                //Debug.Log($"*Task DONE!!!!: [{name}] [{TaskIndex}]-[{currentState.tasks[TaskIndex].name }]");
            }
        }

        

        /// <summary> Check if the time elapsed of a task using a duration or CountDown time </summary>
        /// <param name="duration">Duration of the countDown|CoolDown</param>
        /// <param name="index">Index of the Task on the AI State Tasks list</param>
        public bool CheckIfDecisionsCountDownElapsed(float duration, int index)
        {
            DecisionsTime[index] += Time.deltaTime;
            return DecisionsTime[index] >= duration;
        }

        /// <summary>Reset the Time elapsed on a Task using its index from the Tasks list</summary>
        /// <param name="Index">Index of the Task on the AI state</param>
        public void ResetTaskTime(int Index) => TasksTime[Index] = 0;
        
        /// <summary>Set the time on which a task has tarted on the current AI State</summary>
        public void SetElapsedTaskTime(int Index) => TasksTime[Index] = Time.time;

        /// <summary>Reset the Time elapsed on a Decision using its index from the Transition List </summary>
        /// <param name="Index">Index of the Decision on the AI State Transition List</param>
        public void ResetDecisionTime(int Index) => DecisionsTime[Index] = 0;

        public virtual bool OnAnimatorBehaviourMessage(string message, object value) =>   this.InvokeWithParams(message, value);

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

            StartBrain();
        }

        void OnDisable()
        {
            AIMovement.OnTargetArrived.RemoveListener(OnTargetArrived);
            AIMovement.OnTargetPositionArrived.RemoveListener(OnPositionArrived);

            Animal.OnStateChange.RemoveListener(OnAnimalStateChange);
            Animal.OnStanceChange.RemoveListener(OnAnimalStanceChange);
            Animal.OnModeStart.RemoveListener(OnAnimalModeStart);
            Animal.OnModeEnd.RemoveListener(OnAnimalModeEnd);

            AIMovement.Stop();
            StopAllCoroutines();
            AIMovement.StopAllCoroutines();

        }
        #endregion

        #region SelfAnimal Event Listeners
        void OnAnimalStateChange(int state)
        {
            currentState?.OnAnimalStateEnter(this, Animal.ActiveState);
            currentState?.OnAnimalStateExit(this, Animal.LastState);

            if (state == StateEnum.Death) //meaning this animal has died
            {
                for (int i = 0; i < currentState.tasks.Length; i++)         //Exit the Current Tasks
                    currentState.tasks[i].ExitAIState(this, i);

                enabled = false;

                if (RemoveAIOnDeath)
                {
                    Destroy(AIMovement.Agent);
                   // AIMovement.enabled = false;
                    Destroy(AIMovement);
                    Destroy(this);
                }
            }
        }

        void OnAnimalStanceChange(int stance) => currentState.OnAnimalStanceChange(this, Animal.Stance.ID);

        void OnAnimalModeStart(int mode, int ability) => currentState.OnAnimalModeStart(this, Animal.ActiveMode);

        void OnAnimalModeEnd(int mode, int ability) => currentState.OnAnimalModeEnd(this, Animal.ActiveMode);


        #endregion

        #region TargetAnimal Event Listeners
        void OnTargetAnimalStateChange(int state)
        {
            currentState.OnTargetAnimalStateEnter(this, Animal.ActiveState);
            currentState.OnTargetAnimalStateExit(this, Animal.LastState);
        }

        private void OnTargetArrived(Transform target) => currentState.OnTargetArrived(this, target);

        private void OnPositionArrived(Vector3 position) => currentState.OnPositionArrived(this, position);

        private void OnTargetSet(Transform target)
        {
            Target = target;

            if (target)
            {
                TargetAnimal = target.FindComponent<MAnimal>();// ?? target.GetComponentInChildren<MAnimal>();

                TargetStats = null;
                var TargetStatsC = target.FindComponent<Stats>();// ?? target.GetComponentInChildren<Stats>();

                TargetHasStats = TargetStatsC != null;
                if (TargetHasStats) TargetStats = TargetStatsC.stats_D;

                // SetLastWayPoint(target);
            }
        }

        public bool CheckForPreviusTaskDone(int index)
        {
            if (index == 0) return true;

            if (!TasksStarted[index] && IsTaskDone(index - 1))
                return true;

            return false;
        }

        public void SetLastWayPoint(Transform target)
        {
            var newLastWay = target.gameObject.FindInterface<IWayPoint>();
            if (newLastWay != null)   LastWayPoint = target?.gameObject.FindInterface<IWayPoint>(); //If not is a waypoint save the last one
        }

        public void SetLastWayPoint()
        {
            if (Target)
                LastWayPoint = Target.GetComponentInParent<IWayPoint>() ?? LastWayPoint; //If not is a waypoint save the last one
        }

        #endregion


#if UNITY_EDITOR

        [SerializeField] private int Editor_Tabs1;


        void Reset()
        {
            remainInState = MTools.GetInstance<MAIState>("Remain in State");
            AIMovement = this.FindComponent<MAnimalAIControl>();

            if (AIMovement)
            {
                //AIMovement.AutoInteract = false;
                AIMovement.AutoNextTarget = false;
                AIMovement.UpdateTargetPosition = false;
                AIMovement.MoveAgentOnMovingTarget = false;
                AIMovement.LookAtTargetOnArrival = false;

                if (Animal) Animal.isPlayer.Value = false; //Make sure this animal is not the Main Player

            }
            else
            {
                Debug.LogWarning("There's No AI Control in this GameObject, Please add one");
            }
        }

        void OnDrawGizmos()
        {
            if (isActiveAndEnabled && currentState && Eyes)
            {
                Gizmos.color = currentState.GizmoStateColor;
                Gizmos.DrawWireSphere(Eyes.position, 0.2f);

                if (debug)
                {
                    if (currentState != null)
                    {
                        if (currentState.tasks != null)
                            foreach (var task in currentState.tasks)
                                task?.DrawGizmos(this);

                        if (currentState.transitions != null)
                            foreach (var tran in currentState.transitions)
                                tran?.decision?.DrawGizmos(this);
                    }
                }

                if (Application.isPlaying)
                {
                    string desicions = "";

                    //UnityEditor.Handles.color = currentState.GizmoStateColor;
                    //for (int i = 0; i < currentState.transitions.Length; i++)
                    //{
                    //    desicions += "\n" + "Decision[" + i + "] - " + currentState.transitions[i].decision.name + " [" + DecisionResult[i].ToString()+"]";
                    //}

                    var Styl =  new GUIStyle(EditorStyles.boldLabel); 
                    Styl.normal.textColor = Color.yellow;

                    UnityEditor.Handles.Label(Eyes.position, "State: " + currentState.name + desicions, Styl);
                }
            }
        }
#endif 
    }

    public enum Affected { Self, Target }
    public enum ExecuteTask { OnStart, OnUpdate, OnExit }

    [System.Serializable]
    public struct BrainVars
    {
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public Vector3 V3Value; 
        public Component[] Components;
        //public Component ComponentValue;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MAnimalBrain)), CanEditMultipleObjects]
    public class MAnimalBrainEditor : Editor
    {
        SerializedProperty AIMovement, Eyes, debug, TransitionCoolDown, RemoveAIOnDeath, Editor_Tabs1,
            currentState, remainInState, OnTaskStarted, OnDecisionSucceded, OnAIStateChanged;

        protected string[] Tabs1 = new string[] { "AI States" ,"References" ,"Events" };

        private void OnEnable()
        {
            AIMovement = serializedObject.FindProperty("AIMovement");
            Eyes = serializedObject.FindProperty("Eyes");
            TransitionCoolDown = serializedObject.FindProperty("TransitionCoolDown");
            RemoveAIOnDeath = serializedObject.FindProperty("RemoveAIOnDeath");
            currentState = serializedObject.FindProperty("currentState");
            remainInState = serializedObject.FindProperty("remainInState");

            OnTaskStarted = serializedObject.FindProperty("OnTaskStarted");
            OnDecisionSucceded = serializedObject.FindProperty("OnDecisionSucceeded");
            OnAIStateChanged = serializedObject.FindProperty("OnAIStateChanged");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            debug = serializedObject.FindProperty("debug");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Brain Logic for the Animal");
            EditorGUILayout.BeginVertical(MTools.StyleGray);
            {

                Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);


                if (Editor_Tabs1.intValue == 1) DrawReferences();
                else if (Editor_Tabs1.intValue == 0) DrawAIStates();
                else DrawEvents();


                //EditorGUILayout.PropertyField(debug);
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAIStates()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            MTools.DrawScriptableObject(currentState, false);
            MalbersEditor.DrawDebugIcon(debug);
            EditorGUILayout.EndHorizontal();
            // EditorGUILayout.PropertyField(remainInState);
            EditorGUILayout.PropertyField(TransitionCoolDown);
            EditorGUILayout.PropertyField(RemoveAIOnDeath);

            

            EditorGUILayout.EndVertical();
        }

        private void DrawEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(OnAIStateChanged);
            EditorGUILayout.PropertyField(OnTaskStarted);
            EditorGUILayout.PropertyField(OnDecisionSucceded);
            EditorGUILayout.EndVertical();
        }

        private void DrawReferences()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(AIMovement, new GUIContent("AI Control"));
            EditorGUILayout.PropertyField(Eyes);
            EditorGUILayout.EndVertical();
        }
    }
#endif
}