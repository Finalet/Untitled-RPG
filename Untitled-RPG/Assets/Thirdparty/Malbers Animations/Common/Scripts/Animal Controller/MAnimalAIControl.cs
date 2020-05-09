using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Utilities;
using UnityEngine.Events;
using MalbersAnimations.Events;
using System;
using UnityEngine.AI;

namespace MalbersAnimations.Controller
{
    public class MAnimalAIControl : MonoBehaviour, IAIControl, IAITarget
    {
        [HideInInspector] public int Editor_Tabs1;

        #region Components References
        private NavMeshAgent agent;                 //The NavMeshAgent
        protected MAnimal animal;                    //The Animal Script
        #endregion

        #region Internal Variables
        /// <summary>The way to know if there no Target Position vector to go to</summary>
        protected static Vector3 NullVector = MalbersTools.NullVector;

        /// <summary>Target Last Position (Useful to know if the Target is moving)</summary>
        protected Vector3 TargetLastPosition = NullVector;

        /// <summary>Stores the Remainin distance to the Target's Position</summary>
        public float RemainingDistance { get; private set; }

        /// <summary>Stores the Remainin distance to the Target's Position</summary>
        protected float DefaultStopDistance;

        /// <summary>When Assigning a Target it will automatically start moving</summary>       
        public bool MoveAgent { get; set; }
       
        private IEnumerator I_WaitToNextTarget;
        private IEnumerator IFlyOffMesh;
        private IEnumerator IClimbOffMesh;
        #endregion

        #region Public Variables
        [SerializeField] protected float stoppingDistance = 0.6f;

        /// <summary>The animal will change automatically to Walk if the distance to the target is this value</summary>
        [SerializeField] protected float walkDistance = 1f;

        internal void ResetStoppingDistance() { StoppingDistance = DefaultStopDistance; }


        /// <summary>Default Speed Stored for the Animal while is using the AI Control/summary>
        private int defaultGroundIndex;
        [SerializeField] private Transform target;

        /// <summary>Means the Animal will go to a next Target when it reaches the current target automatically</summary>
        public bool AutoNextTarget = true;
        /// <summary>Means the Animal will interact to any Waypoint automatically when he arrived to it</summary>
        public bool AutoInteract = true;

        /// <summary>If the Target Moves the Agent will move too ... to the given destination</summary>
        public bool MoveAgentOnMovingTarget = true;

        /// <summary>If the Target Moves the Agent will move too ... to the given destination</summary>
        public bool LookAtTargetOnArrival = false;

        /// <summary>Check if the Animal Moved every x Seconds</summary>
        public float MovingTargetInterval = 0.2f;

        /// <summary>If the Target moves then it will foollow it</summary>
        [SerializeField] private bool updateTargetPosition = true;

        public bool debug = false;
        public bool debugGizmos = true;
        #endregion

        #region Properties 
        /// <summary>is the Animal, Flying, swimming, On Free Mode?</summary>
        public bool FreeMove { get; private set; }

        /// <summary>Is the Animal Playing a mode</summary>
        public bool IsOnMode { get; private set; }



        /// <summary>Has the animal Arrived to their current destination</summary>
        public bool HasArrived  { get; internal set; }
        //{
        //    get => hasArriv;
        //    internal set
        //    {
        //        hasArriv = value;
        //        Debug.Log(hasArriv);
        //    }
        //}
        //private bool hasArriv;


        public bool IsGrounded { get; private set; }

        public bool IsMovingOffMesh { get; private set; }

        /// <summary>Is the Target a WayPoint?</summary>
        public IWayPoint IsWayPoint { get; private set; }

        /// <summary>Destination Point == NullVector which means that the Point is Empty </summary>
      //  public bool NullDestination => DestinationPosition == NullVector;


        /// <summary>Is the Target an AITarget</summary>
        public IAITarget IsAITarget { get; private set; }
        #endregion 

        #region Events
        [Space]
        public Vector3Event OnTargetPositionArrived = new Vector3Event();
        public TransformEvent OnTargetArrived = new TransformEvent();
        public TransformEvent OnTargetSet = new TransformEvent();
        #endregion

        #region Properties

        public WayPointType pointType = WayPointType.Ground;

        public WayPointType TargetType => pointType;


