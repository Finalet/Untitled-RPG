using UnityEngine;
namespace MalbersAnimations.Events
{
    /// <summary>Simple Event Raiser On Enable</summary>
    public class UnityEventRaiser : MonoBehaviour
    {
        public float Delayed = 0;
        public UnityEngine.Events.UnityEvent OnEnableEvent;

        public void OnEnable()
        {
            if (Delayed > 0)
            {
                Invoke("StartEvent", Delayed);
            }
            else
            {
                OnEnableEvent.Invoke();
            }
        }

        private void StartEvent()
        {
            OnEnableEvent.Invoke();
        }


        public void DestroyMe(float time)
        { Destroy(gameObject, time); }

        public void DestroyMe()
        { Destroy(gameObject); }

        public void DestroyGameObject(GameObject go)
        { Destroy(go); }

        public void DestroyComponent(Component component)
        { Destroy(component); }

        public void Parent(Transform newParent)
        {
            transform.parent = newParent;
        }

        public void Parent_Local(Transform newParent)
        {
            transform.parent = newParent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}