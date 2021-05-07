using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RainOfArrows : Skill
{
    [Header("Skill Vars")]
    public GameObject arrowPrefab;
    public float strength = 100;
    public GameObject intialArrow;
    public float skillDistance;
    public float rainFrequency;
    public float rainDuration;
    public float rainRadius;
    [Range(0, 1)] public float rainChanceToHitEnemy;
    
    [Header("Sounds")]
    public AudioClip castingSound;
    public AudioClip shootSound;

    LayerMask ignorePlayer;
    GameObject newArrow;
    Transform shootPosition;
    bool grabBowstring;
    bool isCanceled = false;

    protected override void Start()
    {
        base.Start();
        shootPosition = PlayerControlls.instance.rightHandWeaponSlot;
    }

    protected override float actualDistance () {
        return skillDistance + characteristics.hunterSkillDistanceIncrease;
    } 

    protected override void CustomUse() {
        Invoke("SpawnRain", 2);
    }
    void SpawnRain () {
        if (isCanceled)
            return;

        RainOfArrowsRain rain = new GameObject().AddComponent(typeof(RainOfArrowsRain)) as RainOfArrowsRain;
        rain.name = skillName;
        rain.transform.position = pickedPosition + Vector3.up * 20;
        rain.skill = this;
    }

    protected override void CastingAnim() {
        animator.CrossFade("Attacks.Hunter.Rain of Arrows", 0.25f);
        StartCoroutine(SpawnArrowIE());
        PlaySound(castingSound, 0.15f, 0.6f);
    }

    public void Shoot (bool spawnArrows = false) {
        finishedCast = true;
        ignorePlayer =~ LayerMask.GetMask("Player");
        
        if (spawnArrows)
            newArrow = Instantiate(intialArrow, WeaponsController.instance.BowObj.transform);

        if (newArrow != null) {
            newArrow.SetActive(true);
            Rigidbody rb = newArrow.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce(newArrow.transform.forward * 50, ForceMode.Impulse);
            newArrow.transform.SetParent(null);
            newArrow.transform.DOScale(0, 0.5f).SetDelay(1);
            Destroy(newArrow, 2);
        }

        playerControlls.isAttacking = false;
        grabBowstring = false;
        WeaponsController.instance.BowObj.GetComponent<Bow>().ReleaseString();
        newArrow = null;
        PlaySound(shootSound, 0, 1, 0, 0.25f);

        isCanceled = false;
    }

    public void GrabBowstring () {
        grabBowstring = true;
        StartCoroutine(GrabBowstringIE());
    }

    protected override void InterruptCasting() {
        base.InterruptCasting();

        if (newArrow != null) {
            Destroy(newArrow.gameObject);
        }
        newArrow = null;
        
        isCanceled = true;
        grabBowstring = false;
        WeaponsController.instance.BowObj.GetComponent<Bow>().ReleaseString();
    }

    IEnumerator GrabBowstringIE () {
        while (grabBowstring) {
            WeaponsController.instance.BowObj.GetComponent<Bow>().bowstring.position = shootPosition.transform.position;
            yield return null;
        }
    }
    IEnumerator SpawnArrowIE () {
        newArrow = Instantiate(intialArrow, WeaponsController.instance.BowObj.transform);
        newArrow.SetActive(true);
        while (newArrow != null) {
            newArrow.transform.position = shootPosition.position + 0.03f * newArrow.transform.forward;
            newArrow.transform.LookAt(WeaponsController.instance.BowObj.transform);
            yield return null;
        }
    }

    IEnumerator ShootArrow() {
        newArrow.GetComponent<Rigidbody>().AddForce(newArrow.transform.forward * 10, ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f);
        newArrow.GetComponent<Rigidbody>().AddForce(Vector3.up * 50, ForceMode.Impulse);
    }
}
