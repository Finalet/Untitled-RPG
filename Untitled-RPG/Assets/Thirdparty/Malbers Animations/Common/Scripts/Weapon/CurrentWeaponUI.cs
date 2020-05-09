using UnityEngine;
using UnityEngine.UI;

namespace MalbersAnimations.HAP
{
    public class CurrentWeaponUI : MonoBehaviour
    {
        public Text WeaponName;

        public void UIWeaponName(GameObject weaponName)
        {
            WeaponName.text = WeaponName != null ? (weaponName.name.Replace("(Clone)", "")) : "None";
        }
    }
}