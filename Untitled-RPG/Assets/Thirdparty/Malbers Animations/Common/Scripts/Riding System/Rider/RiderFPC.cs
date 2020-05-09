using UnityEngine;

namespace MalbersAnimations.HAP
{
    public class RiderFPC : MRider
    {
        [Header("First Person Properties"), Tooltip("Offset Position to move the FPC on the Mount Point")]
        public Vector3 MountOffset = Vector3.up;
        [Tooltip("If True the FPC will rotate with the Animal")]
        public bool FollowRotation = false;

        public override void MountAnimal()
        {
            if (!CanMount) return;

            Start_Mounting();

            UpdateRiderTransform();
            Vector3 AnimalForward = Vector3.ProjectOnPlane(Montura.transform.forward, Montura.Animal.UpVector);
            transform.rotation = Quaternion.LookRotation(AnimalForward, -Physics.gravity);
        }

        public override void DismountAnimal()
        {
            if (!CanDismount) return;

            Start_Dismounting();

            transform.position = new Vector3(MountTrigger.transform.position.x, transform.position.y, MountTrigger.transform.position.z);
            if (RB) RB.velocity = Vector3.zero;
        }

        void Update()
        {
            if ((LinkUpdate & UpdateMode.Update) == UpdateMode.Update) UpdateRiderTransform();
        }

        private void LateUpdate()
        {
            if ((LinkUpdate & UpdateMode.LateUpdate) == UpdateMode.LateUpdate || ForceLateUpdateLink) UpdateRiderTransform();
        }

        private void FixedUpdate()
        {
            if ((LinkUpdate & UpdateMode.FixedUpdate) == UpdateMode.FixedUpdate) UpdateRiderTransform();
        }


        /// <summary>Updates the Rider Position to the Mount Point</summary>
        public override void UpdateRiderTransform()
        {
            if (IsRiding)
            {
                transform.position = Montura.MountPoint.TransformPoint(MountOffset);

                if (FollowRotation)
                {
                    transform.rotation *= Quaternion.FromToRotation(transform.up, Vector3.up) * Montura.Animal.transform.rotation;
                }
            }
        }
    }
}