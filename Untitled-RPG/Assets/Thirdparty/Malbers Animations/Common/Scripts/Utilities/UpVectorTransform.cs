using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    /// <summary>
    /// Uses a transform as the UpVector
    /// </summary>
    public class UpVectorTransform : MonoBehaviour
    {
        public GameObject source;
        private IGravity upVector;

        void Start()
        {
            upVector = source.GetComponentInChildren<IGravity>();
        }
        void Update()
        {
            if (upVector != null)
            {
                transform.up = upVector.UpVector;
            }
        }
    }
}