        /// <summary>Reference of the Nav Mesh Agent</summary>
        public NavMeshAgent Agent
        {
            get
            {
                if (agent == null)
                    agent = GetComponentInChildren<NavMeshAgent>();
                return agent;
            }
        }

        /// <summary>Current Stopping Distance for the Next Waypoint</summary>
        public float StoppingDistance
        {
            get { return stoppingDistance; }
            set { Agent.stoppingDistance = stoppingDistance = value; }
        }

        /// <summary>Get the Position of this AI target</summary>
        public Vector3 GetPosition() { return Agent.transform.position; }


        /// <summary>is the Target transform moving??</summary>
        public bool TargetIsMoving { get; private set; }


        /// <summary> Is the Animal waiting x time to go to the Next waypoint</summary>
        public bool IsWaiting { get; private set; }

        /// <summary>Destination Position to use on Agent.SetDestination()</summary>
        public Vector3 DestinationPosition { get; private set; }

        public Transform NextTarget { get; private set; }
        public Transform Target => target;

        /// <summary>Update The Target Position from the Target. This should be false if the Position you want to go is different than the Target's position</summary>
        public bool UpdateTargetPosition { get => updateTargetPosition; set => updateTargetPosition = value; }


        #endregion

        #region Unity Functions

        void Awake() { animal = GetComponent<MAnimal>(); }


        void Start() { StartAgent(); }
        void OnEnable()
        {
            animal.OnStateChange.AddListener(OnState);           
            animal.OnModeStart.AddListener(OnModeStart);
            animal.OnModeEnd.AddListener(OnModeEnd);
            animal.OnGrounded.AddListener(OnGrounded);
        }

       

        void OnDisable()
        {
            animal.OnStateChange.RemoveListener(OnState);           //Listen when the Animations changes..
            animal.OnModeStart.RemoveListener(OnModeStart);           //Listen when the Animations changes..
            animal.OnModeEnd.RemoveListener(OnModeEnd);           //Listen when the Animations changes..
            animal.OnGrounded.RemoveListener(OnGrounded);           //Listen when the Animations changes..
                                                                    //  animal.OnStateChange.RemoveListener(OnStateChanged);           //Listen when the Animations changes..
        }

      

        void Update() { Updating(); }
        #endregion

        protected virtual void StartAgent()
        {
            Agent.updateRotation = false;                                       //The Animal will control the rotation . NOT THE AGENT
            Agent.updatePosition = false;                                       //The Animal will control the  postion . NOT THE AGENT
            DefaultStopDistance = StoppingDistance;                             //Store the Started Stopping Distance
            Agent.stoppingDistance = StoppingDistance;

            animal.UseSmoothVertical = false;                               //IMPORTANT ... This Avoid the Animal to stop when they are turinng

            HasArrived = false;
            TargetIsMoving = false;
            var targ = target;

            target = null;
            SetTarget(targ);                                                  //Set the first Target (IMPORTANT)  it also set the next future targets

            IsWaiting = false;
            MoveAgent = true;

            var locomotion = animal.State_Get(StateEnum.Locomotion);

            if (locomotion != null && locomotion.SpeedSet != null)
            {
                defaultGroundIndex = locomotion.SpeedSet.StartVerticalIndex;
            }

            InvokeRepeating("CheckMovingTarget", 0f, MovingTargetInterval);
        }

        protected virtual void Updating()
        {
            if (IsMovingOffMesh) return;                                    //Do nothing while is moving ofmesh (THE Coroutine is in charge of the movement)

            Agent.nextPosition = agent.transform.position;                  //Update the Agent Position to the Transform position   IMPORTANT!!!!

            if (MovingTargetInterval == 0)
                CheckMovingTarget();

            if (FreeMove)
            {
                //Debug.Log("FreeMovement");
                FreeMovement();
            }
            else if (IsGrounded)                                               //if we are on a NAV MESH onGround
            {
                if (IsWaiting || IsOnMode)
                {
                    return;    //If the Animal is Waiting do nothing . .... he is doing something else... wait until he's finish
                }
                else
                    UpdateAgent();
            }
        }

        /// <summary> Check if the Target is moving </summary>
        private void CheckMovingTarget()
        {
            if (target)
            {
                TargetIsMoving = (target.position - TargetLastPosition).magnitude > 0.005f;
                TargetLastPosition = target.position;

                if (TargetIsMoving)
                    Update_TargetPos();
            }
        }


