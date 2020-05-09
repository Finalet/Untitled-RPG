using UnityEngine;

namespace MalbersAnimations
{
    /// <summary> Used on the New Controller</summary>
    public interface IMDamage
    {
        Vector3 HitDirection { get; set; }

        /// <summary>Calls the Damage Directional Logic </summary>
        void Damage(int ID, int index);
    }

    public interface IMHitLayer
    {  
        /// <summary>Layers to Hit</summary>
        LayerMask HitLayer { get; set; }

        /// <summary>What to do with the Triggers ... Ignore them? Use them?</summary>
        QueryTriggerInteraction TriggerInteraction { get; set; }
    }


    public static class Damager
    {
        public static void SetDamage(Vector3 direction, Transform Root)
        {
            var enemy = Root.GetComponentInChildren<IMDamage>();                             //Get the Animal on the Other collider
            if (enemy != null)                                                               //if the other does'nt have the Damagable Interface dont send the Damagable stuff
            {
                enemy.HitDirection = direction;
                enemy.Damage(0,-1);
            }
        }
    }
}