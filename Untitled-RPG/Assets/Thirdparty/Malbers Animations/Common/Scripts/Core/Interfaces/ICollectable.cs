using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Interface to Identify Collectables Item</summary>
    public interface ICollectable
    {
        /// <summary>Applies the Item Dropped Logic</summary>
        void Drop();

        /// <summary>Applies the Item Picked Logic</summary>

        void Pick();

        /// <summary>Can the Collectable be droped or Picked?</summary>
        bool InCoolDown { get; }

        /// <summary> Is the Item Picked?</summary>
        bool IsPicked { get; set; }
    }
}