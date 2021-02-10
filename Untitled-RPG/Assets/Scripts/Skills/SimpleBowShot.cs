using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SimpleBowShot : Skill
{

    bool once;
    [Header("Skill Vars")]
    public float distance = 20;
    public MultiAimConstraint spineIKtransform;
    Transform aimTarget;

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
            //aimTarget = spineIKtransform.transform.GetChild(0);
            //spineIKtransform.weight = 1f;
        }
        //aimTarget.transform.position = playerControlls.playerCamera.transform.position + playerControlls.playerCamera.transform.forward * distance/2;
        playerControlls.isAttacking = false;
    }

    public override void Use()
    {
        animator.Play("AttacksUpperBody.Hunter.Simple Bow Shot_release");
        once = false;
        coolDownTimer = coolDown;
        //spineIKtransform.weight = 0.0f;
        playerControlls.playerCamera.GetComponent<CameraControll>().isAiming = false;
        playerControlls.isAttacking = false;
    }
}
