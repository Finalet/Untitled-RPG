using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Damage/Simple Throw")]
    public class SimpleThrow : MonoBehaviour, IAnimatorListener
    {
        public GameObject projectile;
        [RequiredField] public Transform throwOrigin;
        public float Force = 500;
        public ForceMode forceMode = ForceMode.Force;

        public void Throw() { Throw(projectile); }

        public void Throw(GameObject b)
        {
            projectile = b;

            if (projectile && !projectile.IsPrefab())
            {
                projectile.transform.position = throwOrigin.position;
                projectile.transform.parent = null;

                var rb = projectile.GetComponent<Rigidbody>();
                var col = projectile.GetComponent<Collider>();

                if (col)
                {
                    col.enabled = true;
                    col.isTrigger = false;
                }

                if (rb)
                {
                    rb.isKinematic = false;
                    rb.AddForce(throwOrigin.forward * Force, forceMode);
                }

                projectile = null;
            }
        }

        public void SetProjectile(GameObject b) { projectile = b; }


        public virtual bool OnAnimatorBehaviourMessage(string message, object value)
        { return this.InvokeWithParams(message, value); }

    }
}