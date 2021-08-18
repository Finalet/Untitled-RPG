using UnityEngine;
using System.Collections;
using MalbersAnimations.Events;
 
using UnityEngine.AI;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Animal Controller/AI/AI Control")]
    public class MAnimalAIControl : MonoBehaviour, IAIControl, IAITarget,  IAnimatorListener
    {
        #region Components References
        /// <summary> Reference for the Agent</summary>
        [SerializeField, RequiredField] private NavMeshAgent agent;     

        /// <summary> Reference for the Animal</summary>
        [RequiredField] public MAnimal animal;


        /// <summary>Check if this Animal has an Interactor Component</summary>
        public IInteractor Interactor { get; internal set; }
        #endregion

        #region Internal Variables
        /// <summary>Target Last Position (Useful to know if the Target is moving)</summary>
        protected Vector3 TargetLastPosition;

        /// <summary>Stores the Remainin distance to the Target's Position</summary>
        public virtual float RemainingDistance  { get; internal set; }
        //{
        //    get => m_RemainingDistance;
        //    set
        //    {
        //        m_RemainingDistance = value;
        //        Debug.Log($"m_RemainingDistance: {m_RemainingDistance} ");
        //    }
        //}
        //private float m_RemainingDistance;

        /// <summary>Stores the Remainin distance to the Target's Position</summary>
        protected float DefaultStopDistance;
        public bool InOffMeshLink { get; protected set; }

        /// <summary>Changes the value of the Agent.isStopped</summary>       
        public virtual bool MoveAgent 
        {
            get => moveAgent;
            set
            {
                moveAgent = value; 
                if (Agent.isOnNavMesh)  Agent.isStopped = !value;
               // Debug.Log($"MoveAgent: {MoveAgent} ");
            }
        }
        private bool moveAgent;



        /// <summary>Has the animal Arrived to their current destination</summary>
        public bool HasArrived  { get; internal set; }
        //{
        //    get => hasArriv;
        //    internal set
        //    {
        //        hasArriv = value;
        //        Debug.Log("HasArrived: " + hasArriv);
        //    }
        //}
        //private bool hasArriv;


        private IEnumerator I_WaitToNextTarget;
        private IEnumerator IFreeMoveOffMesh;
        private IEnumerator IClimbOffMesh;
        #endregion

        #region Public Variables
        [SerializeField] protected float stoppingDistance = 0.6f;
        [SerializeField] protected float PointStoppingDistance = 0.6f;

        /// <summary>The animal will change automatically to Walk if the distance to the target is this value</summary>
        [SerializeField] protected float walkDistance = 1f;

        internal void ResetStoppingDistance() { StoppingDistance = DefaultStopDistance; }

        [SerializeField] private Transform target;
        [SerializeField] private Transform nextTarget;

        [Tooltip("States that will reset the Offmesh link, in case the animal was in one.")]
        [SerializeField] private List<StateID> ResetOffMesh = new List<StateID>();

        /// <summary>Means the Animal will go to a next Target when it reaches the current target automatically</summary>
        public bool AutoNextTarget = true;

        ///// <summary>Means the Animal will interact to any Waypoint automatically when he arrived to it</summary>
        //public bool AutoInteract = true;

        /// <summary>If the Target Moves the Agent will move too ... to the given destination</summary>
        public bool MoveAgentOnMovingTarget = true;

        public void SetMoveAgentOnMovingTarget(bool value) => MoveAgentOnMovingTarget = value;

        /// <summary>The Animal will Rotate/Look at the Target when he arrives to it</summary>
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
        public  bool FreeMove  { get; private set; }
        //{
        //    get => freeMove;
        //    internal set
        //    {
        //        freeMove = value;
        //        Debug.Log(freeMove);
        //    }
        //}
        //private bool freeMove;


        /// <summary>Current Stopping Distance for the Next Waypoint</summary>
        public virtual float StoppingDistance
        {
            get => stoppingDistance;
            set
            {
                Agent.stoppingDistance = stoppingDistance = value;
                //Debug.Log("StoppingDistance: "+ value);
            }
        }



        /// <summary>Is the Animal Playing a mode</summary>
        public bool IsOnMode { get; private set; }

     


        public virtual bool IsGrounded { get; protected set; }

        public virtual bool IsMovingOffMesh { get; protected set; }

        /// <summary>Is the Target a WayPoint?</summary>
        public IWayPoint IsWayPoint { get; protected set; }
      
        /// <summary>Is the Target an AITarget</summary>
        public IAITarget IsAITarget { get; protected set; }

        /// <summary>AITarget Position</summary>
        public Vector3 AITargetPos { get; protected set; }

        /// <summary>Is the Target an AITarget</summary>
        public IInteractable IsTargetInteractable { get; protected set; }
        #endregion 

        #region Events
        [Space]
        public Vector3Event OnTargetPositionArrived = new Vector3Event();
        public TransformEvent OnTargetArrived = new TransformEvent();
        public TransformEvent OnTargetSet = new TransformEvent();
        #endregion

        #region Properties
     

        /// <summary>Reference of the Nav Mesh Agent</summary>
        public virtual NavMeshAgent Agent => agent; 

        /// <summary>Self AI Target Position</summary>
        public virtual Vector3 GetPosition() => animal.Center;

        /// <summary> Self Target Type </summary>
        public virtual WayPointType TargetType => animal.FreeMovement ? WayPointType.Air : WayPointType.Ground;


        /// <summary>is the Target transform moving??</summary>
        public virtual bool TargetIsMoving { get; internal set; }


        /// <summary> Is the Animal waiting x time to go to the Next waypoint</summary>
        public virtual bool IsWaiting { get; internal set; }

        /// <summary>Destination Position to use on Agent.SetDestination()</summary>
        public virtual Vector3 DestinationPosition { get; set; }
        //{
        //    get => m_DestinationPosition;
        //    set
        //    {
        //        m_DestinationPosition = value;
        //        if (debug)  Debug.Log($"DP ={m_DestinationPosition} IsAI <{IsAITarget != null}>  Targ<{Target}>");
        //    }
        //}
        //Vector3 m_DestinationPosition;

        public virtual Transform NextTarget { get => nextTarget; set => nextTarget = value; }
        public virtual Transform Target => target;

        /// Height Diference from the Target and the Agent
        public virtual float TargetHeight => Mathf.Abs(DestinationPosition.y - Agent.transform.position.y);

        /// <summary>Update The Target Position from the Target. This should be false if the Position you want to go is different than the Target's position</summary>
        public virtual bool UpdateTargetPosition { get => updateTargetPosition; set => updateTargetPosition = value; }

        #endregion 
        public virtual void SetActive(bool value) => enabled = value;


        #region Unity Functions 
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) =>  this.InvokeWithParams(message, value);


        private void Awake()
        {
            if (animal == null) animal = this.FindComponent<MAnimal>();

            Interactor = animal.FindInterface<IInteractor>();

            DefaultStopDistance = StoppingDistance;                             //Store the Started Stopping Distance
            AgentPosition = Agent.transform.localPosition;
        }

        protected Vector3 AgentPosition;

        protected virtual void OnEnable()
        { 
            animal.OnStateChange.AddListener(OnState);
            animal.OnModeStart.AddListener(OnModeStart);
            animal.OnModeEnd.AddListener(OnModeEnd);
            animal.OnGrounded.AddListener(OnGrounded);     

            Invoke(nameof(StartAgent),0.1f);
        }

        protected virtual void OnDisable()
        {
            animal.OnStateChange.RemoveListener(OnState);           //Listen when the Animations changes..
            animal.OnModeStart.RemoveListener(OnModeStart);           //Listen when the Animations changes..
            animal.OnModeEnd.RemoveListener(OnModeEnd);           //Listen when the Animations changes..
            animal.OnGrounded.RemoveListener(OnGrounded);           //Listen when the Animations changes..

            Stop();
            StopAllCoroutines();
        }

        protected virtual void Update() { Updating(); }
        #endregion

        public virtual void StartAgent()
        {
            FreeMove = (animal.ActiveState.General.FreeMovement);
            if (FreeMove) Agent.enabled = false;
            IsWaiting = true;

            if (agent == null) agent.FindComponent<NavMeshAgent>();

            Agent.updateRotation = false;                                       //The Animal will control the rotation . NOT THE AGENT
            Agent.updatePosition = false;                                       //The Animal will control the  postion . NOT THE AGENT
         
            Agent.stoppingDistance = StoppingDistance;

          
            OnGrounded(animal.Grounded);

            HasArrived = false;
            TargetIsMoving = false;
            var targ = target;
            target = null;
            SetTarget(targ);                                                  //Set the first Target (IMPORTANT)  it also set the next future targets

            InvokeRepeating(nameof(CheckMovingTarget), 0f, MovingTargetInterval);
        }

        public virtual void Updating()
        {
            Agent.nextPosition = agent.transform.position;                  //Update the Agent Position to the Transform position   IMPORTANT!!!!
            agent.transform.localPosition = AgentPosition;                       //Important! Reset the Agent Position to the default Position
          
            if (IsMovingOffMesh) return;                                    //Do nothing while is moving ofmesh (THE Coroutine is in charge of the movement)
            if (IsWaiting) return;                                          //Do nothing while is waiting

            if (MovingTargetInterval <= 0) CheckMovingTarget();


            if (FreeMove)
            {
                //Debug.Log("FreeMovement");
                FreeMovement();
            }
            else if (IsGrounded)                                               //if we are on a NAV MESH onGround
            {
                if (IsOnMode)
                {
                    return;    //If the Animal is Waiting do nothing . .... he is doing something else... wait until he's finish
                }
                else
                {
                  

                   

                    UpdateAgent();
                }
            }
        }

        /// <summary> Check if the Target is moving </summary>
        public virtual void CheckMovingTarget()
        {
            if (target)
            {
               // TargetIsMoving = (target.position -  TargetLastPosition).sqrMagnitude > 0.005f;
                TargetIsMoving = (target.position != TargetLastPosition);
                TargetLastPosition = target.position;
                if (TargetIsMoving)    Update_TargetPos();
            }
        }


        public virtual void Move()
        {
            if (!MoveAgent)
                ResumeAgent();                              //Only Resume the Agent in case the Animal is set to move on moving Target.
            else
             if (Agent.isOnNavMesh) Agent.SetDestination(DestinationPosition);  //Go to the Current Destination;
        }

        /// <summary>Update The Target Position </summary>
        protected virtual void Update_TargetPos()
        {
            if (UpdateTargetPosition)
            {
               if (IsAITarget != null) AITargetPos = IsAITarget.GetPosition(); //Update the AI Target Pos if the Target moved

                DestinationPosition = GetTargetPosition();              //Update the Target Position 

                var DistanceOnMovingTarget = Vector3.Distance(DestinationPosition, Agent.transform.position); //Double check if the Animal is far from the target

                if (DistanceOnMovingTarget >= StoppingDistance)
                {
                    HasArrived = false; //Check if the animal hasn't arrived to a moving target
                }

                if (MoveAgentOnMovingTarget)  Move();
            }
        }



        /// <summary> Updates the Agents using he animation root motion </summary>
        public virtual void UpdateAgent()
        {
            if (Agent.pathPending || !Agent.isOnNavMesh) return;    //Means is still calculating the path to go

            if (HasArrived || !MoveAgent)
            {
                if (LookAtTargetOnArrival)
                {
                    var LookAtDir = (target != null ? target.position : DestinationPosition) - transform.position;
                    animal.RotateAtDirection(LookAtDir);
                }
                return;
            }

         //   Debug.Log("UpdateAgent");

            RemainingDistance = Agent.remainingDistance;                //Store the remaining distance -- but if navMeshAgent is still looking for a path Keep Moving

            if (!HasArrived && RemainingDistance <= StoppingDistance)                   //if We Arrive to the Destination
            {
                Arrive_Destination();
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
        public virtual void SetTarget(GameObject target) => SetTarget(target, true);
        public virtual void SetTarget(GameObject target, bool move) => SetTarget(target != null ? target.transform : null, move); //Null reference added

        public virtual void SetTarget(Transform target) => SetTarget(target, true);

        /// <summary>Remove the current Target and stop the Agent </summary>
        public virtual void ClearTarget() => SetTarget((Transform)null, false);

        /// <summary>Remove the current Target </summary>
        public virtual void NullTarget() => target = null;

        /// <summary>Assign a new Target but it does not move it to it</summary>
        public virtual void SetTargetOnly(Transform target) => SetTarget(target, false);

        /// <summary>Set the next Target</summary>   
        public virtual void SetTarget(Transform target, bool Move)
        {
            if (target == Target  && !HasArrived) return;           //Don't assign the same target if we are travelling to that target

            this.target = target;
         
            IsWaiting = false;
            RemainingDistance = float.MaxValue; //Set the Remaining Distance as the Max Float Value
            HasArrived = false;

            OnTargetSet.Invoke(target);         //Invoked that the Target has changed.

            if (this.target != null)
            {
                TargetLastPosition = target.position;                   //Since is a new Target "Reset the Target last position"
                DestinationPosition = target.position;                  //Update the Target Position 

                if (animal.IsPlayingMode) animal.Mode_Interrupt();      //In Case it was making any Mode Interrupt it because there's a new target to go to.


                IsAITarget = target.gameObject.FindInterface<IAITarget>();
                if (IsAITarget != null) AITargetPos = IsAITarget.GetPosition();

                IsTargetInteractable = target.FindInterface<IInteractable>();
                IsWayPoint = target.FindInterface<IWayPoint>(); 

                StoppingDistance = GetTargetStoppingDistance();
                DestinationPosition = GetTargetPosition();

                NextTarget = null;
                if (IsWayPoint != null) NextTarget = IsWayPoint.NextTarget(); //Find the Next Target on the Waypoint

                StopWaitCoroutine(); //If the Animal was waiting Reset the waiting IMPORTANT!!

                CheckAirTarget();

                //Resume the Agent is MoveAgent is true
                if (Move) ResumeAgent(); //THe Agent will move if the move parameter is true... if not it will only calculate all the logic without moving

                if (MoveAgent) Debuging($"<color=yellow>is travelling to [NEW TARGET]: <B>{target.name}</B>  - {DestinationPosition} </color>");
            }
            else
            {

                IsAITarget = null;                  //Reset the AI Target
                IsTargetInteractable = null;        //Reset the AI Target Interactable
                IsWayPoint = null;                  //Reset the Waypoint

                Stop(); //Means the Target is null so Stop the Animal
            }
        }

        public virtual Vector3 GetTargetPosition()
        {
            var TargetPos = (IsAITarget != null) ? AITargetPos : target.position;
            if (TargetPos == Vector3.zero) TargetPos = target.position; //HACK FOR WHEN THE TARGET REMOVED THEIR AI TARGET COMPONENT
            return TargetPos;
        }


        public void TargetArrived(GameObject target) { }

        public virtual float GetTargetStoppingDistance() => IsAITarget != null ? IsAITarget.StopDistance() : DefaultStopDistance;

        public virtual void SetNextTarget(GameObject next) => NextTarget = next.transform;


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
                StopWaitCoroutine();

                I_WaitToNextTarget = C_WaitToNextTarget(IsWayPoint.WaitTime, NextTarget);   //IMPORTANT YOU NEED TO WAIT 1 FRAME ALWAYS TO GO TO THE NEXT WAYPOINT
                StartCoroutine(I_WaitToNextTarget); 
            }
            else
            {
                Debuging("SetTarget(NextTarget);");
                SetTarget(NextTarget);
            }
        }

        internal void StopWaitCoroutine()
        {
            if (I_WaitToNextTarget != null) StopCoroutine(I_WaitToNextTarget);          //if there's a coroutine active then stop it
        }

        /// <summary> Check if the Next Target is a Air Target, if true then go to it</summary>
        internal virtual bool CheckAirTarget()
        {
            if (IsAirDestination && !FreeMove)    //If the animal can fly, there's a new wayPoint & is on the Air
            {
               if (NextTarget) Debuging(": Next Waypoint is AIR",  NextTarget.gameObject);
                animal.State_Activate(StateEnum.Fly);
                FreeMove = true;
                MoveAgent = false; //Stop the Agent
            }

            return IsAirDestination;
        }

        internal bool IsAirDestination  => IsAITarget != null && IsAITarget.TargetType == WayPointType.Air;
        internal bool IsGroundDestination  => IsAITarget != null && IsAITarget.TargetType == WayPointType.Ground;
        #endregion


        /// <summary>Set the next Destination Position without having a target</summary>   
        public virtual void SetDestination(Vector3 newDestination, bool Move)
        {
            if (newDestination == DestinationPosition) return; //Means that you're already going to the same point

            StoppingDistance = PointStoppingDistance; //Reset the stopping distance when Set Destination is used.
            //target = null;
            HasArrived = false;
            IsWaiting = false;
            animal.Mode_Interrupt();             //In Case it was making any Mode;


            if (MoveAgent) Debuging($"<color=yellow>is travelling to: {DestinationPosition} </color>");
             
            IsWayPoint = null;

            if (I_WaitToNextTarget != null)
                StopCoroutine(I_WaitToNextTarget);                          //if there's a coroutine active then stop it
            
            DestinationPosition = newDestination;                           //Update the Target Position 
           
            if (Move) ResumeAgent();
        }

        /// <summary>Set the next Destination Position without having a target</summary>   
        public virtual void SetDestination(Vector3Var newDestination) => SetDestination(newDestination.Value);
        public virtual void SetDestination(Vector3 PositionTarget) => SetDestination(PositionTarget, true);

        public virtual void SetDestinationClearTarget(Vector3 PositionTarget)
        {
            target = null;
            SetDestination(PositionTarget, true);
        }

        /// <summary> Stop the Agent and the Animal</summary>
        public virtual void Stop()
        {
            MoveAgent = false; //Stop the Agent from Moving

            if (Agent.isOnNavMesh) Agent.ResetPath();

            animal.StopMoving();
        }


        /// <summary>Check the Status of the Next Target</summary>
        protected virtual void Arrive_Destination()
        {
            if (!FreeMove && Agent.pathStatus != NavMeshPathStatus.PathComplete) //Check when the Agent is trapped on an NavMesh that cannot exit
            {
                Debuging($"[{Agent.pathStatus}] - Destination <Actions> Ignored. Force Stop");
                Stop();
                return;
            }

            HasArrived = true;
            RemainingDistance = 0;

            OnTargetArrived.Invoke(target);                                 //Invoke the Event On Target Arrived
            OnTargetPositionArrived.Invoke(DestinationPosition);            //Invoke the Event On Target Position Arrived

            if (target)
            {
                Debuging($"<color=green>has arrived to: <B>{target.name}</B> - {DestinationPosition} </color>");

                if (IsTargetInteractable != null && IsTargetInteractable.Auto) //If the interactable is set to Auto!!!!!!!
                {
                    if (Interactor != null)
                    {
                        Interactor.Interact(IsTargetInteractable);
                    }
                    else IsTargetInteractable.Interact(0, animal.gameObject); //Do an Empty Interaction

                    Debuging($"Interact with : <b><{IsTargetInteractable.Owner.name}></b>");
                }
            }
            else
            {
                Debuging($"<color=green>has arrived to: <B>{DestinationPosition}</B></color>");

                Stop(); //The target was removed
                return;
            }

            IsAITarget?.TargetArrived(animal.gameObject);              //Call the method that the Target has arrived to the destination

            if (IsWayPoint != null)   //If we have arrived to a WayPoint
            {
                if (IsAITarget.TargetType == WayPointType.Ground) FreeMove = false;         //if the next waypoing is on the Ground then set the free Movement to false
                if (AutoNextTarget) SetNextTarget();                                        //Set Next Target

            }
            else
            {
                Stop();
            }
        }


        /// <summary>  Resets the global parameters for a Target </summary>
        public virtual void DefaultMoveToTarget()
        {
            UpdateTargetPosition = true;           // Make Sure to update the Target position on the Animal
            MoveAgentOnMovingTarget = true;        // Check if the Target moves
            LookAtTargetOnArrival = true;
            animal.LockMovement = false;
        }


        /// <summary>Resume the Agent component</summary>
        public virtual void ResumeAgent()
        {
            if (!FreeMove)
            {
                Agent.enabled = true;                               //Enable the Agent first before checking if its on a NavMesh
                if (!Agent.isOnNavMesh) return;                     //No nothing if we are not on a Nav mesh or the Agent is disabled

                Agent.SetDestination(DestinationPosition);          //If there's a position to go to set it as destination
                MoveAgent = true;                                   //Start the Agent again
                CompleteOffMeshLink();
            }
        }




        protected virtual void FreeMovement()
        {
            RemainingDistance = Vector3.Distance(animal.transform.position, DestinationPosition);

            animal.Move((DestinationPosition - animal.transform.position).normalized); //Important to be normalized!!

            if (RemainingDistance < StoppingDistance)   //We arrived to our destination
            {
                if (IsGroundDestination && !IsGrounded)
                {
                    //Keep Moving
                }
                else
                {
                    Arrive_Destination();
                }
            }
        }

        protected virtual void CheckOffMeshLinks()
        {
            if (Agent.isOnOffMeshLink && !InOffMeshLink)                         //Check if the Agent is on a OFF MESH LINK
            {
                 InOffMeshLink = true;                                            //Just to avoid entering here again while we are on a OFF MESH LINK
                OffMeshLinkData OMLData = Agent.currentOffMeshLinkData;
              
                if (OMLData.linkType == OffMeshLinkType.LinkTypeManual)              //Means that it has a OffMesh Link component
                {
                    OffMeshLink CurrentOML = OMLData.offMeshLink;                    //Check if the OffMeshLink is a Manually placed  Link

                    if (CurrentOML)
                    {
                        Zone IsOffMeshZone =
                        CurrentOML.GetComponentInParent<Zone>();                     //Search if the OFFMESH IS An ACTION ZONE (EXAMPLE CRAWL)

                        if (IsOffMeshZone)                                           //if the OffmeshLink is a zone and is not making an action
                        {
                            if (debug) Debuging($"<color=white>is On a <b>[OffmeshLink]</b> -> [{IsOffMeshZone.name}]</color>");

                            IsOffMeshZone.ActivateZone(animal);                      //Activate the Zone
                            return;
                        }


                        var DistanceEnd = (transform.position - CurrentOML.endTransform.position).sqrMagnitude;
                        var DistanceStart = (transform.position - CurrentOML.startTransform.position).sqrMagnitude;


                        //Debug.Log("OMMESH FLY");

                        if (CurrentOML.CompareTag("Fly"))
                        {
                            var FarTransform = DistanceEnd > DistanceStart ? CurrentOML.endTransform : CurrentOML.startTransform;
                            //Debug.Log("OMMESH FLY");
                            Debuging($"<color=white>is On a <b>[OffmeshLink]</b> -> [Fly]</color>");
                            FlyOffMesh(FarTransform);
                        }
                        else if (CurrentOML.CompareTag("Climb"))
                        {
                            Debuging($"<color=white>is On a <b>[OffmeshLink]</b> -> [Climb] -> { CurrentOML.transform.name}</color>");
                            //StartCoroutine(MTools.AlignTransform_Rotation(transform, CurrentOML.transform.rotation, 0.15f));         //Aling the Animal to the Link Position
                            StartCoroutine(MTools.AlignTransform(transform, CurrentOML.transform, 0.15f));         //Aling the Animal to the Link Position

                            Debug.DrawRay(CurrentOML.transform.position, CurrentOML.transform.forward, Color.white, 3);

                            ClimbOffMesh();
                        }
                        else if (CurrentOML.area == 2)  //2 is Off mesh Jump
                        {
                            var NearTransform = DistanceEnd < DistanceStart ? CurrentOML.endTransform : CurrentOML.startTransform;
                            StartCoroutine(MTools.AlignTransform_Rotation(transform, NearTransform.rotation, 0.15f));         //Aling the Animal to the Link Position
                            animal.State_Activate(StateEnum.Jump);       //if the OffMesh Link is a Jump type activate the jump
                            Debuging($"<color=white>is On a <b>[OffmeshLink]</b> -> [Jump]</color>");
                        }
                    }
                }
                else if (OMLData.linkType == OffMeshLinkType.LinkTypeJumpAcross)             //Means that it has a OffMesh Link component
                {
                    animal.State_Activate(StateEnum.Jump); //2 is Jump State
                }
            }
        }

        /// <summary>Called when the Animal Enter an Action, Attack, Damage or something similar</summary>
        public virtual void OnModeStart(int ModeID, int ability)
        {
            Debuging("has Started a Mode: <B>" + animal.ActiveMode.ID.name + "</B>. Ability: <B>" + animal.ActiveMode.ActiveAbility.Name + "</B>");
            if (animal.ActiveMode.AllowMovement) return; //Don't stop the Animal Movevemt if the Mode can make movements
            IsOnMode = true;
            
            if (MoveAgent) Stop(); //If the Agent was moving Stop it

            Agent.enabled = false; //Disable it when doing a Mode that does not move!
        }

       //private bool wasMovingBeforeMode;
        /// <summary>  Listen if the Animal Has finished a mode  </summary>
        public virtual void OnModeEnd(int ModeID, int ability)
        {
            IsOnMode = false;
            //MoveAgent = wasMovingBeforeMode;
            ResumeAgent();

            CompleteOffMeshLink();
        }

        /// <summary> Completes the OffmeshLink in case the animal was in one  </summary>
        private void CompleteOffMeshLink()
        {
            if (Agent.isOnOffMeshLink)
            {
                Agent.CompleteOffMeshLink();
            }
            InOffMeshLink = false;
        }

        public virtual void OnState(int stateID)
        {
            if (animal.ActiveStateID == StateEnum.Swim)  OnGrounded(true); //Force Grounded to true when is swimming the animal *HACK*

            if (ResetOffMesh != null && ResetOffMesh.Exists(x=> x.ID == stateID))
            {
                CompleteOffMeshLink();
            }

        }

        /// <summary>Check when the Animal changes the Grounded State</summary>
        protected virtual void OnGrounded(bool grounded)
        {
            if (IsGrounded != grounded)
            {
                IsGrounded = grounded;


                if (animal.ActiveStateID == StateEnum.Swim) IsGrounded = true; //Force Grounded to true when is swimming the animal

                if (IsGrounded)
                {
                    FreeMove = false; //Check if the Current target is an Air Target (Bug)

                    if (!Agent.enabled && !IsOnMode)//If it just landed or it was flying
                    {
                        Agent.enabled = true;
                        ResetFreeMoveOffMesh();

                        if (!Agent.isOnNavMesh) return;

                        ResumeAgent();
                    }
                }
                else
                {
                    Agent.enabled = false;      //Disable the Agent when the animal is not grounded
                    animal.DeltaAngle = 0;      //???

                    CheckAirTarget();        //Check again the Air Target in case it was a miss Grounded.
                }
            }
        }
        protected virtual void FlyOffMesh(Transform target)
        {
            ResetFreeMoveOffMesh();
            IFreeMoveOffMesh = C_FreeMoveOffMesh(target);
            StartCoroutine(IFreeMoveOffMesh);
        }

        protected virtual void ClimbOffMesh()
        {
            if (IClimbOffMesh != null) StopCoroutine(IClimbOffMesh);
            IClimbOffMesh = C_Climb_OffMesh();
            StartCoroutine(IClimbOffMesh);
        }

        public virtual float StopDistance() => DefaultStopDistance;

        protected virtual void ResetFreeMoveOffMesh()
        {
            if (IFreeMoveOffMesh != null)
            {
                IsMovingOffMesh = false;
                StopCoroutine(IFreeMoveOffMesh);
                IFreeMoveOffMesh = null;
            }
        }

        /// <summary>Change to walking when the Animal is near the Target Radius (To Avoid going forward(by intertia) while stoping </summary>
        protected virtual void CheckWalkDistance()
        {
            if (IsGrounded && walkDistance > 0)
            {
                if (walkDistance > RemainingDistance)
                {
                    animal.CurrentSpeedIndex = 1; //Set to the lowest speed.
                }
                else
                {
                    if (animal.CurrentSpeedSet != null &&
                        animal.CurrentSpeedIndex != animal.CurrentSpeedSet.StartVerticalIndex.Value)
                        animal.CurrentSpeedIndex = animal.CurrentSpeedSet.StartVerticalIndex.Value;  //Restore the Current Speed Index to the Speed Set
                }
            }
        }

        protected virtual IEnumerator C_WaitToNextTarget(float time, Transform NextTarget)
        {
            IsWaiting = true;
            Debuging("<color=white>is waiting " + time.ToString("F2") + " seconds</color>");
            Stop();
           

            yield return null; //SUUUUUUUUUPER  IMPORTANT!!!!!!!!!

            if (time > 0)
                yield return new WaitForSeconds(time); 

            SetTarget(NextTarget);
        }

        protected virtual IEnumerator C_FreeMoveOffMesh(Transform target)
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

        protected virtual IEnumerator C_Climb_OffMesh()
        {
            animal.State_Activate(StateEnum.Climb); //Set the State to Climb
            IsMovingOffMesh = true;
            yield return null;
            Agent.enabled = false;


            while (animal.ActiveState.ID == StateEnum.Climb)
            {
                animal.SetInputAxis(Vector3.forward); //Move Upwards on the Climb
                yield return null;
            }

            Debuging("Exit Climb State Off Mesh");

            IsMovingOffMesh = false;
        }

        protected virtual void Debuging(string Log) { if (debug) Debug.Log($"<B>{animal.name}:</B> " + Log); }

        protected virtual void Debuging(string Log, GameObject obj) { if (debug) Debug.Log($"<B>{animal.name}:</B> "+ Log, obj); }

