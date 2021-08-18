using UnityEngine;
using System.Collections;
using MalbersAnimations.Controller;

namespace MalbersAnimations.HAP
{
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Malbers/Riding/Pulling Horses")]
    public class PullingHorses : MonoBehaviour
    {
        public MAnimal MainAnimal;
        public MAnimal SecondAnimal;
        public Rigidbody RB { get; private set; }

        public Vector3 PullingDirection { get; set; }          //Calculation for the Animator Velocity converted to RigidBody Velocityble
        public bool CurrentAngleSide { get; set; }               //True if is in the Right Side ... False if is in the Left Side
        public bool CanRotateInPlace { get; set; }

        public Vector3 RotationOffset;

        Vector3 RHorseInitialPos;
        Vector3 LHorseInitialPos;


        // Use this for initialization
        void Start()
        {
            if (!MainAnimal)
            {
                Debug.LogWarning("MainAnimal is Empty, Please set the Main Animal");
                return;
            }
            if (!SecondAnimal) SecondAnimal = MainAnimal;

            RB = GetComponent<Rigidbody>();

            MainAnimal.transform.parent = transform;
            SecondAnimal.transform.parent = transform;

            RHorseInitialPos = MainAnimal.transform.localPosition;          //Store the position of the Right Main Horse
            LHorseInitialPos = SecondAnimal.transform.localPosition;        //Store the position of the Right Main Horse

            MainAnimal.DisablePositionRotation = true;  
            SecondAnimal.DisablePositionRotation = true;

            SecondAnimal.RootMotion = false;
            MainAnimal.RootMotion = false;
        }

        void FixedUpdate()
        {
            var time = Time.fixedDeltaTime;

            if (time > 0)
            {
                RB.velocity = MainAnimal.AdditivePosition / time;
                var RotationPoint = transform.TransformPoint(RotationOffset);
                transform.RotateAround(RotationPoint, MainAnimal.UpVector, MainAnimal.HorizontalSmooth * time * MainAnimal.CurrentSpeedModifier.rotation);          //Rotate around Speed
            }


            MainAnimal.transform.localPosition = RHorseInitialPos;
            SecondAnimal.transform.localPosition = LHorseInitialPos;
        }


        void LateUpdate()
        {
            MainAnimal.transform.localPosition = RHorseInitialPos;// new Vector3(RHorseInitialPos.x, MainHorse.transform.localPosition.y, RHorseInitialPos.z);
            SecondAnimal.transform.localPosition = LHorseInitialPos;// new Vector3(RHorseInitialPos.x, MainHorse.transform.localPosition.y, RHorseInitialPos.z);

            if (SecondAnimal != null && SecondAnimal != MainAnimal)
            {
                SecondAnimal.RawInputAxis = MainAnimal.RawInputAxis;
                SecondAnimal.UseRawInput = MainAnimal.UseRawInput;
                SecondAnimal.Sprint = MainAnimal.sprint;
                SecondAnimal.CurrentSpeedIndex = MainAnimal.CurrentSpeedIndex;
                SecondAnimal.MovementDetected = MainAnimal.MovementDetected;
            }
        }

        void OnDrawGizmos()
        {
            var RotationPoint = transform.TransformPoint(RotationOffset);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(RotationPoint, 0.05f);
            Gizmos.DrawSphere(RotationPoint, 0.05f);
        }
    }
}