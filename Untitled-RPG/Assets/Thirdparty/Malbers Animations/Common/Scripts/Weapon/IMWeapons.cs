using UnityEngine;
using UnityEngine.Events;
using System;
using MalbersAnimations.Events;
using MalbersAnimations.HAP;

namespace MalbersAnimations.Weapons
{
    public interface IMWeapon : IMHitLayer
    {
        /// <summary> Is the Weapon available</summary>
        bool Active { get; set; }

        /// <summary>Gets the Weapon Type to match the RiderAbility</summary>
        WeaponID WeaponType { get; }

        /// <summary> Unique Weapon ID</summary>
        int WeaponID { get; }

        /// <summary>This will give information to use if on the animation for  Store and Draw weapon.</summary>
        WeaponHolder Holder { get; set; }

        Vector3 PositionOffset { get; }
        Vector3 RotationOffset { get; }

        bool RightHand { get; }
      
        float MinDamage  { get; }
        float MaxDamage { get; }
        float MinForce { get; set; }
        float MaxForce { get; set; } 

        /// <summary> Is the Weapon Equiped </summary>
        bool IsEquiped  { get; set; }

        /// <summary>Enables the Main Attack</summary>
        bool MainAttack { get; set; }

        /// <summary>Enables the Secondary Attack or Aiming</summary>
        bool SecondAttack { get; set; }

        void ResetWeapon();


        /// <summary>Owner of the Weapon</summary>
        Transform Owner { get; set; }

        /// <summary>Play the sounds clips</summary>
        /// <param name="ID">ID is the index on the list of clips</param>
        void PlaySound(int ID);
    }

    public interface IMelee : IMWeapon
    {
        /// <summary>Set that  the melee Weapon can cause damage</summary>
        void CanDoDamage(bool value);
        /// <summary> Ask if the melee weapon can cause damage</summary>
        bool CanCauseDamage { get; set; }
    }

    public interface IBow : IMWeapon
    {
        /// <summary> Instace of the Arrow Equipped</summary>
        GameObject ArrowInstance  { get; set; }

        /// <summary>Transform that holds the position of the string Knot.</summary>
        Transform KNot { get; }

        ///// <summary>Tension Limit for the Bow</summary>
        //float TensionLimit { get; set; }

        /// <summary>Transform that holds the position that the head of the arrow rest</summary>
        Transform ArrowPoint { get; }
        /// <summary>Time in seconds to bend the bow </summary>
        float HoldTime { get;  }

        /// <summary>Release the arrow to fly and hit something </summary>
        /// <param name="Direction"> Direction for the Arrow</param>
        void ReleaseArrow(Vector3 Direction);
        /// <summary> Instantiate an arrow ready to shooot</summary>
        void EquipArrow();

        /// <summary> Destroy the Arrow in case of Unequip</summary>
        void DestroyArrow();

        /// <summary>Rotate and modify the bow Bones to bend it from Min = 0 to Max = 1</summary>
        /// <param name="normalizedTime">0 = Relax, 1 = Stretch</param>
        void BendBow(float normalizedTime);

        /// <summary>Restore the Bow Knot to its Initial Position</summary>
        void RestoreKnot();
    }

    public interface IArrow
    {
        LayerMask HitMask { get; set; }

        /// <summary>What to do with the Triggers ... Ignore them? Use them?</summary>
        QueryTriggerInteraction TriggerInteraction { get; set; }

        /// <summary>Distance from the Tip to the End of the Arrow</summary>
        float TailOffset { get; set; }
        float Damage { get; set; }

        void ShootArrow(float force, Vector3 Direction);
    }

    public interface IGun : IMWeapon
    {
        /// <summary>Is the Weapon Automatic</summary>
        bool IsAutomatic { get; set; }
        /// <summary>Is the weapon is on the Aiming State </summary>
        bool IsAiming { get; set; }
        
        /// <summary> Total Ammo of the gun</summary>
        int TotalAmmo { get; set; }

        /// <summary> Ammo in Chamber</summary>
        int AmmoInChamber { get; set; }
       
        /// <summary> Fires the Weapon in a RayCastHit</summary>
        void FireProyectile(RaycastHit Direction);
        
        /// <summary> Make all the Calcultations to Restore ammo in the Chamber</summary>
        /// <returns>True = if it can reload</returns>
        bool Reload();
    } 
}
