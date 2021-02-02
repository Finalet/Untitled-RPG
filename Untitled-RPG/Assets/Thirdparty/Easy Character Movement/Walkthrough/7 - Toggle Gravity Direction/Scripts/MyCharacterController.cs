using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.ToggleGravity
{
    /// <summary>
    /// This shows how to extend BaseCharacterController to toggle gravity direction (Up and Down) and how
    /// to orient character against gravity direction.
    /// </summary>

    public class MyCharacterController : BaseCharacterController
    {
        /// <summary>
        /// Overrides BaseCharacterController Animate method.
        /// </summary>

        protected override void Animate()
        {
            // Add your character animator related code here...
        }

        /// <summary>
        /// Overrides BaseCharacterController HandleInput method. 
        /// </summary>

        protected override void HandleInput()
        {
            // Toggle pause / resume.
            // By default, will restore character's velocity on resume (eg: restoreVelocityOnResume = true)

            if (Input.GetKeyDown(KeyCode.P))
                pause = !pause;

            // Handle input, Here we allow horizontal movement only

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = 0.0f
            };

            jump = Input.GetButton("Jump");

            crouch = Input.GetKey(KeyCode.C);


            // If the character is not grounded (eg: jumping, falling, etc),
            // allow to toggle gravity direction

            if ( !movement.isGrounded && Input.GetKeyDown(KeyCode.E))
            {
                movement.gravity *= -1.0f;
                movement.DisableGrounding();
            }
        }

        /// <summary>
        /// Overrides BaseCharacterController HandleInput method. 
        /// </summary>

        protected override void UpdateRotation()
        {
            // Rotate towards movement direction (input)

            RotateTowardsMoveDirection();

            // Orient character against gravity direction

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -movement.gravity) * movement.rotation;

            movement.rotation = Quaternion.Slerp(movement.rotation, targetRotation, 5.0f * Time.deltaTime);
        }

        /// <summary>
        /// Overrides BaseCharacterController Awake method. 
        /// </summary>

        public override void Awake()
        {
            // Initialize BaseCharacterController

            base.Awake();

            // Disable movement in Z to force 2D motion (X / Y) only.
            
            var rb = GetComponent<Rigidbody>();
            if (rb)
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        }
    }
}
