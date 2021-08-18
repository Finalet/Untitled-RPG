 
using System.Collections.Generic;
using UnityEngine; 
using MalbersAnimations.Scriptables; 

namespace MalbersAnimations.Controller
{
    public class Jump : State
    {
        public override string StateName => "Jump/Root Motion Jump";
        

        /// <summary>If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing</summary>
        [Header("Jump Parameters")]
        [Tooltip("If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing")]
        public BoolReference JumpPressed;
        [Tooltip("Check if the Animal can control the Forward Movement of the Jump")]
        public BoolReference ForwardPressed;

        /// <summary>If the Forward input is pressed, the Animal will keep going Forward while the Jump Animation is Playing</summary>
        //[Tooltip("If the Forward input is pressed, the Animal will keep going Forward while the Jump Animation is Playing")]
        //public bool JumpForwardPressed;
        public float JumpPressedLerp = 5;
        private float JumpPressHeight_Value = 1;
        private float JumpPressForward = 1;
        private float JumpPressForwardAdditive = 0;

        //private float JumpPressForward_Value = 1;
        public BoolReference AirControl = new BoolReference(true);
        //private float JumpPressForward_Value = 1;
        public FloatReference AirRotation = new FloatReference(10);
        public List<JumpProfile> jumpProfiles = new List<JumpProfile>();
        protected MSpeed JumpSpeed;

        protected bool OneCastingFall_Ray = false;

        /// <summary> Current Jump Profile</summary>
        protected JumpProfile activeJump;
        private RaycastHit JumpRay;

        private bool CanJumpAgain;
        private Vector3 JumpStartDirection;

        public override bool TryActivate() => InputValue && CanJumpAgain;

        public override void ResetStateValues()
        {
            CanJumpAgain = true;
            JumpPressHeight_Value = 1;
            JumpPressForward = 1;
            JumpPressForwardAdditive = 0;
            OneCastingFall_Ray = 
            GoingDown = false;
        }

        public override void AwakeState()
        {
            if (string.IsNullOrEmpty(EnterTag)) EnterTag.Value = "JumpStart";
            if (string.IsNullOrEmpty(ExitTag)) ExitTag.Value = "JumpEnd";

            base.AwakeState();
        } 


        public override void Activate()
        {
            base.Activate();
            
            IgnoreLowerStates = true;                   //Make sure while you are on Jump State above the list cannot check for Trying to activate State below him
            IsPersistent = true;                 //IMPORTANT!!!!! DO NOT ELIMINATE!!!!!

            animal.currentSpeedModifier.animator = 1;
            General.CustomRotation = true;
            CanJumpAgain = false;


            activeJump = jumpProfiles != null ? jumpProfiles[0] : new JumpProfile();
           
            foreach (var jump in jumpProfiles)                          //Save/Search the Current Jump Profile by the Lowest Speed available
            {
                if (jump.LastState == null)
                {
                    if (jump.VerticalSpeed <= animal.VerticalSmooth)
                    {
                        activeJump = jump;
                    }
                }
                else
                {
                    if (jump.VerticalSpeed <= animal.VerticalSmooth && jump.LastState == animal.LastState.ID)
                    {
                        activeJump = jump;
                    }
                }
            }

            Debugging($"Jump Profile: <B>[{ activeJump.name}]</B>");

        }

        public override void EnterTagAnimation()
        {
            if (CurrentAnimTag == EnterTagHash)
            {
                Debugging($"[EnterTag - {EnterTag.Value}]");

                if (!animal.RootMotion)
                {
                    var JumpStartSpeed = new MSpeed(animal.CurrentSpeedModifier)
                    {
                        name = "JumpStartSpeed",
                        position = animal.HorizontalSpeed,
                        animator = 1,
                        rotation = AirRotation.Value,
                    };

                    animal.SetCustomSpeed(JumpStartSpeed,true);       //Set the Current Speed to the Jump Speed Modifier

                }

                JumpStartDirection = animal.Forward;

                if (animal.TerrainSlope > 0) //Means we are jumping uphill
                    animal.UseCustomAlign = true;
            }
            else if (CurrentAnimTag == ExitTagHash)
            {
                Debugging($"[EnterTag - {ExitTag.Value}] - Allow Exit");
                AllowExit();
            }
        }

        public override Vector3 Speed_Direction()
        {
            if (animal.HasExternalForce)
            {
                return Vector3.ProjectOnPlane(animal.ExternalForce, animal.UpVector);
            }
            else if (AirControl)
            {
                return base.Speed_Direction();
            }
            else
            {
                return JumpStartDirection;
            }
        }