        /// <summary> Updates the Agents using he animation root motion </summary>
        protected virtual void UpdateAgent()
        {
            if (Agent.pathPending || !Agent.isOnNavMesh)
                return;    //Means is still calculating the path to go

            RemainingDistance = Agent.remainingDistance;                //Store the remaining distance -- but if navMeshAgent is still looking for a path Keep Moving

            if (RemainingDistance <= StoppingDistance)                   //if We Arrive to the Destination
            {
                if (!HasArrived)
                    Arrive_Destination();
                else //HasArrived == true
                {
                    if (LookAtTargetOnArrival)
                    {
                        var LookAtDir = (target != null ? target.position : DestinationPosition) - transform.position;
                        animal.LookAtDirection(LookAtDir);
                    }
                }
            }
            else
            {
                HasArrived = false;
                animal.Move(Agent.desiredVelocity);                     //Move the Animal using the Agent Direction

                CheckWalkDistance();
                CheckOffMeshLinks();
            }
        }



        #region Set Assing Target and Next Targets
        public virtual void SetTarget(GameObject target) { SetTarget(target.transform, true); }

        public virtual void SetTarget(Transform target) { SetTarget(target, true); }

        /// <summary>Assign a new Target but it does not move it to it</summary>
        public virtual void SetTargetOnly(Transform target) { SetTarget(target, false); }


        /// <summary>Moves to the Assigned Target</summary>
        public virtual void MoveToDestination()
        {
            CheckAirTarget();
            Debuging("is travelling to : <B>" + target.name + "</B>");
            MoveAgent = true;
            ResumeAgent();
        }

        /// <summary>Set the next Target</summary>   
        public virtual void SetTarget(Transform target, bool Move)
        {
            IsWaiting = false;
            animal.Mode_Interrupt();             //In Case it was making any Mode;
            this.target = target;

            IsAITarget = null; //Reset the AI Target
            OnTargetSet.Invoke(target);
            //Debug.LogWarning("SetTarget:", target);

            if (target != null)
            {
                TargetLastPosition = target.position; //Since is a new Target "Reset the Target last position"
                DestinationPosition = target.position;                           //Update the Target Position 
                IsAITarget = target.GetComponentInParent<IAITarget>();

                HasArrived = false;

                StoppingDistance = GetTargetStoppingDistance();
                DestinationPosition = GetTargetPosition();

                IsWayPoint = target.GetComponent<IWayPoint>();
                NextTarget = IsWayPoint?.NextTarget();
                RemainingDistance = float.MaxValue;
                MoveAgent = Move;

                MoveToDestination();
            }
            else
            {
                Stop(); //Means the Target is null so Stop the Animal
            }
        }


        public Vector3 GetTargetPosition()
        {
            return IsAITarget != null ? IsAITarget.GetPosition() : target.position;
        }

        public float GetTargetStoppingDistance()
        {
            return IsAITarget != null ? IsAITarget.StopDistance() : DefaultStopDistance;
        }

        /// <summary>Set the Target from  on the NextTargets Stored on the Waypoints or Zones</summary>
        public virtual void SetNextTarget()
        {
            if (NextTarget == null)
            {
                Debuging("There's no Next Target");
                Stop();
                return;
            }

            if (IsWayPoint != null)
            {
                if (I_WaitToNextTarget != null) StopCoroutine(I_WaitToNextTarget); //if there's a coroutine active then stop it

                I_WaitToNextTarget = C_WaitToNextTarget(IsWayPoint.WaitTime, NextTarget); //IMPORTANT YOU NEED TO WAIT 1 FRAME ALWAYS TO GO TO THE NEXT WAYPOINT
                StartCoroutine(I_WaitToNextTarget);
            }
        }

        /// <summary> Check if the Next Target is a Air Target</summary>
        private void CheckAirTarget()
        {
            if (IsWayPoint != null && IsWayPoint.TargetType == WayPointType.Air)    //If the animal can fly, there's a new wayPoint & is on the Air
            {
                Debuging(name + ": NextTarget is AIR", NextTarget.gameObject);
                animal.State_Activate(StateEnum.Fly);
                FreeMove = true;
            }
        }
        #endregion

