using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;
//using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

namespace MalbersAnimations.Controller
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/states/climb")]
    /// <summary>Climb Logic </summary>
    public class Climb : State
    {
        public override string StateName => "Climb/Simple Climb";

        /// <summary>Air Resistance while falling</summary>
        [Header("Climb Parameters"), Space]

        [Tooltip("Name of the Pivot needed to cast a ray to Identify Climbable surfaces")]
        public string Pivot = "Climb";
        [Tooltip("Tag used to identify climbable surfaces. Default: [Climb]")]
        public string SurfaceTag = "Climb";
        [Tooltip("Climb automatically when is near a climbable surface")]
        public bool automatic = false;
        [Tooltip("When aligning the Animal to the Wall, this will be the distance needed to separate it from it ")]
        public float WallDistance = 0.2f;
        [Tooltip("Smoothness value to align the animal to the wall")]
        public float AlignSmoothness = 10f;
        [Tooltip("Distance from the Hip Pivot to the Ground")]
        public float GroundDistance = 0.5f;
        [Tooltip("Distance diferente to cast the Left and Right Ray to find Corners on the walls.")]
        public float HorizontalDistance = 0.2f;
        [Tooltip("Angle needed to find Perpendicular Corners. This value must be different than zero.")]
        public float InnerAngle = 3f;

        [Header("Exit State Status")]
        [Tooltip("When the Exit Condition Climb Up an edge is executed. The State Exit Status will change to this value")]
        public int ClimbLedge = 1;
        [Tooltip("When the Exit Condition Climb Off is executed. The State Exit Status will change to this value")]
        public int ClimbOff = 2;
        [Tooltip("When the Exit Condition Climb Down to the Ground executed. The State Exit Status will change to this value")]
        public int ClimbDown = 3;
       


        // public float OuterAngle = 15f;

        [Header("Ledge Detection")]

        [Tooltip("Offset Position to Cast the First Ray on top on the animal to find the ledge")]
        public Vector3 RayLedgeOffset = Vector3.up;
        [Tooltip("Length of the Ledge Ray")]
        public float RayLedgeLength = 0.4f;
        [Tooltip("MinDistance to exit Climb over a ledge")]
        [FormerlySerializedAs("LedgeMinExit")]
        public float LedgeExitDistance = 0.175f;

        private MPivots ClimbPivot;
        /// <summary> Reference for the current Climbable surface</summary>
        public Transform Wall { get; private set; }
        private Vector3 platform_Pos;
        private Quaternion platform_Rot;

        //private readonly RaycastHit[] ClimbHit = new RaycastHit[1];
        //private readonly RaycastHit[] ClimbHitLeft = new RaycastHit[1];
        //private readonly RaycastHit[] ClimbHitRight = new RaycastHit[1];
        //private readonly RaycastHit[] ClimbDownHit = new RaycastHit[1];
         private readonly RaycastHit[] EdgeHit = new RaycastHit[1];


        private RaycastHit ClimbHit; 
        private RaycastHit ClimbHitLeft; 
        private RaycastHit ClimbHitRight ;
        //private RaycastHit ClimbDownHit;
        //private RaycastHit EdgeHit;



        private bool UsingCameraInput;
        private bool WallClimbTag;

        /// <summary> World Position of the Climb Pivot </summary>
        public Vector3 ClimbPivotPoint => ClimbPivot.World(transform);

        /// <summary> World Position of the Climb Pivot - the Horizontal Distance </summary>
        public Vector3 ClimbPivotPointLeft
        {
            get
            {
                var PointLeft = ClimbPivot.position - new Vector3(-HorizontalDistance, 0, 0);
                return transform.TransformPoint(PointLeft);
            }
        }

        /// <summary> World Position of the Climb Pivot + the Horizontal Distance </summary>
        public Vector3 ClimbPivotPointRight
        {
            get
            {
                var PointLeft = ClimbPivot.position - new Vector3(HorizontalDistance, 0, 0);
                return transform.TransformPoint(PointLeft);
            }
        }

        /// <summary>Starting point above the head of the Ledge Detection to cast a Ray</summary>
        public Vector3 LedgePivotUP => transform.TransformPoint(RayLedgeOffset);

        Vector3 AvNormal;
        private bool HitRayLeft;
        private bool HitRayRight;
       


        public override void AwakeState()
        {
            UsingCameraInput = animal.UseCameraInput; //Store the Animal Current CameraInput

            base.AwakeState();

            ClimbPivot = animal.pivots.Find(x => x.name == Pivot);

            if (ClimbPivot == null)
            { Debug.LogError("The Climb State requires a Pivot named 'Climb'. Please create a new pivot on the Animal Component"); }
        }


        

        public override void StatebyInput()
        {
            if (InputValue)
            {
                if (IsActiveState && InCoreAnimation) //Disable Climb by Input when we are already climbing)
                {
                    animal.State_Allow_Exit(StateEnum.Fall);
                    InputValue = false;
                    if (debug) Debug.Log("<B>Climb:</B> Exit with Climb Input");
                    SetStatus(ClimbOff); //Set the animation to start falling and then fall
                }
                else
                {
                    if (CheckClimbRay())  Activate();
                }
            }
        }

        public override void Activate()
        {
            base.Activate();
            animal.UseCameraInput = false;       //Climb cannot use Camera Input
            animal.DisablePivotChest();
            //animal.UpdateDirectionSpeed = false; //Do not Update the Direction Speed... we are going to Update it internally
        }

        public override void ResetStateValues() 
        {
            Wall = null;
        }

        public override void RestoreAnimalOnExit()
        {
            animal.EnablePivotChest();
           // animal.UpdateDirectionSpeed = true; //Reset the Rotate Direction to the Default value
        }

        /// <summary> CALLED BY THE ANIMATOR </summary>
        public void ClimbResetInput() => animal.UseCameraInput = UsingCameraInput; //Return to the default camera input on the Animal
        
        public override bool TryActivate()
        {
            if (automatic)
            {
                Debug.DrawRay(ClimbPivotPoint, animal.Forward * animal.ScaleFactor * ClimbPivot.multiplier, Color.white);
                return CheckClimbRay();
            }
            return false;
        }


        private bool CheckClimbRay()
        {
            if (Physics.Raycast(ClimbPivotPoint, animal.Forward, out ClimbHit, animal.ScaleFactor * ClimbPivot.multiplier, animal.GroundLayer, IgnoreTrigger))
            {
                WallClimbTag = ClimbHit.transform.gameObject.CompareTag(SurfaceTag);

                Debugging("WallClimbTag = " + WallClimbTag);
              
                AvNormal = ClimbHit.normal;
                return WallClimbTag;
            }
            return false;
        }


        public override void EnterTagAnimation()
        {
            if (InExitAnimation) AllowExit();
        }

        /// <summary>Current Direction Speed Applied to the Additional Speed, by default is the Animal Forward Direction</summary>
        public override Vector3 Speed_Direction()
        {
            return animal.Up * animal.VerticalSmooth + animal.Right * animal.HorizontalSmooth; //IMPORTANT OF ADDITIONAL SPEED
        }

        public override void OnStateMove(float deltatime)
        {
            if (InCoreAnimation)
            {
                WallClimbTag = true;

                var GroundLayer = animal.GroundLayer;
                var Forward = animal.Forward;
                var ScaleFactor = animal.ScaleFactor;
                var mult = ClimbPivot.multiplier;

                var RelativeForward = transform.InverseTransformDirection(Forward);
                var rotation = transform.rotation;

                var LeftInnerForward = rotation * Quaternion.Euler(0, -InnerAngle, 0) * RelativeForward;
                var RightInnerForward = rotation * Quaternion.Euler(0, InnerAngle, 0) * RelativeForward;

               // animal.Speed_Direction = animal.Up * animal.VerticalSmooth + animal.Right * animal.HorizontalSmooth; //IMPORTANT OF ADDITIONAL SPEED

                //HitRayLeft = Physics.RaycastNonAlloc(ClimbPivotPointLeft, LeftInnerForward, ClimbHitLeft, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;
                //HitRayRight = Physics.RaycastNonAlloc(ClimbPivotPointRight, RightInnerForward, ClimbHitRight, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;
                //HitRayCenter = Physics.RaycastNonAlloc(ClimbPivotPoint, Forward, ClimbHit, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;

                HitRayLeft = Physics.Raycast(ClimbPivotPointLeft, LeftInnerForward, out ClimbHitLeft, ScaleFactor * mult, GroundLayer, IgnoreTrigger);
                HitRayRight = Physics.Raycast(ClimbPivotPointRight, RightInnerForward, out ClimbHitRight, ScaleFactor * mult, GroundLayer, IgnoreTrigger);
 
                Debug.DrawRay(ClimbPivotPointLeft, LeftInnerForward * ScaleFactor * mult, HitRayLeft ? Color.green : Color.red);
                Debug.DrawRay(ClimbPivotPointRight, RightInnerForward * ScaleFactor * mult, HitRayRight ? Color.green : Color.red);

                if (Physics.Raycast(ClimbPivotPoint, Forward, out ClimbHit, ScaleFactor * mult, GroundLayer, IgnoreTrigger))
                {
                    CheckMovingWall(ClimbHit.transform, deltatime);
                    AlignToWall(ClimbHit.distance, deltatime);
                }
                else
                {
                    WallClimbTag = false; //There's no wall
                }
              

                if (HitRayLeft || HitRayRight)
                {
                    float distance = 0;
                    Vector3 OrientNormal = Forward;

                    if (HitRayLeft)
                    {
                        distance = ClimbHitLeft.distance;
                        OrientNormal = ClimbHitLeft.normal;
                    }
                    if (HitRayRight)
                    {
                        distance = (distance + ClimbHitRight.distance) / 2;
                        OrientNormal = (OrientNormal + ClimbHitRight.normal).normalized;
                    }

                    AlignToWall(distance, deltatime);
                    AvNormal = Vector3.Lerp(AvNormal, OrientNormal, deltatime * AlignSmoothness);

                    OrientToWall(AvNormal, deltatime);
                }
            }
            else if (InEnterAnimation)  //If we are on Climb Start do a quick alignment to the Wall.
            {
                OrientToWall(AvNormal, deltatime);
            }
        }


        //Reset the Climb Input to the default
        public override void PendingAnimationState()
        {
            ClimbResetInput();
        }


        public override void TryExitState(float DeltaTime)
        {
            var scalefactor = animal.ScaleFactor;
            var forward = animal.Forward;
            var Gravity = animal.Gravity;
            var MainPivot = ClimbPivotPoint + animal.AdditivePosition;

            Debug.DrawRay(MainPivot, Gravity * animal.ScaleFactor * GroundDistance, Color.white);

            //PRESSING DOWN
            if (animal.MovementAxisRaw.z < 0) //Means the animal is going down
            {
                if (Physics.Raycast(MainPivot, Gravity, out _, scalefactor * GroundDistance, animal.GroundLayer, IgnoreTrigger)) //Means that the Animal is going down and touching the ground
                {
                    if (debug) Debug.Log($"<B>[{animal.name}]-> [Climb]</B> -[Try Exit] when Grounded and pressing Backwards");

                    Debugging("[Allow Exit] when Grounded and pressing Down and touched the ground");
                    AllowExit(StateEnum.Idle, ClimbDown); //Force the Idle State to be the next State
                    animal.CheckIfGrounded();
                }
            }
            else if (!HitRayLeft && !HitRayRight) //If both Ray failed means there no wall at all
            {
                Debugging("[Allow Exit] Exit when there's no Rays hitting a climbable surface");
                AllowExit();
            }
            else if (!WallClimbTag)
            {
                Debugging("[Allow Exit] Exit when wall does not have the Climb Tag");
                AllowExit();
            }
            else
            {
                //Check Upper Ground legde Detection
                bool LedgeHit = Physics.RaycastNonAlloc(LedgePivotUP, forward, EdgeHit, scalefactor * RayLedgeLength, animal.GroundLayer, IgnoreTrigger) > 0;
                Debug.DrawRay(LedgePivotUP, forward * scalefactor * RayLedgeLength, LedgeHit ? Color.red : Color.green);


                if (!LedgeHit)
                {
                    var SecondRayPivot = new Ray(LedgePivotUP, forward).GetPoint(RayLedgeLength);

                    LedgeHit = Physics.RaycastNonAlloc(SecondRayPivot, Gravity, EdgeHit, scalefactor * RayLedgeLength, animal.GroundLayer, IgnoreTrigger) > 0;

                    Debug.DrawRay(SecondRayPivot, Gravity * scalefactor * RayLedgeLength, !LedgeHit ? Color.red : Color.green);

                    if (LedgeHit)
                    {
                        var hit = EdgeHit[0];

                        if (hit.distance > LedgeExitDistance * scalefactor)
                        {
                            Debugging("Allow Exit - Exit on a Ledge");
                            AllowExit(StateEnum.Locomotion, ClimbLedge); ; //Force Locomotion State to be the next state, it also set the Exit Status
                        }
                    }
                }
            }
        }

        private void OrientToWall(Vector3 normal, float deltatime)
        {
            Quaternion AlignRot = Quaternion.FromToRotation(transform.forward, -normal) * transform.rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, deltatime * AlignSmoothness); //Calculate the Delta Align Rotation
            animal.AdditiveRotation *= Delta;
    }

        private void CheckMovingWall(Transform hit, float deltatime)
        {
            if (Wall == null || Wall != hit)               //Platforming logic
            {
                Wall = hit;
                platform_Pos = hit.position;
                platform_Rot = hit.rotation;
                WallClimbTag = Wall.gameObject.CompareTag(SurfaceTag); //Find if the Object hitted is a climbable surface
            }

            if (Wall == null) return;

            var DeltaPlatformPos = Wall.position - platform_Pos;

            animal.AdditivePosition += DeltaPlatformPos;                          // Keep the same relative position.

            Quaternion Inverse_Rot = Quaternion.Inverse(platform_Rot);
            Quaternion Delta = Inverse_Rot * Wall.rotation;

            if (Delta != Quaternion.identity)                                        // no rotation founded.. Skip the code below
            {
                var pos = transform.DeltaPositionFromRotate(Wall, Delta);
                animal.AdditivePosition += pos;
            }

            animal.AdditiveRotation *= Delta;

            platform_Pos = Wall.position;
            platform_Rot = Wall.rotation;
        }

        //Align the Animal to the Wall
        private void AlignToWall(float distance, float deltatime)
        {
            float difference = distance - WallDistance * animal.ScaleFactor;

            if (!Mathf.Approximately(distance, WallDistance * animal.ScaleFactor))
            {
                Vector3 align = animal.Forward * difference * deltatime * AlignSmoothness;
                animal.AdditivePosition += align;
            }
        }


#if UNITY_EDITOR
        void Reset()
        {
            ID = MTools.GetInstance<StateID>("Climb");
            General = new AnimalModifier()
            {
                RootMotion = true,
                AdditivePosition = true,
                AdditiveRotation = true,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                Gravity = false,
                CustomRotation = true,
                modify = (modifier)(-1), FreeMovement = false,
                IgnoreLowerStates = true, 
            };
        }
#endif
    }
}