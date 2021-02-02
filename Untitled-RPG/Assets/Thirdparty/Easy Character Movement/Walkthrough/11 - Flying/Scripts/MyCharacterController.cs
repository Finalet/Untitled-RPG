using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.Flying
{
    /// <summary>
    /// Example of a custom character controller.
    ///
    /// This show how to create a custom character controller extending one of the included 'Base' controller.
    /// In this example, we add fly mechanics on top of default ECM functionality.
    /// </summary>

    public class MyCharacterController : BaseCharacterController
    {
        // Is the character Flying?

        private  bool isFlying { get; set; }

        /// <summary>
        /// Handles the logic needed to enter flaying state.
        /// </summary>

        private void EnterFlyState()
        {
            // If already flying, return

            if (isFlying)
                return;

            // If grounded, return

            if (movement.isGrounded)
                return;

            // If jump button not pressed, or still not released, return

            if (!_jump || !_canJump)
                return;

            // Enter flying state

            allowVerticalMovement = true;
            airControl = 1.0f;

            isFlying = true;
        }

        /// <summary>
        /// Handles the logic to leave flying state.
        /// </summary>

        private void ExitFlyState()
        {
            // If not flying, return

            if (!isFlying)
                return;

            // On Landing, cancel flying state
            
            if (!movement.wasGrounded && movement.isGrounded)
            {
                allowVerticalMovement = false;
                airControl = 0.2f;

                isFlying = false;
            }
        }

        protected override void Animate()
        {
            // Add animator related code here...
        }

        protected override void HandleInput()
        {
            // Default ECM Input as used in BaseCharacterController HandleInput method.
            // Replace this with your custom input code here...

            if (Input.GetKeyDown(KeyCode.P))
                pause = !pause;

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };

            jump = Input.GetButton("Jump");

            crouch = Input.GetKey(KeyCode.C);

            // Flying input

            if (isFlying && Input.GetButton("Jump"))
                moveDirection += transform.up;
            else if (isFlying && Input.GetKey(KeyCode.E))
                moveDirection -= transform.up;
        }

        /// <summary>
        /// Override the BaseCharacterMovement Move method in order to add support for flying state.
        /// </summary>

        protected override void Move()
        {
            // Move with acceleration and friction

            var desiredVelocity = CalcDesiredVelocity();

            var currentFriction = isGrounded || isFlying ? groundFriction : airFriction;
            var currentBrakingFriction = useBrakingFriction ? brakingFriction : currentFriction;

            movement.Move(desiredVelocity, speed, acceleration, deceleration, currentFriction, currentBrakingFriction, !allowVerticalMovement);
            
            // Jump logic
            
            Jump();
            UpdateJumpTimer();

            // Fly logic

            EnterFlyState();
            ExitFlyState();
        }
    }
}
