using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    public class Limb : MonoBehaviour
    {
       // [Header("Bones to Align to the Attached Bones")]
        [Tooltip("The bones has to be on the same order than the Attached bones on the Dismember Script")]
        public Transform[] Bones;
    }
}
