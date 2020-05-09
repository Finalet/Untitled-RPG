using System.Collections.Generic;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations.SA
{
    /// <summary>Modified SA Third Person Character Controller</summary>
	[RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class MThirdPersonCharacter : MonoBehaviour, ICharacterMove
    {
        [SerializeField] float m_MovingTurnSpeed = 360;
        [SerializeField] float m_StationaryTurnSpeed = 180;
        [SerializeField] float m_JumpPower = 12f;
        [Range(1f, 4f)]
        [SerializeField] float m_GravityMultiplier = 2f;
        [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMultiplier = 1f;
        [SerializeField] float m_GroundCheckDistance = 0.1f;

        [SerializeField] BoolReference m_SmoothVertical =  new BoolReference( true);
       

        private Transform MainCamera;

        Rigidbody m_Rigidbody;
        Animator m_Animator;
        bool m_IsGrounded;
        float m_OrigGroundCheckDistance;
        const float k_Half = 0.5f;
        float m_TurnAmount;
        float m_ForwardAmount;
        Vector3 m_GroundNormal;

        public bool Jump { get; set; }
        public bool Shift { get; set; }


        void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        void Start()
        {
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            m_OrigGroundCheckDistance = m_GroundCheckDistance;

            MainCamera = MalbersTools.FindMainCamera()?.transform;
        }



        public virtual void SetInputAxis(Vector2 move)
        {
            Vector3 move3 = new Vector3(move.x, 0, move.y);

            SetInputAxis(move3);
        }


        public void Move(Vector3 move)
        {
            if (!isActiveAndEnabled) return;

            if (Shift) move *= 0.5f;

            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.
            if (move.magnitude > 1f) move.Normalize();

            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);
            m_TurnAmount = Mathf.Atan2(move.x, move.z);


            m_ForwardAmount = move.z;

            
         


            ApplyExtraTurnRotation();


            if (m_IsGrounded) HandleGroundedMovement(Jump);     // control and velocity handling is different when grounded and airborne:
            else HandleAirborneMovement();


            if (!m_SmoothVertical && m_ForwardAmount > 0)                       //It will remove slowing Stick push when rotating and going Forward
            {
                m_ForwardAmount = 1;
            }


            // send input and other state parameters to the animator
            UpdateAnimator(move);

        }



        void UpdateAnimator(Vector3 move)
        {
            if (!m_Animator.isActiveAndEnabled) return;

            // update the animator parameters
            m_Animator.SetFloat("Vertical", m_ForwardAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Horizontal", m_TurnAmount, 0.1f, Time.deltaTime);

            m_Animator.SetBool("Grounded", m_IsGrounded);

            if (!m_IsGrounded)
            {
                m_Animator.SetFloat("JumpHeight", m_Rigidbody.velocity.y);
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);

            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded)
            {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            if (m_IsGrounded && move.magnitude > 0)
            {
                m_Animator.speed = m_AnimSpeedMultiplier;
            }
            else
            {
                m_Animator.speed = 1; // don't use that while airborne
            }
        }


        void HandleAirborneMovement()
        {
            // apply extra gravity from multiplier:
            Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);

            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
        }


        void HandleGroundedMovement(bool jump)
        {
            // check whether conditions are right to allow a jump:
            if (jump && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
                // jump!
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                m_IsGrounded = false;
                m_Animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
            }
        }

        void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
            transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
        }

        public void OnAnimatorMove()
        {
            var time = m_Animator.updateMode == AnimatorUpdateMode.AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;

            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (m_IsGrounded && time > 0)
            {
                Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / time;

                // we preserve the existing y part of the current velocity.
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }


        void CheckGroundStatus()
        {
            RaycastHit hitInfo;
            // 0.1f is a small offset to start the ray from inside the character
            // it is also good to note that the transform position in the sample assets is at the base of the character
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
            {
                m_GroundNormal = hitInfo.normal;
                m_IsGrounded = true;
                m_Animator.applyRootMotion = true;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundNormal = Vector3.up;
                m_Animator.applyRootMotion = false;
            }
        }

        public void SetInputAxis(Vector3 inputAxis)
        {
            var Cam_Forward = Vector3.ProjectOnPlane(MainCamera.forward, Vector3.up).normalized; //Normalize the Camera Forward Depending the Up Vector IMPORTANT!
            var Cam_Right = Vector3.ProjectOnPlane(MainCamera.right, Vector3.up).normalized;
            // var Cam_Up = Vector3.ProjectOnPlane(mainCamera.up, Forward).normalized;

            var m_Move = (inputAxis.z * Cam_Forward) + (inputAxis.x * Cam_Right);

            Move(m_Move);
        }
    }
}
