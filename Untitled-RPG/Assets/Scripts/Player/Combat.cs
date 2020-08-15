using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    Animator animator;

    void Start() {  
        animator = GetComponent<Animator>();
    }

    void Update() {
        SetAnimationSpeed();
    }

    void SetAnimationSpeed () {
        animator.SetFloat("AttackSpeed", GetComponent<Characteristics>().attackSpeed.x);
        animator.SetFloat("CastingSpeed", GetComponent<Characteristics>().castingSpeed.x);
    }



#region Voids for skills

    public void SkillHit(AnimationEvent skillID) {
        switch (skillID.intParameter) {
            case 0: AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<Slash>().Hit();
                break;
            case 1: AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<Dash>().Hit(skillID.floatParameter);
                break;
            case 2: AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<StrongAttack>().Hit();
                break;
            case 3: AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<KO>().Hit(skillID.floatParameter);
                break;
            case 6: AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<StoneHit>().ApplyDamage();
                break;
            case 9: AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<Fireball>().FireProjectile();
                break;
            case 11: AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<Hailstone>().FireProjectile();
                break;
            case 12: 
                if (skillID.floatParameter == 1)
                    AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<PowerSphere>().SpawnSphere();
                else if (skillID.floatParameter == 0)
                    AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<PowerSphere>().ShootSphere();
                break;
            case 15: AssetHolder.instance.getSkill(skillID.intParameter).GetComponent<Armageddon>().StartHell();
                break;
        }
    }

#endregion
}
