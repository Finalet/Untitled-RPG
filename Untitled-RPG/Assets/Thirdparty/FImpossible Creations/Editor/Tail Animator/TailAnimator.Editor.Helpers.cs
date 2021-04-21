using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class FTailAnimator2_Editor
    {
        // RESOURCES ----------------------------------------

        public static Texture2D _TexTailAnimIcon { get { if (__texTailAnimIcon != null) return __texTailAnimIcon; __texTailAnimIcon = Resources.Load<Texture2D>("Tail Animator/Tail Animator Icon Small"); return __texTailAnimIcon; } }
        private static Texture2D __texTailAnimIcon = null;
        public static Texture2D _TexWavingIcon { get { if (__texWaving != null) return __texWaving; __texWaving = Resources.Load<Texture2D>("Tail Animator/WavingIcon"); return __texWaving; } }
        private static Texture2D __texWaving = null;
        public static Texture2D _TexPartialBlendIcon { get { if (__texPartBlendIcon != null) return __texPartBlendIcon; __texPartBlendIcon = Resources.Load<Texture2D>("Tail Animator/PartialBlendIcon"); return __texPartBlendIcon; } }
        private static Texture2D __texPartBlendIcon = null;
        public static Texture2D _TexIKIcon { get { if (__texIKIcon != null) return __texIKIcon; __texIKIcon = Resources.Load<Texture2D>("Tail Animator/IKIcon"); return __texIKIcon; } }
        private static Texture2D __texIKIcon = null;
        public static Texture2D _TexDeflIcon { get { if (__texDeflIcon != null) return __texDeflIcon; __texDeflIcon = Resources.Load<Texture2D>("Tail Animator/Deflection"); return __texDeflIcon; } }
        private static Texture2D __texDeflIcon = null;
        private static Texture curveIcon { get { if (_curveIcon == null) _curveIcon = FGUI_Resources.Tex_Curve; /*EditorGUIUtility.IconContent("AudioLowPassFilter Icon").image;*/ return _curveIcon; } }
        private static Texture _curveIcon;
        private static Texture _TexWindIcon { get { if (_windIcon == null) _windIcon = Resources.Load<Texture2D>("Tail Animator/Wind"); return _windIcon; } }
        private static Texture _windIcon;

        private static UnityEngine.Object _manualFile;

        private static GUIStyle smallStyle { get { if (_smallStyle == null) _smallStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic }; return _smallStyle; } }
        private static GUIStyle _smallStyle;

        // HELPER VARIABLES ----------------------------------------

        private TailAnimator2 Get { get { if (_get == null) _get = target as TailAnimator2; return _get; } }
        private TailAnimator2 _get;

        private string topWarning = "";
        private float topWarningAlpha = 0f;

        static bool drawDefaultInspector = false;
        //private Color limitsC = new Color(1f, 1f, 1f, 0.88f);
        private Color c;
        private Color bc;
        private Color defaultValC = new Color(1f, 1f, 1f, 0.825f);


        public List<SkinnedMeshRenderer> skins;
        SkinnedMeshRenderer largestSkin;
        Animator animator;
        Animation animation;


        /// <summary>
        /// Trying to deep find skinned mesh renderer
        /// </summary>
        private void FindComponents()
        {
            if (skins == null) skins = new List<SkinnedMeshRenderer>();

            foreach (var t in Get.transform.GetComponentsInChildren<Transform>())
            {
                SkinnedMeshRenderer s = t.GetComponent<SkinnedMeshRenderer>(); if (s) skins.Add(s);
                if (!animator) animator = t.GetComponent<Animator>();
                if (!animator) if (!animation) animation = t.GetComponent<Animation>();
            }

            if ((skins != null && largestSkin != null) && (animator != null || animation != null)) return;

            if (Get.transform != Get.transform)
            {
                foreach (var t in Get.transform.GetComponentsInChildren<Transform>())
                {
                    SkinnedMeshRenderer s = t.GetComponent<SkinnedMeshRenderer>(); if (!skins.Contains(s)) if (s) skins.Add(s);
                    if (!animator) animator = t.GetComponent<Animator>();
                    if (!animator) if (!animation) animation = t.GetComponent<Animation>();
                }
            }

            // Searching in parent
            if (skins.Count == 0)
            {
                Transform lastParent = Get.transform;

                while (lastParent != null)
                {
                    if (lastParent.parent == null) break;
                    lastParent = lastParent.parent;
                }

                foreach (var t in lastParent.GetComponentsInChildren<Transform>())
                {
                    SkinnedMeshRenderer s = t.GetComponent<SkinnedMeshRenderer>(); if (!skins.Contains(s)) if (s) skins.Add(s);
                    if (!animator) animator = t.GetComponent<Animator>();
                    if (!animator) if (!animation) animation = t.GetComponent<Animation>();
                }
            }

            if (skins.Count > 1)
            {
                largestSkin = skins[0];
                for (int i = 1; i < skins.Count; i++)
                    if (skins[i].bones.Length > largestSkin.bones.Length)
                        largestSkin = skins[i];
            }
            else
                if (skins.Count > 0) largestSkin = skins[0];

        }


        /// <summary>
        /// Checking if transform is child of choosed root bone parent transform
        /// </summary>
        bool IsChildOf(Transform child, Transform rootParent)
        {
            Transform tParent = child;
            while (tParent != null && tParent != rootParent)
            {
                tParent = tParent.parent;
            }

            if (tParent == null) return false; else return true;
        }


        /// <summary>
        /// Checking if transform is child of choosed root bone parent transform
        /// </summary>
        Transform GetLastChild(Transform rootParent)
        {
            Transform tChild = rootParent;
            while (tChild.childCount > 0) tChild = tChild.GetChild(0);
            return tChild;
        }


        /// <summary>
        /// Getting editor selected objects with tail animators to apply changes to multiple tail animator objects
        /// </summary>
        private List<TailAnimator2> GetSelectedTailAnimators()
        {
            List<TailAnimator2> anims = new List<TailAnimator2>();

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                TailAnimator2 t = Selection.gameObjects[i].GetComponent<TailAnimator2>();
                if (t) if (!anims.Contains(t)) anims.Add(t);
            }

            lastSelected = anims;
            return anims;
        }
        List<TailAnimator2> lastSelected;

    }
}
