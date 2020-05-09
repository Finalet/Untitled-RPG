using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    public class GlobalGravity : MonoBehaviour
    {
        public Vector3Var Gravity;
        void Update()
        {
           if (Gravity != null) Gravity.Value = -transform.up;
        }
    }
}
