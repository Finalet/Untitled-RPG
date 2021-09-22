using UnityEngine.Events;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [DisplayWithoutEdit] public bool playerDetected;

    [Space]
    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerExit;

    void Awake() {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) {
            playerDetected = true;
            OnPlayerEnter?.Invoke();
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && other.GetType() == typeof(CapsuleCollider)) {
            playerDetected = false;
            OnPlayerExit?.Invoke();
        }
    }
}
