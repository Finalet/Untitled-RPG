using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.HAP
{
    /// <summary> Rider Combat Mode</summary>
    [RequireComponent(typeof(MRider)/*,typeof(Aim)*/)]
    public partial class RiderCombat: MonoBehaviour , IAnimatorListener
    {
        ///This was left blank intentionally
        /// Callbacks: all the public functions and methods
        /// Logic: all Combat logic is there, Equip, Unequip, Aim Mode...
        /// Variables: well.. that
    }
}