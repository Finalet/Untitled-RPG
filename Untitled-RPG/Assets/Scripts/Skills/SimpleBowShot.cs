using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SimpleBowShot : Skill
{

    bool once;
    [Header("Skill Vars")]
    public float strength = 50;
    public MultiAimConstraint spineIKtransform;
    public GameObject arrowPrefab;

    Transform aimTarget;
    Arrow newArrow;
    Vector3 shootPoint;
    LayerMask ignorePlayer;

    protected override void CustomUse()
    {
        throw new System.NotImplementedException();
    }

    protected override void CustomUseOnHold()
    {
        if (!once) {
            animator.CrossFade("AttacksUpperBody.Hunter.Simple Bow Shot_prepare", 0.25f);
            playerControlls.InterruptCasting();
            once = true;
            playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = true;
            WeaponsController.instance.InstantUnsheatheBow();
            newArrow = Instantiate(arrowPrefab, WeaponsController.instance.BowObj.transform).GetComponent<Arrow>();
            //aimTarget = spineIKtransform.transform.GetChild(0);
            //spineIKtransform.weight = 1f;
        }
        //aimTarget.transform.position = playerControlls.playerCamera.transform.position + playerControlls.playerCamera.transform.forward * distance/2;
        playerControlls.isAttacking = false;
    }

    public override void Use()
    {
        if (newArrow == null)
            newArrow = Instantiate(arrowPrefab, WeaponsController.instance.BowObj.transform).GetComponent<Arrow>();

        ignorePlayer =~ LayerMask.GetMask("Player");

        RaycastHit hit;
        if (Physics.Raycast(PlayerControlls.instance.playerCamera.transform.position, PlayerControlls.instance.playerCamera.transform.forward, out hit, strength, ignorePlayer)) {
            shootPoint = hit.point;        
        } else {
            shootPoint = PlayerControlls.instance.playerCamera.transform.forward * (strength + PlayerControlls.instance.playerCamera.GetComponent<CameraControll>().camDistance) + PlayerControlls.instance.playerCamera.transform.position;
        }

        animator.Play("AttacksUpperBody.Hunter.Simple Bow Shot_release");
        once = false;
        coolDownTimer = coolDown;
        newArrow.Shoot(strength, shootPoint, actualDamage(), skillName);
        //spineIKtransform.weight = 0.0f;
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = false;
        playerControlls.isAttacking = false;

        newArrow = null;
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

    protected override void Update () {
        base.Update();
        
        if (newArrow != null)
            DrawDebugs();
    }
}
