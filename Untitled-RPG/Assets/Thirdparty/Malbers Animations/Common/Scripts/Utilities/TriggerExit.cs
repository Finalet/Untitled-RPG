using UnityEngine;
using MalbersAnimations.Events;

namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// This is used when the collider is in a different gameObject and you need to check the Trigger Events
    /// Create this component at runtime and subscribe to the UnityEvents
    /// </summary>
    public class TriggerExit : MonoBehaviour
    {
        [Tooltip("Ignore this Objects with this layers")]
        public LayerMask Ignore;
        [SerializeField] private bool active = true;

        public ColliderEvent onTriggerExit = new ColliderEvent();

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

       

        void OnTriggerExit(Collider other)
        {
            if (!active) return;
            if (MalbersTools.Layer_in_LayerMask(other.gameObject.layer, Ignore)) return;

            onTriggerExit.Invoke(other);
        }
         
        public virtual void Destroy()
        {
            Destroy(gameObject);
        }

        public virtual void Destroy(float time)
        {
            Destroy(gameObject, time);
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
                Debug.LogError("This Script requires a Collider, please add any type of collider");
            }
        }
    }
}