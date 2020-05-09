using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Tags for Malbers</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Tag")]
    public class Tag : ScriptableObject
    {
        [SerializeField] private int id;

        /// <summary>Hash code for the Tag Name</summary>
        public int ID => id;

        public static implicit operator int(Tag reference)
        {
            return reference.ID;
        }

        /// <summary> Re Calculate the ID</summary>
        private void OnEnable()
        {
            id = name.GetHashCode();
        }
    }
}