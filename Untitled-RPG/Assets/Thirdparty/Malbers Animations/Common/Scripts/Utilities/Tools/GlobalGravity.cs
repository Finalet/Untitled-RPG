using System.Collections; 
using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    /// <summary>
    /// Stores the Value of a Gameobjcet down direction to a Scriptable Vector3 variable
    /// </summary>
    [AddComponentMenu("Malbers/Utilities/Tools/Global Gravity")]
    public class GlobalGravity : MonoBehaviour
    {
        [RequiredField] public Vector3Var Gravity;
        void Update()
        {
            if (Gravity != null) Gravity.Value = -transform.up;
        }
    }
}
