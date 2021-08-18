using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Tools/Gravity Changer")]

    public class GravityChanger : MonoBehaviour
    {
        IGravity animal;
        protected Collider Other;
        void OnTriggerEnter(Collider other)
        {
            Other = other;
            animal = other.GetComponentInParent<IGravity>();
        }

        void Update()
        {
            if (animal != null)
            {
                animal.Gravity = (transform.position - Other.transform.position).normalized;
            }
        }
 

        void OnTriggerExit(Collider other)
        {
            ResetAnimal();
        }

        public virtual void ResetAnimal()
        {
            if (animal != null)    animal.Gravity = Vector3.down;

            animal = null;
            Other = null;
        }
    }
}