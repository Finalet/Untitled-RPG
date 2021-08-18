using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    [CreateAssetMenu(menuName = "Malbers Animations/Preset/Capsule Collider", order = 200)]
    public class CapsuleColliderPreset : ScriptableObject
    {
       public OverrideCapsuleCollider modifier;

        public void Modify(CapsuleCollider collider) => modifier.Modify(collider);
    }
}