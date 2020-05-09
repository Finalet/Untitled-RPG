using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Effect Modifiers/FireBall")]
    public class FireBallEffectM : EffectModifier
    {
        public float velocity = 300;
        Rigidbody rb;  
        IAim aim;

        public override void AwakeEffect(Effect effect){}
        
        public override void StartEffect(Effect effect)
        {
            rb = effect.Instance.GetComponent<Rigidbody>();         //Get the riggidbody of the effect
            aim = effect.Owner.GetComponent<IAim>();                //Check if the owner has lookAt

            effect.Instance.SendMessage("SetOwner", effect.Owner, SendMessageOptions.DontRequireReceiver);

            if (aim != null && aim.Active && !aim.Limited)
            {
                rb.AddForce(aim.AimDirection.normalized * velocity);     //If it has look at take the direction from te lookat
            }
            else
            {
                Animator ownerAnimator = effect.Owner.GetComponent<Animator>();
                Vector3 velocityv = ownerAnimator.velocity.normalized;
                

                if (ownerAnimator.velocity.magnitude < 0.1)
                {
                    velocityv = effect.Owner.transform.forward;
                }
                rb.AddForce(velocityv * velocity);
            }
        }
    }
}