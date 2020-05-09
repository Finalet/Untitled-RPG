using UnityEngine;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller
{
    public class Fall : State
    {
        public enum FallBlending { DistanceNormalized, Distance , VerticalVelocity }

        /// <summary>Air Resistance while falling</summary>
        [Header("Fall Parameters")]
        [Tooltip("Can the Animal be controller while falling?")]
        public BoolReference AirControl = new BoolReference(true);
        [Tooltip("Rotation while falling")]
        public FloatReference AirRotation = new FloatReference(10);
        [Tooltip("Maximum Movement while falling")]
        public FloatReference AirMovement = new FloatReference(0);
        [Tooltip("Lerp value for the Air Movement adjusment")]
        public FloatReference airSmooth = new FloatReference(2);

        //   public FloatReference AirRotationLerp = new FloatReference(5);
        [Space]
        //public float fallRayDownMultiplier;
        public FloatReference FallRayForward = new FloatReference(0.1f);

        /// <summary>Multiplier to check if is falling in front of him</summary>
        [Tooltip("Multiplier for the Fall Ray Length")]
        public FloatReference fallRayMultiplier = new FloatReference(1f);


        /// <summary>Used to Set fallBlend to zero before reaching the ground</summary>
        [Space, Tooltip("Used to Set fallBlend to zero before reaching the ground")]
        public FloatReference LowerBlendDistance;


        public FallBlending BlendFall = FallBlending.DistanceNormalized;

        /// <summary>Distance to Apply a Fall Hard Animation</summary>
        [Space,Header("Landing Options"), Tooltip("Minimum Distance to Apply a Fall Hard Animation")]
        public FloatReference FallMinDistance = new FloatReference(5f);
        public FloatReference FallMaxDistance = new FloatReference(10f);
        public string LandTag = "Land";
        public StatModifier AffectStat;



        private int LandTagHash;
        private float FallCurrentDistance;

        protected Vector3 fall_Point;
        RaycastHit[] FallHits = new RaycastHit[1];
        //  protected Vector3 HorizontalInertia;
        private RaycastHit FallRayCast;

        /// <summary>this is to store the max Y heigth before falling</summary>
        private float MaxHeight;
        /// <summary>While Falling this is the distance to the ground</summary>
        private float DistanceToGround;

        /// <summary> Normalized Value of the Height </summary>
        float FallBlend;
        private Vector3 UpImpulse;
        private MSpeed FallSpeed = MSpeed.Default;

        public Vector3 FallPoint { get; private set; }

        private int fallHits;

        public override bool TryActivate()
        {
            float SprintMultiplier = (animal.CurrentSpeedModifier.Vertical);

            var fall_Pivot = animal.Main_Pivot_Point + (animal.Forward * SprintMultiplier * FallRayForward * animal.ScaleFactor); //Calculate ahead the falling ray

            fall_Pivot += animal.DeltaPos; //Check for the Next Frame

            float Multiplier = animal.Pivot_Multiplier * fallRayMultiplier;

            return TryFallSphereCastNonAlloc(fall_Pivot, Multiplier);
        }


        private bool TryFallSphereCastNonAlloc(Vector3 fall_Pivot, float Multiplier)
        {
            if (animal.RayCastRadius <= 0)
                fallHits = Physics.RaycastNonAlloc(fall_Pivot, -transform.up, FallHits, Multiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore);
            else
                fallHits = Physics.SphereCastNonAlloc(fall_Pivot, animal.RayCastRadius * animal.ScaleFactor, -transform.up, FallHits, Multiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore);

            if (fallHits > 0)
            {
                FallRayCast = FallHits[0];

                DistanceToGround = FallRayCast.distance;

                if (debug)
                {
                    Debug.DrawRay(fall_Pivot, -transform.up * Multiplier, Color.magenta);
                    MalbersTools.DebugPlane(FallRayCast.point, animal.RayCastRadius * animal.ScaleFactor, Color.magenta, true);
                }

                if (!animal.Grounded)
                {
                    var TerrainSlope = Vector3.Angle(FallRayCast.normal, animal.UpVector);
                    var DeepSlope = TerrainSlope > animal.maxAngleSlope;

                    if (DeepSlope)
                    {
                        if (debug && animal.debugStates) Debug.Log("Try <B>Fall</B> State: The Animal is on Air but angle SLOPE of the ground found is too Deep");
                        return true;
                    }
                }
                else if (animal.Grounded && animal.DeepSlope)     //if wefound something but there's a deep slope
                {
                    if (debug && animal.debugStates) Debug.Log("Try <B>Fall</B> State: The Ground angle SLOPE is too Deep");
                    return true;
                }

                if (animal.Height >= DistanceToGround && !animal.Grounded) //If the distance to ground is very small means that we are very close to the ground
                {
                    if (debug && animal.debugStates) Debug.Log("Try <B>Fall</B> State: The distance to ground is very small means that we are very close to the ground. Ground = true");
                   // animal.Grounded = true;     //This Allow Locomotion and Idle to Try Activate themselves
                    return false;
                }
            }
            else
            {
                if (debug && animal.debugStates) Debug.Log("Try <B>Fall</B>: There's no Ground beneath the Animal");
                return true;
            }

            return false;
        }


        public override void ExitState()
        {
            base.ExitState();
            MaxHeight = float.NegativeInfinity;             //Resets MaxHeight
            DistanceToGround = float.PositiveInfinity;
            FallBlend = 1;
            animal.UpdateDirectionSpeed = true; //Reset the Rotate Direction
            animal.CustomSpeed = false;
            animal.SetFloatID(0); //Reset the float ID




            if (AffectStat.ID != null)
            {
                if (FallCurrentDistance < FallMinDistance.Value) return; //Meaning if we are on the safe minimun distance we do not get damage from falling

                AffectStat.Value = (FallCurrentDistance) * 100 / FallMaxDistance;

                AffectStat.ModifyStat(animal.GetComponent<Stats>());
            }
        }

        public override void Activate()
        {
            ResetValues();
            if (BlendFall == FallBlending.DistanceNormalized) animal.SetFloatID(1f);
            FallBlend = 1;
            LandTagHash = Animator.StringToHash(LandTag);
            FallCurrentDistance = 0;

            AffectStat.Value = 0;
            base.Activate();
        }

        public override void AnimationStateEnter()
        {
            if (CurrentAnimTag == AnimTag.FallEdge)
            {
             //   Debug.Log("Enter Fall Edge Animation");
                IgnoreLowerStates = true;
            }
            else if (InMainTagHash)
            {
              //  Debug.Log("Enter Fall");

                UpImpulse = MalbersTools.CleanUpVector(animal.DeltaPos, animal.Forward, animal.Right);   //Clean the Vector from Forward and Horizontal Influence    

                IgnoreLowerStates = false;

                FallSpeed = new MSpeed(animal.CurrentSpeedModifier)
                {
                    name = "FallSpeed",
                    position = animal.HorizontalSpeed,
                    animator = 1,
                    rotation = AirRotation.Value
                };

                if (debug && animal.debugStates)
                    Debug.Log($"Fall Speed: <B>{FallSpeed.position.Value.ToString("F3")}</B>");

               
                //bool GoingUP = Vector3.Dot(animal.DeltaPos, animal.GravityDirection) < 0; //Check if is falling down
                //if (GoingUP)    
                //{
                //    FallCurrentDistance =  -UpImpulse.magnitude/animal.DeltaTime + (animal.GravityForce * animal.GravityMultiplier * animal.DeltaTime);
                //    animal.SetFloatID(FallCurrentDistance);
                //}

                animal.UpdateDirectionSpeed = AirControl;
                animal.SetCustomSpeed(FallSpeed, true);
            }

           
        }

        public override void PendingExitState()
        {
            if (CurrentAnimTag == LandTagHash)
            {
                if (FallMaxDistance.Value > 0 && BlendFall == FallBlending.DistanceNormalized)
                {
                    var floatid = Mathf.Clamp(FallCurrentDistance / FallMaxDistance.Value, 0f, 1f);
                    animal.SetFloatID(floatid);
                }

            }
        }
 

        public override void OnStateMove(float deltaTime)
        {
           if (InMainTagHash)
            {
                animal.AdditivePosition += UpImpulse;

                //Change the Speed to the maximum Speed when AirMovement is enable
                if (AirControl && AirMovement > 0 && AirMovement > CurrentSpeedPos)
                    CurrentSpeedPos = Mathf.Lerp(CurrentSpeedPos, AirMovement, deltaTime * airSmooth);
            }
        }
         

        
        public override void TryExitState(float DeltaTime)
        {
            // Debug.Log("Try Exit Fall");

            bool GoingDown = Vector3.Dot(animal.DeltaPos, animal.GravityDirection) > 0; //Check if is falling down
            
            if (GoingDown) FallCurrentDistance += Vector3.Project(animal.DeltaPos, animal.GravityDirection).magnitude;
           

            FallPoint = animal.Main_Pivot_Point + animal.AdditivePosition;

            if (animal.debugGizmos && debug)
            {
                // MalbersTools.DebugTriangle(animal.Main_Pivot_Point, animal.RayCastRadius * animal.ScaleFactor, Color.magenta);
                MalbersTools.DebugTriangle(FallPoint, animal.RayCastRadius * animal.ScaleFactor, Color.magenta);
                Debug.DrawRay(FallPoint, animal.GravityDirection * 100f, Color.magenta);
            }

            //int hits = Physics.SphereCastNonAlloc(FallPoint, animal.RayCastRadius * animal.ScaleFactor,animal.GravityDirection, FallHits, 100f, animal.GroundLayer, QueryTriggerInteraction.Ignore);
            //if (hits > 0)
            if (Physics.SphereCast(FallPoint, animal.RayCastRadius, animal.GravityDirection, out FallRayCast, 100f, animal.GroundLayer, QueryTriggerInteraction.Ignore))
            {
                DistanceToGround = FallRayCast.distance - (/*animal.AdditivePosition.magnitude +*/ animal.DeltaPos.magnitude);

                if (animal.debugGizmos && debug)
                {
                    MalbersTools.DebugTriangle(FallRayCast.point, animal.RayCastRadius * animal.ScaleFactor, Color.magenta);
                }


                switch (BlendFall)
                {
                    case FallBlending.DistanceNormalized:
                        {
                            if (MaxHeight < DistanceToGround)
                                MaxHeight = DistanceToGround; //get the Highest Distance the first time you touch the ground

                            FallBlend = Mathf.Lerp(FallBlend, (DistanceToGround - LowerBlendDistance) / (MaxHeight - LowerBlendDistance), DeltaTime * 20); //Small blend in case there's a new ground found
                            animal.SetFloatID(FallBlend); //Blend between High and Low Fall
                        }
                        break;
                    case FallBlending.Distance:
                        animal.SetFloatID(FallCurrentDistance);
                        break;
                    case FallBlending.VerticalVelocity:

                        var UpInertia = MalbersTools.CleanUpVector(animal.DeltaPos, animal.Forward, animal.Right).magnitude;   //Clean the Vector from Forward and Horizontal Influence    

                        animal.SetFloatID(UpInertia/animal.DeltaTime * (GoingDown?1:-1));
                        break;
                    default:
                        break;
                }

                //Debug.Log(DistanceToGround);

                if (animal.Height >= DistanceToGround)
                {
                    var TerrainSlope = Vector3.Angle(FallRayCast.normal, animal.UpVector);
                    var DeepSlope = TerrainSlope > animal.maxAngleSlope;
                    
                    if (!DeepSlope)
                    {
                        animal.AlingRayCasting(); //Check one time the Align Rays to calculate the Angle Slope used on the CanFallOnSlope
                        AllowExit();
                        animal.Grounded = true;             //This Allow Locomotion and Idle to Try Activate themselves
                        animal.UseGravity = false;
                    }
                }
            }
        }


        public override void InitializeState() { ResetValues(); }

        void ResetValues()
        {
            MaxHeight = float.NegativeInfinity; //Resets MaxHeight
            DistanceToGround = float.PositiveInfinity;
            FallSpeed = new MSpeed();
            FallBlend = 1;
            FallRayCast = new RaycastHit();
            UpImpulse = Vector3.zero;
            AffectStat.Value = 0;
        }

#if UNITY_EDITOR

        public override void DebugState()
        {
            Gizmos.color = Color.magenta;
           // Gizmos.DrawWireSphere(FallPoint,animal.RayCastRadius);
            Gizmos.DrawWireSphere(animal.Main_Pivot_Point, animal.RayCastRadius);
        }

        /// <summary>This is Executed when the Asset is created for the first time </summary>
        void Reset()
        {
            ID = MalbersTools.GetInstance<StateID>("Fall");
            General = new AnimalModifier()
            {
                RootMotion = false,
                AdditivePosition = true,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                Colliders = true,
                Gravity = true,
                CustomRotation = false,
                modify = (modifier)(-1),
            };

            LowerBlendDistance = 0.1f;
            FallRayForward = 0.1f;
            fallRayMultiplier = 1f;

            FallSpeed.name = "FallSpeed";

            ExitFrame = false; //IMPORTANT
        }
#endif
    }
}