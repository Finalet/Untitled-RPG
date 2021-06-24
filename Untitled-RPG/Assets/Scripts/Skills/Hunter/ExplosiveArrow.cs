using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveArrow : Skill
{
    [Header("Skill Vars")]
    public GameObject arrowPrefab;
    public float strength = 100;
    [Header("Sounds")]
    public AudioClip castingSound;
    public AudioClip shootSound;

    LayerMask ignorePlayer;
    ArrowExplosive newArrow;
    Transform shootPosition;
    Vector3 shootPoint;
    bool grabBowstring;

    protected override void Start()
    {
        base.Start();
        shootPosition = PlayerControlls.instance.rightHandWeaponSlot;
    }

    protected override void CustomUse() {}

    protected override void CastingAnim() {
        animator.CrossFade("Attacks.Hunter.Explosive Arrow", 0.25f);
        StartCoroutine(SpawnArrowIE());
        PlaySound(castingSound, 0.37f, characteristics.castingSpeed.x);
        
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = true;
    }

    public void Shoot () {
        finishedCast = true;
        ignorePlayer =~ LayerMask.GetMask("Player");
        
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, strength, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (strength/2 + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        if (newArrow != null) newArrow.Shoot(strength, shootPoint, CalculateDamage.damageInfo(damageType, baseDamagePercentage, 1), skillName); //could be null if just canceled skill
        playerControlls.isAttacking = false;
        grabBowstring = false;
        WeaponsController.instance.BowObj.GetComponent<Bow>().ReleaseString();
        newArrow = null;
        Invoke("StopAiming", 0.5f);
        PlaySound(shootSound, 0.1f, 1, 0, 0.5f);
    }

    public void GrabBowstring () {
        grabBowstring = true;
        StartCoroutine(GrabBowstringIE());
    }

    void StopAiming () {
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = false;
    }

    protected override void InterruptCasting() {
        base.InterruptCasting();

        if (newArrow != null) {
            Destroy(newArrow.gameObject);
        }
        newArrow = null;
        
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
        newArrow = Instantiate(arrowPrefab, WeaponsController.instance.BowObj.transform).GetComponent<ArrowExplosive>();
        while (newArrow != null) {
            newArrow.transform.position = shootPosition.position + 0.03f * newArrow.transform.forward;
            newArrow.transform.LookAt(WeaponsController.instance.BowObj.transform);
            yield return null;
        }
    }

    public override string getDescription()
    {
        DamageInfo dmg = CalculateDamage.damageInfo(damageType, baseDamagePercentage, 0, 0);
        return $"Launch an arrow that explodes on impact, dealing {dmg.damage} {dmg.damageType} to everyone inside the impact radius.";
    }
}
