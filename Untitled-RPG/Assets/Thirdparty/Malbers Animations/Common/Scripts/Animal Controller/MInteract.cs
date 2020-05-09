using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Utilities
{
    public class MInteract : MonoBehaviour, IInteractable
    {
        public BoolReference m_HasInteracted;
        public UnityEvent OnInteract = new UnityEvent();
       
        public void Interact()
        {
            if (!m_HasInteracted)
            {
                OnInteract.Invoke();
                m_HasInteracted.Value = true;
            }
        }

        public void InstantiateGO(GameObject GO)  { Instantiate(GO, transform.position, transform.rotation); }
      
        public virtual void ResetInteraction() { m_HasInteracted.Value = false; }

        public void DestroyMe() { Destroy(gameObject); }
        public void DestroyMe(float time) { Destroy(gameObject, time); }
        public void DestroyGO(GameObject GO) { Destroy(GO); }      
    }
}