using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levitation : Skill
{
    [Header("Custom vars")] 
    [Tooltip("Increase distance of mage skill by meters")] public float skillDistanceIncrease = 10;
    [Tooltip("Duration in seconds")] public float flightDuration = 120;

    public Transform[] feetsAndHands;
    public ParticleSystem bodypartsVFX;

    protected override void CustomUse() {
        PlayerControlls.instance.TakeOff();
        StartCoroutine(flightTimer());
        for (int i = 0; i < feetsAndHands.Length; i ++) {
            GameObject go = Instantiate(bodypartsVFX, feetsAndHands[i]).gameObject;
            go.SetActive(true);
        }
    }

    IEnumerator flightTimer () {
        yield return new WaitForSeconds(flightDuration);
        PlayerControlls.instance.LandFromFlying();
    }
}
