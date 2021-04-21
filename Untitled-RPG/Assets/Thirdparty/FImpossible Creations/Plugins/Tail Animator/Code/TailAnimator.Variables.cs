using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2 : UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {

        #region Hierarchy Icon

        public string EditorIconPath { get { if (PlayerPrefs.GetInt("AnimsH", 1) == 0) return ""; else return "Tail Animator/Tail Animator Icon Small"; } }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }

        #endregion

        #region Enums

        public enum EFDeltaType { DeltaTime, SmoothDeltaTime, UnscaledDeltaTime, FixedDeltaTime, SafeDelta }
        public enum EAnimationStyle { Quick, Accelerating, Linear }

        #endregion

        public enum ETailCategory { Setup, Tweak, Features, Shaping }
        public ETailCategory _Editor_Category = ETailCategory.Setup;
        public enum ETailFeaturesCategory { Main, Collisions, IK, Experimental }
        public ETailFeaturesCategory _Editor_FeaturesCategory = ETailFeaturesCategory.Main;

        public bool DrawGizmos = true;

        [Tooltip("First bone of tail motion chain")]
        public Transform StartBone;
        [Tooltip("Finish bone of tail motion chain")]
        public Transform EndBone;

        [Tooltip("Adjusting end point for end tail bone motion")]
        public Vector3 EndBoneJointOffset = Vector3.zero;

        public List<Transform> _TransformsGhostChain;
        public int _GhostChainInitCount = -1;

        /// <summary> Initialization method controll flag </summary>
		protected bool initialized = false;
        public bool IsInitialized { get { return initialized; } }
        [Tooltip("Target FPS update rate for Tail Animator.\n\nIf you want Tail Animator to behave the same in low/high fps, set this value for example to 60.\nIt also can help optimizing if your game have more than 60 fps.")]
        public int UpdateRate = 0;

        [Tooltip("If your character Unity's Animator have update mode set to 'Animate Physics' you should enable it here too")]
        public bool AnimatePhysics = false;

        [Tooltip("When using target fps rate you can interpolate coordinates for smoother effect when object with tail is moving a lot")]
        public bool InterpolateRate = false;

        [Tooltip("Simulating tail motion at initiation to prevent jiggle start")]
        public bool Prewarm = false;

        /// <summary> For custom coding if you want to manipulate tail motion weight in additional way </summary>
        internal float OverrideWeight = 1f;
        /// <summary> Multiplier for tail motion weight computed from different conditions (max distance, override weight etc.) </summary>
        protected float conditionalWeight = 1f;

        protected bool collisionInitialized = false;
        protected bool forceRefreshCollidersData = false;
        Vector3 previousWorldPosition;

        /// <summary> Parent transform of first tail transform </summary>
        protected Transform rootTransform;

        protected bool preAutoCorrect = false;



        void OnValidate()
        {
            if (UpdateRate < 0) UpdateRate = 0;

            if (Application.isPlaying)
            {
                RefreshSegmentsColliders();
                //lastCurving = Curving; lastCurving.x += 0.001f;
                if (UseIK) IK_ApplyLimitBoneSettings();
                if (UseWind) TailAnimatorWind.Refresh(this);
            }

            if (UsePartialBlend) { ClampCurve(BlendCurve, 0f, 1f, 0f, 1f); }
        }

        /// <summary>
        /// Getting list of transform for Tail Animator using Start and End Bone Transform guides
        /// </summary>
        public void GetGhostChain()
        {
            if (_TransformsGhostChain == null) _TransformsGhostChain = new System.Collections.Generic.List<Transform>();

            if (EndBone == null)
            {
                // Just traight forward path through children
                _TransformsGhostChain.Clear();

                Transform tChild = StartBone;
                if (tChild == null) tChild = transform;

                _TransformsGhostChain.Add(tChild);

                while (tChild.childCount > 0)
                {
                    tChild = tChild.GetChild(0);
                    if (!_TransformsGhostChain.Contains(tChild)) _TransformsGhostChain.Add(tChild);
                }

                //if (!_TransformsGhostChain.Contains(tChild)) 
                _GhostChainInitCount = _TransformsGhostChain.Count;
            }
            else // Going through parents of 'End Bone' to 'Start Bone'
            {

                System.Collections.Generic.List<Transform> newTrs = new System.Collections.Generic.List<Transform>();

                Transform sBone = StartBone; if (sBone == null) sBone = transform;
                Transform tParent = EndBone;

                newTrs.Add(tParent);

                while (tParent != null && tParent != StartBone)
                {
                    tParent = tParent.parent;
                    if (!newTrs.Contains(tParent)) newTrs.Add(tParent);
                }

                if (tParent == null) // No parent of startbone!
                {
                    Debug.Log("[Tail Animator Editor] " + EndBone.name + " is not child of " + sBone.name + "!");
                    Debug.LogError("[Tail Animator Editor] " + EndBone.name + " is not child of " + sBone.name + "!");
                }
                else
                {
                    if (!newTrs.Contains(tParent)) newTrs.Add(tParent);

                    _TransformsGhostChain.Clear();
                    _TransformsGhostChain = newTrs;

                    _TransformsGhostChain.Reverse();
                    _GhostChainInitCount = _TransformsGhostChain.Count;
                }
            }
        }

    }
}