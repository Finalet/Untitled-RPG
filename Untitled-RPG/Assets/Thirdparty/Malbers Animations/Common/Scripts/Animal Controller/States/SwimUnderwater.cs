using UnityEngine;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    /// <summary>UnderWater Logic</summary>
    public class SwimUnderwater : State
    {
        [Header("UnderWater Parameters")]
        [Range(0, 90)]
        public float Bank = 30;
        [Range(0, 90),Tooltip("Limit to go Up and Down")]
        public float Ylimit = 80;

        protected Vector3 Inertia;
        protected Swim SwimState;
         

        public override void InitializeState()
        {
            SwimState = null;

            SwimState = (Swim)animal.State_Get(4); //Get the Store the Swim State

            if (SwimState == null)
            {
                Debug.LogError("UnderWater State needs Swim State in order to work, please add the Swim State to the Animal");
            }
        }

        public override void Activate()
        {
            base.Activate();
            Inertia = animal.DeltaPos;
        }


        public override bool TryActivate()
        {
            if (SwimState.IsInWater)
            {
                if (animal.MovementAxis.y < -0.25f) //Means that Key Down is Pressed;
                {
                    IgnoreLowerStates = true;
                    return true;
                }
            }
            return false;
        }

         
        public override void OnStateMove(float deltatime)
        {
            animal.FreeMovementRotator(Ylimit, Bank, deltatime);
            animal.AddInertia(ref Inertia, deltatime);
        }


        public override void TryExitState(float DeltaTime)
        {
            SwimState.CheckWater();
            SwimState.FindWaterLevel();

            if (SwimState.PivotAboveWater || !SwimState.IsInWater)
                AllowExit();
        }

        public override void ExitState()
        {
            base.ExitState();
            Inertia = Vector3.zero;
            animal.FreeMovement = false; //Important!!!!
        }


#if UNITY_EDITOR
        void Reset()
        {
            ID = MalbersTools.GetInstance<StateID>("UnderWater");

            General = new AnimalModifier()
            {
                RootMotion = false,
                Grounded = false,
                Sprint = true,
                OrientToGround = false,
                CustomRotation = false,
                FreeMovement  = true,
                IgnoreLowerStates = true,  
                AdditivePosition = true,
                //AdditiveRotation = false,
                Colliders = true, 
                Gravity = false,
                modify = (modifier)(-1),
            };
            IgnoreLowerStates = false;
        }

        public override void DebugState()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(SwimState.WaterPivotPoint, SwimState.Radius*animal.ScaleFactor);
        }
#endif
    }
}
