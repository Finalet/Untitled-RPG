using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class FTailAnimator2_Editor
    {
        Transform startBone { get { return Get.StartBone; } set { Get.StartBone = value; } }


        protected SerializedProperty sp_SmoothingStyle;
        protected SerializedProperty sp_ReactSpeed;
        protected SerializedProperty sp_RotRelev;
        protected SerializedProperty sp_MaxStretching;
        protected SerializedProperty sp_Springiness;
        protected SerializedProperty sp_SpringinessCurling;
        protected SerializedProperty sp_Slithery;
        protected SerializedProperty sp_UpdateRate;
        protected SerializedProperty sp_StartBone;
        protected SerializedProperty sp_Sustain;
        protected SerializedProperty sp_UseWind;
        protected SerializedProperty sp_WindEffectPower;
        protected SerializedProperty sp_WindTurbulencePower;
        protected SerializedProperty sp_WindWorldNoisePower;
        protected SerializedProperty sp_Boost;

        protected SerializedProperty sp_UpdateAsLast;
        protected SerializedProperty sp_SyncWithAnimator;
        protected SerializedProperty sp_DetectZeroKeyframes;
        protected SerializedProperty sp_StartAfterTPose;
        protected SerializedProperty sp_EndBoneJointOffset;

        protected SerializedProperty sp_LengthMultiplier;
        protected SerializedProperty sp_Unify;
        protected SerializedProperty sp_AnimateRoll;
        protected SerializedProperty sp_Axis2D;
        protected SerializedProperty sp_smoothdelta;
        protected SerializedProperty sp_useCollision;
        protected SerializedProperty sp_collMode;
        protected SerializedProperty sp_curving;
        protected SerializedProperty sp_DynamicWorldCollidersInclusion;
        protected SerializedProperty sp_gravity;
        protected SerializedProperty sp_colScale;
        protected SerializedProperty sp_colScaleMul;
        protected SerializedProperty sp_colBoxDim;
        protected SerializedProperty sp_colDiffFact;
        protected SerializedProperty sp_colWithOther;
        protected SerializedProperty sp_colIgnored;
        protected SerializedProperty sp_colSameLayer;
        protected SerializedProperty sp_colCustomLayer;
        protected SerializedProperty sp_colAddRigs;
        protected SerializedProperty sp_RootRotationOffset;
        protected SerializedProperty sp_RootPositionOffset;
        protected SerializedProperty sp_CollisionSpace;
        protected SerializedProperty sp_ReflectCollision;
        protected SerializedProperty sp_DetailedCollision;
        protected SerializedProperty sp_IncludedColliders;
        protected SerializedProperty sp_RigidbodyMass;

        protected SerializedProperty sp_AngleLimit;
        protected SerializedProperty sp_TailAnimatorAmount;
        protected SerializedProperty sp_AngleLimitAxis;
        protected SerializedProperty sp_LimitSmoothing;
        protected SerializedProperty sp_MotionInfluence;
        protected SerializedProperty sp_CollidersType;
        protected SerializedProperty sp_OptimizeWithMesh;

        protected SerializedProperty sp_wavType;
        protected SerializedProperty sp_useWav;
        protected SerializedProperty sp_cosAd;
        protected SerializedProperty sp_wavSp;
        protected SerializedProperty sp_wavRa;
        protected SerializedProperty sp_wavAx;
        protected SerializedProperty sp_tailRotOff;
        protected SerializedProperty sp_altWave;
        protected SerializedProperty sp_AnimatePhysics;
        protected SerializedProperty sp_FixedCycle;
        protected SerializedProperty sp_CollisionSlippery;

        protected SerializedProperty sp_UsePosSpeedCurve;
        protected SerializedProperty sp_UseRotSpeedCurve;
        protected SerializedProperty sp_UseSlitCurve;
        protected SerializedProperty sp_UseCurvingCurve;
        protected SerializedProperty sp_UseLengthMulCurve;
        protected SerializedProperty sp_UseGravCurv;
        protected SerializedProperty sp_PosCurve;
        protected SerializedProperty sp_RotCurve;
        protected SerializedProperty sp_SlitCurve;
        protected SerializedProperty sp_CurvCurve;
        protected SerializedProperty sp_LengthMulCurve;
        protected SerializedProperty sp_GravityCurve;
        protected SerializedProperty sp_InclusionRadius;
        protected SerializedProperty sp_SpringCurve;
        protected SerializedProperty sp_UseSpringCurve;
        protected SerializedProperty sp_SlipperyCurve;
        protected SerializedProperty sp_UseSlipperyCurve;
        protected SerializedProperty sp_Curling;
        protected SerializedProperty sp_CurlingCurve;
        protected SerializedProperty sp_UseCurlingCurve;
        protected SerializedProperty sp_IgnoreMeshColliders;
        protected SerializedProperty sp_CollideWithDisabledColliders;

        protected SerializedProperty sp_BlendCurve;
        private SerializedProperty sp_DeltaType;

        //private SerializedProperty sp_SimulationSpeed;
        private SerializedProperty sp_Optim;
        private SerializedProperty sp_Prewarm;


        private SerializedProperty sp_IKTarget;
        private SerializedProperty sp_IKBlend;
        private SerializedProperty sp_IKAnimatorBlend;
        private SerializedProperty sp_IKAutoWeights;
        private SerializedProperty sp_IKweightCurve;
        private SerializedProperty sp_IKAutoAngleLimit;
        private SerializedProperty sp_IKAutoAngleLimits;
        private SerializedProperty sp_IKReactionQuality;
        private SerializedProperty sp_IKSmoothing;
        private SerializedProperty sp_IKBaseReactionWeight;
        private SerializedProperty sp_IKContinous;
        private SerializedProperty sp_IKLimitSettings;
        private SerializedProperty sp_UseIK;
        private SerializedProperty sp_usePartialBlend;
        private SerializedProperty sp_Detach;

        private SerializedProperty sp_Deflection;
        private SerializedProperty sp_DeflectionStartAngle;
        private SerializedProperty sp_DeflectOnlyCollisions;
        private SerializedProperty sp_DeflectionSmooth;
        private SerializedProperty sp_DeflectionFalloff;

        private SerializedProperty sp_UseMaxDistance;
        private SerializedProperty sp_DistanceFrom;
        private SerializedProperty sp_MaximumDistance;
        private SerializedProperty sp_MaxOutDistanceFactor;
        private SerializedProperty sp_DistanceWithoutY;
        private SerializedProperty sp_DistanceMeasurePoint;
        private SerializedProperty sp_FadeDuration;


        //private SerializedProperty sp_IKMaxStretching;
        //private SerializedProperty sp_IKStretchCurve;

        protected virtual void OnEnable()
        {
            FGUI_Finders.ResetFinders();

            sp_StartBone = serializedObject.FindProperty("StartBone");
            sp_SmoothingStyle = serializedObject.FindProperty("SmoothingStyle");
            sp_ReactSpeed = serializedObject.FindProperty("ReactionSpeed");
            sp_RotRelev = serializedObject.FindProperty("RotationRelevancy");
            sp_Sustain = serializedObject.FindProperty("Sustain");
            sp_MaxStretching = serializedObject.FindProperty("MaxStretching");
            sp_Springiness = serializedObject.FindProperty("Springiness");
            sp_SpringinessCurling = serializedObject.FindProperty("SpringyCurling");
            sp_Slithery = serializedObject.FindProperty("Slithery");
            sp_UpdateRate = serializedObject.FindProperty("UpdateRate");
            sp_DeltaType = serializedObject.FindProperty("DeltaType");
            sp_TailAnimatorAmount = serializedObject.FindProperty("TailAnimatorAmount");
            sp_DetectZeroKeyframes = serializedObject.FindProperty("DetectZeroKeyframes");
            sp_UpdateAsLast = serializedObject.FindProperty("UpdateAsLast");
            sp_SyncWithAnimator = serializedObject.FindProperty("SyncWithAnimator");
            sp_StartAfterTPose = serializedObject.FindProperty("StartAfterTPose");
            sp_EndBoneJointOffset = serializedObject.FindProperty("EndBoneJointOffset");
            sp_Boost = serializedObject.FindProperty("Tangle");
            sp_collMode = serializedObject.FindProperty("CollisionMode");

            sp_AnimatePhysics = serializedObject.FindProperty("AnimatePhysics");
            sp_FixedCycle = serializedObject.FindProperty("FixedCycle");
            sp_LengthMultiplier = serializedObject.FindProperty("LengthMultiplier");
            sp_smoothdelta = serializedObject.FindProperty("SafeDeltaTime");
            sp_useCollision = serializedObject.FindProperty("UseCollision");
            sp_curving = serializedObject.FindProperty("Curving");
            sp_DynamicWorldCollidersInclusion = serializedObject.FindProperty("DynamicWorldCollidersInclusion");
            sp_gravity = serializedObject.FindProperty("Gravity");
            sp_InclusionRadius = serializedObject.FindProperty("InclusionRadius");
            sp_colScale = serializedObject.FindProperty("CollidersScaleCurve");
            sp_colScaleMul = serializedObject.FindProperty("CollidersScaleMul");
            sp_colBoxDim = serializedObject.FindProperty("BoxesDimensionsMul");
            sp_colDiffFact = serializedObject.FindProperty("CollisionsAutoCurve");
            sp_colWithOther = serializedObject.FindProperty("CollideWithOtherTails");
            sp_RigidbodyMass = serializedObject.FindProperty("RigidbodyMass");
            sp_Detach = serializedObject.FindProperty("DetachChildren");


            sp_colIgnored = serializedObject.FindProperty("IgnoredColliders");
            sp_Unify = serializedObject.FindProperty("UnifyBendiness");
            sp_AnimateRoll = serializedObject.FindProperty("AnimateRoll");
            sp_Axis2D = serializedObject.FindProperty("Axis2D");
            sp_colSameLayer = serializedObject.FindProperty("CollidersSameLayer");
            sp_colCustomLayer = serializedObject.FindProperty("CollidersLayer");
            sp_colAddRigs = serializedObject.FindProperty("CollidersAddRigidbody");
            sp_RootRotationOffset = serializedObject.FindProperty("RootRotationOffset");
            sp_RootPositionOffset = serializedObject.FindProperty("RootPositionOffset");
            sp_CollisionSpace = serializedObject.FindProperty("CollisionSpace");
            sp_ReflectCollision = serializedObject.FindProperty("ReflectCollision");
            sp_IncludedColliders = serializedObject.FindProperty("IncludedColliders");
            sp_DetailedCollision = serializedObject.FindProperty("CheapCollision");
            sp_IncludedColliders = serializedObject.FindProperty("IncludedColliders");

            sp_AngleLimit = serializedObject.FindProperty("AngleLimit");
            sp_AngleLimitAxis = serializedObject.FindProperty("AngleLimitAxis");
            //sp_AngleLimitAxisTo = serializedObject.FindProperty("LimitAxisRange");
            sp_LimitSmoothing = serializedObject.FindProperty("LimitSmoothing");
            sp_MotionInfluence = serializedObject.FindProperty("MotionInfluence");
            sp_CollidersType = serializedObject.FindProperty("CollidersType");
            sp_OptimizeWithMesh = serializedObject.FindProperty("OptimizeWithMesh");

            sp_altWave = serializedObject.FindProperty("AlternateWave");
            sp_wavType = serializedObject.FindProperty("WavingType");
            sp_useWav = serializedObject.FindProperty("UseWaving");
            sp_cosAd = serializedObject.FindProperty("CosinusAdd");
            sp_wavSp = serializedObject.FindProperty("WavingSpeed");
            sp_wavRa = serializedObject.FindProperty("WavingRange");
            sp_wavAx = serializedObject.FindProperty("WavingAxis");
            sp_tailRotOff = serializedObject.FindProperty("RotationOffset");
            sp_BlendCurve = serializedObject.FindProperty("BlendCurve");
            sp_CollisionSlippery = serializedObject.FindProperty("CollisionSlippery");
            //sp_BlendChainValue = serializedObject.FindProperty("BlendChainValue");
            //sp_BlendChainFaloff = serializedObject.FindProperty("BlendChainFaloff");

            sp_UsePosSpeedCurve = serializedObject.FindProperty("UsePosSpeedCurve");
            sp_UseRotSpeedCurve = serializedObject.FindProperty("UseRotSpeedCurve");
            sp_UseSlitCurve = serializedObject.FindProperty("UseSlitheryCurve");
            sp_UseCurvingCurve = serializedObject.FindProperty("UseCurvingCurve");
            sp_UseLengthMulCurve = serializedObject.FindProperty("UseLengthMulCurve");
            sp_UseGravCurv = serializedObject.FindProperty("UseGravityCurve");
            sp_PosCurve = serializedObject.FindProperty("PosCurve");
            sp_RotCurve = serializedObject.FindProperty("RotCurve");
            sp_SlitCurve = serializedObject.FindProperty("SlitheryCurve");
            sp_CurvCurve = serializedObject.FindProperty("CurvCurve");
            sp_LengthMulCurve = serializedObject.FindProperty("LengthMulCurve");
            sp_InclusionRadius = serializedObject.FindProperty("InclusionRadius");
            sp_GravityCurve = serializedObject.FindProperty("GravityCurve");
            sp_SpringCurve = serializedObject.FindProperty("SpringCurve");
            sp_UseSpringCurve = serializedObject.FindProperty("UseSpringCurve");
            sp_UseSlipperyCurve = serializedObject.FindProperty("UseSlipperyCurve");
            sp_SlipperyCurve = serializedObject.FindProperty("SlipperyCurve");
            sp_UseCurlingCurve = serializedObject.FindProperty("UseCurlingCurve");
            sp_IgnoreMeshColliders = serializedObject.FindProperty("IgnoreMeshColliders");
            sp_CollideWithDisabledColliders = serializedObject.FindProperty("CollideWithDisabledColliders");
            sp_Curling = serializedObject.FindProperty("Curling");
            sp_CurlingCurve = serializedObject.FindProperty("CurlingCurve");
            sp_UseWind = serializedObject.FindProperty("UseWind");
            sp_WindEffectPower = serializedObject.FindProperty("WindEffectPower");
            sp_WindTurbulencePower = serializedObject.FindProperty("WindTurbulencePower");
            sp_WindWorldNoisePower = serializedObject.FindProperty("WindWorldNoisePower");
            //sp_SimulationSpeed = serializedObject.FindProperty("SimulationSpeed");
            sp_Optim = serializedObject.FindProperty("InterpolateRate");
            sp_Prewarm = serializedObject.FindProperty("Prewarm");

            sp_UseIK = serializedObject.FindProperty("UseIK");
            sp_usePartialBlend = serializedObject.FindProperty("UsePartialBlend");
            sp_IKTarget = serializedObject.FindProperty("IKTarget");
            sp_IKAutoWeights = serializedObject.FindProperty("IKAutoWeights");
            sp_IKweightCurve = serializedObject.FindProperty("IKReactionWeightCurve");
            sp_IKAutoAngleLimit = serializedObject.FindProperty("IKAutoAngleLimit");
            sp_IKAutoAngleLimits = serializedObject.FindProperty("IKAutoAngleLimits");
            sp_IKReactionQuality = serializedObject.FindProperty("IKReactionQuality");
            sp_IKSmoothing = serializedObject.FindProperty("IKSmoothing");
            sp_IKBaseReactionWeight = serializedObject.FindProperty("IKBaseReactionWeight");
            sp_IKContinous = serializedObject.FindProperty("IKContinousSolve");
            //sp_IKMaxStretching = serializedObject.FindProperty("IKMaxStretching");
            //sp_IKStretchCurve = serializedObject.FindProperty("IKStretchCurve");
            sp_IKLimitSettings = serializedObject.FindProperty("IKLimitSettings");
            sp_IKBlend = serializedObject.FindProperty("IKBlend");
            sp_IKAnimatorBlend = serializedObject.FindProperty("IKAnimatorBlend");

            sp_Deflection = serializedObject.FindProperty("Deflection");
            sp_DeflectionFalloff = serializedObject.FindProperty("DeflectionFalloff");
            sp_DeflectionSmooth = serializedObject.FindProperty("DeflectionSmooth");
            sp_DeflectionStartAngle = serializedObject.FindProperty("DeflectionStartAngle");
            sp_DeflectOnlyCollisions = serializedObject.FindProperty("DeflectOnlyCollisions");

            sp_UseMaxDistance = serializedObject.FindProperty("UseMaxDistance");
            sp_DistanceFrom = serializedObject.FindProperty("DistanceFrom");
            sp_MaximumDistance = serializedObject.FindProperty("MaximumDistance");
            sp_MaxOutDistanceFactor = serializedObject.FindProperty("MaxOutDistanceFactor");
            sp_DistanceWithoutY = serializedObject.FindProperty("DistanceWithoutY");
            sp_DistanceMeasurePoint = serializedObject.FindProperty("DistanceMeasurePoint");
            sp_FadeDuration = serializedObject.FindProperty("FadeDuration");


            FindComponents();
            Get.CheckForNullsInGhostChain();

            // First assignment
            if (!startBone)
            {
                if (skins.Count != 0)
                {
                    // If skins found ad this transform is part of any skeleton then this transform will be start bone
                    bool contains = false;

                    for (int i = 0; i < skins.Count; i++)
                    {
                        if (contains) break;

                        for (int b = 0; b < skins[i].bones.Length; b++)
                            if (skins[i].bones[b] == Get.transform)
                            { contains = true; break; }
                    }

                    if (contains)
                    {
                        startBone = Get.transform;
                        Get.GetGhostChain();
                    }
                    else
                    {
                        // If skins are found, we assigning first bone from it
                        startBone = largestSkin.bones[0];
                        Get.GetGhostChain();
                    }
                }

            }


            if (Get._TransformsGhostChain == null)
            {
                Get._TransformsGhostChain = new System.Collections.Generic.List<Transform>();
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }


            if (Get.UseIK || Get.UseWaving)
            {
                if (Get.UseWaving) drawWaving = true; else drawWaving = false;
            }

            if (Get.UnifyBendiness > 0f || Get.Sustain > 0f)
                drawTweakAdditional = true;

            if (((Get.ReactionSpeed >= 1f || Get.ReactionSpeed >= 0.99f) && !Get.UseRotSpeedCurve) && (Get.RotationRelevancy >= 1f && !Get.UseRotSpeedCurve))
                drawSmoothing = false;
            else
                if (Get.UseRotSpeedCurve || Get.UsePosSpeedCurve) drawSmoothing = true;

            if (Get.UseCollision) drawCollisions = true;

            if (Get.Deflection > Mathf.Epsilon) drawDefl = true;

            if (Get._editor_animatorViewedCounter < 1)
            {
                Get._editor_animatorViewedCounter++;

                if (startBone == null)
                {
                    topWarning = "No skinned bones found - assign 'Start Bone'";
                    topWarningAlpha = 1.5f;
                }
                else
                {
                    topWarning = "Automatically found start bone - please check if it is correct one";
                    topWarningAlpha = 1.85f;
                }
            }

            SetupLangs();
        }

        void OnDisable()
        {
            if (hideSkin) for (int i = 0; i < skins.Count; i++)
                {
                    skins[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
        }

    }
}
