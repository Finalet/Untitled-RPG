using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Ragdoll/Limb")] 
    public class Limb : MonoBehaviour
    {
        [Tooltip("The bones has to be on the same order than the Attached bones on the Dismember Script")]
        public Transform[] Bones;
    }
}