        public virtual void SetDefaultGroundIndex(int val)
        { defaultGroundIndex = val; }

        /// <summary>Set the next Destination Position without having a target</summary>   
        public virtual void SetDestination(Vector3 newDestination, bool Move)
        {
            if (newDestination == DestinationPosition) return; //Means that you're already going to the same point


            IsWaiting = false;
            animal.Mode_Interrupt();             //In Case it was making any Mode;

            if (!target) ResetStoppingDistance(); //If there's no target reset the Stopping Distance


            Debuging("is travelling to : " + newDestination);

            IsWayPoint = null;

            if (I_WaitToNextTarget != null)
                StopCoroutine(I_WaitToNextTarget);                          //if there's a coroutine active then stop it

            MoveAgent = Move;

            DestinationPosition = newDestination;                           //Update the Target Position 
            ResumeAgent();
        }


        public void SetDestination(Vector3 PositionTarget) { SetDestination(PositionTarget, true); }

        /// <summary> Stop the Agent and the Animal</summary>
        public virtual void Stop()
        {
           //Debug.Log("Stop");

            if (Agent.isOnNavMesh)
                Agent.isStopped = true;

            if (IsOnMode) animal.Mode_Interrupt(); //Only Stop if the Animal was on a mode

            animal.StopMoving();

           // IsWaiting = false;
            MoveAgent = false;
        }


        /// <summary>Check the Status of the Next Target</summary>
        private void Arrive_Destination()
        {
            HasArrived = true;

            RemainingDistance = 0;

           if (target)  Debuging("has arrived to Destination: "+ target.name);

            if (IsWayPoint != null)     //If we have arrived to a WayPoint
            {
                if (IsWayPoint.TargetType == WayPointType.Ground) FreeMove = false;         //if the next waypoing is on the Ground then set the free Movement to false
                if (AutoInteract) IsWayPoint.TargetArrived(gameObject);                     //Call the method that the Target has arrived to the destination
                if (AutoNextTarget) SetNextTarget();                                        //Set Next Target
               
            }
            else
            {
                Stop();
            }

            OnTargetArrived.Invoke(target);                                 //Invoke the Event On Target Arrived
            IsWayPoint?.TargetArrived(gameObject);                          //Send to the Waypoint that the we have Arrived
            OnTargetPositionArrived.Invoke(DestinationPosition);            //Invoke the Event On Target Position Arrived
        }

        /// <summary>Resume the Agent component</summary>
        public void ResumeAgent()
        {
           if (!FreeMove)
                Agent.enabled = true;

            if (!Agent.isOnNavMesh) return;                             //No nothing if we are not on a Nav mesh or the Agent is disabled

           // if (!NullDestination)
            {
                Agent.SetDestination(DestinationPosition);                       //If there's a position to go to set it as destination
                Agent.isStopped = !MoveAgent;                                    //Start the Agent again
            } 

          //  Debug.LogWarning("ResumeAgent:"+ DestinationPosition); 
        }

        /// <summary>Update The Target Position </summary>
        protected virtual void Update_TargetPos()
        {
            if (UpdateTargetPosition)
            {
               // StoppingDistance = GetTargetStoppingDistance();         //Update also the Stopping Distance
                DestinationPosition = GetTargetPosition();              //Update the Target Position 
                MoveAgent = MoveAgentOnMovingTarget;

                ResumeAgent();
            }
        }


        protected virtual void FreeMovement()
        {
            if (IsWaiting) return;
            if (!target /*|| NullDestination*/) return;      //If we have no were to go then Skip the code

            RemainingDistance = target ? Vector3.Distance(animal.transform.position, target.position) : 0;

            var Direction = (target.position - animal.transform.position).normalized;

            animal.Move(Direction);

            if (RemainingDistance < StoppingDistance)   //We arrived to our destination
                Arrive_Destination();
        }

