using UnityEngine;

namespace MalbersAnimations
{
    public class Fireball : MonoBehaviour
    {
        public float force = 500;
        public float radius = 2;
        public float life = 10f;
        public GameObject explotion;

        protected Transform owner;

        public void SetOwner(Transform owner) { this.owner = owner; }

        void Start()  { Destroy(gameObject, life); }     //If the ball has not touch anything destroy it
      
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 2) return;                   //Do not collide with Ignore Raycast!
            if (other.isTrigger) return;                               //Do not collide with Ignore Raycast!
            if (other.transform.root == owner) return;                 //Don't hit yourself

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

            foreach (var nearbyObjects in colliders)
            {
                if (nearbyObjects.GetComponent<Fireball>()) continue;

                Rigidbody rb = nearbyObjects.GetComponent<Rigidbody>();
                if (rb) rb.AddExplosionForce(force, transform.position, radius);
            }

            Destroy(gameObject,0.1f);
            //create fireball explotion after collides
            GameObject fireballexplotion = Instantiate(explotion);
            fireballexplotion.transform.position = transform.position;

            Destroy(fireballexplotion, 2f);

        }
    }
}