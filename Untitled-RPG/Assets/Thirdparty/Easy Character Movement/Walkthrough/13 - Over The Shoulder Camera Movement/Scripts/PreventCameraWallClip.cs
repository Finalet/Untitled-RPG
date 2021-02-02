using UnityEngine;

namespace OrangeTech.Cameras
{
    /// <summary>
    /// Version light de PreventCameraWallClip, esta version mas robusta la respalde en Desarrollo/Documents.
    /// </summary>

    public class PreventCameraWallClip : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [SerializeField]
        private float _clipMoveTime = 0.05f;

        [SerializeField]
        private float _returnTime = 0.4f;

        [SerializeField]
        private float _sphereCastRadius = 0.1f;

        [SerializeField]
        private float _minDistance = 0.5f;

        [SerializeField]
        private LayerMask _collisionMask = 1;

        #endregion

        #region FIELDS

        private float _moveVelocity;

        private float _referenceDistance;
        private float _currentDistance;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Time taken (in seconds) to move when avoiding cliping (low value = fast).
        /// </summary>

        public float clipMoveTime
        {
            get { return _clipMoveTime; }
            set { _clipMoveTime = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Time taken to move back towards desired position,
        /// when not clipping (typically should be a higher value than clipMoveTime).
        /// </summary>

        public float returnTime
        {
            get { return _returnTime; }
            set { _returnTime = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The radius of the sphere used to test for object between camera and target.
        /// </summary>

        public float sphereCastRadius
        {
            get { return _sphereCastRadius; }
            set { _sphereCastRadius = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// The closest distance the camera can be from the target.
        /// </summary>

        public float minDistance
        {
            get { return _minDistance; }
            set { _minDistance = value; }
        }

        /// <summary>
        /// Layers to collide against.
        /// </summary>

        public LayerMask collisionMask
        {
            get { return _collisionMask; }
            set { _collisionMask = value; }
        }

        /// <summary>
        /// The point at which the camera pivots around.
        /// </summary>

        public Transform pivotTransform { get; private set; }

        /// <summary>
        /// The transform of the camera.
        /// </summary>

        public Transform cameraTransform { get; private set; }

        #endregion

        #region MONOBEHAVIOUR

        public void OnValidate()
        {
            clipMoveTime = _clipMoveTime;
            returnTime = _returnTime;

            sphereCastRadius = _sphereCastRadius;
            minDistance = _minDistance;
        }

        public void Awake()
        {
            // Cache and initialize

            var cam = GetComponentInChildren<Camera>();

            cameraTransform = cam.transform;
            pivotTransform = cameraTransform.parent;

            _referenceDistance = cameraTransform.localPosition.magnitude;
            _currentDistance = _referenceDistance;
        }

        public void LateUpdate()
        {
            // Casteamos un rayo desde el pivote hacia la camara

            var origin = pivotTransform.position + pivotTransform.forward * sphereCastRadius;
            var direction = -pivotTransform.forward;

            var targetDistance = _referenceDistance;

            RaycastHit hit;
            if (Physics.SphereCast(origin, sphereCastRadius, direction, out hit, _referenceDistance + sphereCastRadius,
                collisionMask, QueryTriggerInteraction.Ignore))
            {
                // Si chocamos con algo,
                // movemos la camara la posicion encontrada

                targetDistance = -pivotTransform.InverseTransformPoint(hit.point).z;
            }

            // Interpolamos hacia targetDistance

            _currentDistance = Mathf.SmoothDamp(_currentDistance, targetDistance, ref _moveVelocity,
                _currentDistance > targetDistance ? clipMoveTime : returnTime);
            _currentDistance = Mathf.Clamp(_currentDistance, minDistance, _referenceDistance);

            // Actualizamos la posion local de la camara

            cameraTransform.localPosition = -Vector3.forward * _currentDistance;
        }

        #endregion
    }
}