        protected virtual void CheckOffMeshLinks()
        {
            if (Agent.isOnOffMeshLink /*&& !EnterOFFMESH*/)                         //Check if the Agent is on a OFF MESH LINK
            {
               // EnterOFFMESH = true;                                            //Just to avoid entering here again while we are on a OFF MESH LINK
                OffMeshLinkData OMLData = Agent.currentOffMeshLinkData;

                if (OMLData.linkType == OffMeshLinkType.LinkTypeManual)              //Means that it has a OffMesh Link component
                {
                    OffMeshLink CurrentOML = OMLData.offMeshLink;                    //Check if the OffMeshLink is a Manually placed  Link

                    Zone IsOffMeshZone =
                        CurrentOML.GetComponentInParent<Zone>();                     //Search if the OFFMESH IS An ACTION ZONE (EXAMPLE CRAWL)

                    if (IsOffMeshZone)                                               //if the OffmeshLink is a zone and is not making an action
                    {
                        IsOffMeshZone.CurrentAnimal = animal;
                        IsOffMeshZone.ActivateZone(true);                            //Activate the Zone
                        return;
                    }


                    var DistanceEnd = (transform.position - CurrentOML.endTransform.position).sqrMagnitude;
                    var DistanceStart = (transform.position - CurrentOML.startTransform.position).sqrMagnitude;


                    //Debug.Log("OMMESH FLY");

                    if (CurrentOML.CompareTag("Fly"))
                    {
                        var FarTransform = DistanceEnd > DistanceStart ? CurrentOML.endTransform : CurrentOML.startTransform;
                        //Debug.Log("OMMESH FLY");

                        FlyOffMesh(FarTransform);
                    }
                    else if (CurrentOML.CompareTag("Climb"))
                    {
                        StartCoroutine(MalbersTools.AlignTransform_Rotation(transform, CurrentOML.transform.rotation, 0.15f));         //Aling the Animal to the Link Position
                        ClimbOffMesh();
                    }
                    else if (CurrentOML.area == 2)  //2 is Off mesh Jump
                    {
                        var NearTransform = DistanceEnd < DistanceStart ? CurrentOML.endTransform : CurrentOML.startTransform;
                        StartCoroutine(MalbersTools.AlignTransform_Rotation(transform, NearTransform.rotation, 0.15f));         //Aling the Animal to the Link Position
                        animal.State_Activate(StateEnum.Jump); //2 is Jump State                                                              //if the OffMesh Link is a Jump type
                    }
                }
                else if (OMLData.linkType == OffMeshLinkType.LinkTypeJumpAcross)             //Means that it has a OffMesh Link component
                {
                    animal.State_Activate(StateEnum.Jump); //2 is Jump State
                }
            }
        }

        /// <summary>Called when the Animal Enter an Action, Attack, Damage or something similar</summary>
        private void OnModeStart(int ModeID)
        {
            IsOnMode = true;
            animal.StopMoving();     //Means the Animal is making a Mode
            Debuging("has Started a Mode: <B>" + animal.ActiveMode.ID.name + "</B>. Ability: <B>" + animal.ActiveMode.ActiveAbility.Name + "</B>");
            Agent.enabled = false;
        }

        private void OnModeEnd(int ModeID)
        { 
            IsOnMode = false;

            if (Agent.isOnOffMeshLink) 
                Agent.CompleteOffMeshLink();


            ResumeAgent();
        }

        private void OnState(int stateID)
        {
            if (animal.ActiveStateID == StateEnum.Swim) 
                OnGrounded(true); //Force Grounded to true when is swimming the animal
        }

        /// <summary>Check when the Animal changes the Grounded State</summary>
        protected virtual void OnGrounded(bool grounded)
        {
            IsGrounded = grounded;

            //Checking if we are swimming

            if (animal.ActiveStateID == StateEnum.Swim) IsGrounded = true; //Force Grounded to true when is swimming the animal

            if (IsGrounded)
            {
                CheckAirTarget();        //Check again the Air Target in case it was a miss Grounded.
                Agent.enabled = true;

                if (!Agent.isOnNavMesh) 
                    return;

                ResetFlyingOffMesh();

                // EnterOFFMESH = false;

                if (Agent.isOnOffMeshLink)
                    Agent.CompleteOffMeshLink();

                FreeMove = false;

                ResumeAgent();
            }
            else
            {
                if (Agent.isOnNavMesh)                  //Needs to pause the AGENT since the animal is no longer on the ground and NavMesh
                    Agent.isStopped = true;

                Agent.enabled = false;
                animal.DeltaAngle = 0;
            }
        }
        protected void FlyOffMesh(Transform target)
        {
            ResetFlyingOffMesh();

            IFlyOffMesh = C_FlyOffMesh(target);
            StartCoroutine(IFlyOffMesh);
        }

