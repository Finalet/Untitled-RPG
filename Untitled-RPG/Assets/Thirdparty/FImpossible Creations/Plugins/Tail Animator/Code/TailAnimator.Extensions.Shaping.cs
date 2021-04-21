using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        [Tooltip("Rotation offset for tail (just first (root) bone is rotated)")]
        public Quaternion RotationOffset = Quaternion.identity;


        [Tooltip("Rotate each segment a bit to create curving effect")]
        public Quaternion Curving = Quaternion.identity;
        [Tooltip("Spread curving rotation offset weight over tail segments")]
        public bool UseCurvingCurve = false;
        [FPD_FixedCurveWindow(0f, -1f, 1f, 1f, 0.75f, .75f, 0.75f, 0.85f)]
        public AnimationCurve CurvCurve = AnimationCurve.EaseInOut(0f, 0.75f, 1f, 1f);
        Quaternion lastCurving = Quaternion.identity;
        Keyframe[] lastCurvingKeys;

        [Tooltip("Make tail longer or shorter")]
        public float LengthMultiplier = 1f;
        [Tooltip("Spread length multiplier weight over tail segments")]
        public bool UseLengthMulCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 3f)]
        public AnimationCurve LengthMulCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
        float lastLengthMul = 1f;
        Keyframe[] lastLengthKeys;

        [Tooltip("Spread gravity weight over tail segments")]
        public bool UseGravityCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.85f, .35f, 0.25f, 0.85f)]
        [Tooltip("Spread gravity weight over tail segments")]
        public AnimationCurve GravityCurve = AnimationCurve.EaseInOut(0f, .65f, 1f, 1f);
        [Tooltip("Simulate gravity weight for tail logics")]
        public Vector3 Gravity = Vector3.zero;
        Vector3 lastGravity = Vector3.zero;
        Keyframe[] lastGravityKeys;


        void ShapingParamsUpdate()
        {
            Shaping_UpdateCurving();
            Shaping_UpdateGravity();
            Shaping_UpdateLengthMultiplier();
        }



        void Shaping_UpdateCurving()
        {
            if (!FEngineering.QIsZero(Curving))
            {
                if (UseCurvingCurve) // Operations with curve
                {
                    _ex_bone = TailSegments[0];
                    while (_ex_bone != null) { _ex_bone.Curving = Quaternion.LerpUnclamped(Quaternion.identity, Curving, CurvCurve.Evaluate(_ex_bone.IndexOverlLength)); _ex_bone = _ex_bone.ChildBone; }
                }
                else // Operations without curve
                {
                    if (!FEngineering.QIsSame(Curving, lastCurving))
                        for (int i = 0; i < TailSegments.Count; i++) TailSegments[i].Curving = Curving;
                }
            }
            else // Curving reset
            {
                if (!FEngineering.QIsSame(Curving, lastCurving))
                    for (int i = 0; i < TailSegments.Count; i++) TailSegments[i].Curving = Quaternion.identity;
            }
        }


        void Shaping_UpdateGravity()
        {
            if (!FEngineering.VIsZero(Gravity))
            {
                if (UseGravityCurve) // Operations with curve
                {
                    //if (!FEngineering.VIsSame(Gravity, lastGravity) || KeysChanged(GravityCurve.keys, lastGravityKeys))
                    //{
                    //    for (int i = 0; i < TailSegments.Count; i++)
                    //    {
                    //        TailSegments[i].Gravity = Gravity * GetValueFromCurve(i, GravityCurve) / 40f;
                    //        TailSegments[i].Gravity *= (1 + ((TailSegments[i].Index / 2f) * (1f - TailSegments[i].Slithery)));
                    //    }
                    //}

                    _ex_bone = TailSegments[0];
                    while (_ex_bone != null) { _ex_bone.Gravity = Gravity * 40f * GravityCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; }
                }
                else // Operations without curve
                {
                    if (!FEngineering.VIsSame(Gravity, lastGravity))
                        for (int i = 0; i < TailSegments.Count; i++)
                        {
                            TailSegments[i].Gravity = Gravity / 40f;
                            TailSegments[i].Gravity *= (1 + ((TailSegments[i].Index / 2f) * (1f - TailSegments[i].Slithery)));
                        }
                }
            }
            else // Gravity reset
            {
                if (!FEngineering.VIsSame(Gravity, lastGravity))
                {
                    for (int i = 0; i < TailSegments.Count; i++)
                    {
                        TailSegments[i].Gravity = Vector3.zero;
                        TailSegments[i].GravityLookOffset = Vector3.zero;
                    }
                }
            }
        }


        void Shaping_UpdateLengthMultiplier()
        {
            if (UseLengthMulCurve)
            {
                //if (lastLengthMul != LengthMultiplier || KeysChanged(LengthMulCurve.keys, lastLengthKeys))
                //{
                //    for (int i = 0; i < TailSegments.Count; i++) TailSegments[i].LengthMultiplier = /*LengthMultiplier */ GetValueFromCurve(i, LengthMulCurve);
                //}

                _ex_bone = TailSegments[0];
                while (_ex_bone != null) { _ex_bone.LengthMultiplier = LengthMulCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; }
            }
            else
            {
                if (lastLengthMul != LengthMultiplier)
                    for (int i = 0; i < TailSegments.Count; i++) TailSegments[i].LengthMultiplier = LengthMultiplier;
            }
        }



        /// <summary>
        /// Remembering previous parameters to avoid triggering full tail iterations every frame
        /// </summary>
        void ShapingEndUpdate()
        {
            lastCurving = Curving;
            if (!UseCurvingCurve) if (lastCurvingKeys != null) { lastCurvingKeys = null; lastCurving.x += 0.001f; }
            //if (UseCurvingCurve) lastCurvingKeys = CurvCurve.keys; else if (lastCurvingKeys != null) { lastCurvingKeys = null; lastCurving.x += 0.001f; }

            lastGravity = Gravity;
            if (!UseGravityCurve) if (lastGravityKeys != null) { lastGravityKeys = null; lastGravity.x += 0.001f; }
            //if (UseGravityCurve) lastGravityKeys = GravityCurve.keys; else if (lastGravityKeys != null) { lastGravityKeys = null; lastGravity.x += 0.001f; }

            lastLengthMul = LengthMultiplier;
            if (!UseLengthMulCurve) if (lastLengthKeys != null) { lastLengthKeys = null; lastLengthMul += 0.0001f; }
            //if (UseLengthMulCurve) lastLengthKeys = LengthMulCurve.keys; else if (lastLengthKeys != null) { lastLengthKeys = null; lastLengthMul += 0.0001f; }
        }

    }
}