        /// <summary> Make the Jump Start</summary>
        public override void EnterCoreAnimation()
        {
            Debugging($"[Enter Core Tag - [Jump]");

            OneCastingFall_Ray = false;                                 //Reset Values IMPROTANT
            JumpPressHeight_Value = 1;
            JumpPressForward = 1;
            JumpPressForwardAdditive = 0;
            IsPersistent = true;
            animal.UseGravity = false;
            animal.ResetGravityValues();

            JumpSpeed = new MSpeed(animal.CurrentSpeedModifier) //Inherit the Vertical and the Lerps
            {
                name = "JumpSpeed " + activeJump.name,
                position = animal.RootMotion ? 0 : animal.HorizontalSpeed * activeJump.ForwardMultiplier, //Inherit the Horizontal Speed you have from the last state
                animator = 1, 
                rotation = (AirRotation.Value) ,
                lerpPosAnim = JumpPressedLerp,
                lerpPosition= JumpPressedLerp,
            };


            animal.SetCustomSpeed(JumpSpeed);       //Set the Current Speed to the Jump Speed Modifier
            JumpStartDirection = animal.Forward;

            if (animal.TerrainSlope > 0)    //Means we are jumping uphill HACK
                animal.UseCustomAlign = true;
        }

        public override void OnStateMove(float deltaTime)
        {
            if (InCoreAnimation)
            {
                if (activeJump.JumpLandDistance == 0) return; //Meaning is a false Jump Like neigh on the Horse IMPORTANT!!!!

                if (JumpPressed)
                {
                    JumpPressHeight_Value = Mathf.MoveTowards(JumpPressHeight_Value, InputValue ? 1 : 0, deltaTime * JumpPressedLerp);
                }

                if (ForwardPressed)
                {
                    JumpPressForward = Mathf.MoveTowards(JumpPressForward, animal.MovementAxis.z, deltaTime * JumpPressedLerp); 
                    JumpPressForwardAdditive = Mathf.MoveTowards(JumpPressForwardAdditive, animal.MovementAxis.z, deltaTime * JumpPressedLerp); 
                }

                if (!General.RootMotion) //If the Jump is NOT Root Motion!!
                {
                    Vector3 ExtraJumpHeight = (animal.UpVector * activeJump.HeightMultiplier);
                    animal.AdditivePosition += ExtraJumpHeight * deltaTime * JumpPressHeight_Value;
                }
                else //If the Jump IS Root Motion!! ***********************************************************************
                {
                    Vector3 RootMotionUP = Vector3.Project(Anim.deltaPosition, animal.UpVector);         //Get the Up vector of the Root Motion Animation

                    bool isGoingUp = Vector3.Dot(RootMotionUP, animal.Up) > 0;  //Check if the Jump Root Animation is going  UP;

                    if (isGoingUp)
                    {
                        animal.AdditivePosition -= RootMotionUP;                                     //Remove the default Root Motion Jump
                        animal.AdditivePosition += 
                            (RootMotionUP * activeJump.HeightMultiplier * JumpPressHeight_Value);    //Add the New Root Motion Jump scaled by the Height Multiplier 
                    }

                  // Vector3 RootMotionForward = Vector3.ProjectOnPlane(Anim.deltaPosition, animal.Up);
                    Vector3 RootMotionForward = Anim.deltaPosition - RootMotionUP;

                    animal.AdditivePosition -= RootMotionForward;                                                             //Remove the default Root Motion Jump

                    if (!AirControl.Value) //If is cannot controlled in air?
                    {
                        //Add the New Root Motion Jump scaled by the Height Multiplier 
                        animal.AdditivePosition += 
                            JumpStartDirection * RootMotionForward.magnitude * activeJump.ForwardMultiplier;
                      
                    }
                    else
                    animal.AdditivePosition += 
                            (RootMotionForward * activeJump.ForwardMultiplier  * JumpPressForward ) 
                            + (animal.Forward * activeJump.ForwardPressed * JumpPressForwardAdditive*deltaTime);      //Add the New Root Motion Jump scaled by the Height Multiplier 
                }
            }

                //if (OneCastingFall_Ray && animal.StateTime >= activeJump.fallingTime) //Meaning it can complete the Land animation
                //{
                //    if (Physics.Raycast(animal.Main_Pivot_Point, animal.Gravity, out RaycastHit FallRayCast, JumpRay.distance, animal.GroundLayer, IgnoreTrigger))
                //    {
                //        if (debug)
                //            Debug.DrawRay(animal.Main_Pivot_Point, animal.Gravity * JumpRay.distance, Color.red, 0.25f);

                //        var DistanceToGround = FallRayCast.distance;
                //        if (animal.Height > DistanceToGround)
                //        {
                //            animal.CheckIfGrounded();
                //        }
                //    }
                //}
        }

