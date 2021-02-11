﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SimpleBowShot : AimingSkill
{
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
    }

    protected override void KeepAiming()
    {
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = true;
        //aimTarget.transform.position = playerControlls.playerCamera.transform.position + playerControlls.playerCamera.transform.forward * distance/2;
        if(newArrow == null && coolDownTimer <= coolDown/2f) { //Releading
            StartAiming();
        }
    }

    public override void CancelAiming()
    {
        animator.CrossFade("AttacksUpperBody.Hunter.Empty", 0.25f);

        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = false;
        playerControlls.isAttacking = false;

        if (newArrow != null) Destroy(newArrow.gameObject);
        newArrow = null;

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
        newArrow.Shoot(strength, shootPoint, actualDamage(), skillName);
        //spineIKtransform.weight = 0.0f;
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

    // protected override void Update () {
    //     base.Update();
        
    //     if (newArrow != null)
    //         DrawDebugs();
    // }
}
