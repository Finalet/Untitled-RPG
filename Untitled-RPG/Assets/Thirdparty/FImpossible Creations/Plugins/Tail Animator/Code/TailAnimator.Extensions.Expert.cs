using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        public bool UseSlitheryCurve = false;
        [FPD_FixedCurveWindow(0, 0f, 1f, 1.2f, .1f, 0.8f, 1f, 0.9f)]
        public AnimationCurve SlitheryCurve = AnimationCurve.EaseInOut(0f, .75f, 1f, 1f);
        float lastSlithery = -1f;
        Keyframe[] lastSlitheryCurvKeys;

        public bool UseCurlingCurve = false;
        [FPD_FixedCurveWindow(0, 0f, 1f, 1f, .65f, 0.4f, 1f, 0.9f)]
        public AnimationCurve CurlingCurve = AnimationCurve.EaseInOut(0f, .7f, 1f, 0.3f);
        float lastCurling = -1f;
        Keyframe[] lastCurlingCurvKeys;

        public bool UseSpringCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.9f, 0.7f, 0.2f, 0.9f)]
        public AnimationCurve SpringCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
        float lastSpringiness = -1f;
        Keyframe[] lastSpringCurvKeys;

        public bool UseSlipperyCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.2f, 0.9f, 0.6f, 0.9f)]
        public AnimationCurve SlipperyCurve = AnimationCurve.EaseInOut(0f, .7f, 1f, 1f);
        float lastSlippery = -1f;
        Keyframe[] lastSlipperyCurvKeys;

        public bool UsePosSpeedCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, .2f, 1f, 0.3f, .9f)]
        public AnimationCurve PosCurve = AnimationCurve.EaseInOut(0f, .7f, 1f, 1f);
        float lastPosSpeeds = -1f;
        Keyframe[] lastPosCurvKeys;

        public bool UseRotSpeedCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.7f, 0.7f, 0.7f, 0.9f)]
        public AnimationCurve RotCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.9f);
        float lastRotSpeeds = -1f;
        Keyframe[] lastRotCurvKeys;


        [Tooltip("Spreading Tail Animator motion weight over bones")]
        public bool UsePartialBlend = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.2f, .5f, 0.85f)]
        public AnimationCurve BlendCurve = AnimationCurve.EaseInOut(0f, .95f, 1f, .45f);
        float lastTailAnimatorAmount = -1f;
        Keyframe[] lastBlendCurvKeys;
        TailSegment _ex_bone;


        void ExpertParamsUpdate()
        {
            Expert_UpdatePosSpeed();
            Expert_UpdateRotSpeed();
            Expert_UpdateSpringiness();
            Expert_UpdateSlithery();
            Expert_UpdateCurling();
            Expert_UpdateSlippery();
            Expert_UpdateBlending();
        }


        void ExpertCurvesEndUpdate()
        {
            lastPosSpeeds = ReactionSpeed;
            if (!UsePosSpeedCurve) if (lastPosCurvKeys != null) { lastPosCurvKeys = null; lastPosSpeeds += 0.001f; }

            lastRotSpeeds = RotationRelevancy;
            if (!UseRotSpeedCurve) if (lastRotCurvKeys != null) { lastRotCurvKeys = null; lastRotSpeeds += 0.001f; }

            lastSpringiness = Springiness;
            if (!UseSpringCurve) if (lastSpringCurvKeys != null) { lastSpringCurvKeys = null; lastSpringiness += 0.001f; }

            lastSlithery = Slithery;
            if (!UseSlitheryCurve) if (lastSlitheryCurvKeys != null) { lastSlitheryCurvKeys = null; lastSlithery += 0.001f; }

            lastCurling = Curling;
            if (!UseCurlingCurve) if (lastCurlingCurvKeys != null) { lastCurlingCurvKeys = null; lastCurling += 0.001f; }

            lastSlippery = CollisionSlippery;
            if (!UseSlipperyCurve) if (lastSlipperyCurvKeys != null) { lastSlipperyCurvKeys = null; lastSlippery += 0.001f; }

            lastTailAnimatorAmount = TailAnimatorAmount;
            if (!UsePartialBlend) if (lastBlendCurvKeys != null) { lastBlendCurvKeys = null; lastTailAnimatorAmount += 0.001f; }
        }


        void Expert_UpdatePosSpeed()
        {
            if (UsePosSpeedCurve)
            {
                //if (KeysChanged(PosCurve.keys, lastPosCurvKeys)) // for (int i = 0; i < TailBones.Count; i++) TailBones[i].PositionSpeed = GetValueFromCurve(i, PosCurve);
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.PositionSpeed = PosCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; } }
            }
            else
            {
                if (lastPosSpeeds != ReactionSpeed) // for (int i = 0; i < TailBones.Count; i++) TailBones[i].PositionSpeed = ReactionSpeed;
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.PositionSpeed = ReactionSpeed; _ex_bone = _ex_bone.ChildBone; } }
            }
        }


        void Expert_UpdateRotSpeed()
        {
            if (UseRotSpeedCurve)
            {
                //if (KeysChanged(RotCurve.keys, lastRotCurvKeys)) //for (int i = 0; i < TailBones.Count; i++) TailBones[i].RotationSpeed = GetValueFromCurve(i, RotCurve);
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.RotationSpeed = RotCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; } }
            }
            else
            {
                if (lastRotSpeeds != RotationRelevancy) //for (int i = 0; i < TailBones.Count; i++) TailBones[i].RotationSpeed = RotationRelevancy;
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.RotationSpeed = RotationRelevancy; _ex_bone = _ex_bone.ChildBone; } }
            }
        }


        void Expert_UpdateSpringiness()
        {
            if (UseSpringCurve)
            {
                //if (KeysChanged(SpringCurve.keys, lastSpringCurvKeys)) //for (int i = 0; i < TailBones.Count; i++) TailBones[i].Springiness = GetValueFromCurve(i, SpringCurve);
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.Springiness = SpringCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; } }
            }
            else
            {
                if (lastSpringiness != Springiness) //for (int i = 0; i < TailBones.Count; i++) TailBones[i].Springiness = Springiness;
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.Springiness = Springiness; _ex_bone = _ex_bone.ChildBone; } }
            }
        }


        void Expert_UpdateSlithery()
        {
            if (UseSlitheryCurve)
            {
                //if (KeysChanged(SlitheryCurve, lastSlitheryCurvKeys)) //for (int i = 0; i < TailBones.Count; i++) TailBones[i].Slithery = GetValueFromCurve(i, SlitheryCurve);
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.Slithery = SlitheryCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; } }
            }
            else
            {
                if (lastSlithery != Slithery)
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.Slithery = Slithery; _ex_bone = _ex_bone.ChildBone; } }
            }
        }


        void Expert_UpdateCurling()
        {
            if (UseCurlingCurve)
            {
                //if (KeysChanged(CurlingCurve.keys, lastCurlingCurvKeys)) //for (int i = 0; i < TailBones.Count; i++) TailBones[i].Curling = GetValueFromCurve(i, CurlingCurve);
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.Curling = CurlingCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; } }
            }
            else
            {
                if (lastCurling != Curling)
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.Curling = Curling; _ex_bone = _ex_bone.ChildBone; } }
            }
        }


        void Expert_UpdateSlippery()
        {
            if (UseSlipperyCurve)
            {
                //if (KeysChanged(SlipperyCurve.keys, lastSlipperyCurvKeys)) //for (int i = 0; i < TailBones.Count; i++) TailBones[i].Slippery = GetValueFromCurve(i, SlipperyCurve);
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.Slippery = SlipperyCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; } }
            }
            else
            {
                if (lastSlippery != CollisionSlippery) //for (int i = 0; i < TailBones.Count; i++) TailBones[i].Slippery = CollisionSlippery;
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.Slippery = CollisionSlippery; _ex_bone = _ex_bone.ChildBone; } }
            }
        }


        void Expert_UpdateBlending()
        {
            if (UsePartialBlend)
            {
                //if (KeysChanged(BlendCurve.keys, lastBlendCurvKeys)) // for (int i = 0; i < TailBones.Count; i++) TailBones[i].BlendValue = GetValueFromCurve(i, BlendCurve);//Mathf.Clamp(Mathf.Pow(GetValueFromCurve(i, BlendCurve), .3f /*3*/), 0f, 1.5f);
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.BlendValue = BlendCurve.Evaluate(_ex_bone.IndexOverlLength); _ex_bone = _ex_bone.ChildBone; } }
            }
            else
            {
                if (lastTailAnimatorAmount != TailAnimatorAmount)
                { _ex_bone = TailSegments[0]; while (_ex_bone != null) { _ex_bone.BlendValue = TailAnimatorAmount; _ex_bone = _ex_bone.ChildBone; } }
            }
        }

    }
}