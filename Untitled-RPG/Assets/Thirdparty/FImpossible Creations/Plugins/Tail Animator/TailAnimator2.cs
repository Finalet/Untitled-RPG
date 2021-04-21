using UnityEngine;

namespace FIMSpace.FTail
{
    [AddComponentMenu("FImpossible Creations/Tail Animator 2")]
    [DefaultExecutionOrder(-4)]
    public partial class TailAnimator2 : MonoBehaviour
    {
        /// -------- THIS IS PARTIAL CLASS - REST OF THE CODE IN SEPARATED .cs FILES -------- \\\


        #region Public Inspector Variables

        [Tooltip("Blending Slithery - smooth & soft tentacle like movement (value = 1)\nwith more stiff & springy motion (value = 0)\n\n0: Stiff somewhat like tree branch\n1: Soft like squid tentacle / Animal tail")]
        [Range(0f, 1.2f)]
        public float Slithery = 1f;

        [Tooltip("How curly motion should be applied to tail segments")]
        [Range(0f, 1f)]
        public float Curling = 0.5f;

        [Tooltip("Elastic spring effect making motion more 'meaty'")]
        [Range(0f, 1f)]
        public float Springiness = 0.0f;

        [Tooltip("If you want to limit stretching/gumminess of position motion when object moves fast. Recommended adjust to go with it under 0.3 value.\nValue = 1: Unlimited stretching")]
        [Range(0f, 1f)]
        public float MaxStretching = .375f;

