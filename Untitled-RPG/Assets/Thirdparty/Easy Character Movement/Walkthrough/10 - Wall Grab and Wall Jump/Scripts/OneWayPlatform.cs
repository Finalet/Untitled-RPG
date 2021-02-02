using UnityEngine;

namespace ECM.Examples
{
    public class OneWayPlatform : MonoBehaviour
    {
        public Collider platformCollider;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            platformCollider.enabled = false;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            platformCollider.enabled = true;
        }
    }
}
