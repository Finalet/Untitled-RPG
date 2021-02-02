using ECM.Controllers;
using UnityEngine;

namespace ECM.Walkthrough.WallGrabWallJump
{
    /// <summary>
    /// This shows how to extend BaseCharacterController to perform common 2D constrained movement and adds Wall-Grab, Wall-SlideDown, and Wall-Jump.
    /// </summary>

    public class MyCharacterController : BaseCharacterController
    {
        #region EDITOR EXPOSED FIELDS

        [Header("Wall Jump")]
        [SerializeField]
        private float _wallJumpHeight = 4.0f;

        [SerializeField]
        private float _wallGrabbedMaxFallSpeed = 2.0f;

        #endregion

        #region FIELDS

        private float _referenceMaxFallSpeed;

        private bool _isWallGrabbed;
        private Vector3 _wallNormal;

        #endregion

        #region PROPERTIES

        public float wallJumpHeight
        {
            get { return _wallJumpHeight; }
            set { _wallJumpHeight = Mathf.Max(0.0f, value); }
        }

        public float wallGrabbedMaxFallSpeed
        {
            get { return _wallGrabbedMaxFallSpeed; }
            set { _wallGrabbedMaxFallSpeed = Mathf.Max(0.0f, value); }
        }

        public float wallJumpImpulse
        {
            get { return Mathf.Sqrt(2.0f * wallJumpHeight * movement.gravity.magnitude); }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Overrides BaseCharacterController Animate method.
        /// </summary>
        
        protected override void Animate()
        {
            // Add your character animator related code here...
        }

        /// <summary>
        /// Perform Wall-Grab Mechanics. Called OnCollisionEnter, OnCollisionStay.
        /// </summary>

        private void WallGrab(Collision collision)
        {
            // If 'other' is not a 'grabbable', return

            if (!collision.transform.CompareTag("Wall"))
                return;

            // If already grabbed, return

            if (_isWallGrabbed)
                return;

            // If grounded, return, wall-grab is allowed only on a jump

            if (movement.isGrounded)
                return;

            // If jump is still going up, return

            if (movement.velocity.y > 0f)
                return;

            // Cache wall normal and update grab state info

            _wallNormal = collision.contacts[0].normal;
            _wallNormal = Vector3.ProjectOnPlane(_wallNormal, Vector3.up);

            // Perform wall grab

            movement.maxFallSpeed = wallGrabbedMaxFallSpeed;
            movement.velocity = Vector3.Project(movement.velocity, _wallNormal);

            _isWallGrabbed = true;
        }

        /// <summary>
        /// Performs wall-jump mechanics.
        /// </summary>
        
        private void WallJump()
        {
            // If not wall-grabbed, return

            if (!_isWallGrabbed)
                return;

            // If jump button not pressed, or still not released, return

            if (!_jump || !_canJump)
                return;

            // Is jump button pressed within jump tolerance?

            if (_jumpButtonHeldDownTimer > jumpPreGroundedToleranceTime)
                return;

            _canJump = false;           // Halt jump until jump button is released
            _isJumping = true;          // Update isJumping flag
            _updateJumpTimer = true;    // Allow mid-air jump to be variable height

            // Apply wall-jump impulse

            var wallJumpVelocity = (_wallNormal + Vector3.up).normalized * wallJumpImpulse;
            movement.velocity = Vector3.ProjectOnPlane(movement.velocity, transform.up) + wallJumpVelocity;

            // Un-Grab Wall

            _isWallGrabbed = false;
            movement.maxFallSpeed = _referenceMaxFallSpeed;

            // Change character's view direction

            moveDirection = _wallNormal;
            movement.rotation = Quaternion.LookRotation(moveDirection);
        }

        /// <summary>
        /// Perform character movement logic.
        /// 
        /// NOTE: Must be called in FixedUpdate.
        /// </summary>

        protected new void Move()
        {
            // Apply movement

            // If using root motion and root motion is being applied (eg: grounded),
            // move without acceleration / deceleration, let the animation takes full control

            var desiredVelocity = CalcDesiredVelocity();

            if (useRootMotion && applyRootMotion)
                movement.Move(desiredVelocity, speed, !allowVerticalMovement);
            else
            {
                // Move with acceleration and friction

                var currentFriction = isGrounded ? groundFriction : airFriction;
                var currentBrakingFriction = useBrakingFriction ? brakingFriction : currentFriction;

                movement.Move(desiredVelocity, speed, acceleration, deceleration, currentFriction,
                    currentBrakingFriction, !allowVerticalMovement);
            }

            // Jump, MidAirJump, Wall-grab, wall-jump mechanics

            if (_isWallGrabbed)
                WallJump();
            else
            {
                Jump();
                MidAirJump();
            }

            UpdateJumpTimer();

            // Update root motion state,
            // should animator root motion be enabled? (eg: is grounded)

            applyRootMotion = useRootMotion && movement.isGrounded;
        }

        /// <summary>
        /// Overrides BaseCharacterController HandleInput method. 
        /// </summary>
        
        protected override void HandleInput()
        {
            // Handle input, Here we allow horizontal movement only

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = 0.0f
            };

            jump = Input.GetButton("Jump");
        }

        /// <summary>
        /// Overrides BaseCharacterController HandleInput method. 
        /// </summary>
        
        protected override void UpdateRotation()
        {
            // Rotate towards movement direction (input), in this case left / right.

            // Here we update character rotation to change direction instead of smoothly rotate to new direction

            if (moveDirection.sqrMagnitude > 0.0001f)
                movement.rotation = Quaternion.LookRotation(moveDirection);
        }

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Validate editor exposed fields.
        /// </summary>

        public override void OnValidate()
        {
            // Validate base class

            base.OnValidate();

            // Validate this editor exposed fields

            wallJumpHeight = _wallJumpHeight;

            wallGrabbedMaxFallSpeed = _wallGrabbedMaxFallSpeed;
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

            // Cache 'reference' max fall speed

            _referenceMaxFallSpeed = movement.maxFallSpeed;
        }

        public void OnCollisionEnter(Collision collision)
        {
            WallGrab(collision);
        }

        public void OnCollisionStay(Collision collision)
        {
            WallGrab(collision);

            // If wall-grabbed and slide-down to ground, un-grab wall

            if (!_isWallGrabbed || !movement.isGrounded)
                return;

            // Un-grab Wall

            movement.maxFallSpeed = _referenceMaxFallSpeed;
            
            _wallNormal = Vector3.zero;

            _isWallGrabbed = false;
        }

        public void OnCollisionExit(Collision collision)
        {
            // If the 'other' is not a 'grabbable' return

            if (!collision.transform.CompareTag("Wall"))
                return;

            // If not grabbed, return

            if (!_isWallGrabbed)
                return;

            // Reset wall grab info

            _wallNormal = Vector3.zero;

            // Wall-Grab

            movement.maxFallSpeed = _referenceMaxFallSpeed;

            _isWallGrabbed = false;
        }

        /// <summary>
        /// Overrides BaseCharacterController FixedUpdate to implement our custom game mechanics (eg: Wall-Grab, Wall-Jump, etc).
        /// </summary>

        public override void FixedUpdate()
        {
            // Perform character movement

            Move();
        }

        #endregion
    }
}