#if UNITY_EDITOR

        [HideInInspector] public int Editor_Tabs1;
      

        protected virtual void Reset()
        {
            agent = gameObject.FindComponent<NavMeshAgent>();
            animal = gameObject.FindComponent<MAnimal>();
        }

        protected virtual void OnDrawGizmos()
        {
            if (!debugGizmos || Agent == null || Agent.path == null) return; 

            Gizmos.color = Color.yellow;

          
            for (int i = 1; i < Agent.path.corners.Length; i++)
            {
                Gizmos.DrawLine(Agent.path.corners[i - 1], Agent.path.corners[i]);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Agent.transform.position, 0.01f);

            var Pos = (Application.isPlaying && target) ? target.position : Agent.transform.position;

            if (Application.isPlaying && target != null && IsAITarget != null)
            {
                Pos = AITargetPos;
            }

            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawWireDisc(Pos, Vector3.up, walkDistance);

            UnityEditor.Handles.color = HasArrived ? Color.green : Color.red;
            UnityEditor.Handles.DrawWireDisc(Pos, Vector3.up, StoppingDistance);

            if (Application.isPlaying/* && !NullDestination*/)
            {
                if (IsAirDestination)
                    Gizmos.DrawWireSphere(DestinationPosition, StoppingDistance);
                else
                    UnityEditor.Handles.DrawWireDisc(DestinationPosition, Vector3.up, StoppingDistance);
            }
        }

     

#endif
    }
}