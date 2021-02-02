using UnityEngine;

namespace OrangeTech.Cameras
{
    /// <summary>
    /// OverShoulderCameraController
    /// 
    /// Third-Person-Shooter camera style, it follows the character over its shoulder.
    /// 
    /// The rig is designed so the root object of the rig should always move towards the target's position.
    /// The camera offset is specified X / Y values of the "Pivot" object, and the forward offset of the final "Camera" object,
    /// is specified by the camera Z value.
    /// 
    /// Rig      -> position will move towards target.
	///  Pivot   -> adjust Y position for height, X position for horizontal offset.
    ///   Camera -> adjust Z position for distance away from target.
    /// 
    /// EJ: position the camera over the right shoulder 1 unit behind character.
    /// 
    /// Rig    -> Target position.
    /// Pivot  -> X = 0.5f   Y = 1.3f
    /// Camera -> Z = -1.0f
    /// 
    /// </summary>

    public sealed class OverShoulderCameraController : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [SerializeField]
        private Transform _target;

        [SerializeField]
        private float _minPitch = -30.0f;

        [SerializeField]
        private float _maxPitch = 60.0f;

        [SerializeField]
        private float _pitchDampTime = 0.2f;

        #endregion

        #region FIELDS

        private float _targetPitch;

        private float _pitch;
        private float _pitchVelocity;

        #endregion

        #region PROPERTIES

        public Transform target
        {
            get { return _target; }
            set { _target = value; }
        }

        public float minPitch
        {
            get { return _minPitch; }
            set { _minPitch = value; }
        }

        public float maxPitch
        {
            get { return _maxPitch; }
            set { _maxPitch = value; }
        }

        public float pitchDampTime
        {
            get { return _pitchDampTime; }
            set { _pitchDampTime = Mathf.Max(0.0f, value); }
        }

        public Transform pivotTransform { get; private set; }

        public Transform cameraTransform { get; private set; }

        #endregion

        #region METHODS

        /// <summary>
        /// Clamps a angle (in degrees) to be within given range.
        /// </summary>

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360.0f)
                angle += 360.0f;

            if (angle > 360.0f)
                angle -= 360.0f;

            return Mathf.Clamp(angle, min, max);
        }

        #endregion

        #region MONOBEHABIOUR

        public void OnValidate()
        {
            minPitch = _minPitch;
            maxPitch = _maxPitch;

            pitchDampTime = _pitchDampTime;
        }

        public void Awake()
        {
            var cam = GetComponentInChildren<Camera>();

            cameraTransform = cam.transform;
            pivotTransform = cameraTransform.parent;

            _pitch = _targetPitch = pivotTransform.localEulerAngles.x;
        }

        public void LateUpdate()
        {
            // Assign camera position to target position (our character)

            transform.position = target.position;

            // Assign character's yaw rotation as the camera yaw rotation (Y-Axis)

            var targetYawRotation = Quaternion.LookRotation( Vector3.ProjectOnPlane(target.forward, transform.up) );
            transform.rotation = targetYawRotation;
            
            // Perform mouse look (up / down)

            _targetPitch -= Input.GetAxis("Mouse Y") * 2.0f;
            _targetPitch = ClampAngle(_targetPitch, minPitch, maxPitch);

            _pitch = Mathf.SmoothDampAngle(_pitch, _targetPitch, ref _pitchVelocity, pitchDampTime);

            pivotTransform.localRotation = Quaternion.Euler(_pitch, 0.0f, 0.0f);
        }

        #endregion
    }
}