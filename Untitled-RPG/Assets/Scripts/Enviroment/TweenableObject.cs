using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

public class TweenableObject : MonoBehaviour
{
    public Vector3 moveBy;
    public float moveDuration = 0.5f;
    public bool pingPong;
    [ShowIf("pingPong"), Min(0.1f)] public float pingPongDelay;

    public void Move() {
        transform.DOBlendableMoveBy(moveBy, moveDuration);
        if (pingPong) transform.DOBlendableMoveBy(-moveBy, moveDuration).SetDelay(pingPongDelay);
    }
}