        protected void ClimbOffMesh()
        {
            if (IClimbOffMesh != null) StopCoroutine(IClimbOffMesh);
            IClimbOffMesh = C_Climb_OffMesh();
            StartCoroutine(IClimbOffMesh);
        }

        public float StopDistance()
        { return DefaultStopDistance; }

        private void ResetFlyingOffMesh()
        {
            if (IFlyOffMesh != null)
            {
                IsMovingOffMesh = false;
                StopCoroutine(IFlyOffMesh);
                IFlyOffMesh = null;
            }
        }

        /// <summary>Change to walking when the Animal is near the Target Radius (To Avoid going forward(by intertia) while stoping </summary>
        private void CheckWalkDistance()
        {
            if (IsGrounded && walkDistance > 0)
            {
                if (walkDistance > RemainingDistance)
                {
                    animal.CurrentSpeedIndex = 1;
                }
                else
                {
                    if (animal.CurrentSpeedIndex != defaultGroundIndex)
                        animal.CurrentSpeedIndex = defaultGroundIndex;
                }
            }
        }

        protected IEnumerator C_WaitToNextTarget(float time, Transform NextTarget)
        {
            IsWaiting = true;
            Debuging("is waiting " + time.ToString("F2") + " seconds");
            Stop();

            yield return null; //SUUUUUUUUUPER  IMPORTANT!!!!!!!!!

            if (time > 0)
                yield return new WaitForSeconds(time);

            SetTarget(NextTarget);
        }

        internal IEnumerator C_FlyOffMesh(Transform target)
        {
            animal.State_Activate(StateEnum.Fly); //Set the State to Fly
            IsMovingOffMesh = true;
            float distance = float.MaxValue;

            while (distance > StoppingDistance)
            {
                animal.Move((target.position - animal.transform.position).normalized);
                distance = Vector3.Distance(animal.transform.position, target.position);
                yield return null;
            }
            animal.ActiveState.AllowExit();

            Debuging("Exit Fly State Off Mesh");

            
            
            
            IsMovingOffMesh = false;
        }

        internal IEnumerator C_Climb_OffMesh()
        {
            animal.State_Activate(StateEnum.Climb); //Set the State to Climb
            IsMovingOffMesh = true;
            yield return null;
            Agent.enabled = false;


            while (animal.ActiveState.ID == StateEnum.Climb)
            {
                animal.MoveWorld(Vector3.forward); //Move Upwards on the Climb
               // distance = Vector3.Distance(animal.transform.position, target.position);
                yield return null;
            }

            Debuging("Exit Climb State Off Mesh");

            IsMovingOffMesh = false;
        }


        protected void Debuging(string Log) { if (debug) Debug.Log($"<B>{name}:</B> " + Log); }

        protected void Debuging(string Log, GameObject obj) { if (debug) Debug.Log(Log, obj); }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!debugGizmos) return;
            if (Agent == null) { return; }
            if (Agent.path == null) { return; }

            Gizmos.color = Color.yellow;

            Vector3 pos = Agent ? Agent.transform.position : transform.position;

            for (int i = 1; i < Agent.path.corners.Length; i++)
            {
                Gizmos.DrawLine(Agent.path.corners[i - 1], Agent.path.corners[i]);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Agent.transform.position, 0.01f);

            var Pos = (Application.isPlaying && target) ? target.position : Agent.transform.position;

            if (Application.isPlaying && IsAITarget != null)
            {
                Pos = IsAITarget.GetPosition();
            }

            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawWireDisc(Pos, Vector3.up, walkDistance);

            UnityEditor.Handles.color = HasArrived ? Color.green : Color.red;
            UnityEditor.Handles.DrawWireDisc(Pos, Vector3.up, StoppingDistance);

            if (Application.isPlaying/* && !NullDestination*/)
            {
                if (IsWayPoint != null && IsWayPoint.TargetType == WayPointType.Air)
                    Gizmos.DrawWireSphere(DestinationPosition, StoppingDistance);
                else
                    UnityEditor.Handles.DrawWireDisc(DestinationPosition, Vector3.up, StoppingDistance);
            }
        }
      
#endif
    }
}