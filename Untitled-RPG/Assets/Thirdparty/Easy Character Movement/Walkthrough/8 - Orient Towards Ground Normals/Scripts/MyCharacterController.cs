using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.OrientToGround
{
    /// <summary>
    /// Example of a custom character controller.
    ///
    /// This show how to create a custom character controller extending one of the included 'Base' controller.
    /// In this example, we show how to orient the character towards ground normals.
    /// </summary>

    public class MyCharacterController : BaseCharacterController
    {
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

        protected override void UpdateRotation()
        {
            // Rotate towards move direction

            RotateTowardsMoveDirection();

            // Orient towards surface normal or world up if not grounded

            Vector3 surfaceNormal;

            if (!movement.isGrounded)
                surfaceNormal = Vector3.up;
            else
            {
                var castOrigin = transform.TransformPoint(movement.capsuleCollider.center);
                var castDirection = -transform.up;
                var castDistance = 2.0f;
                var castMask = movement.groundMask;

                RaycastHit hitInfo;
                bool hit = Physics.Raycast(castOrigin, castDirection, out hitInfo, castDistance, castMask);

                surfaceNormal = hit ? hitInfo.normal : movement.surfaceNormal;
            }
            
            var characterUp = transform.up;
            Vector3 smoothedNormal = Vector3.Slerp(characterUp, surfaceNormal, 10.0f * Time.deltaTime);

            movement.rotation = Quaternion.FromToRotation(characterUp, smoothedNormal) * movement.rotation;
        }
    }
}
