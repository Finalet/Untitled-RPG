using JetBrains.Annotations;
using MalbersAnimations.Weapons;
using System.Dynamic;
using UnityEngine;

namespace MalbersAnimations
{
    [System.Serializable]
    [UnityEngine.CreateAssetMenu(menuName = "Malbers Animations/ID/Holster", fileName = "New Holster ID", order = -1000)]
    public class HolsterID : IDs { }


    [System.Serializable]
    public class Holster
    {
        public HolsterID ID;
        public int Index;
        /// <summary> Transform Reference for the holster</summary>
#pragma warning disable CA2235 // Mark all non-serializable fields
        [Tooltip(" Transform Reference for the holster")]
        public Transform Transform;

        /// <summary> Transform Reference for the holster</summary>
        [Tooltip("Weapon GameObject asociated to the Holster")]
        public MWeapon Weapon;
#pragma warning restore CA2235 // Mark all non-serializable fields

        public int GetID => ID != null ? ID.ID : 0;

        public void SetWeapon(MWeapon weap) => Weapon = weap;
        public void SetWeapon(GameObject weap) => Weapon = weap.GetComponent<MWeapon>();

        public bool PrepareWeapon()
        {
            if (Weapon)
            {
                if (Weapon.gameObject.IsPrefab()) //if it is a prefab then instantiate it!!
                {
                    if (Transform.childCount > 0)
                    {
                        Object.Destroy(Transform.GetChild(0).gameObject);
                    }

                    Weapon = GameObject.Instantiate(Weapon, Transform);
                    Weapon.name = Weapon.name.Replace("(Clone)", "");
                    Weapon.transform.SetLocalTransform(Weapon.HolsterOffset);
                }

                var mweapon = Weapon.GetComponent<MWeapon>();
                if (mweapon != null)  mweapon.Holster = ID;

                var IsCollectable =  Weapon.GetComponent<ICollectable>();
                IsCollectable?.Pick();

                return true;
            }
            return false;
        }


        public static implicit operator int(Holster reference) => reference.ID;
    }
}
