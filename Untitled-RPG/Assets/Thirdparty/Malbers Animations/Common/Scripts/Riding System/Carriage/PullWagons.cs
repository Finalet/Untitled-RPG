using UnityEngine;

namespace MalbersAnimations.HAP
{
    [AddComponentMenu("Malbers/Riding/Pull Wagons")]

    public class PullWagons : MonoBehaviour
    {
        Rigidbody rb;
        public Transform rotationPivot;
        public float Pullforce = 5;
        public float RotationA = 10;


        void Start()
        {
            rb = GetComponent<Rigidbody>();

            if (rotationPivot == null)
            {
                rotationPivot = transform;
            }
        }

        void FixedUpdate()
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            var newVelocity = (transform.forward * Pullforce * v);
            newVelocity.y = rb.velocity.y;

            transform.RotateAround(rotationPivot.position, Vector3.up, h * RotationA);

            // rb.AddTorque(transform.up * RotationA * h, ForceMode.VelocityChange);

            rb.velocity = newVelocity;
        }
    }
}