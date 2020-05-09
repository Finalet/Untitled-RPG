using UnityEngine;
using System.Collections;

namespace MalbersAnimations.HAP
{
    /// <summary>This Script controls the behavior of the wagons </summary>
    public class WagonController : MonoBehaviour
    {
        [Header("Horse")]
        public Rigidbody HorseRigidBody;
        public ConfigurableJoint HorseJoint;
        

        /// How much the Animal can turn Regarding the Wagons Front Wheels
        public float MaxTurnAngle = 45f;

        protected PullingHorses DHorses;

        public Transform CenterOfMass;

        [Header("Colliders")]
        public Transform BodyCollider;
        public Transform StearCollider;
        [Space]
        public WheelCollider[] WheelColliders;

        [Space]
        [Header("Meshes")]
        public Transform Body;
        public Transform StearMesh;
        public Transform[] WheelMeshes;

        protected Rigidbody RB;
        protected float currentAngle;

        [Space]
        public bool debug;
        public Color DebugColor;

        // Use this for initialization
        void Start()
        {
            RB = GetComponent<Rigidbody>();

            if (HorseRigidBody)
            {
                DHorses = HorseRigidBody.GetComponent<PullingHorses>();   //Get the Reference for the PullingHorses Script
            }

            if (Body && BodyCollider) Body.parent = BodyCollider;               //Parent Body to the Collider
            if (StearMesh && StearCollider) StearMesh.parent = StearCollider;   //Parent Body to the Collider

            if (HorseRigidBody && HorseJoint)
            {
                HorseJoint.connectedBody = HorseRigidBody;
            }

            if (CenterOfMass) RB.centerOfMass = CenterOfMass.localPosition;

        }

        /// <summary>Updates the Meshes of the Wheel Colliders</summary>
        void UpdateWheelMeshes()
        {
            for (int i = 0; i < WheelColliders.Length; i++)
            {
                Quaternion rot;
                Vector3 pos;
                if (WheelColliders[i])
                {
                    if (DHorses)
                    {
                       StopWheels(!DHorses.MainAnimal.MovementDetected, i);
                    }

                    WheelColliders[i].GetWorldPose(out pos, out rot);
                    WheelMeshes[i].position = pos;
                    WheelMeshes[i].rotation = rot;
                }
            }
        }

        void StopWheels(bool stop, int Index)
        {
            WheelColliders[Index].brakeTorque = stop ? 0.9f : 0;
        }

        void FixedUpdate()
        {
           if (CenterOfMass) RB.centerOfMass = CenterOfMass.localPosition;
            UpdateWheelMeshes();
            GetStearAngle();
        }

        protected virtual void GetStearAngle()
        {
            if (!DHorses) return;
            if (!StearCollider || !BodyCollider) return;

            //_rigidBody.centerOfMass = CenterOfMass.position;

            Vector3 BodyDirection = BodyCollider.forward;
            Vector3 StearDirection = StearCollider.forward;

            BodyDirection.y = StearDirection.y = 0;

            currentAngle = Vector3.Angle(BodyDirection, StearDirection);    //Calculate the current angle

            float Side = Vector3.Dot(BodyDirection, StearCollider.right);  //Which Side is Rotating the Stear

            currentAngle = currentAngle * (Side > 0 ? 1 : -1);

            DHorses.CurrentAngleSide = (Side > 0 ? true : false);

            if (DHorses)
            {
                if ((currentAngle >= MaxTurnAngle && DHorses.MainAnimal.MovementAxis.x <= 0) ||
                   (currentAngle <= -MaxTurnAngle && DHorses.MainAnimal.MovementAxis.x >= 0))
                {
                    DHorses.CanRotateInPlace = false;
                }
                else
                {
                    DHorses.CanRotateInPlace = true;
                }

                if (RB.velocity.magnitude < 0.01f)
                {
                    RB.velocity = DHorses.PullingDirection; //Get a headStart
                }
            }
        }


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!debug) return;

            BoxCollider[] allColliders = GetComponentsInChildren<BoxCollider>();

            foreach (BoxCollider box in allColliders)
            {
                if (box.GetComponent<MonoBehaviour>())
                {
                    continue;
                }
                var sizeX = transform.lossyScale.x * box.size.x;
                var sizeY = transform.lossyScale.y * box.size.y;
                var sizeZ = transform.lossyScale.z * box.size.z;
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(box.bounds.center, box.transform.rotation, new Vector3(sizeX, sizeY, sizeZ));

                Gizmos.matrix = rotationMatrix;
                Gizmos.color = DebugColor;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
                Gizmos.color = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }

            foreach (var wheel in WheelColliders)
            {
                if (wheel)
                {
                    Vector3 pos;
                    Quaternion rot;
                    wheel.GetWorldPose(out pos, out rot);
                    UnityEditor.Handles.color = DebugColor;

                    UnityEditor.Handles.DrawSolidDisc(pos, wheel.transform.right, wheel.radius);
                    UnityEditor.Handles.color = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1);
                    UnityEditor.Handles.DrawWireDisc(pos, wheel.transform.right, wheel.radius);
                }
            }
        }
#endif
    }
}