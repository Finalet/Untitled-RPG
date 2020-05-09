using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller
{
    public class JumpBasic : State
    {
        [Header("Jump Parameters")]
        /// <summary>If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing</summary>
        [Tooltip("If the Jump input is pressed, the Animal will keep going Up while the Jump Animation is Playing")]
        public BoolReference JumpPressed = new BoolReference(false);
        [Tooltip("Can the Animal be Rotated while Jumping?")]
        public BoolReference AirControl = new BoolReference(true);
        [Tooltip("Smooth Value for Changing Speed Movement on the Air")]
        public FloatReference AirSmooth = new FloatReference(5);
        [Tooltip("How much Rotation the Animal can do while Jumping")]
        public FloatReference AirRotation = new FloatReference(10);
        [Tooltip("How much Movement the Animal can do while Jumping")]
        public FloatReference AirMovement = new FloatReference(5);
        [Tooltip("How High the animal can Jump")]
        public FloatReference Height = new FloatReference(10);

        [Tooltip("Amount of jumps the Animal can do (Double and Triple Jumps)")]
        public IntReference JumpAmount = new IntReference(1);
        private int JumpsPerformanced = 0;

        [Tooltip("How Long the Jump State will last depending on the Jump Animation Normalized Time")]
        [Range(0, 1)]
        public float ExitTime = 0.5f;

        protected MSpeed JumpSpeed;
      //  private bool CanJumpAgain;
        private float JumpPressHeight_Value = 1;

        public override void StatebyInput()
        {
            if (InputValue && /*CanJumpAgain*/ JumpsPerformanced <JumpAmount)
            {
                Activate();
                JumpsPerformanced++;
               // CanJumpAgain = false;
            }
        }

        public override void Activate()
        {
            base.Activate();
            IgnoreLowerStates = true;                   //Make sure while you are on Jump State above the list cannot check for Trying to activate State below him
            animal.currentSpeedModifier.animator = 1;

            IsPersistent = true;                 //IMPORTANT!!!!! DO NOT ELIMINATE!!!!! 
        }


        public override void AnimationStateEnter()
        {
            if (InMainTagHash)
            {
                JumpPressHeight_Value = 1;
                JumpSpeed = new MSpeed(animal.CurrentSpeedModifier) //Inherit the Vertical and the Lerps
                {
                    name = "Jump Basic Speed",
                    position = animal.HorizontalSpeed, //Inherit the Horizontal Speed you have from the last state
                    animator = 1,
                    lerpPosition = AirSmooth,
                    rotation = AirRotation
                };

                animal.UpdateDirectionSpeed = AirControl;
                if (debug) Debug.Log($" Basic JumpSpeed: {JumpSpeed.position.Value}");
                animal.SetCustomSpeed(JumpSpeed,true);       //Set the Current Speed to the Jump Speed Modifier
            }
        }

        public override void OnStateMove(float deltaTime)
        {
            if (InMainTagHash)
            {
                if (JumpPressed)
                {
                    JumpPressHeight_Value = Mathf.Lerp(JumpPressHeight_Value, InputValue ? 1 : 0, deltaTime * AirSmooth);
                }

                Vector3 ExtraJumpHeight = (animal.UpVector * Height);
                animal.AdditivePosition += ExtraJumpHeight * deltaTime * JumpPressHeight_Value;


                if (AirMovement > CurrentSpeedPos && AirControl)
                    CurrentSpeedPos = Mathf.Lerp(CurrentSpeedPos, AirMovement, deltaTime * AirSmooth);
            }
        }

        public override void TryExitState(float DeltaTime)
        {
            if (animal.AnimState.normalizedTime >= ExitTime)
            {
                IgnoreLowerStates = false;
                IsPersistent = false;
            }
        }


        /// <summary>Is called when a new State enters</summary>
        public override void NewActiveState(StateID newState)
        {
            if (newState == StateEnum.Idle ||newState == StateEnum.Locomotion)
            {
                JumpsPerformanced = 0;          //Reset the amount of jumps performanced
            }

            if (newState == StateEnum.Fall && animal.LastState.ID != ID) //If we were not jumoung then increase the Double Jump factor
            {
                JumpsPerformanced++; //If we are in fall animation then increase a Jump perfomanced
            }
        }

        public override void ResetState()
        {
            //CanJumpAgain = true;
            JumpPressHeight_Value = 1;
        }

        public override void ExitState()
        {
            base.ExitState();
          //  CanJumpAgain = true;
            JumpPressHeight_Value = 1;
            animal.UpdateDirectionSpeed = true; //Reset the Rotate Direction
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
                RootMotion = false,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                CustomRotation = false,
                IgnoreLowerStates = true, //IMPORTANT!
                Persistent = false,
                AdditivePosition = true,
                Colliders = true,
                Gravity = true,
                modify = (modifier)(-1),
            };

            ExitFrame = false;
        }
#endif
    }
}
