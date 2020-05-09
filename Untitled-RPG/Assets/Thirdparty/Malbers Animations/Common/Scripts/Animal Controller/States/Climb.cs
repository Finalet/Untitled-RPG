using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

namespace MalbersAnimations.Controller
{
    /// <summary>Climb Logic </summary>
    public class Climb : State
    {
        public enum ClimbDetectionType { RayCast, Triggers };


        public static int Climb_Ledge = 1;
        public static int Climb_Off = 2;

        /// <summary>Air Resistance while falling</summary>
        [Header("Climb Parameters"), Space]
        public string Pivot = "Climb";
        [Tooltip("Walls need to have the Tag Climb to be climbable")]
        public string ClimbTag = "Climb";
        [Tooltip("Climb automatically when is near a climbable wall")]
        public bool automatic = false;
        public float WallDistance = 0.2f;
        public float AlignSmoothness = 10f;
        public float GroundDistance = 0.5f;
        public float HorizontalDistance = 0.2f;
        public float InnerAngle = 3f;
       // public float OuterAngle = 15f;

        [Header("Ledge Detection")]
        public float LedgeRays = 0.4f;
        public Vector3 LedgeUp = Vector3.up;
        [Tooltip("MinDistance to exit Climb over a ledge")]
        public float LedgeMinExit = 0.4f;

        //  public ClimbDetectionType DetectionType = ClimbDetectionType.RayCast;

        private MPivots ClimbPivot;
        protected Transform wall;
        protected Vector3 platform_Pos;
        protected Quaternion platform_Rot;

        RaycastHit[] ClimbHit = new RaycastHit[1];
        RaycastHit[] ClimbHitLeft = new RaycastHit[1];
        RaycastHit[] ClimbHitRight = new RaycastHit[1];
        RaycastHit[] ClimbDownHit = new RaycastHit[1];
        RaycastHit[] EdgeHit = new RaycastHit[1];


        /// <summary> World Position of the Climb Pivot </summary>
        public Vector3 ClimbPivotPoint { get { return ClimbPivot.World(transform); } }

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
        public Vector3 LedgePivotUP { get { return transform.TransformPoint(LedgeUp); } }

        public override void StatebyInput()
        {
            if (InputValue && IsActiveState && InMainTagHash) //Disable Climb by Input when we are already climbing
            {
                AllowExit();
                InputValue = false;
                if (debug) Debug.Log("<B>Climb:</B> Exit with Climb Input");
              //animal.SetIntID(2); //Set the animation to start falling and then fall
                animal.State_SetStatus(Climb_Off); //Set the animation to start falling and then fall
                animal.State_Force(3); //Force Fall State to activate NEXT
            }
        }

        public override void AwakeState()
        {
            useCameraInput = animal.UseCameraInput;

            base.AwakeState();

            ClimbHit = new RaycastHit[1];
            ClimbPivot = animal.pivots.Find(x => x.name == Pivot);

            if (ClimbPivot == null)
            { Debug.LogError("The Climb State requires a Pivot named 'Climb'. Please create a new pivot on the Animal Component"); }

            if (!animal.hasStateStatus)
            { Debug.LogError("The Climb State requires a the StateStatus Animator Parameter. Please add it to the Animator Controller and set the Climb Exit Animations with the State Status as the transition condition"); }

        }

        bool useCameraInput;
        bool WallClimbTag;

        public override void Activate()
        {
            base.Activate();
            animal.UseCameraInput = false;   //Climb cannot use Camera Input
            animal.DisablePivotChest();
         
        }

        public override void AnimationStateEnter()
        {
            if (CurrentAnimTag  == AnimTag.ClimbStart)
            {
              
            }
        }

        public override void ExitState()
        {
            base.ExitState();
            //animal.UseCameraInput = useCameraInput; //Return to the default camera input on the Animal
            animal.EnablePivotChest();
            wall = null;
            ClimbHit = new RaycastHit[1];
            ClimbHitLeft = new RaycastHit[1];
            ClimbHitRight = new RaycastHit[1];
            ClimbDownHit = new RaycastHit[1];
            EdgeHit = new RaycastHit[1];
        }

        public override void PendingExitState()
        {
            animal.UseCameraInput = useCameraInput; //Return to the default camera input on the Animal
        }

        public override bool TryActivate()
        {
            bool findClimbable = Physics.RaycastNonAlloc(ClimbPivotPoint, animal.Forward, ClimbHit, animal.ScaleFactor * ClimbPivot.multiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore) > 0;
            Debug.DrawRay(ClimbPivotPoint, animal.Forward * animal.ScaleFactor * ClimbPivot.multiplier, Color.red);

            if (findClimbable)
            {
                if (automatic)
                    WallClimbTag = ClimbHit[0].transform.gameObject.CompareTag(ClimbTag);
                else
                    WallClimbTag = ClimbHit[0].transform.gameObject.CompareTag(ClimbTag) && InputValue;

                //if (WallClimbTag)
                //{
                //    Debug.Log("Alogn");
                //    animal.StartCoroutine(MalbersTools.AlignLookAtTransform(transform, ClimbHit[0].point, 0.15f));         //Aling the Animal to the Link Position
                //}
                return WallClimbTag;
            }

            return false;
        }

