using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SimpleBowShot : AimingSkill
{
    [Header("Skill Vars")]
    public float strength = 50;
    public MultiAimConstraint spineIKtransform;
    public GameObject arrowPrefab;
    public Transform rightHand;

    [Header("Sounds")]
    public AudioClip drawSound;
    public AudioClip[] shootSounds;

    Transform aimTarget;
    Arrow newArrow;
    Vector3 shootPoint;
    LayerMask ignorePlayer;
    bool grabBowstring;

    protected override void CustomUse(){}

    protected override void StartAiming()
    {

        animator.CrossFade("AttacksUpperBody.Hunter.Simple Bow Shot_prepare", 0.25f);
        playerControlls.InterruptCasting();
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = true;
        WeaponsController.instance.InstantUnsheatheBow();
        newArrow = Instantiate(arrowPrefab, WeaponsController.instance.BowObj.transform).GetComponent<Arrow>();
        //aimTarget = spineIKtransform.transform.GetChild(0);
        //spineIKtransform.weight = 1f;
        
        playerControlls.isAttacking = false;
        Combat.instanace.AimingSkill = this;

        PlaySound(drawSound, 0, 1, 0, 0.5f);
    }

    protected override void KeepAiming()
    {
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = true;
        //aimTarget.transform.position = playerControlls.playerCamera.transform.position + playerControlls.playerCamera.transform.forward * distance/2;
        if(newArrow == null && coolDownTimer <= coolDown/2f) { //Releading
            StartAiming();
        }
        
        if (grabBowstring) WeaponsController.instance.BowObj.GetComponent<Bow>().bowstring.position = rightHand.transform.position;
        if (newArrow != null) {
            newArrow.transform.position = rightHand.position + 0.03f * newArrow.transform.forward;
            newArrow.transform.LookAt(WeaponsController.instance.BowObj.transform);
        }
    }

    public override void CancelAiming()
    {
        animator.CrossFade("AttacksUpperBody.Hunter.Empty", 0.25f);

        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = false;
        playerControlls.isAttacking = false;

        if (newArrow != null) Destroy(newArrow.gameObject);
        newArrow = null;

        WeaponsController.instance.BowObj.GetComponent<Bow>().ReleaseString();
        grabBowstring = false;

        Combat.instanace.AimingSkill = null;
    }

    public override void Shoot()
    {
        if (newArrow == null)
            newArrow = Instantiate(arrowPrefab, WeaponsController.instance.BowObj.transform).GetComponent<Arrow>();

        ignorePlayer =~ LayerMask.GetMask("Player");

        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, strength, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (strength/2 + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        animator.Play("AttacksUpperBody.Hunter.Simple Bow Shot_release");
        coolDownTimer = coolDown;
        newArrow.Shoot(strength, shootPoint, CalculateDamage.damageInfo(skillTree, baseDamagePercentage), skillName);
        WeaponsController.instance.BowObj.GetComponent<Bow>().ReleaseString();
        grabBowstring = false;
        //spineIKtransform.weight = 0.0f;
        playerControlls.isAttacking = false;

        newArrow = null;

        PlaySound(shootSounds[Random.Range(0, shootSounds.Length)], 0, 1.2f, 0, 0.25f);
    }

    void DrawDebugs () {
        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, strength+characteristics.magicSkillDistanceIncrease, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (strength + characteristics.magicSkillDistanceIncrease + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }
        Debug.DrawLine(newArrow.transform.position, shootPoint, Color.blue);
        Debug.DrawRay(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward * (strength + characteristics.magicSkillDistanceIncrease + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance), Color.red);
    }

    public void GrabBowstring() {
        grabBowstring = true;
    }

    // protected override void Update () {
    //     base.Update();
        
    //     if (newArrow != null)
    //         DrawDebugs();
    // }
}
