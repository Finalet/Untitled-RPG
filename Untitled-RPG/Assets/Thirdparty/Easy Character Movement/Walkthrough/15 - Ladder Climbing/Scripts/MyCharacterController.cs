using ECM.Controllers;
using UnityEngine;

namespace ECM.Examples.ClimbingLadders
{
    /// <summary>
    /// Example of a custom character controller.
    ///
    /// This show how to create a custom character controller extending one of the included 'Base' controller.
    /// In this example, we add ladder mechanics on top of default ECM functionality.
    /// </summary>

    public class MyCharacterController : BaseCharacterController
    {
        #region EDITOR EXPOSED FIELDS

        [Space(10f)]
        [Header("Ladder Climbing")]

        public float ClimbingSpeed = 5.0f;
        public float GrabbingTime = 0.25f;

        public LayerMask ladderMask;

        #endregion

        #region FIELDS

        private Ladder _activeLadder;
        private float _ladderPathPosition;

        private Vector3 _ladderStartPosition;
        private Vector3 _ladderTargetPosition;

        private Quaternion _ladderStartRotation;
        private Quaternion _ladderTargetRotation;

        private float _ladderTimer;

        #endregion

        #region PROPERTIES

        // Ladder input command

        public bool interact { get; set; }

        // Ladder Climb phase states

        public bool grabbing { get; private set; }
        public bool grabbed { get; private set; }
        public bool releasing { get; private set; }

        #endregion

        protected override void Animate()
        {
            // Add animator related code here...
        }

        private void GrabLadder()
        {
            if (!interact)
                return;

            // Do an overlap test to check for any ladder trigger (eg: OnTriggerEnter like)

            var characterPosition = transform.position;
            var characterRotation = transform.rotation;

            var overlappedColliders = movement.OverlapCapsule(characterPosition, characterRotation, out int overlapCount, ladderMask, QueryTriggerInteraction.Collide);
            if (overlapCount == 0)
                return;

            _activeLadder = overlappedColliders[0].GetComponent<Ladder>();
            if (!_activeLadder)
                return;

            interact = false;

            grabbing = true;
                
            _ladderStartPosition = characterPosition;
            _ladderTargetPosition = _activeLadder.ClosestPointOnPath(characterPosition, out _ladderPathPosition);

            _ladderStartRotation = characterRotation;
            _ladderTargetRotation = _activeLadder.transform.rotation;

            allowVerticalMovement = true;
            movement.capsuleCollider.isTrigger = true;
            movement.DisableGroundDetection();
        }

        private void ReleaseLadder()
        {
            // No iteraction button, return

            if (!interact)
                return;

            // If not grabbed, return

            if (!grabbed)
                return;

            // change to releasing phase

            grabbed = false;
            releasing = true;

            _ladderStartPosition = transform.position;
            _ladderStartRotation = transform.rotation;

            _ladderTargetPosition = _ladderStartPosition;
            _ladderTargetRotation = _activeLadder.BottomPoint.rotation;
        }

        private void LadderClimbing()
        {
            // Calculate desired velocity

            Vector3 desiredVelocity = Vector3.zero;

            if (grabbing || releasing)
            {
                _ladderTimer += Time.deltaTime;

                if (_ladderTimer <= GrabbingTime)
                {
                    Vector3 interpolatedPosition = Vector3.Lerp(_ladderStartPosition, _ladderTargetPosition, _ladderTimer / GrabbingTime);
                    desiredVelocity = (interpolatedPosition - transform.position) / Time.deltaTime;
                }
                else
                {
                    // If target has been reached, change ladder phase

                    _ladderTimer = 0.0f;

                    if (grabbing)
                    {
                        // Switch to ladder climb state

                        grabbing = false;
                        grabbed = true;
                    }
                    else if (releasing)
                    {
                        // Switch to default state

                        releasing = false;

                        _activeLadder = null;

                        allowVerticalMovement = false;
                        movement.capsuleCollider.enabled = true;
                        movement.EnableGroundDetection();
                    }
                }
            }
            else if (grabbed)
            {
                // Get the path position from character's current position

                _activeLadder.ClosestPointOnPath(transform.position, out _ladderPathPosition);

                if (Mathf.Abs(_ladderPathPosition) < 0.05f)
                {
                    // Compute desired velocity along ladder up axis

                    desiredVelocity = moveDirection.z * ClimbingSpeed * _activeLadder.transform.up;
                }
                else
                {
                    // If reached on of the ladder path extremes,
                    // change to releasing phase

                    grabbed = false;
                    releasing = true;

                    _ladderStartPosition = transform.position;
                    _ladderStartRotation = transform.rotation;

                    if (_ladderPathPosition > 0.0f)
                    {
                        // Above ladder path top point

                        _ladderTargetPosition = _activeLadder.TopPoint.position;
                        _ladderTargetRotation = _activeLadder.TopPoint.rotation;
                    }
                    else if (_ladderPathPosition < 0.0f)
                    {
                        // Below ladder path bottom point

                        _ladderTargetPosition = _activeLadder.BottomPoint.position;
                        _ladderTargetRotation = _activeLadder.BottomPoint.rotation;
                    }
                }
            }

            // Allow release ladder at any time

            ReleaseLadder();

            // Perform character movement

            movement.Move(desiredVelocity, ClimbingSpeed, !allowVerticalMovement);
        }

        protected override void Move()
        {
            if (_activeLadder == null)
            {
                // Default state

                var desiredVelocity = CalcDesiredVelocity();

                var currentFriction = isGrounded ? groundFriction : airFriction;
                var currentBrakingFriction = useBrakingFriction ? brakingFriction : currentFriction;

                movement.Move(desiredVelocity, speed, acceleration, deceleration, currentFriction,
                    currentBrakingFriction, !allowVerticalMovement);

                Jump();
                MidAirJump();
                UpdateJumpTimer();

                // Check if we are in ladder zone and see if we want / can grab it.
                // Init a ladder climbing state

                GrabLadder();
            }
            else
            {
                // Ladder Climbing state

                LadderClimbing();
            }
        }

        protected override void UpdateRotation()
        {
            if (grabbing || releasing)
            {
                // Align to ladder

                movement.rotation = Quaternion.Slerp(_ladderStartRotation, _ladderTargetRotation, _ladderTimer / GrabbingTime);
            }
            else if (grabbed)
            {
                // Do not update rotation
            }
            else
            {
                // Default movement, rotate towards move direction

                RotateTowardsMoveDirection();
            }
        }

        protected override void HandleInput()
        {
            // Default ECM Input as used in BaseCharacterController HandleInput method.
            // Replace this with your custom input code here...

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

            interact = Input.GetKey(KeyCode.E);
        }
    }
}
