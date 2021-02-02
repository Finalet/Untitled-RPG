using ECM.Common;
using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.FallDamage
{
    /// <summary>
    /// Custom character controller.
    /// This shows how to extend the Move method in order to add custom game mechanics on top of default ECM functionality.
    /// </summary>

    public class MyCharacterController : BaseCharacterController
    {
        private Vector3 _groundedPosition;

        protected override void Animate()
        {
            // Add animator related code here...
        }

        protected override void HandleInput()
        {
            // Toggle pause / resume.
            // By default, will restore character's velocity on resume (eg: restoreVelocityOnResume = true)

            if (Input.GetKeyDown(KeyCode.P))
                pause = !pause;

            // Handle user input

            jump = Input.GetButton("Jump");

            crouch = Input.GetKey(KeyCode.C);

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };

            // Transform the given moveDirection to be relative to the main camera's view direction.
            // Here we use the included extension .relativeTo...

            var mainCamera = Camera.main;
            if (mainCamera != null)
                moveDirection = moveDirection.relativeTo(mainCamera.transform);
        }

        /// <summary>
        /// Override Move method.
        /// This allows to extend the BaseCharacterMovement Move method, to add custom game mechanics on top of ECM Default movement code.
        /// </summary>

        protected override void Move()
        {
            // Call ECM Default Move method (movement, jump, etc)

            base.Move();

            // If character just landed...

            if (!movement.wasGrounded && movement.isGrounded)
            {
                // Compute fallen vertical distance

                var fallenDistance = Vector3.Dot(transform.position - _groundedPosition, transform.up);
                if (fallenDistance < 0.0f)
                {
                    // if your fallen distance is grater than a given 'safe fall distance' apply fall damage!

                    Debug.Log("fallenDistance: " + fallenDistance);
                }
            }

            // If character is grounded, cache its current grounded position

            if (movement.isGrounded)
                _groundedPosition = transform.position;

        }
    }
}