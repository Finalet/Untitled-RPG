using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Weapons
{
    /// <summary>All the Setup of the Combat Abilities are scripted on the Children of this class</summary>
    public abstract class OldIKProfile : ScriptableObject
    {
        /// <summary>Called on the Late Update of the Rider Combat Script </summary>
        public virtual void LateUpdate_IK(IMWeaponOwner RC) { }

        /// <summary> Stuff Set in the OnAnimatorIK </summary>
        public virtual void OnAnimator_IK(IMWeaponOwner RC) { }
    }
}