        private bool GoingDown;
     //   private bool DeltaGoingDown;


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
            if (normalizedTime > 0.33f) //Need to happen the First 1/3 of the Root Jump Animation
            {
                GoingDown = Vector3.Dot(animal.DeltaPos, animal.Gravity) > 0; //Check if is falling down

                if (GoingDown)
                // if (activeJump.CliffTime.IsInRange(normalizedTime))
                {
                    var MainPivot = animal.Main_Pivot_Point;

                    var RayLength = activeJump.CliffLandDistance * animal.ScaleFactor;

                    if (debug)
                        Debug.DrawRay(MainPivot, -animal.Up * RayLength, Color.black, 0.1f);

                    if (Physics.Raycast(MainPivot, -animal.Up, out JumpRay, RayLength, animal.GroundLayer, IgnoreTrigger))
                    {
                        if (debug) MTools.DebugTriangle(JumpRay.point, 0.1f, Color.black);

                        var TerrainSlope = Vector3.Angle(JumpRay.normal, animal.UpVector);
                        var DeepSlope = TerrainSlope > animal.maxAngleSlope;

                        if (!DeepSlope)       //Jump to a jumpable cliff not an inclined one
                        {
                            Debugging("[Allow Exit] on a Cliff");
                            AllowExit();
                            animal.CheckIfGrounded();
                            GoingDown = false;

                          //  Debug.Break();
                        }
                    }
                }
            }
        }

        /// <summary>Check if the animal can change to fall state if there's no future ground to land on</summary>
        private void Check_for_Falling()
        {
            AllowExit(); //Set the Allow exit just in case
            OneCastingFall_Ray = true;

            if (activeJump.JumpLandDistance == 0)
            {
                animal.CheckIfGrounded(); //We are still on the ground
                return;  //Meaning that is a False Jump (like Neigh on the Horse)
            }

            float RayLength = animal.ScaleFactor * activeJump.JumpLandDistance; //Ray Distance with the Scale Factor
            var MainPivot = animal.Main_Pivot_Point;
            var Direction = -animal.Up;


            if (activeJump.JumpLandDistance > 0) //greater than 0 it can complete the Jump on an even Ground 
            {
                if (debug)
                    Debug.DrawRay(MainPivot, Direction * RayLength, Color.red, 0.25f);

                if (Physics.Raycast(MainPivot, Direction, out JumpRay, RayLength, animal.GroundLayer, IgnoreTrigger))
                {
                    Debugging($"Min Distance to complete <B>[{ activeJump.name}]</B> -> { JumpRay.distance:F4}");
                    if (debug) MTools.DebugTriangle(JumpRay.point, 0.1f, Color.yellow);

                    var GroundSlope = Vector3.Angle(JumpRay.normal, animal.UpVector);
                    
                    if (GroundSlope > animal.maxAngleSlope)     //if we found something but there's a deep slope
                    {
                        Debugging($"[AllowExit] Try to Land but the Sloope was too Deep. Slope: {GroundSlope:F2}");
                        animal.UseGravity = General.Gravity;
                        return;
                    }

                    IgnoreLowerStates = true;                           //Means that it can complete the Jump Ignore Fall Locomotion and Idle
                    Debugging($"Can finish the Jump. Going to Jump End");
                }
                else
                {
                    animal.UseGravity = General.Gravity;
                    Debugging($"[Allow Exit] - <B>Jump [{activeJump.name}] </B> Go to Fall..No Ground was found");
                }
            }
        }

       


#if UNITY_EDITOR
        internal void Reset()
        {
            ID = MTools.GetInstance<StateID>("Jump");
            Input = "Jump";

            SleepFromState = new List<StateID>() { MTools.GetInstance<StateID>("Fall"), MTools.GetInstance<StateID>("Fly") };
            SleepFromMode = new List<ModeID>() { MTools.GetInstance<ModeID>("Action"), MTools.GetInstance<ModeID>("Attack1") };


            EnterTag.Value = "JumpStart";
            ExitTag.Value = "JumpEnd";

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                CustomRotation = true,
                IgnoreLowerStates = true, //IMPORTANT!
                Persistent = false,
                AdditivePosition = true,
                AdditiveRotation = true,
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
        [Tooltip("Minimal Vertical Speed to Activate this Jump")]
        public float VerticalSpeed;

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
       
        [Tooltip("Extra forward Movement to move the Animal Forward")]
        public float ForwardPressed;

        [Tooltip("Last State the animal was before making the Jump")]
        public StateID LastState;
    }
}