        public override void TryExitState(float DeltaTime)
        {
            if (InMainTagHash)
            {
                var scalefactor = animal.ScaleFactor;
                var forward = animal.Forward;
                var gravityDir = animal.GravityDirection;

                var MainPivot = ClimbPivotPoint + animal.AdditivePosition;
                Debug.DrawRay(MainPivot, animal.GravityDirection * animal.ScaleFactor * GroundDistance, Color.white);

                //PRESSING DOWN
                if (animal.MovementAxis.z < 0) //Means the animal is going down
                {
                    int HitRaysCount = Physics.RaycastNonAlloc(MainPivot, gravityDir, ClimbDownHit, scalefactor * GroundDistance, animal.GroundLayer, QueryTriggerInteraction.Ignore);

                    if (HitRaysCount > 0) //Means that the Animal is going down and touching the ground
                    {
                        if (debug) Debug.Log("<B>Climb:</B> Exit when Grounded and pressing Backwards");
                      
                        animal.Grounded = true;
                        AllowExit();
                        return;
                    }
                }

                if (!HitRayLeft && !HitRayRight)
                {
                    if (debug) Debug.Log("<B>Climb:</B> Exit when there's no Rays hitting the wall");
                    AllowExit();
                    return;
                }

                if (!WallClimbTag)
                {
                    if (debug) Debug.Log("<B>Climb:</B> Exit when wall does not have the Climb Tag");
                    AllowExit();
                    return;
                }

                //Check Upper Ground legde Detection

                bool LedgeHit = Physics.RaycastNonAlloc(LedgePivotUP, forward, EdgeHit, scalefactor * LedgeRays, animal.GroundLayer, QueryTriggerInteraction.Ignore) > 0;

                Debug.DrawRay(LedgePivotUP, forward * scalefactor* LedgeRays, LedgeHit ? Color.red : Color.green);

                if (!LedgeHit)
                {
                    var SecondRayPivot = new Ray(LedgePivotUP, forward).GetPoint(LedgeRays);


                    LedgeHit = Physics.RaycastNonAlloc(SecondRayPivot,  gravityDir, EdgeHit, scalefactor * LedgeRays, animal.GroundLayer, QueryTriggerInteraction.Ignore) > 0;

                    Debug.DrawRay(SecondRayPivot, gravityDir * scalefactor * LedgeRays, !LedgeHit ? Color.red : Color.green);

                    if (LedgeHit)
                    {
                        var hit = EdgeHit[0];

                        if (hit.distance > LedgeMinExit*scalefactor)
                        {
                            if (debug) Debug.Log("<B>Climb:</B> Exit On a Ledge");
                            animal.State_SetStatus(Climb_Ledge);
                            animal.State_Force(1); //Force Locomotion State instead of FALL when climbing on a ledge
                            AllowExit();
                        }
                    }

                }
            }
        }

