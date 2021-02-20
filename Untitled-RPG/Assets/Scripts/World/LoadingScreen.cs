using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LoadingScreen : MonoBehaviour
{

    public GameObject sword;
    public Material platformMat;
    public Light fakeLight;
    [Space]
    [ColorUsageAttribute(true,true)] public Color baseRedColor;
    [ColorUsageAttribute(true,true)] public Color glowingRedColor;

    float cycle = 2;
    
    void Start() {
        platformMat.SetColor("_EmissionColor", baseRedColor);

        sword.transform.DOBlendableMoveBy(Vector3.up * 0.3f, cycle)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        sword.transform.DOBlendableRotateBy(Vector3.up * 10, cycle/2)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    
        platformMat.DOColor(glowingRedColor, "_EmissionColor", cycle)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        DOTween.To(()=> fakeLight.intensity, x=> fakeLight.intensity = x, 2.5f, cycle)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);;
    }
}
