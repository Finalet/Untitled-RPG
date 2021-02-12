using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bow : MonoBehaviour
{
    public Transform bowstring;

    Vector3 bowstringStartPos;

    void Start() {
        bowstringStartPos = bowstring.transform.localPosition;
    }

    public void ReleaseString () {
        bowstring.transform.DOLocalMove(bowstringStartPos, 0.2f).SetEase(Ease.Linear);
    }
}
