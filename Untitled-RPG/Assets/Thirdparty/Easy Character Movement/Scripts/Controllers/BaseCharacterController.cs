using ECM.Components;
using ECM.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace ECM.Controllers
{
    /// <summary>
    /// Base Character Controller.
    /// 
    /// A general purpose character controller and base of other controllers.
    /// It handles keyboard input, and allows for a variable height jump, however this default behaviour
    /// can easily be modified or completely replaced overriding this related methods in a derived class.
    /// </summary>

    public class BaseCharacterController : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Header("Movement")]
        [Tooltip("Maximum movement speed (in m/s).")]
        [SerializeField]
        private float _speed = 5.0f;

        [Tooltip("Maximum turning speed (in deg/s).")]
        [SerializeField]
        private float _angularSpeed = 540.0f;

        [Tooltip("The rate of change of velocity.")]
        [SerializeField]
        private float _acceleration = 50.0f;

        [Tooltip("The rate at which the character's slows down.")]
        [SerializeField]
        private float _deceleration = 20.0f;

        [Tooltip(
            "Setting that affects movement control. Higher values allow faster changes in direction.\n" +
            "If useBrakingFriction is false, this also affects the ability to stop more quickly when braking.")]
        [SerializeField]
        private float _groundFriction = 8f;

        [Tooltip("Should brakingFriction be used to slow the character? " +
                 "If false, groundFriction will be used.")]
        [SerializeField]
        private bool _useBrakingFriction;

        [Tooltip("Friction coefficient applied when braking (when there is no input acceleration).\n" +
                 "Only used if useBrakingFriction is true, otherwise groundFriction is used.")]
        [SerializeField]
        private float _brakingFriction = 8f;

        [Tooltip("Friction coefficient applied when 'not grounded'.")]
        [SerializeField]
        private float _airFriction;

        [Tooltip("When not grounded, the amount of lateral movement control available to the character.\n" +
                 "0 - no control, 1 - full control.")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float _airControl = 0.2f;

        [Header("Crouch")]
        [Tooltip("Can the character crouch")]
        [SerializeField]
        private bool _canCrouch = true;

        [Tooltip("The character's capsule height while standing.")]
        [SerializeField]
        private float _standingHeight = 2.0f;

        [Tooltip("The character's capsule height while crouching.")]
        [SerializeField]
        private float _crouchingHeight = 1.0f;

        [Header("Jump")]
        [Tooltip("The initial jump height (in meters).")]
        [SerializeField]
        private float _baseJumpHeight = 1.5f;

        [Tooltip("The extra jump time (e.g. holding jump button) in seconds.")]
        [SerializeField]
        private float _extraJumpTime = 0.5f;

        [Tooltip("Acceleration while jump button is held down, given in meters / sec^2."+
                 "As rule of thumb, configure it to your character's gravity.")]
        [SerializeField]
        private float _extraJumpPower = 25.0f;

        [FormerlySerializedAs("_jumpToleranceTime")]
        [Tooltip("How early before hitting the ground you can press jump, and still perform the jump.\n" +
                 "Typical values goes from 0.15f to 0.5f.")]
        [SerializeField]
        private float _jumpPreGroundedToleranceTime = 0.15f;

        [Tooltip("How long after leaving the ground you can press jump, and still perform the jump." +
                 "Typical values goes from 0.15f to 0.5f.")]
        [SerializeField]
        private float _jumpPostGroundedToleranceTime = 0.15f;

        [Tooltip("Maximum mid-air jumps")]
        [SerializeField]
        private float _maxMidAirJumps = 1;

        [Header("Animation")]
        [Tooltip("Should use root motion?\n" +
                 "This requires a 'RootMotionController' attached to the 'Animator' game object.")]
        [SerializeField]
        private bool _useRootMotion;

        [Tooltip("Should root motion handle character rotation?\n" +
                 "This requires a 'RootMotionController' attached to the 'Animator' game object.")]
        [SerializeField]
        private bool _rootMotionRotation;

        #endregion

        #region FIELDS

        private Vector3 _moveDirection;

        protected bool _canJump = true;
        protected bool _jump;
        protected bool _isJumping;

        protected bool _updateJumpTimer;
        protected float _jumpTimer;
        protected float _jumpButtonHeldDownTimer;
        protected float _jumpUngroundedTimer;

        protected int _midAirJumpCount;

        private bool _allowVerticalMovement;
        
        private bool _restoreVelocityOnResume = true;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Cached CharacterMovement component.
        /// </summary>

        public CharacterMovement movement { get; private set; }

        /// <summary>
        /// Cached animator component (if any).
        /// </summary>

        public Animator animator { get; set; }

        /// <summary>
        /// Cached root motion controller component (if any).
        /// </summary>

        public RootMotionController rootMotionController { get; set; }

        /// <summary>
        /// Allow movement along y-axis and disable gravity force.
        /// Eg: Flaying, Ladder climb, etc.
        /// </summary>

        public bool allowVerticalMovement
        {
            get { return _allowVerticalMovement; }
            set
            {
                _allowVerticalMovement = value;

                if (movement)
                    movement.useGravity = !_allowVerticalMovement;
            }
        }

        /// <summary>
        /// Maximum movement speed (in m/s).
        /// </summary>

        public float speed
        {
            get { return _speed; }
            set { _speed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Maximum turning speed (in deg/s).
        /// </summary>

        public float angularSpeed
        {
            get { return _angularSpeed; }
            set { _angularSpeed = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The rate of change of velocity.
        /// </summary>

        public float acceleration
        {
            get { return movement.isGrounded ? _acceleration : _acceleration * airControl; }
            set { _acceleration = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The rate at which the character's slows down.
        /// </summary>

        public float deceleration
        {
            get { return movement.isGrounded ? _deceleration : _deceleration * airControl; }
            set { _deceleration = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Setting that affects movement control. Higher values allow faster changes in direction.
        /// If useBrakingFriction is false, this also affects the ability to stop more quickly when braking.
        /// </summary>

        public float groundFriction
        {
            get { return _groundFriction; }
            set { _groundFriction = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Should brakingFriction be used to slow the character?
        /// If false, groundFriction will be used.
        /// </summary>

        public bool useBrakingFriction
        {
            get { return _useBrakingFriction; }
            set { _useBrakingFriction = value; }
        }

        /// <summary>
        /// Friction applied when braking (eg: when there is no input acceleration).  
        /// Only used if useBrakingFriction is true, otherwise groundFriction is used.
        /// 
        /// Braking is composed of friction (velocity dependent drag) and constant deceleration.
        /// </summary>

        public float brakingFriction
        {
            get { return _brakingFriction; }
            set { _brakingFriction = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Friction applied when not grounded (eg: falling, flying, etc).
        /// If useBrakingFriction is false, this also affects the ability to stop more quickly when braking while in air. 
        /// </summary>

        public float airFriction
        {
            get { return _airFriction; }
            set { _airFriction = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// When not grounded, the amount of lateral movement control available to the character.
        /// 0 is no control, 1 is full control.
        /// </summary>

        public float airControl
        {
            get { return _airControl; }
            set { _airControl = Mathf.Clamp01(value); }
        }

        /// <summary>
        /// Can crouch? Enable / Disable crouching behavior.
        /// </summary>

        public bool canCrouch
        {
            get { return _canCrouch; }
            set { _canCrouch = value; }
        }

        /// <summary>
        /// The character's capsule height while standing.
        /// </summary>

        public float standingHeight
        {
            get { return _standingHeight; }
            set { _standingHeight = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The character's capsule height while crouching.
        /// </summary>

        public float crouchingHeight
        {
            get { return _crouchingHeight; }
            set { _crouchingHeight = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The initial jump height (in meters).
        /// </summary>

        public float baseJumpHeight
        {
            get { return _baseJumpHeight; }
            set { _baseJumpHeight = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Computed jump impulse.
        /// </summary>

        public float jumpImpulse
        {
            //get { return Mathf.Sqrt(2.0f * baseJumpHeight * movement.gravity); }
            get { return Mathf.Sqrt(2.0f * baseJumpHeight * movement.gravity.magnitude); }
        }

        /// <summary>
        /// The extra jump time (e.g. holding jump button) in seconds.
        /// </summary>

        public float extraJumpTime
        {
            get { return _extraJumpTime; }
            set { _extraJumpTime = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Acceleration while jump button is held down, given in meters / sec^2.
        /// </summary>

        public float extraJumpPower
        {
            get { return _extraJumpPower; }
            set { _extraJumpPower = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// How early before hitting the ground you can press jump, and still perform the jump.
        /// Typical values goes from 0.05f to 0.5f, the higher the value, the easier to "chain" jumps and vice-versa.
        /// </summary>

        public float jumpPreGroundedToleranceTime
        {
            get { return _jumpPreGroundedToleranceTime; }
            set { _jumpPreGroundedToleranceTime = Mathf.Max(value, 0.0f); }
        }

        /// <summary>
        /// How long after leaving the ground you can press jump, and still perform the jump.
        /// Typical values goes from 0.05f to 0.5f, the higher the value, the easier to "chain" jumps and vice-versa.
        /// </summary>

        public float jumpPostGroundedToleranceTime
        {
            get { return _jumpPostGroundedToleranceTime; }
            set { _jumpPostGroundedToleranceTime = Mathf.Max(value, 0.0f); }
        }

        /// <summary>
        /// Maximum mid-air jumps.
        /// </summary>

        public float maxMidAirJumps
        {
            get { return _maxMidAirJumps; }
            set { _maxMidAirJumps = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Should move with root motion?
        /// </summary>

        public bool useRootMotion
        {
            get { return _useRootMotion; }
            set { _useRootMotion = value; }
        }

        /// <summary>
        /// Should root motion handle character's rotation?
        /// </summary>

        public bool useRootMotionRotation
        {
            get { return _rootMotionRotation; }
            set { _rootMotionRotation = value; }
        }

        /// <summary>
        /// Should root motion be applied?
        /// </summary>

        public bool applyRootMotion
        {
            get { return animator != null && animator.applyRootMotion; }
            set
            {
                if (animator != null)
                    animator.applyRootMotion = value;
            }
        }

        /// <summary>
        /// Jump input command.
        /// </summary>

        public bool jump
        {
            get { return _jump; }
            set
            {
                // If jump is released, allow to jump again

                if (_jump && value == false)
                {
                    _canJump = true;
                    _jumpButtonHeldDownTimer = 0.0f;
                }

                // Update jump value; if pressed, update held down timer

                _jump = value;
                if (_jump)
                    _jumpButtonHeldDownTimer += Time.deltaTime;
            }
        }

        /// <summary>
        /// Is the character jumping? (moving up result of jump button press).
        /// </summary>

        public bool isJumping
        {
            get { return _isJumping; }
        }

        /// <summary>
        /// True if character is falling, false if not.
        /// </summary>

        public bool isFalling
        {
            get { return !movement.isGrounded && movement.velocity.y < 0.0001f; }
        }

        /// <summary>
        /// Is the character standing on 'ground'?
        /// </summary>

        public bool isGrounded
        {
            get { return movement.isGrounded; }
        }

        /// <summary>
        /// Movement input command. The desired move direction.
        /// </summary>

        public Vector3 moveDirection
        {
            get { return _moveDirection; }
            set { _moveDirection = Vector3.ClampMagnitude(value, 1.0f); }
        }

        /// <summary>
        /// Toggle pause / resume.
        /// </summary>

        public bool pause { get; set; }

        /// <summary>
        /// Is the character paused?
        /// </summary>

        public bool isPaused { get; private set; }

        /// <summary>
        /// Should saved velocity (when pause == true) be restored on resume (when pause == false)?
        /// If true, the saved rigidbody velocity will be restored on resume, if false, the rigidbody will be reset (zero).
        /// </summary>

        public bool restoreVelocityOnResume
        {
            get { return _restoreVelocityOnResume; }
            set { _restoreVelocityOnResume = value; }
        }

        /// <summary>
        /// Crouch input command.
        /// </summary>

        public bool crouch { get; set; }

        /// <summary>
        /// Is the character crouching?
        /// </summary>

        public bool isCrouching { get; protected set; }

        #endregion

        #region METHODS

        /// <summary>
        /// Pause Rigidbody physical interaction, will restore current velocities (linear, angular) if desired (restoreVelocityOnResume == true).
        /// While paused, will turn the Rigidbody into kinematic, preventing any physical interaction.
        /// </summary>

        private void Pause()
        {
            if (pause && !isPaused)
            {
                // Pause 

                movement.Pause(true);
                isPaused = true;
            }
            else if (!pause && isPaused)
            {
                // Resume

                movement.Pause(false, restoreVelocityOnResume);
                isPaused = false;
            }
        }

        /// <summary>
        /// Rotate the character towards a given direction vector.
        /// </summary>
        /// <param name="direction">The target direction</param>
        /// <param name="onlyLateral">Should it be restricted to XZ only?</param>

        public void RotateTowards(Vector3 direction, bool onlyLateral = true)
        {
            movement.Rotate(direction, angularSpeed, onlyLateral);
        }

        /// <summary>
        /// Rotate the character towards move direction vector (input).
        /// </summary>
        /// <param name="onlyLateral">Should it be restricted to XZ only?</param>

        public void RotateTowardsMoveDirection(bool onlyLateral = true)
        {
            RotateTowards(moveDirection, onlyLateral);
        }

        /// <summary>
        /// Rotate the character towards its velocity vector.
        /// </summary>
        /// <param name="onlyLateral">Should it be restricted to XZ only?</param>

        public void RotateTowardsVelocity(bool onlyLateral = true)
        {
            RotateTowards(movement.velocity, onlyLateral);
        }

        /// <summary>
        /// Perform jump logic.
        /// </summary>

        protected virtual void Jump()
        {
            // Update _isJumping flag state

            if (isJumping)
            {
                // On landing, reset _isJumping flag

                if (!movement.wasGrounded && movement.isGrounded)
                    _isJumping = false;
            }

            // Update jump ungrounded timer (post jump tolerance time)

            if (movement.isGrounded)
                _jumpUngroundedTimer = 0.0f;
            else
                _jumpUngroundedTimer += Time.deltaTime;

            // If jump button not pressed, or still not released, return

            if (!_jump || !_canJump)
                return;

            // Is jump button pressed within pre jump tolerance time?

            if (_jumpButtonHeldDownTimer > _jumpPreGroundedToleranceTime)
                return;

            // If not grounded or no post grounded tolerance time remains, return

            if (!movement.isGrounded && _jumpUngroundedTimer > _jumpPostGroundedToleranceTime)
                return;

            _canJump = false;           // Halt jump until jump button is released
            _isJumping = true;          // Update isJumping flag
            _updateJumpTimer = true;    // Allow mid-air jump to be variable height

            // Prevent _jumpPostGroundedToleranceTime condition to pass until character become grounded again (_jumpUngroundedTimer reseted).

            _jumpUngroundedTimer = _jumpPostGroundedToleranceTime;

            // Apply jump impulse

            movement.ApplyVerticalImpulse(jumpImpulse);

            // 'Pause' grounding, allowing character to safely leave the 'ground'

            movement.DisableGrounding();
        }

        /// <summary>
        /// Mid-air jump logic.
        /// </summary>

        protected virtual void MidAirJump()
        {
            // Reset mid-air jumps counter

            if (_midAirJumpCount > 0 && movement.isGrounded)
                _midAirJumpCount = 0;

            // If jump button not pressed, or still not released, return

            if (!_jump || !_canJump)
                return;

            // If grounded, return

            if (movement.isGrounded)
                return;

            // Have mid-air jumps?

            if (_midAirJumpCount >= _maxMidAirJumps)
                return;

            _midAirJumpCount++;         // Increase mid-air jumps counter

            _canJump = false;           // Halt jump until jump button is released
            _isJumping = true;          // Update isJumping flag
            _updateJumpTimer = true;    // Allow mid-air jump to be variable height

            // Apply jump impulse

            movement.ApplyVerticalImpulse(jumpImpulse);

            // 'Pause' grounding, allowing character to safely leave the 'ground'

            movement.DisableGrounding();
        }

        /// <summary>
        /// Perform variable jump height logic.
        /// </summary>

        protected virtual void UpdateJumpTimer()
        {
            if (!_updateJumpTimer)
                return;

            // If jump button is held down and extra jump time is not exceeded...

            if (_jump && _jumpTimer < _extraJumpTime)
            {
                // Calculate how far through the extra jump time we are (jumpProgress),

                var jumpProgress = _jumpTimer / _extraJumpTime;

                // Apply proportional extra jump power (acceleration) to simulate variable height jump,
                // this method offers better control and less 'floaty' feel.

                var proportionalJumpPower = Mathf.Lerp(_extraJumpPower, 0f, jumpProgress);
                movement.ApplyForce(transform.up * proportionalJumpPower, ForceMode.Acceleration);

                // Update jump timer

                _jumpTimer = Mathf.Min(_jumpTimer + Time.deltaTime, _extraJumpTime);
            }
            else
            {
                // Button released or extra jump time ends, reset info

                _jumpTimer = 0.0f;
                _updateJumpTimer = false;
            }
        }

        /// <summary>
        /// Handle character's Crouch / UnCrouch.
        /// </summary>
        
        protected virtual void Crouch()
        {
            // If crouching behaviour is disabled, return

            if (!canCrouch)
                return;

            // Process crouch input command

            if (crouch)
            {
                // If already crouching, return

                if (isCrouching)
                    return;

                // Set capsule crouching height

                movement.SetCapsuleHeight(crouchingHeight);

                // Update Crouching state

                isCrouching = true;
            }
            else
            {
                // If not crouching, return

                if (!isCrouching)
                    return;

                // Check if character can safely stand up

                if (!movement.ClearanceCheck(standingHeight))
                    return;

                // Character can safely stand up, set capsule standing height

                movement.SetCapsuleHeight(standingHeight);

                // Update crouching state

                isCrouching = false;
            }
        }

        /// <summary>
        /// Calculate the desired movement velocity.
        /// Eg: Convert the input (moveDirection) to movement velocity vector,
        ///     use navmesh agent desired velocity, etc.
        /// </summary>

        protected virtual Vector3 CalcDesiredVelocity()
        {
            // If using root motion and root motion is being applied (eg: grounded),
            // use animation velocity as animation takes full control

            if (useRootMotion && applyRootMotion)
                return rootMotionController.animVelocity;

            // else, convert input (moveDirection) to velocity vector

            return moveDirection * speed;
        }

        /// <summary>
        /// Perform character movement logic.
        /// 
        /// NOTE: Must be called in FixedUpdate.
        /// </summary>

        protected virtual void Move()
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

            // Jump logic
            
            Jump();
            MidAirJump();
            UpdateJumpTimer();

            // Update root motion state,
            // should animator root motion be enabled? (eg: is grounded)

            applyRootMotion = useRootMotion && movement.isGrounded;
        }

        /// <summary>
        /// Perform character animation.
        /// </summary>

        protected virtual void Animate() { }

        /// <summary>
        /// Update character's rotation.
        /// By default ECM will rotate the character towards its movement direction,
        /// however this default behavior can easily be modified overriding this method in a derived class.
        /// </summary>

        protected virtual void UpdateRotation()
        {
            if (useRootMotion && applyRootMotion && useRootMotionRotation)
            {
                // Use animation rotation to rotate our character
                
                movement.rotation *= animator.deltaRotation;
            }
            else
            {
                // Rotate towards movement direction (input)

                RotateTowardsMoveDirection();
            }
        }

        /// <summary>
        /// Handles input.
        /// </summary>

        protected virtual void HandleInput()
        {
            // Toggle pause / resume.
            // By default, will restore character's velocity on resume (eg: restoreVelocityOnResume = true)

            if (Input.GetKeyDown(KeyCode.P))
                pause = !pause;

            // Handle user input

            moveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };

            jump = Input.GetButton("Jump");

            crouch = Input.GetKey(KeyCode.C);
        }

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Validate editor exposed fields.
        /// 
        /// NOTE: If you override this, it is important to call the parent class' version of method
        /// eg: base.OnValidate, in the derived class method implementation, in order to fully validate the class.  
        /// </summary>

        public virtual void OnValidate()
        {
            speed = _speed;
            angularSpeed = _angularSpeed;

            acceleration = _acceleration;
            deceleration = _deceleration;

            groundFriction = _groundFriction;
            brakingFriction = _brakingFriction;

            airFriction = _airFriction;
            airControl = _airControl;

            canCrouch = _canCrouch;
            crouchingHeight = _crouchingHeight;
            standingHeight = _standingHeight;

            baseJumpHeight = _baseJumpHeight;
            extraJumpTime = _extraJumpTime;
            extraJumpPower = _extraJumpPower;
            jumpPreGroundedToleranceTime = _jumpPreGroundedToleranceTime;
            jumpPostGroundedToleranceTime = _jumpPostGroundedToleranceTime;
            maxMidAirJumps = _maxMidAirJumps;
        }

        /// <summary>
        /// Initialize this component.
        /// 
        /// NOTE: If you override this, it is important to call the parent class' version of method,
        /// (eg: base.Awake) in the derived class method implementation, in order to fully initialize the class. 
        /// </summary>

        public virtual void Awake()
        {
            // Cache components

            movement = GetComponent<CharacterMovement>();
            movement.platformUpdatesRotation = true;

            animator = GetComponentInChildren<Animator>();

            rootMotionController = GetComponentInChildren<RootMotionController>();
        }

        public virtual void FixedUpdate()
        {
            // Pause / resume character

            Pause();

            // If paused, return

            if (isPaused)
                return;

            // Perform character movement

            Move();

            // Handle crouch

            Crouch();
        }

        public virtual void Update()
        {
            // Handle input

            HandleInput();

            // If paused, return

            if (isPaused)
                return;

            // Update character rotation (if not paused)

            UpdateRotation();

            // Perform character animation (if not paused)

            Animate();
        }
        
        #endregion
    }
}