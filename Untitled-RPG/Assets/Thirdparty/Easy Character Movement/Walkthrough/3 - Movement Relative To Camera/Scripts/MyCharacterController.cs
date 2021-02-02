using ECM.Common;
using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.MovementRelativeToCamera
{
    /// <summary>
    /// Custom character controller. This shows how make the character move relative to MainCamera view direction.
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

            // Transform the given moveDirection to be relative to the main camera's view direction.
            // Here we use the included extension .relativeTo...

            var mainCamera = Camera.main;
            if (mainCamera != null)
                moveDirection = moveDirection.relativeTo(mainCamera.transform);
        }
    }
}