        public override void OnStateMove(float deltatime)
        {
            if (InMainTagHash || CurrentAnimTag == AnimTag.ClimbStart)
            {
                var GroundLayer = animal.GroundLayer;
                var Forward = animal.Forward;
                var ScaleFactor = animal.ScaleFactor;
                var mult = ClimbPivot.multiplier;

                var RelativeForward = transform.InverseTransformDirection(Forward);
                var rotation = transform.rotation;

                var LeftInnerForward = rotation * Quaternion.Euler(0, -InnerAngle, 0) * RelativeForward;
                var RightInnerForward = rotation * Quaternion.Euler(0, InnerAngle, 0) * RelativeForward;

                //var LeftOuterForward = rotation * Quaternion.Euler(0,   OuterAngle, 0) * RelativeForward;
                //var RightOuterForward = rotation * Quaternion.Euler(0,- OuterAngle, 0) * RelativeForward;

                HitRayLeft = Physics.RaycastNonAlloc(ClimbPivotPointLeft, LeftInnerForward, ClimbHitLeft, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;
                HitRayRight = Physics.RaycastNonAlloc(ClimbPivotPointRight, RightInnerForward, ClimbHitRight, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;
                HitRayCenter = Physics.RaycastNonAlloc(ClimbPivotPoint, Forward, ClimbHit, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;

                Debug.DrawRay(ClimbPivotPointLeft, LeftInnerForward * ScaleFactor * mult, HitRayLeft ? Color.green:  Color.red);
                Debug.DrawRay(ClimbPivotPointRight, RightInnerForward * ScaleFactor * mult, HitRayRight ? Color.green : Color.red);
                
                
                //Debug.DrawRay(ClimbPivotPoint, Forward * ScaleFactor * mult,  Color.green  );  var LeftOuterForward = rotation * Quaternion.Euler(0,   OuterAngle, 0) * RelativeForward;
                //var RightOuterForward = rotation * Quaternion.Euler(0,- OuterAngle, 0) * RelativeForward;

                //HitRayLeft = Physics.RaycastNonAlloc(ClimbPivotPointLeft, LeftOuterForward, ClimbHitLeft, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;
                //HitRayRight = Physics.RaycastNonAlloc(ClimbPivotPointRight, RightOuterForward, ClimbHitRight, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;
                //HitRayCenter = Physics.RaycastNonAlloc(ClimbPivotPoint, Forward, ClimbHit, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;

                //Debug.DrawRay(ClimbPivotPointLeft, LeftOuterForward * ScaleFactor * mult, HitRayLeft ? Color.green:  Color.red);
                //Debug.DrawRay(ClimbPivotPointRight, RightOuterForward * ScaleFactor * mult, HitRayRight ? Color.green : Color.red);
                //Debug.DrawRay(ClimbPivotPoint, Forward * ScaleFactor * mult,  Color.green  );


                //if (!HitRayLeft) //Means did not found the Outer Forward
                //{
                //    HitRayLeft = Physics.RaycastNonAlloc(ClimbPivotPointLeft, LeftInnerForward, ClimbHitLeft, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;
                //    Debug.DrawRay(ClimbPivotPointLeft, LeftInnerForward * ScaleFactor * mult, HitRayLeft ? Color.green : Color.red);
                //}

                //if (!HitRayRight) //Means did not found the Outer Forward
                //{
                //    HitRayRight = Physics.RaycastNonAlloc(ClimbPivotPointRight, RightInnerForward, ClimbHitRight, ScaleFactor * mult, GroundLayer, QueryTriggerInteraction.Ignore) > 0;
                //    Debug.DrawRay(ClimbPivotPointRight, RightInnerForward * ScaleFactor * mult, HitRayRight ? Color.green : Color.red);
                //}

                if (HitRayCenter)
                {
                    var hitCenter = ClimbHit[0];
                    MovingWall(hitCenter.transform, deltatime);
                    AlignToWall(hitCenter.distance, deltatime);
                }
                //return;



                if (HitRayLeft || HitRayRight)
                {
                    float distance = 0;
                    Vector3 OrientNormal = Forward;

                    if (HitRayLeft)
                    {
                        var hitLeft = ClimbHitLeft[0];
                        distance = hitLeft.distance;
                        OrientNormal = hitLeft.normal;
                    }
                    if (HitRayRight)
                    {
                        var hitRight = ClimbHitRight[0];
                        distance = (distance + hitRight.distance) / 2;
                        OrientNormal = (OrientNormal + hitRight.normal).normalized;
                    }

                    AlignToWall(distance, deltatime);
                    AvNormal = Vector3.Lerp(AvNormal, OrientNormal, deltatime * AlignSmoothness);

                    OrientToWall(AvNormal, deltatime);
                }
            }
        }

        Vector3 AvNormal;
        private bool HitRayLeft;
        private bool HitRayRight;

        public bool HitRayCenter { get; private set; }

        private void OrientToWall(Vector3 normal, float deltatime)
        {
            Quaternion AlignRot = Quaternion.FromToRotation(transform.forward, -normal) * transform.rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, deltatime * AlignSmoothness); //Calculate the Delta Align Rotation

             //transform.rotation *= Delta;
            animal.AdditiveRotation *= Delta;
    }

        private void MovingWall(Transform hit, float deltatime)
        {
            if (wall == null || wall != hit)               //Platforming logic
            {
                wall = hit;
                WallClimbTag = hit.gameObject.CompareTag(ClimbTag);
                platform_Pos = wall.position;
                platform_Rot = wall.rotation;
            }

            if (wall == null) return;

            var DeltaPlatformPos = wall.position - platform_Pos;

            animal.AdditivePosition += DeltaPlatformPos;                          // Keep the same relative position.

            Quaternion Inverse_Rot = Quaternion.Inverse(platform_Rot);
            Quaternion Delta = Inverse_Rot * wall.rotation;

            if (Delta != Quaternion.identity)                                        // no rotation founded.. Skip the code below
            {
                var pos = transform.DeltaPositionFromRotate(wall, Delta);
                animal.AdditivePosition += pos;
            }

            animal.AdditiveRotation *= Delta;

            platform_Pos = wall.position;
            platform_Rot = wall.rotation;
        }

        //Align the Animal to the Wall
        private void AlignToWall(float distance, float deltatime)
        { 
            float difference =  distance - WallDistance*animal.ScaleFactor;

            if (!Mathf.Approximately(distance, WallDistance * animal.ScaleFactor))
            {
                Vector3 align = animal.Forward * difference * deltatime * AlignSmoothness;
                animal.AdditivePosition += align;
            }
        }


#if UNITY_EDITOR
        void Reset()
        {
            ID = MalbersTools.GetInstance<StateID>("Climb");
            General = new AnimalModifier()
            {
                RootMotion = true,
                AdditivePosition = true,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                Colliders = true,
                Gravity = false,
                CustomRotation = true,
                modify = (modifier)(-1), FreeMovement = false,
                IgnoreLowerStates = true, 
            };
        }
#endif
    }
}