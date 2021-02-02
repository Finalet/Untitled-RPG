using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.Swimming
{
    /// <summary>
    /// Example of a custom character controller.
    ///
    /// This show how to create a custom character controller extending one of the included 'Base' controller.
    /// In this example, we add swim mechanics on top of default ECM functionality.
    /// </summary>
    
    public class MyCharacterController : BaseCharacterController
    {
        [Space(20f)] public float swimSpeed = 2.0f;

        public float swimAcceleration = 10.0f;
        public float swimDeceleration = 5.0f;
        public float swimFriction = 1.0f;

        public LayerMask waterMask = 1;

        // Is the character swimming?

        public bool isSwimming { get; set; }

        /// <summary>
        /// Handle water zone enter / exit.
        /// </summary>

        private void HandleWaterZone()
        {
            // Water zone (enter / exit)

            var characterPosition = transform.position;
            var characterRotation = transform.rotation;

            // Perform an overlap check in order to detect any water zone

            int overlapCount;
            var overlappedColliders = movement.OverlapCapsule(characterPosition, characterRotation,
                out overlapCount, waterMask, QueryTriggerInteraction.Collide);

            if (overlapCount <= 0)
                return;

            // Will check if the capsule is inside the water zone (eg: the point on top of the capsule).

            var swimmingReferencePoint = characterPosition + characterRotation * Vector3.up * standingHeight;
            Vector3 closestPoint = overlappedColliders[0].ClosestPoint(swimmingReferencePoint);
            
            if (closestPoint == swimmingReferencePoint)
            {
                // If not swimming and capsule is inside water zone (eg: referencePoint (top of capsule) is below water plane), enter swimming state

                if (!isSwimming)
                {
                    isSwimming = true;
                    allowVerticalMovement = true;

                    movement.DisableGroundDetection();
                    movement.cachedRigidbody.drag = 2.5f;
                }
            }
            else
            {
                // If swimming, and our reference point (top of capsule) is out of water,
                // exit swimming state

                if (isSwimming)
                {
                    isSwimming = false;
                    allowVerticalMovement = false;

                    movement.EnableGroundDetection();
                    movement.cachedRigidbody.drag = 0.0f;

                    // If requested and can jump, perform jump

                    if (_jump && _canJump)
                    {
                        // Halt jump until jump button is released

                        _canJump = false;

                        movement.DisableGrounding();
                        movement.ApplyVerticalImpulse(jumpImpulse);
                    }
                }
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

            // Swimming Input

            if (isSwimming && Input.GetButton("Jump"))
                moveDirection += transform.up;
            else if (isSwimming && Input.GetKey(KeyCode.E))
                moveDirection -= transform.up;
        }

        /// <summary>
        /// Override CalcDesiredVelocity in order to add swim state support.
        /// </summary>
        protected override Vector3 CalcDesiredVelocity()
        {
            return isSwimming ? moveDirection * swimSpeed : moveDirection * speed;
        }

        /// <summary>
        /// Extend default Move method in order to add swim mechanics on top of ECM default movement.
        /// </summary>
        protected override void Move()
        {
            // Water zone (enter / exit)

            HandleWaterZone();

            // Movement

            var desiredVelocity = CalcDesiredVelocity();

            if (isSwimming)
            {
                // Swimming state movement

                movement.Move(desiredVelocity, swimSpeed, swimAcceleration, swimDeceleration, swimFriction,
                    swimFriction, !allowVerticalMovement);
            }
            else
            {
                // Default state movement

                var currentFriction = isGrounded ? groundFriction : airFriction;
                var currentBrakingFriction = useBrakingFriction ? brakingFriction : currentFriction;

                movement.Move(desiredVelocity, speed, acceleration, deceleration, currentFriction,
                    currentBrakingFriction, !allowVerticalMovement);

                // Jump logic

                Jump();
                MidAirJump();
                UpdateJumpTimer();
            }
        }

        protected override void UpdateRotation()
        {
            // Rotate towards movement direction (input)

            var rotationSpeed = isSwimming ? angularSpeed * 0.5f : angularSpeed;

            movement.Rotate(moveDirection, rotationSpeed);
        }
    }
}