using UnityEngine;

namespace AwesomeTechnologies.Demo
{
    public class ExtendedFlycam : MonoBehaviour
    {
        public float CameraSensitivity = 90;
        public float ClimbSpeed = 4;
        public float NormalMoveSpeed = 10;
        public float SlowMoveFactor = 0.25f;
        public float FastMoveFactor = 3;

        private float _rotationX;
        private float _rotationY;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _rotationX = transform.eulerAngles.y;
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (Input.GetMouseButton(1))
            {
                _rotationX += Input.GetAxis("Mouse X") * CameraSensitivity * Time.deltaTime;
                _rotationY += Input.GetAxis("Mouse Y") * CameraSensitivity * Time.deltaTime;
                _rotationY = Mathf.Clamp(_rotationY, -90, 90);
            }

            Quaternion targetRotation = Quaternion.AngleAxis(_rotationX, Vector3.up);
            targetRotation *= Quaternion.AngleAxis(_rotationY, Vector3.left);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4f);

            float speedFactor = 1f;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) speedFactor = FastMoveFactor;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) speedFactor = SlowMoveFactor;

            transform.position += transform.forward * NormalMoveSpeed * speedFactor * Input.GetAxis("Vertical") *
                                  Time.deltaTime;
            transform.position += transform.right * NormalMoveSpeed * speedFactor * Input.GetAxis("Horizontal") *
                                  Time.deltaTime;
            float upAxis = 0;
            if (Input.GetKey(KeyCode.Q)) upAxis = -0.5f;
            if (Input.GetKey(KeyCode.E)) upAxis = 0.5f;
            transform.position += transform.up * NormalMoveSpeed * speedFactor * upAxis * Time.deltaTime;
        }
    }
}