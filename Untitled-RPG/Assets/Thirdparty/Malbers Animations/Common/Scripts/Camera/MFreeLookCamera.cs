using UnityEngine;
using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// This is the same Camera FreeLookCam of the Stardard Assets Modify to Fit My Needs
/// </summary>
namespace MalbersAnimations
{
    public class MFreeLookCamera : MonoBehaviour
    {
        public enum UpdateType                                      // The available methods of updating are:
        {
            FixedUpdate,                                            // Update in FixedUpdate (for tracking rigidbodies).
            LateUpdate,                                             // Update in LateUpdate. (for tracking objects that are moved in Update)
        }

#if REWIRED
        [Header("Rewired Connection")]
#else
        [HideInInspector]
#endif
        public string PlayerID = "Player0";

        [Space]

        public Transform m_Target;                                  // The target object to follow
        public UpdateType updateType = UpdateType.FixedUpdate;      // stores the selected update type
        /// <summary>Stores the Update type when the game starts </summary>
        internal UpdateType defaultUpdate;

        public float m_MoveSpeed = 10f;                             // How fast the rig will move to keep up with the target's position.
        [Range(0f, 10f)]
        public float m_TurnSpeed = 10f;                             // How fast the rig will rotate from user input.
        public float m_TurnSmoothing = 10f;                         // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
        public float m_TiltMax = 75f;                               // The maximum value of the x axis rotation of the pivot.
        public float m_TiltMin = 45f;                               // The minimum value of the x axis rotation of the pivot.

        [Header("Camera Input Axis")]
        public InputAxis Vertical = new InputAxis("Mouse Y", true, false);
        public InputAxis Horizontal = new InputAxis("Mouse X", true, false);
        private IGravity TargetGravity;

        [Space]
        public bool m_LockCursor = false;                           // Whether the cursor should be hidden and locked.
        [Space]
        public FreeLockCameraManager manager;
        public FreeLookCameraState DefaultState;

        [HideInInspector] public UnityEvent OnStateChange = new UnityEvent();

        /// <summary>Additional FOV when Sprinting</summary>
        [Space, Header("Sprint Field of View"), Tooltip("Additional FOV when Sprinting")]
        public FloatReference SprintFOV = new FloatReference(10f);
        /// <summary>Duration of the Transition when Sprinting </summary>
        [Tooltip("Additional FOV when Sprinting")]
        public FloatReference FOVTransition = new FloatReference(1f);

        private float m_LookAngle;                                  // The rig's y axis rotation.
        private float m_TiltAngle;                                  // The pivot's x axis rotation.
        private Vector3 m_PivotEulers;
        private Vector3 m_UpVector;
        private Quaternion m_PivotTargetRot;
        private Quaternion m_TransformTargetRot;

        /// <summary>Next Camera State to Go to</summary>
        protected FreeLookCameraState NextState;
        /// <summary> Current Camera State </summary>
        protected FreeLookCameraState currentState;

        IEnumerator IChangeStates;
        IEnumerator IChange_FOV;

        public Transform Target
        {
            get { return m_Target; }
        }

        /// <summary>Main Camera</summary>
        public Camera Cam { get; private set; }

        /// <summary>Main Camera Transform</summary>
        public Transform CamT { get; private set; }

        public Transform Pivot { get; private set; }


        /// <summary>Camera Horizontal Input Value</summary>
        public float XCam { get; set; }

        /// <summary>Camera Vertical Input Value</summary>
        public float YCam { get; set; }

        /// <summary> Stores the Current FOV of the Camera</summary>
        public float ActiveFOV { get; internal set; }

        private IInputSystem inputSystem;

        protected void Awake()
        {
            Cam = GetComponentInChildren<Camera>();
            CamT = GetComponentInChildren<Camera>().transform;
            Pivot = Cam.transform.parent;

            currentState = null;
            NextState = null;

            if (manager) manager.SetCamera(this);

            if (DefaultState) Set_State(DefaultState);

            Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;  // Lock or unlock the cursor.
            Cursor.visible = !m_LockCursor;

            m_PivotEulers = Pivot.rotation.eulerAngles;
            m_PivotTargetRot = Pivot.transform.localRotation;
            m_TransformTargetRot = transform.localRotation;


            ActiveFOV = Cam.fieldOfView;

            inputSystem = DefaultInput.GetInputSystem(PlayerID);

            Horizontal.InputSystem = Vertical.InputSystem = inputSystem;        //Update the Input System on the Axis
            defaultUpdate = updateType;

            if (DefaultState == null)
            {
                DefaultState = ScriptableObject.CreateInstance<FreeLookCameraState>();

                DefaultState.CamFOV = Cam.fieldOfView;
                DefaultState.PivotPos = Pivot.localPosition;
                DefaultState.CamPos = CamT.localPosition;


                DefaultState.name = "Default State";

                OnStateChange.Invoke();
            }
        }

        void Start()
        {
            GetTargetGravity();
        }

        void GetTargetGravity()
        {
            if (Target != null)
            {
                TargetGravity = Target.GetComponentInChildren<IGravity>();

                if (TargetGravity == null)
                {
                    TargetGravity = Target.GetComponentInParent<IGravity>();
                }
            }
        }


        public virtual void Set_State(FreeLookCameraState profile)
        {
            Pivot.localPosition = profile.PivotPos;
            Cam.transform.localPosition = profile.CamPos;
            Cam.fieldOfView = ActiveFOV = profile.CamFOV;
            OnStateChange.Invoke();
        }

