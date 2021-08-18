using MalbersAnimations.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    /// <summary>Swim Logic</summary>
    public class Swim : State
    {
        public override string StateName => "Swim";

        [Header("Swim Paramenters")] 
        public LayerMask WaterLayer = 16;

        [Tooltip("Lerp value for the animal to stay align to the water level ")]
        public float AlignSmooth = 10;
        [Tooltip("When entering the water the Animal will sink for a while... Higher values, will return to the surface faster")]
        public float Bounce = 5;
        [Tooltip("If the Animal Enters it will wait this time to try exiting the water")]
        public float TryExitTime = 0.5f;
        protected float EnterWaterTime;
        [Tooltip("Means the Water does not change the shape. Less RayCasting")]
        public bool WaterIsStatic = true;
        [Tooltip("Gives an extra impulse when entering the state using the accumulated  inertia")]
        public bool KeepInertia = true;
        [Tooltip("Spherecast radius to find water using the Water Pivot")]
        public float m_Radius = 0.1f;

        [Tooltip("Ray to the Front to check if the Animal has touched a Front Ground")]
        public float FrontRayLength = 1;

        public bool PivotAboveWater { get; private set; }

        /// <summary>Has the animal found Water</summary>
        public bool IsInWater { get; private set; }
     
        protected MPivots WaterPivot;
        protected Vector3 WaterNormal = Vector3.up;
        protected Vector3 HorizontalInertia;

        /// <summary>Water Collider used on the Sphere Cast</summary>
        protected Collider[] WaterCollider;
        /// <summary>Point Above the Animal to Look Down and Find the Water level and Water Normal</summary>
        // private Vector3 WaterUPAnimalPos;
        private Vector3 WaterPivot_Dist_from_Water;
        private Vector3 WaterUPPivot => WaterPivotPoint + animal.DeltaVelocity + (animal.UpVector * UpMult);

        private Vector3 UpImpulse;
        const float UpMult = 30;

        public Vector3 WaterPivotPoint => WaterPivot.World(animal.transform)+animal.DeltaVelocity;


        public override void InitializeState()
        {
            WaterPivot = animal.pivots.Find(p => p.name.ToLower().Contains("water"));      //Find the Water Pivot
            if (WaterPivot == null) Debug.LogError("No Water Pivot Found.. please create a Water Pivot");

            WaterCollider = new Collider[1];
            IsInWater = false;
        }

        public override bool TryActivate()
        {
            if (animal.ActiveStateID == StateEnum.UnderWater && animal.Sprint && animal.UpDownSmooth > 0)
                return false; //If we are underwater and we are sprinting Upwards ... dont enter this state.. go to fall directly

            CheckWater();

            if (IsInWater)
            {
                var waterCol = WaterCollider[0];

                Ray WaterRay = new Ray(WaterUPPivot, animal.Gravity);

                if (waterCol.Raycast(WaterRay, out RaycastHit WaterHit, 100f))
                {
                    WaterNormal = WaterHit.normal;
                }

                 EnterWaterTime = Time.time;

                return true;
            }

            return false;
        }
        public override void Activate()
        {
            base.Activate();

            HorizontalInertia = Vector3.ProjectOnPlane(animal.DeltaPos, animal.UpVector);
            UpImpulse = Vector3.Project(animal.DeltaPos, animal.UpVector);          //Clean the Vector from Forward and Horizontal Influence    
            IgnoreLowerStates = true;                                               //Ignore Falling, Idle and Locomotion while swimming 
            animal.UseGravity = false; //IMPORTANT
            animal.InertiaPositionSpeed = Vector3.zero;                             //THIS MOTHER F!#$ER was messing with the water entering
            animal.Force_Reset();
        }


        public void CheckWater()
        {
            int WaterFound = Physics.OverlapSphereNonAlloc(WaterPivotPoint, m_Radius * animal.ScaleFactor, WaterCollider, WaterLayer);  
            IsInWater = WaterFound > 0;                 //Means the Water SphereOverlap found Water
        }

        public override void TryExitState(float DeltaTime)
        {
            if (!InExitAnimation && MTools.ElapsedTime(EnterWaterTime, TryExitTime)) //do not try to exit if the animal just enter the water
            {
                CheckWater();
                if (!IsInWater)
                {
                    Debugging("[Allow Exit] No Longer in water");
                    animal.CheckIfGrounded();
                    AllowExit();
                }
            }
        }


        public override void AllowExit()
        {
            if (CanExit)
            {
                IgnoreLowerStates = false;
                IsPersistent = false;
                IsInWater = false;
            }
        }

        public override void OnStateMove(float deltatime)
        {
            if (IsInWater && !InExitAnimation)
            {
                if (KeepInertia) animal.AddInertia(ref HorizontalInertia, 3);
                if (Bounce > 0) animal.AddInertia(ref UpImpulse, Bounce);

                var waterCol = WaterCollider[0];                                                //Get the Water Collider
                                                                                                // var WaterUPSurface = waterCol.ClosestPoint(WaterUPAnimalPos);

                if (!WaterIsStatic)                     //Means that the Water Changes shape
                {
                    Ray WaterRay = new Ray(WaterUPPivot, animal.Gravity * UpMult);

                    if (waterCol.Raycast(WaterRay, out RaycastHit WaterHit, 100f)) WaterNormal = WaterHit.normal;        //RayCast to find the Water Normal
                }

                animal.AlignRotation(WaterNormal, deltatime,  AlignSmooth > 0 ? AlignSmooth : 5);                                     //Aling the Animal to the Water 

                //Find the Water Level
                FindWaterLevel();

                //var Smoothness = 1f; //SNAP there's no Main Pivos Touching the a ground mean is on the water
                var rayColor = (Color.blue + Color.cyan) /2;

                //HACK so it does not come out of the water
                if (FrontRayLength > 0 &&
                    Physics.Raycast(WaterPivotPoint, animal.Forward, out RaycastHit FrontRayWater, FrontRayLength, animal.GroundLayer, QueryTriggerInteraction.Ignore))
                {
                    var FrontPivot = Vector3.Angle(FrontRayWater.normal, animal.UpVector);

                    rayColor = Color.cyan;

                    if (FrontPivot > animal.maxAngleSlope) //BAD Angle Slope
                    {
                        rayColor = Color.black;
                        animal.transform.position += WaterPivot_Dist_from_Water;
                        animal.ResetUPVector();
                    }
                }
                else
                {
                   if (AlignSmooth > 0)
                        animal.AdditivePosition += WaterPivot_Dist_from_Water * (deltatime * AlignSmooth);
                    else
                    {
                        animal.transform.position += WaterPivot_Dist_from_Water;
                        animal.ResetUPVector();
                    }
                }
                if (debug) Debug.DrawRay(WaterPivotPoint, animal.Forward * FrontRayLength, rayColor);
            }
        }

        public void FindWaterLevel()
        {
            if (IsInWater)
            {
                var waterCol = WaterCollider[0];
                var PivotPointDistance = waterCol.ClosestPoint(WaterUPPivot); //IMPORTANT IS NOT CLOSEST POINT ON BOUNDS THAT CAUSES ERRORS
             
                // MTools.DrawWireSphere(WaterUPPivot, Color.white, Radius, .5f);
               // Debug.Log("FindWaterLevel");

                WaterPivot_Dist_from_Water = Vector3.Project((PivotPointDistance - WaterPivotPoint), animal.UpVector);

               // MTools.DrawWireSphere(WaterPivot_Dist_from_Water, Color.yellow, Radius,.5f);
                //Debug.Break();

                PivotAboveWater = Vector3.Dot(WaterPivot_Dist_from_Water, animal.UpVector) < 0;
            }
            else
            {
                PivotAboveWater = true;
            }
        }

        public override void ResetStateValues()
        { 
            WaterCollider = new Collider[1];
            IsInWater = false;
            EnterWaterTime = 0;
        }

#if UNITY_EDITOR

        void Reset()
        {
            ID = MTools.GetInstance<StateID>("Swim");

            WaterCollider = new Collider[1];            //Set the Array to 1

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = false,
                Sprint = true,
                OrientToGround = false,
                CustomRotation = true,
                IgnoreLowerStates = true, //IMPORTANT
                AdditivePosition = true,
                AdditiveRotation = true, 
                Gravity = false,
                modify = (modifier)(-1),
                
            };
            // SpeedIndex = 0;
        }


        public override void StateGizmos(MAnimal animal)
        {
            if (Application.isPlaying)
            {
                if (IsInWater)
                {
                    var scale = animal.ScaleFactor;
                    Collider WaterCol = WaterCollider[0];

                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(WaterPivotPoint, m_Radius * scale);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(animal.transform.position, m_Radius * scale);

                    if (WaterCol)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(WaterCol.ClosestPoint(WaterUPPivot), m_Radius * scale);
                        Gizmos.DrawSphere(WaterUPPivot, m_Radius * scale);
                    }
                }
            }
        }
#endif
    }
}
