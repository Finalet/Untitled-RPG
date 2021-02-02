using ECM.Controllers;
using ECM.Examples.Common;
using UnityEngine;

namespace ECM.Walkthrough.OverShoulderCamera
{
    /// <summary>
    /// Example of a custom character controller.
    ///
    /// This show how to create a custom character controller extending one of the included 'Base' controller.
    /// In this example, we implement an over the shoulder camera movement.
    /// </summary>

    public sealed class MyCharacterController : BaseCharacterController
    {
        #region EDITOR EXPOSED FIELDS

        [Header("CUSTOM CONTROLLER")]
        [Tooltip("The character's follow camera.")]
        public Transform playerCamera;

        [Tooltip("The character's walk speed.")]
        [SerializeField]
        private float _walkSpeed = 2.5f;

        [Tooltip("The character's run speed.")]
        [SerializeField]
        private float _runSpeed = 5.0f;

        #endregion

        #region FIELDS

        private float _targetYaw;

        private float _yaw;
        private float _yawVelocity;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The character's walk speed.
        /// </summary>

        public float walkSpeed
        {
            get { return _walkSpeed; }
            set { _walkSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The character's run speed.
        /// </summary>

        public float runSpeed
        {
            get { return _runSpeed; }
            set { _runSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Walk input command.
        /// </summary>

        public bool walk { get; private set; }

        #endregion

        #region METHODS

        /// <summary>
        /// Get target speed based on character state (eg: running, walking, etc).
        /// </summary>

        private float GetTargetSpeed()
        {
            return walk ? walkSpeed : runSpeed;
        }

        /// <summary>
        /// Overrides 'BaseCharacterController' CalcDesiredVelocity method to handle different speeds,
        /// eg: running, walking, etc.
        /// </summary>

        protected override Vector3 CalcDesiredVelocity()
        {
            // Set 'BaseCharacterController' speed property based on this character state

            speed = GetTargetSpeed();

            // Return desired velocity vector

            return base.CalcDesiredVelocity();
        }

        /// <summary>
        /// Overrides 'BaseCharacterController' Animate method.
        /// 
        /// This shows how to handle your characters' animation states using the Animate method.
        /// The use of this method is optional, for example you can use a separate script to manage your
        /// animations completely separate of movement controller.
        /// 
        /// </summary>

        protected override void Animate()
        {
            // If no animator, return

            if (animator == null)
                return;

            // Update the animator parameters

            var forwardAmount = Mathf.InverseLerp(0.0f, runSpeed, Mathf.Abs(movement.forwardSpeed));

            animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
            animator.SetBool("OnGround", movement.isGrounded);

            if (!movement.isGrounded)
                animator.SetFloat("Jump", movement.velocity.y, 0.1f, Time.deltaTime);
        }

        /// <summary>
        /// Overrides 'BaseCharacterController' HandleInput,
        /// to perform custom controller input.
        /// </summary>

        protected override void HandleInput()
        {
            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };

            // Transform move direction to be relative to character's current orientation

            moveDirection = transform.TransformDirection(moveDirection);

            // In this implementation, we rotate the character's (along its yaw) using the mouse

            // Smoothly rotate the character's (along its Y-axis) using the horizontal mouse input.

            _targetYaw += Input.GetAxis("Mouse X") * 2.0f;
            _targetYaw = Utils.WrapAngle(_targetYaw);
            
            // Walk, jump inputs

            walk = Input.GetButton("Fire3");
            jump = Input.GetButton("Jump");

        }

        protected override void UpdateRotation()
        {
            // Perform character (CharacterMovement component) rotation, around its Y - axis(yaw rotation)

            _yaw = Mathf.SmoothDampAngle(_yaw, _targetYaw, ref _yawVelocity, 0.1f);
            
            movement.rotation = Quaternion.Euler(0.0f, _yaw, 0.0f);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            // However if your third person camera handles the rotation, and you want to match the character
            // rotation to the camera's one, please use this section

            // Generate a target rotation from cameras current look direction

            //var targetRot = Quaternion.LookRotation(Camera.main.transform.forward, transform.up);

            // Interpolate character's rotation towards camera's view direction

            //var newRotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 20f);
            //movement.rotation = Quaternion.Euler(0.0f, newRotation.eulerAngles.y, 0f);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Overrides 'BaseCharacterController' OnValidate method,
        /// to perform this class editor exposed fields validation.
        /// </summary>

        public override void OnValidate()
        {
            // Validate 'BaseCharacterController' editor exposed fields

            base.OnValidate();

            // Validate this editor exposed fields

            walkSpeed = _walkSpeed;
            runSpeed = _runSpeed;
        }

        private void OnEnable()
        {
            // Initialize vars

            _yaw = _targetYaw = transform.eulerAngles.y;
        }

        #endregion
    }
}