        #region Private Methods
        protected void FollowTarget(float deltaTime)
        {
            if (m_Target == null) return;

            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime * m_MoveSpeed);  // Move the rig towards target position.
        }

        private void UpdateState(FreeLookCameraState state)
        {
            if (state == null) return;

            Pivot.localPosition = state.PivotPos;
            CamT.localPosition = state.CamPos;
            Cam.fieldOfView = ActiveFOV = state.CamFOV;
            OnStateChange.Invoke();
        }

        private void HandleRotationMovement(float time)
        {
            if (Time.timeScale < float.Epsilon) return;

            if (Horizontal.active) XCam = Horizontal.GetAxis;
            if (Vertical.active) YCam = Vertical.GetAxis;

            m_LookAngle += XCam * m_TurnSpeed;                                                     // Adjust the look angle by an amount proportional to the turn speed and horizontal input.

            if (TargetGravity != null) m_UpVector = Vector3.Slerp(m_UpVector, TargetGravity.UpVector, time * 15);
           // transform.rotation = Quaternion.FromToRotation(transform.up, m_UpVector) * transform.rotation; //This Make it f
            m_TransformTargetRot = Quaternion.FromToRotation(transform.up, m_UpVector) * Quaternion.Euler(0f, m_LookAngle, 0f);                       // Rotate the rig (the root object) around Y axis only:

            m_TransformTargetRot =  Quaternion.Euler(0f, m_LookAngle, 0f);                       // Rotate the rig (the root object) around Y axis only:

            m_TiltAngle -= YCam * m_TurnSpeed;                                                 // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
            m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);                  // and make sure the new value is within the tilt range

            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y, m_PivotEulers.z); // Tilt input around X is applied to the pivot (the child of this object)

            Pivot.localRotation = Quaternion.Slerp(Pivot.localRotation, m_PivotTargetRot, m_TurnSmoothing * time);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * time) ;
        }
 

        void FixedUpdate()
        {
            if (updateType == UpdateType.FixedUpdate)
            {
                FollowTarget(Time.fixedDeltaTime);   
                HandleRotationMovement(Time.fixedDeltaTime);
            }
        }

        void LateUpdate()
        {
            if (updateType == UpdateType.LateUpdate)
            { 
                FollowTarget(Time.deltaTime);
                HandleRotationMovement(Time.deltaTime);
            }
        }
        #endregion

        public void Set_State_Smooth(FreeLookCameraState state) { SetState(state, false); }

        public void Set_State_Temporal(FreeLookCameraState state) { SetState(state, true); }

        internal void SetState(FreeLookCameraState state, bool temporal)
        {
            if (state == null) return;

            NextState = state;

            if (currentState && NextState == currentState) return;

            if (IChangeStates != null) StopCoroutine(IChangeStates);

            if (!temporal) DefaultState = state; //Changes wth this

            IChangeStates = StateTransition(state.transition);
            StartCoroutine(IChangeStates);
        }

        public virtual void Set_State_Default_Smooth() { SetState(DefaultState, true); }

        public virtual void Set_State_Default() { Set_State(DefaultState); }

        public virtual void ToggleSprintFOV(bool val)
        {
            ChangeFOV(val ? ActiveFOV + SprintFOV.Value : ActiveFOV);
        }

        public void ChangeFOV(float newFOV)
        {
            if (IChange_FOV != null) StopCoroutine(IChange_FOV);

            IChange_FOV = C_SprintFOV(newFOV, FOVTransition);
            StartCoroutine(IChange_FOV);
        }


        #region Coroutines
        private IEnumerator StateTransition(float time)
        {
            float elapsedTime = 0;
            currentState = NextState;

            while (elapsedTime < time)
            {

          
                Pivot.localPosition = Vector3.Lerp(Pivot.localPosition, NextState.PivotPos, Mathf.SmoothStep(0, 1, elapsedTime / time));
                CamT.localPosition = Vector3.Lerp(CamT.localPosition, NextState.CamPos, Mathf.SmoothStep(0, 1, elapsedTime / time));
                Cam.fieldOfView = ActiveFOV = Mathf.Lerp(Cam.fieldOfView, NextState.CamFOV, Mathf.SmoothStep(0, 1, elapsedTime / time));
                OnStateChange.Invoke();
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            UpdateState(NextState);

            NextState = null;
            yield return null;
        }
        private IEnumerator C_SprintFOV(float newFOV, float time)
        {
            float elapsedTime = 0f;
            float startFOV = Cam.fieldOfView;

            while (elapsedTime < time)
            {
                Cam.fieldOfView = Mathf.Lerp(startFOV, newFOV, Mathf.SmoothStep(0, 1, elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            Cam.fieldOfView = newFOV;
           yield return null;
        }
        #endregion


        public virtual void SetTarget(Transform newTransform)
        {
            m_Target = newTransform;
            GetTargetGravity();
        }

        public virtual void SetTarget(GameObject newGO) { SetTarget(newGO.transform); }

        /// <summary> When the Rider is Aiming is necesary to change the Update Mode to Late Update</summary>
        public virtual void ForceUpdateMode(bool val)
        {
            updateType = val ? UpdateType.LateUpdate : defaultUpdate;
        }
    }
}
