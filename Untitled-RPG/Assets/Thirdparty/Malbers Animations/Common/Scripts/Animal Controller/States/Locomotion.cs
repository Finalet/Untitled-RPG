using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    /// <summary>This will be in charge of the Movement While is on the Ground </summary>
    public class Locomotion : State
    {
        public override string StateName => "Locomotion";
        [Header("Locomotion Parameters")]  

        [Tooltip("Backward Offset Position of the BackFall Ray")]
        public FloatReference FallRayBackwards = new FloatReference(0.3f);

        [Space(10), Tooltip("Makes the Animal Stop Moving when is near a Wall")]
        public bool WallStop = false;
        [Hide("WallStop", true, false)] public float WalRayLength = 1f;


        [Space(10), Tooltip("Makes the Animal avoid ledges, Useful when the Animal without a Fall State, like the Elephant")]
        public bool AntiFall = false;

        [Hide("AntiFall", true, false)] public float frontDistance = 0.5f;
        [Hide("AntiFall", true, false)] public float frontSpace = 0.2f;
        [Space]
        [Hide("AntiFall", true, false)] public float BackDistance = 0.5f;
        [Hide("AntiFall", true, false)] public float BackSpace = 0.2f;
        [Space]
        [Hide("AntiFall", true, false)] public float FallMultiplier = 1f;
        [Hide("AntiFall", true, false)] public Color DebugColor = Color.yellow;

        /// <summary> The Locomotion also works as the Idle Animation </summary>
        public bool HasIdle { get; private set; }


        public override void InitializeState()
        {
            HasIdle = animal.HasState(StateEnum.Idle); //Check if the animal has Idle State if it does not have then Locomotion is IDLE TOO
        }


        /// <summary>This try to enable the Locomotion Logic</summary>
        public override bool TryActivate()
        {
            if (animal.Grounded)
            { 
                if (!HasIdle) return true; //Return true if is grounded (Meaning Locomotion is also the IDLE STATE

                if (animal.MovementAxisSmoothed != Vector3.zero || animal.MovementDetected) //If is moving? 
                {
                    SetStatus((int)animal.CurrentSpeedModifier.Vertical.Value); //When entering Locomotion the State set the Status the current Speed Modifier.
  
                    return true;
                }
            }
            return false;
        }


        public override void EnterCoreAnimation()
        {
            SetStatus(0);//Use the Status on the Animator to show the Vertical Speed used on start of the state
       

            //if (ResetIntertia && General.RootMotion)
            //    animal.ResetInertiaSpeed();  //BUG THAT IT WAS MAKING GO FASTER WHEN ENTERING LOCOMOTION

        }

        public override void OnStatePreMove(float deltatime)
        {
            Wall_Stop();
            Anti_Fall();
        }

        public override void OnStateMove(float deltatime)
        {
            SetFloatSmooth(0, deltatime * CurrentSpeed.lerpPosition);
        }


        private void Wall_Stop()
        {
            if (WallStop)
            {
                var MainPivotPoint = animal.Main_Pivot_Point;
                if (Physics.Raycast(MainPivotPoint, animal.Forward, out _, WalRayLength, animal.GroundLayer, QueryTriggerInteraction.Ignore))
                {
                    Gizmos.color = Color.red; 
                    Debug.DrawRay(MainPivotPoint, animal.Forward * WalRayLength,Color.red);
                    animal.MovementAxis.z = 0;
                }
                else
                {
                    Debug.DrawRay(MainPivotPoint, animal.Forward * WalRayLength, DebugColor);
                }
            }
        }



        /// <summary> The Locomotion Uses the Status State Animator Parameter to know which speed Index is using  </summary>
        public override void SpeedModifierChanged(MSpeed speed, int SpeedIndex)
        {
            if (InCoreAnimation) SetStatus(SpeedIndex);
        }

        //───────────────────────────────────────── ANTI FALL CODE ──────────────────────────────────────────────────────────────────

        private void Anti_Fall()
        {
            if (AntiFall)
            {
                bool BlockForward = false;

                var ForwardMov = animal.MovementAxisRaw.z; // Get the Raw movement that enters on the animal witouth any modifications
                var Dir = animal.TerrainSlope > 0 ? animal.Gravity : -animal.Up;

                float SprintMultiplier = (animal.CurrentSpeedModifier.Vertical).Value;
                SprintMultiplier += animal.Sprint ? 1f : 0f; //Check if the animal is sprinting

                float scale = animal.ScaleFactor;
                var RayMultiplier = animal.Pivot_Multiplier * FallMultiplier; //Get the Multiplier

                var MainPivotPoint = animal.Pivot_Chest.World(animal.transform);

                RaycastHit[] hits = new RaycastHit[1];

                Vector3 Center;
                Vector3 Left;
                Vector3 Right;


                if (ForwardMov > 0)              //Means we are going forward
                {
                    Center = MainPivotPoint + (animal.Forward * frontDistance * SprintMultiplier * scale); //Calculate ahead the falling ray
                    Left = Center + (animal.Right * frontSpace * scale);
                    Right = Center + (-animal.Right * frontSpace * scale);
                }
                else if (ForwardMov < 0)  //Means we are going backwards
                {
                    Center = MainPivotPoint - (animal.Forward * BackDistance * SprintMultiplier * scale); //Calculate ahead the falling ray
                    Left = Center + (animal.Right * BackSpace * scale);
                    Right = Center + (-animal.Right * BackSpace * scale);
                }
                else
                { return; }

                Debug.DrawRay(Center, Dir * RayMultiplier, DebugColor);
                Debug.DrawRay(Left, Dir * RayMultiplier, DebugColor);
                Debug.DrawRay(Right, Dir * RayMultiplier, DebugColor);

                var fallHits = Physics.RaycastNonAlloc(Center, Dir, hits, RayMultiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore);

                if (fallHits == 0)
                {
                    BlockForward = true; //Means there's 2 rays that are falling
                }
                else
                    fallHits = Physics.RaycastNonAlloc(Left, Dir, hits, RayMultiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore);
                if (fallHits == 0)
                {
                    BlockForward = true; //Means there's 2 rays that are falling
                }
                else
                {
                    fallHits = Physics.RaycastNonAlloc(Right, Dir, hits, RayMultiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore);
                    if (fallHits == 0)
                    {
                        BlockForward = true; //Means there's 2 rays that are falling
                    }
                }

                if (BlockForward) animal.MovementAxis.z = 0;
                //animal.Remove_HMovement = BlockForward;
            }
            else if (!animal.UseCameraInput && animal.MovementAxisRaw.z < 0) //Meaning is going backwards so AntiFall B
            {
                var MainPivotPoint = animal.Has_Pivot_Hip ? animal.Pivot_Hip.World(animal.transform) : animal.Pivot_Chest.World(animal.transform);
                MainPivotPoint += animal.Forward * -(FallRayBackwards * animal.ScaleFactor);
                RaycastHit[] hits = new RaycastHit[1];

                var RayMultiplier = animal.Pivot_Multiplier; //Get the Multiplier
                Debug.DrawRay(MainPivotPoint, -animal.Up * RayMultiplier, Color.white);

                var fallHits = Physics.RaycastNonAlloc(MainPivotPoint, -animal.Up, hits, RayMultiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore);
               
                if (fallHits == 0)
                {
                    animal.MovementAxis.z = 0;
                    //animal.Remove_HMovement = true;
                }
            }
        }

        public override void StateGizmos(MAnimal animal)
        {
            if (!IsActiveState)
            {
                if (AntiFall) PaintRays(animal);

                if (WallStop)
                {
                    var MainPivotPoint = animal.Main_Pivot_Point;
                    Debug.DrawRay(MainPivotPoint, animal.Forward * WalRayLength, DebugColor);
                }
            }
        }



        void PaintRays(MAnimal animal)
        {
            float scale = animal.ScaleFactor;
            var Dir = animal.TerrainSlope > 0 ? animal.Gravity : -animal.Up;
            var RayMultiplier = animal.Pivot_Multiplier * FallMultiplier; //Get the Multiplier
            var MainPivotPoint = animal.Pivot_Chest.World(animal.transform);

            var FrontCenter = MainPivotPoint + (animal.Forward * frontDistance * scale); //Calculate ahead the falling ray
            var FrontLeft = FrontCenter + (animal.Right * frontSpace * scale);
            var FrontRight = FrontCenter + (-animal.Right * frontSpace * scale);
            var BackCenter = MainPivotPoint - (animal.Forward * BackDistance * scale); //Calculate ahead the falling ray
            var BackLeft = BackCenter + (animal.Right * BackSpace * scale);
            var BackRight = BackCenter + (-animal.Right * BackSpace * scale);

            Debug.DrawRay(FrontCenter, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(FrontLeft, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(FrontRight, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(BackCenter, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(BackLeft, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(BackRight, Dir * RayMultiplier, DebugColor);
        }


#if UNITY_EDITOR
        void Reset()
        {
            ID = MTools.GetInstance<StateID>("Locomotion");

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = true,
                Sprint = true,
                OrientToGround = true,
                CustomRotation = false,
                IgnoreLowerStates = false, 
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };
        }
#endif
    }
}