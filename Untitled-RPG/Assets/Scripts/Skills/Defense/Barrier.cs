using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Barrier : Skill
{
    [Header("Custom vars")]
    public GameObject barrier;
    public Material barrierMaterial;
    public float duration;

    public Transform[] shields;
    public AudioClip sfx;

    float raiseSpeed = 0.5f;

    protected override void CustomUse () {
        StartCoroutine(use());
        animator.CrossFade("Attacks.Defense.Barrier", 0.25f);
    }

    IEnumerator use () {
        PlaySound(sfx, 0, characteristics.attackSpeed.y);
        yield return new WaitForSeconds(0.56f * characteristics.attackSpeed.x);
        barrier.transform.SetParent(null);
        barrier.SetActive(true);
        barrierMaterial.SetFloat("Progress", 0);
        barrierMaterial.DOFloat(1, "Progress", raiseSpeed);
        for (int i = 0; i < shields.Length; i++) {
            shields[i].localScale = Vector3.zero;
            shields[i].DOScale(Vector3.one * 0.7f, raiseSpeed).SetEase(Ease.OutBack);
            shields[i].DOBlendableLocalMoveBy(Vector3.up * 0.2f, 1).SetLoops(Mathf.RoundToInt(duration/raiseSpeed), LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        yield return new WaitForSeconds(duration);
        barrierMaterial.DOFloat(0, "Progress", raiseSpeed);
        for (int i = 0; i < shields.Length; i++) {
            shields[i].DOScale(Vector3.zero, raiseSpeed);
        }
        yield return new WaitForSeconds(raiseSpeed);
        barrier.SetActive(false);
        barrier.transform.SetParent(transform);
        barrier.transform.localPosition = Vector3.zero;
        barrier.transform.localRotation = Quaternion.identity;
    }

    public override string getDescription()
    {
        return $"Erect a walls around you for {duration} seconds.";
    }
}
