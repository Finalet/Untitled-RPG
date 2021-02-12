using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AimingSkill : Skill
{
    public override void Use() {
        //Empty because not supposed to be used from here
    }

    public virtual void UseButtonDown() {
        if (!skillActive() || playerControlls.isRolling || playerControlls.isGettingHit || playerControlls.isCastingSkill || playerControlls.isAttacking)
            return;

        StartAiming();
    }

    public virtual void UseButtonHold () {
        if (!skillActive() || playerControlls.isRolling || playerControlls.isGettingHit || playerControlls.isCastingSkill || playerControlls.isAttacking)
            return;
        
        KeepAiming();
    }

    public virtual void UseButtonUp () {
        if (!skillActive() || isCoolingDown || playerControlls.isRolling || playerControlls.isGettingHit || playerControlls.isCastingSkill || playerControlls.isAttacking)
            return;
        
        Shoot();
    }

    protected abstract void StartAiming ();
    protected abstract void KeepAiming ();
    public abstract void Shoot ();
    public abstract void CancelAiming ();
}
