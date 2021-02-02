using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.CustomRotation
{
    /// <summary>
    /// Custom character controller.
    /// This shows how use the UpdateRotation method to perform a tank-like rotation.
    /// </summary>

    public class MyCharacterController : BaseCharacterController
    {
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
        }

        protected override Vector3 CalcDesiredVelocity()
        {
            // By default ECM will use the moveDirection to compute our desired velocity, however you can use this method
            // to modify this behaviour, in this case we simple move the character's toward its current view direction

            return transform.forward * speed * moveDirection.z;
        }

        protected override void UpdateRotation()
        {
            // By default an ECM character will be rotated toward its given moveDirection, however you can use this method
            // to modify it, in this example to perform a tank-like rotation.

            // Generate a rotation around character's vertical axis (Yaw rotation)

            var rotateAmount = moveDirection.x * angularSpeed * Time.deltaTime;

            // Rotate character along its y-axis (yaw rotation)

            movement.rotation *= Quaternion.Euler(0f, rotateAmount, 0f);
        }
    }
}