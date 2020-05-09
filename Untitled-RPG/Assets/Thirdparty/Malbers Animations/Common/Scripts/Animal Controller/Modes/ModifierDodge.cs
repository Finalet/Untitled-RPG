using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Mode Modifier/Directional Dodge")]
    public class ModifierDodge : ModeModifier
    {
        /// <summary>True: Dodge only Left or right?\nFalse: All Directios</summary>
        [Tooltip("True: Dodge only Left or right?\nFalse: All Directios")]
         public BoolReference horizontal = new BoolReference(true);
        /// <summary>Apply Extra movement to the Dodge</summary>
        [Tooltip("Apply Extra movement to the Dodge")]
        public BoolReference MoveDodge = new BoolReference(true);

        /// <summary>How Much it will mode if Move Dodge is enabled</summary>
        [Tooltip("How Much it will mode if Move Dodge is enabled")]
        public FloatReference DodgeDistance = new FloatReference(1);

        private bool left;

        public override void OnModeEnter(Mode mode)
        {
            if (horizontal)
            {
                var Horizontal = mode.Animal.MovementAxis.x;
                left = Horizontal < 0;
                mode.AbilityIndex = left ? 1 : 2;
               // Debug.Log(mode.AbilityIndex.Value);
            }
        }

        public override void OnModeMove(Mode mode, AnimatorStateInfo stateinfo, Animator anim, int LayerIndex)
        {
            if (MoveDodge/* && !anim.IsInTransition(LayerIndex)*/)
            {
                var animal = mode.Animal;

                if (horizontal)
                {
                    animal.transform.position += animal.Right *  animal.DeltaTime * DodgeDistance * (left ? -1 : 1);
                }
                else
                {
                    var Direction = animal.MovementAxis.normalized;

                    animal.transform.position += Direction * animal.DeltaTime * DodgeDistance;
                }
            }
        }
    }
}