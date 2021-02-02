using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.FPSCrouching
{
    /// <summary>
    /// Custom FPS character controller.
    /// This shows how to replace the default crouch behaviour (scale based).
    /// </summary>

    public class MyCharacterController : BaseFirstPersonController
    {
        protected override void Animate()
        {
            // Add animator related code here...
        }

        protected override void AnimateView()
        {
            // Override the AnimateView method in order to replace the default crouch 'animation' method

            // Modify camera pivot local position to simulate crouching state
 
            Vector3 targetPosition = isCrouching
                ? new Vector3(0.0f, crouchingHeight , 0.0f)
                : new Vector3(0.0f, standingHeight - 0.35f, 0.0f);
 
            cameraTransform.localPosition =
                Vector3.MoveTowards(cameraTransform.localPosition, targetPosition, 5.0f * Time.deltaTime);
        }

        protected override void HandleInput()
        {
            // As with BaseCharacterController, you can use the HandleInput method to modify its default input code if needed

            // Toggle pause / resume.
            // By default, will restore character's velocity on resume (eg: restoreVelocityOnResume = true)

            if (Input.GetKeyDown(KeyCode.P))
                pause = !pause;

            // Player input

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };

            run = Input.GetButton("Fire3");

            jump = Input.GetButton("Jump");

            crouch = Input.GetKey(KeyCode.C);
        }
    }
}