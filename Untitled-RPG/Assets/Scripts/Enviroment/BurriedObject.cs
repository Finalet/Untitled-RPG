using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class BurriedObject : MonoBehaviour
{

    public GameObject burriedObject;
    [Space]
    public int digsRequired = 3;
    public AudioClip digCompletedSFX;
    public UnityEvent OnCompleteDigging;
    
    bool completedDigging;
    int numberOfDiggs;
    float dis;

    void Awake() {
        dis = (transform.position.y - burriedObject.transform.position.y)/(float)digsRequired;
    }

    public void DigOut() {
        if (completedDigging) return;

        burriedObject.transform.DOBlendableMoveBy(Vector3.up * dis, 0.5f);
        numberOfDiggs++;

        if (numberOfDiggs >= 3) {
            OnCompleteDigging?.Invoke();
            completedDigging = true;
            UIAudioManager.instance.PlayUISound(digCompletedSFX, 1.1f);
        }
    }
}