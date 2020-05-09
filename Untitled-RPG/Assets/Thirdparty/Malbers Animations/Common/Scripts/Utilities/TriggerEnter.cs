using UnityEngine;
using MalbersAnimations.Events;

namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// This is used when the collider is in a different gameObject and you need to check the Trigger Events
    /// Create this component at runtime and subscribe to the UnityEvents
    /// </summary>
    public class TriggerEnter : MonoBehaviour
    {
        [Tooltip("Ignore this Objects with this layers")]
        public LayerMask Ignore;
        [SerializeField] private bool active = true;

        public ColliderEvent onTriggerEnter = new ColliderEvent();

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }
       

        void OnTriggerEnter(Collider other)
        {
            if (!active) return;
            if (MalbersTools.Layer_in_LayerMask(other.gameObject.layer, Ignore)) return;

            onTriggerEnter.Invoke(other);
        }
         
        public virtual void Destroy()
        {
            Destroy(gameObject);
        }

        public virtual void Destroy(float time)
        {
            Destroy(gameObject, time);
        }

        public virtual void Instantiate_(GameObject gameObject)
        {
            Instantiate(gameObject, transform.position, transform.rotation);
        }

        private void Reset()
        {
            var collider = GetComponent<Collider>();
            Active = true;

            if (collider)
            {
                collider.isTrigger = true;
            }
            else
            {
                Active = false;
                Debug.LogError("This Script requires a Collider, please add any type of collider");
            }
        }
    }
}