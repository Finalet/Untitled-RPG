using FIMSpace.FTools;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        // IK Definition
        public bool UseIK = false;
        bool ikInitialized = false;
        [SerializeField] FIK_CCDProcessor IK;

        // IK Parameters
        [Tooltip("Target object to follow by IK")]
        public Transform IKTarget;

        public bool IKAutoWeights = true;
        [Range(0f,1f)]
        public float IKBaseReactionWeight = .65f;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.2f, .5f, 0.85f)]
        public AnimationCurve IKReactionWeightCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, .25f);

        public bool IKAutoAngleLimits = true;
        //[Range(0f, 181f)]
        [FPD_Suffix(0, 181, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float IKAutoAngleLimit = 40f;
        [Tooltip("If ik process should work referencing to previously computed CCDIK pose (can be more precise but need more adjusting in weights and angle limits)")]
        public bool IKContinousSolve = false;

        [FPD_Suffix(0f,1f)]
        [Tooltip("How much IK motion sohuld be used in tail animator motion -> 0: turned off")]
        public float IKBlend = 1f;
        [FPD_Suffix(0f,1f)]
        [Tooltip("If syncing with animator then applying motion of keyframe animation for IK")]
        public float IKAnimatorBlend = 0.5f;

        [Range(1, 32)]
        [Tooltip("How much iterations should do CCDIK algorithm in one frame")]
        public int IKReactionQuality = 2;
        [Range(0f, 1f)]
        [Tooltip("Smoothing reactions in CCD IK algorithm")]
        public float IKSmoothing = .0f;
        [Range(0f, 1.5f)]
        public float IKMaxStretching = 0f;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.9f, .4f, 0.5f)]
        public AnimationCurve IKStretchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        public List<IKBoneSettings> IKLimitSettings;


        void InitIK()
        {
            IK = new FIK_CCDProcessor(_TransformsGhostChain.ToArray());

            if (IKAutoWeights) IK.AutoWeightBones(IKBaseReactionWeight); else IK.AutoWeightBones(IKReactionWeightCurve);
            if (IKAutoAngleLimits) IK.AutoLimitAngle(IKAutoAngleLimit, 4f + IKAutoAngleLimit / 15f);

            IK.Init(_TransformsGhostChain[0]);
            ikInitialized = true;

            IK_ApplyLimitBoneSettings();
        }


        void UpdateIK()
        {
            if (!ikInitialized) InitIK();
            if (IKBlend <= Mathf.Epsilon) return;

            if (IKTarget == null) IK.IKTargetPosition = TailSegments[TailSegments.Count - 1].ProceduralPosition;
            else IK.IKTargetPosition = IKTarget.position;

            IK.IKWeight = IKBlend;
            IK.SyncWithAnimator = IKAnimatorBlend; //else IK.SyncWithAnimator = 0f;
            IK.ReactionQuality = IKReactionQuality;
            IK.Smoothing = IKSmoothing;
            IK.MaxStretching = IKMaxStretching;
            IK.StretchCurve = IKStretchCurve;
            IK.ContinousSolving = IKContinousSolve;
            if (Axis2D == 3) IK.Use2D = true; else IK.Use2D = false;

            IK.Update();
        }
        

        /// <summary>
        /// FC: Helper class to manage ik bones settings
        /// </summary>
        [System.Serializable]
        public class IKBoneSettings
        {
            [Range(0f, 181f)]
            public float AngleLimit = 45f;
            [Range(0f, 181f)]
            public float TwistAngleLimit = 5f;
        }


        /// <summary>
        /// Applying changes to IK bones only when changes occuring
        /// </summary>
        public void IK_ApplyLimitBoneSettings()
        {
            if (!IKAutoAngleLimits)
            {
                if (IKLimitSettings.Count != _TransformsGhostChain.Count)
                    IK_RefreshLimitSettingsContainer();

                if (IK.IKBones.Length != IKLimitSettings.Count)
                {
                    Debug.Log("[TAIL ANIMATOR IK] Wrong IK bone count!");
                    return;
                }

                if (!IKAutoAngleLimits)
                {
                    for (int i = 0; i < IKLimitSettings.Count; i++)
                    {
                        IK.IKBones[i].AngleLimit = IKLimitSettings[i].AngleLimit;
                        IK.IKBones[i].TwistAngleLimit = IKLimitSettings[i].TwistAngleLimit;
                    }
                }
            }

            if ( ikInitialized) if (IKAutoWeights) IK.AutoWeightBones(IKBaseReactionWeight); else IK.AutoWeightBones(IKReactionWeightCurve);
            if (IKAutoAngleLimits) IK.AutoLimitAngle(IKAutoAngleLimit, 10f + IKAutoAngleLimit / 10f);
        }

        /// <summary>
        /// Generating new IK Limit Settings list with the same length as ghost transforms chain
        /// </summary>
        public void IK_RefreshLimitSettingsContainer()
        {
            IKLimitSettings = new List<IKBoneSettings>();
            for (int i = 0; i < _TransformsGhostChain.Count; i++)
            {
                IKLimitSettings.Add(new IKBoneSettings());
            }
        }

    }
}