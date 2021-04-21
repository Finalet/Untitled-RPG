using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        public bool UseWind = false;
        [FPD_Suffix(0f, 2.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped)]
        public float WindEffectPower = 1f;
        [FPD_Suffix(0f, 2.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped)]
        public float WindTurbulencePower = 1f;
        [FPD_Suffix(0f, 1.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped)]
        public float WindWorldNoisePower = 0.5f;

        /// <summary> Wind power vector used by Tail Animator Wind component </summary>
        public Vector3 WindEffect = Vector3.zero;
    }
}