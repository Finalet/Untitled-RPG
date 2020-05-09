using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller
{
    public class Jump : State
    {
        /// <summary>If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing</summary>
        [Header("Jump Parameters")]
        [Tooltip("If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing")]
        public bool JumpPressed;
        /// <summary>If the Forward input is pressed, the Animal will keep going Forward while the Jump Animation is Playing</summary>
        //[Tooltip("If the Forward input is pressed, the Animal will keep going Forward while the Jump Animation is Playing")]
        //public bool JumpForwardPressed;
        public float JumpPressedLerp = 5;
        private float JumpPressHeight_Value = 1;
        //private float JumpPressForward_Value = 1;
        public FloatReference AirRotation = new FloatReference(10);
        //private float JumpPressForward_Value = 1;
        public BoolReference AirControl = new BoolReference(true);

        public List<JumpProfile> jumpProfiles = new List<JumpProfile>();
        protected MSpeed JumpSpeed;

        protected bool OneCastingFall_Ray = false;
      
        /// <summary> Current Jump Profile</summary>
        protected JumpProfile activeJump;
        private RaycastHit JumpRay;

        private bool CanJumpAgain;
        private Vector3 JumpStartDirection;

        public override void StatebyInput()
        {
            if (InputValue && CanJumpAgain)
            {
                Activate();  //Remember*****************here to enable the double jump
                CanJumpAgain = false;
            }
        }

        public override void Activate()
        {
            base.Activate();
            IgnoreLowerStates = true;                   //Make sure while you are on Jump State above the list cannot check for Trying to activate State below him
            animal.currentSpeedModifier.animator = 1;

            activeJump = jumpProfiles != null ? jumpProfiles[0] : new JumpProfile();

            IsPersistent = true;                 //IMPORTANT!!!!! DO NOT ELIMINATE!!!!!

            foreach (var jump in jumpProfiles)                          //Save/Search the Current Jump Profile by the Lowest Speed available
            {
                if (jump.VerticalSpeed <= animal.VerticalSmooth)
                {
                    activeJump = jump;
                }
            }
        }

        public override void AnimationStateEnter()
        {
            if (CurrentAnimTag == AnimTag.JumpEnd)
            {
                IgnoreLowerStates = false;
                IsPersistent = false;
            }

            //-------------------------JUMP START--------------------------------------------
            if (InMainTagHash)  //Meaning The Jump is about to start
            {
                JumpStart();
            }
        }

        private void JumpStart()
        {
            OneCastingFall_Ray = false;                                 //Reset Values IMPROTANT
            JumpPressHeight_Value = 1;
            //JumpPressForward_Value = 1;
            IsPersistent = true;
           
            JumpSpeed = new MSpeed(animal.CurrentSpeedModifier) //Inherit the Vertical and the Lerps
            {
                name = "JumpNoRootSpeed",
                position = animal.HorizontalSpeed * activeJump.ForwardMultiplier, //Inherit the Horizontal Speed you have from the last state
                animator = 1,
                //rotation = !animal.UseCameraInput ? AirRotation : 1,
                rotation = AirControl.Value ? (!animal.UseCameraInput ? AirRotation.Value : AirRotation.Value /10f) : 0f,
            };


            if (animal.RootMotion) JumpSpeed.position = 0; //Reset it if the Animal is using RootMotion

            animal.SetCustomSpeed(JumpSpeed);       //Set the Current Speed to the Jump Speed Modifier

            JumpStartDirection = animal.Forward;
        }

        public override void OnStateMove(float deltaTime)
        {
            if (InMainTagHash)
            {
                if (activeJump.JumpLandDistance == 0) return; //Meaning is a false Jump Like neigh on the Horse IMPORTANT!!!!

                if (JumpPressed)
                {
                    JumpPressHeight_Value = Mathf.Lerp(JumpPressHeight_Value, InputValue ? 1 : 0, deltaTime * JumpPressedLerp);
                }

                if (!General.RootMotion) //If the Jump is NOT Root Motion!!
                {
                    Vector3 ExtraJumpHeight = (animal.UpVector * activeJump.HeightMultiplier);
                    animal.AdditivePosition += ExtraJumpHeight * deltaTime * JumpPressHeight_Value;
                }
                else //If the Jump IS Root Motion!!
                {
                    Vector3 RootMotionUP = Vector3.Project(Anim.deltaPosition, animal.UpVector);         //Get the Up vector of the Root Motion Animation

                    bool isGoingUp = Vector3.Dot(RootMotionUP, animal.Up) > 0;  //Check if the Jump Root Animation is going  UP;

                    if (isGoingUp)
                    {
                        animal.AdditivePosition -= RootMotionUP;                                                            //Remove the default Root Motion Jump
                        animal.AdditivePosition += RootMotionUP * activeJump.HeightMultiplier * JumpPressHeight_Value;      //Add the New Root Motion Jump scaled by the Height Multiplier 
                    }

                    Vector3 RootMotionForward = Vector3.ProjectOnPlane(Anim.deltaPosition, animal.Up);

                    animal.AdditivePosition -= RootMotionForward;                                                             //Remove the default Root Motion Jump

                    if (!AirControl.Value)
                    {
                        animal.AdditivePosition += JumpStartDirection * RootMotionForward.magnitude * activeJump.ForwardMultiplier;// * JumpPressForward_Value;      //Add the New Root Motion Jump scaled by the Height Multiplier 
                        return;
                    }


                    animal.AdditivePosition += RootMotionForward * activeJump.ForwardMultiplier;// * JumpPressForward_Value;      //Add the New Root Motion Jump scaled by the Height Multiplier 
                }
            }
        }


        public override void TryExitState(float DeltaTime)
        {
            if (animal.StateTime >= activeJump.fallingTime && !OneCastingFall_Ray)
            {
                Check_for_Falling();
            }
            Can_Jump_on_Cliff(animal.StateTime);
        }


        private void Can_Jump_on_Cliff(float normalizedTime)
        {
            if (normalizedTime >= activeJump.CliffTime.minValue && normalizedTime <= activeJump.CliffTime.maxValue)
            {
                if (debug) Debug.DrawRay(animal.Main_Pivot_Point, animal.GravityDirection * activeJump.CliffLandDistance * animal.ScaleFactor, Color.black);
             

                if (Physics.Raycast(animal.Main_Pivot_Point, animal.GravityDirection, out JumpRay, activeJump.CliffLandDistance * animal.ScaleFactor, animal.GroundLayer, QueryTriggerInteraction.Ignore))
                {
                    if (debug) MalbersTools.DebugTriangle(JumpRay.point, 0.1f, Color.black);

                    var TerrainSlope = Vector3.Angle(JumpRay.normal, animal.UpVector);
                    var DeepSlope = TerrainSlope > animal.maxAngleSlope;

                    if (!DeepSlope)       //Jump to a jumpable cliff not an inclined one
                    {
                        if (debug) Debug.Log("<B>Jump</b> State: Exit on a Cliff ");
                        animal.Grounded = true; //Force the animal to be grounded so it can find go to Locomotion or IDLE
                        IgnoreLowerStates = false;
                        IsPersistent = false;
                    }
                }
            }
        }

        /// <summary>Check if the animal can change to fall state if there's no future ground to land on</summary>
        private void Check_for_Falling()
        {
            IgnoreLowerStates = false;              //Means that it will directly to fall
            OneCastingFall_Ray = true;
            IsPersistent = false;

            if (activeJump.JumpLandDistance == 0)
            {
                animal.Grounded = true; //We are still on the ground
                return;  //Meaning that is a False Jump (like Neigh on the Horse)
            }

            float RayLength = animal.ScaleFactor * activeJump.JumpLandDistance; //Ray Distance with the Scale Factor

            if (debug)  
            Debug.Log("Doing: <b>" + activeJump.name + "</b>. RayLegth:"+ RayLength);

          


            var MainPivot = animal.Main_Pivot_Point;
            var Direction = animal.GravityDirection;


            
            if (activeJump.JumpLandDistance > 0) //greater than 0 it can complete the Jump on an even Ground 
            {
                if (debug)
                    Debug.DrawRay(MainPivot, Direction * RayLength, Color.red, 0.25f);

                if (Physics.Raycast(MainPivot, Direction, out JumpRay, RayLength, animal.GroundLayer, QueryTriggerInteraction.Ignore))
                {
                    if (debug)
                    {
                        Debug.Log("Min Distance to complete " + activeJump.name + " " + JumpRay.distance);
                        MalbersTools.DebugTriangle(JumpRay.point, 0.1f, Color.yellow);
                    }

                    var TerrainSlope = Vector3.Angle(JumpRay.normal, animal.UpVector);
                    var DeepSlope = TerrainSlope > animal.maxAngleSlope;

                    if (DeepSlope)     //if wefound something but there's a deep slope
                    {
                        if (debug) Debug.Log("Jump State: Try to Land but the Sloope is too Deep. Exiting Jump State " + TerrainSlope);
                        IgnoreLowerStates = false;
                        return;
                    }

                    IgnoreLowerStates = true;                           //Means that it can complete the Jump
                    if (debug) Debug.Log("Can finish the Jump. Going to Jump End");

                }
                else
                {
                    if (debug) Debug.Log(activeJump.name + ": Go to Fall.. No ground was found");
                    IgnoreLowerStates = false;
                }
            }
        }

        public override void ResetState()
        {
            CanJumpAgain = true;
            JumpPressHeight_Value = 1;
            //JumpPressForward_Value= 1;
        }

        public override void ExitState()
        {
            base.ExitState();
            CanJumpAgain = true;
            JumpPressHeight_Value = 1;
          //  JumpPressForward_Value = 1;
        }

        public override void JustWakeUp()
        {
            if (animal.ActiveStateID == 5) //Means is Underwater State..
            {
                IsSleepFromState = true; //Keep Sleeping if you are in Underwater
            }
        }


#if UNITY_EDITOR
        void Reset()
        {
            ID = MalbersTools.GetInstance<StateID>("Jump");
            Input = "Jump";

            SleepFromState = new List<StateID>() { MalbersTools.GetInstance<StateID>("Fall"), MalbersTools.GetInstance<StateID>("Fly") };
            SleepFromMode = new List<ModeID>() { MalbersTools.GetInstance<ModeID>("Action"), MalbersTools.GetInstance<ModeID>("Attack1") };


            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                CustomRotation = false,
                IgnoreLowerStates = true, //IMPORTANT!
                Persistent = false,
                AdditivePosition = true,
                //AdditiveRotation = true,
                Colliders = true,
                Gravity = false,
                modify = (modifier)(-1),
            };

            ExitFrame = false;

            jumpProfiles = new List<JumpProfile>()
            { new JumpProfile()
            { name = "Jump", /*stepHeight = 0.1f,*/ fallingTime = 0.7f, /* fallRay = 2, ForwardMultiplier = 1,*/  HeightMultiplier =  1, JumpLandDistance = 1.7f}
            };
        }
#endif
    }







    /// <summary>Different Jump parameters on different speeds</summary>
    [System.Serializable]
    public struct JumpProfile
    {
        /// <summary>Name to identify the Jump Profile</summary>
        public string name;

        /// <summary>Maximum Vertical Speed to Activate this Jump</summary>
        [Tooltip("Maximum Vertical Speed to Activate this Jump")]
        public float VerticalSpeed;
        /// <summary>Ray Length to check if the ground is at the same level all the time</summary>
      ////  [Header("Checking Fall")]
      //  [Tooltip("Ray Length to check if the ground is at the same level all the time")]
      //  public float fallRay;

        ///// <summary>Terrain difference to be sure the animal will fall</summary>
        //[Tooltip("Terrain difference to be sure the animal will fall ")]
        //public float stepHeight;

        /// <summary>Min Distance to Complete the Land when the Jump is on the Highest Point, this needs to be calculate manually</summary>
        [Tooltip("Min Distance to Complete the Land when the Jump is on the Highest Point")]
        public float JumpLandDistance;

        /// <summary>Animation normalized time to change to fall animation if the ray checks if the animal is falling </summary>
        [Tooltip("Animation normalized time to change to fall animation if the ray checks if the animal is falling ")]
        [Range(0, 1)]
        public float fallingTime;

        /// <summary>Range to Calcultate if we can land on Higher ground </summary>
        //[Header("Land on a Cliff")]
        [Tooltip("Range to Calcultate if we can land on Higher ground")]
        [MinMaxRange(0, 1)]
        public RangedFloat CliffTime;

        /// <summary>Maximum distance to land on a Cliff </summary>
        [Tooltip("Maximum distance to land on a Cliff ")]
        public float CliffLandDistance;


       // [Space]
        /// <summary>Height multiplier to increase/decrease the Height Jump</summary>
        public float HeightMultiplier;
        ///// <summary>Forward multiplier to increase/decrease the Forward Speed of the Jump</summary>
        public float ForwardMultiplier;

    }
}
