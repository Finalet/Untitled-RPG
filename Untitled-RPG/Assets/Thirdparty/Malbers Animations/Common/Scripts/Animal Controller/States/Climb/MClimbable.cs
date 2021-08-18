using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    public interface IClimbable
    {
        /// <summary> When the character arrives to this surface it will automatically start to climb it </summary>
        bool Automatic { get; set; }

        /// <summary>Global Ladder Collider</summary>
        Collider ClimbCollider { get; }
    }

    /// <summary> Component to identify Climbable surfaces</summary>
    public class MClimbable : MonoBehaviour, IClimbable
    {
        public BoolReference m_Automatic = new BoolReference();
        private Collider m_ClimbCollider;

        public bool Automatic { get => m_Automatic.Value; set =>  m_Automatic.Value = value; }
        public Collider ClimbCollider { get => m_ClimbCollider; }

        private void Start()
        {
            m_ClimbCollider = GetComponent<Collider>();
        }
    }
}