        [Tooltip("Limiting max rotation angle for each tail segment")]
        [FPD_Suffix(1f, 181f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°")]
        public float AngleLimit = 181f;
        [Tooltip("If you need specific axis to be limited.\nLeave unchanged to limit all axes.")]
        public Vector3 AngleLimitAxis = Vector3.zero;
        [Tooltip("If you want limit axes symmetrically leave this parameter unchanged, if you want limit one direction of axis more than reversed, tweak this parameter")]
        public Vector2 LimitAxisRange = Vector2.zero;
        [Tooltip("If limiting shouldn't be too rapidly performed")]
        [Range(0f, 1f)]
        public float LimitSmoothing = 0.5f;

        [Tooltip("If your object moves very fast making tail influenced by speed too much then you can controll it with this parameter")]
        [FPD_Suffix(0f, 1.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped)]
        public float MotionInfluence = 1f;

        [Tooltip("If first bone of chain should also be affected with whole chain")]
        public bool IncludeParent = true;
        [Tooltip("By basic algorithm of Tail Animator different sized tails with different number of bones would animate with different bending thanks to this toggle every setup bends in very similar amount.\n\nShort tails will bend more and longer oner with bigger amount of bones less with this option enabled.")]
        [Range(0f, 1f)]
        public float UnifyBendiness = 0f;

        [Tooltip("Reaction Speed is defining how fast tail segments will return to target position, it gives animation more underwater/floaty feeling if it's lower")]
        [Range(0f, 1f)]
        public float ReactionSpeed = .9f;
        [Tooltip("Sustain is similar to reaction speed in reverse, but providing sustain motion effect when increased")]
        [Range(0f, 1f)]
        public float Sustain = 0f;
        [Tooltip("Rotation speed is defining how fast tail segments will return to target rotation, it gives animation more lazy feeling if it's lower")]
        [Range(0f, 1f)]
        public float RotationRelevancy = 1f;

        [Tooltip("Smoothing motion values change over time style to be applied for 'Reaction Speed' and 'Rotation Relevancy' parameters")]
        public EAnimationStyle SmoothingStyle = EAnimationStyle.Accelerating;

        [Tooltip("Delta time type to be used by algorithm")]
        public EFDeltaType DeltaType = EFDeltaType.SafeDelta;

        //[Tooltip("If tail motion should cooperate with keyframed animation if your model is not animated then disable this")]
        //public bool SyncWithAnimator = true;

        //[Tooltip("IF your model is not animated in any other way than Tail Animator then you can toggle it to avoid some unneccesary operations for optimization")]
        //public bool NotAnimated = false;

        [Tooltip("Useful when you use other components to affect bones hierarchy and you want this component to follow other component's changes\n\nIt can be really useful when working with 'Spine Animator'")]
        public bool UpdateAsLast = true;
        [Tooltip("Checking if keyframed animation has some empty keyframes which could cause unwanted twisting errors")]
        public bool DetectZeroKeyframes = true;
        [Tooltip("Initializing Tail Animator after first frames of game to not initialize with model's T-Pose but after playing some other animation")]
        public bool StartAfterTPose = true;

        [Tooltip("If you want Tail Animator to stop computing when choosed mesh is not visible in any camera view (editor's scene camera is detecting it too)")]
        public Renderer OptimizeWithMesh;

        [Tooltip("Blend Source Animation (keyframed / unanimated) and Tail Animator")]
        [FPD_Suffix(0f, 1f)]
        public float TailAnimatorAmount = 1f;

        [Tooltip("Removing transforms hierachy structure to optimize Unity's calculations on Matrixes.\nIt can give very big boost in performance for long tails but it can't work with animated models!")]
        public bool DetachChildren = false;

        [Tooltip("If tail movement should not move in depth you can use this parameter")]
        /// <summary> 0: Unlimited   1: X is Depth  2: Y is Depth  3: Z is Depth </summary>
        public int Axis2D = 0;

        [Tooltip("[Experimental: Works only with Slithery Blend set to >= 1] Making each segment go to target pose in front of parent segment creating new animation effect")]
        [Range(-1f ,1f)]
        public float Tangle = 0f;

        [Tooltip("Making tail animate also roll rotation like it was done in Tail Animator V1 ! Use Rotation Relevancy Parameter (set lower than 0.5) !")]
        public bool AnimateRoll = false;

        public Transform BaseTransform { get { if (_baseTransform) return _baseTransform; else if (_TransformsGhostChain != null) if (_TransformsGhostChain.Count > 0) _baseTransform = _TransformsGhostChain[0]; if (_baseTransform != null) return _baseTransform; return transform; } }
        private Transform _baseTransform;

        #endregion

        /// <summary>
        /// Initialize component for correct work
        /// </summary>
        void Start()
        {
            if (UpdateAsLast) { enabled = false; enabled = true; }
            if (StartAfterTPose) startAfterTPoseCounter = 6; else Init();
        }


#if UNITY_2019_1_OR_NEWER
        /// <summary>
        /// Setting curves which can't be created automatically when component is added to new object
        /// </summary>
        void Reset()
        {
            Keyframe key1 = new Keyframe(0f, 0f, 0.1f, 0.1f, 0.0f, 0.5f);
            Keyframe key2 = new Keyframe(1f, 1f, 5f, 0f, 0.1f, 0.0f);
            DeflectionFalloff = new AnimationCurve(new Keyframe[2] { key1, key2 });
        }
#endif

        /// <summary>
        /// Between Update() and LateUpdate() occurs Unity Animator's changes to transforms
        /// We are using transform addRotation in Tail Animator algorithms and if bones are not animated
        /// we would overrotate bones every frame, we setting here initial local coords to prevent it
        /// If bones are animated it's rotations will be overrided after Update()
        /// </summary>
        void Update()
        {
            CheckIfTailAnimatorShouldBeUpdated();

            // Preparations for target update
            DeltaTimeCalculations();

            if (AnimatePhysics) return;
            if (!updateTailAnimator) return;

            PreCalibrateBones();
        }


        /// <summary>
        /// Sames as in Update() but for models with 'Animate Physics' enabled
        /// </summary>
        void FixedUpdate()
        {
            if (!AnimatePhysics) return;
            if (!updateTailAnimator) return;

            PreCalibrateBones();
        }


        /// <summary>
        /// Updating bones after unity Animators [execution order -> Update() : UnityAnimators() : LateUpdate()]
        /// </summary>
        void LateUpdate()
        {
            if (!updateTailAnimator) return;

            ExpertParamsUpdate();
            ShapingParamsUpdate();

            // Preparing tail animator for calculating motion
            CalibrateBones();

            UpdateTailAlgorithm();

            // Shaping / expert parameters refresh + motion influence
            EndUpdate();
        }


        void EndUpdate()
        {
            ShapingEndUpdate();
            ExpertCurvesEndUpdate();
            previousWorldPosition = BaseTransform.position;
        }

    }
}