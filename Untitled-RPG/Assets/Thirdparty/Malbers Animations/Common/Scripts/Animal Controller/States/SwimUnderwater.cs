using UnityEngine;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    /// <summary>UnderWater Logic</summary>
    public class SwimUnderwater : State
    {
        public override string StateName => "UnderWater";

        [Header("UnderWater Parameters")]
        [Range(0, 90)]
        public float Bank = 30;
        [Range(0, 90),Tooltip("Limit to go Up and Down")]
        public float Ylimit = 80;
        [Tooltip("It will push the animal down into the water for a given time")]
        public float EnterWaterDrag = 10;
        [Tooltip("If the Animal Enters it will wait this time to try exiting the water")]
        public float TryExitTime = 0.5f;
        protected float EnterWaterTime;

        protected Vector3 Inertia;
        protected Swim SwimState;
         

        public override void InitializeState()
        {
            SwimState = null;
            SwimState = (Swim)animal.State_Get(StateEnum.Swim); //Get the Store the Swim State
 
            if (SwimState == null)
            {
                Debug.LogError("UnderWater State needs Swim State in order to work, please add the Swim State to the Animal");
            }
        }

        public override void Activate()
        {
            base.Activate();
            Inertia = animal.DeltaPos;
            EnterWaterTime = Time.time;
        }


        public override Vector3 Speed_Direction() => animal.FreeMovement ?  animal.PitchDirection : animal.Forward;
       
        public override bool TryActivate()
        {
           if (SwimState == null) return false;

            if (!SwimState.IsActiveState)  //If we are not already swimming we need to check is we are on water
                SwimState.CheckWater();


            if (SwimState.IsInWater)
            {
                if (animal.MovementAxisRaw.y < -0.25f) //Means that Key Down is Pressed;
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
            animal.AddInertia(ref Inertia ,EnterWaterDrag);
        }


        public override void TryExitState(float DeltaTime)
        {
            if (MTools.ElapsedTime(EnterWaterTime, TryExitTime)) //do not try to exit if the animal just enter the water
            {
                SwimState.CheckWater();
                SwimState.FindWaterLevel();

                //Debug.Log($"PivotAboveWater {SwimState.PivotAboveWater} ");
                //Debug.Log($"IsInWater {SwimState.IsInWater} ");


                if ( SwimState.PivotAboveWater ||  !SwimState.IsInWater)
                {
                    Debugging("[Allow Exit]");
                    AllowExit();
                }
            }
        }

        public override void ResetStateValues()
        {
            Inertia = Vector3.zero;
            EnterWaterTime = 0;
        }

        public override void RestoreAnimalOnExit()
        {
            animal.FreeMovement = false; //Important!!!!
        }


#if UNITY_EDITOR
        void Reset()
        {
            ID = MTools.GetInstance<StateID>("UnderWater");

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
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };
            IgnoreLowerStates = false;
        }

        public override void StateGizmos(MAnimal animal)
        {
            if (Application.isPlaying && SwimState != null && animal != null)   
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(SwimState.WaterPivotPoint, SwimState.m_Radius * animal.ScaleFactor);
            }
        }
#endif
    }
}
