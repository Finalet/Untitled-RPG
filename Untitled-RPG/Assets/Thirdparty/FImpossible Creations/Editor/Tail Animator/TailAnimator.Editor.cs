using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    [CustomEditor(typeof(TailAnimator2))]
    [CanEditMultipleObjects]
    public partial class FTailAnimator2_Editor : Editor
    {
        [MenuItem("CONTEXT/TailAnimator2/Switch displaying header bar")]
        private static void HideFImpossibleHeader(MenuCommand menuCommand)
        {
            int current = EditorPrefs.GetInt("FTailHeader", 1);
            if (current == 1) current = 0; else current = 1;
            EditorPrefs.SetInt("FTailHeader", current);
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Spine Animator Inspector");

            serializedObject.Update();

            TailAnimator2 Get = (TailAnimator2)target;
            string title = drawDefaultInspector ? " Default Inspector" : (" " + Get._editor_Title);

            if (EditorPrefs.GetInt("FTailHeader", 1) == 1)
            {
                HeaderBoxMain(title, ref Get.DrawGizmos, ref drawDefaultInspector, _TexTailAnimIcon, Get, 27);
            }
            else
                GUILayout.Space(4);

            if (drawDefaultInspector)
                DrawDefaultInspector();
            else
                DrawNewGUI();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawCategoryButton(TailAnimator2.ETailCategory target, Texture icon, string lang)
        {
            if (Get._Editor_Category == target) GUI.backgroundColor = new Color(0.1f, 1f, 0.2f, 1f);

            int height = 28;
            int lim = 360;
            if (choosedLang == ELangs.русский) lim = 390;

            if (EditorGUIUtility.currentViewWidth > lim)
            {
                if (GUILayout.Button(new GUIContent("  " + Lang(lang), icon), FGUI_Resources.ButtonStyle, GUILayout.Height(height))) Get._Editor_Category = target;
            }
            else
                if (GUILayout.Button(new GUIContent(icon, Lang(lang)), FGUI_Resources.ButtonStyle, GUILayout.Height(height))) Get._Editor_Category = target;

            GUI.backgroundColor = bc;
        }

        void DrawCategoryButton(TailAnimator2.ETailFeaturesCategory target, Texture icon, string lang)
        {
            if (Get._Editor_FeaturesCategory == target) GUI.backgroundColor = new Color(0.1f, 1f, 0.2f, 1f);

            int height = 24;
            int lim = 360;
            if (choosedLang == ELangs.русский) lim = 390;

            if (EditorGUIUtility.currentViewWidth > lim)
            {
                if (GUILayout.Button(new GUIContent("  " + Lang(lang), icon), FGUI_Resources.ButtonStyle, GUILayout.Height(height))) Get._Editor_FeaturesCategory = target;
            }
            else
            if (GUILayout.Button(new GUIContent(icon, Lang(lang)), FGUI_Resources.ButtonStyle, GUILayout.Height(height))) Get._Editor_FeaturesCategory = target;

            GUI.backgroundColor = bc;
        }


        void DrawNewGUI()
        {
            #region Preparations for unity versions and skin

            c = Color.Lerp(GUI.color * new Color(0.8f, 0.8f, 0.8f, 0.7f), GUI.color, Mathf.InverseLerp(0f, 0.15f, Get.TailAnimatorAmount));
            bc = GUI.backgroundColor;

            RectOffset zeroOff = new RectOffset(0, 0, 0, 0);
            float substr = .18f; float bgAlpha = 0.08f; if (EditorGUIUtility.isProSkin) { bgAlpha = 0.1f; substr = 0f; }

#if UNITY_2019_3_OR_NEWER
            int headerHeight = 22;
#else
            int headerHeight = 25;
#endif

            Get._editor_IsInspectorViewingColliders = Get._Editor_FeaturesCategory == TailAnimator2.ETailFeaturesCategory.Collisions;
            Get._editor_IsInspectorViewingIncludedColliders = drawInclud && Get._editor_IsInspectorViewingColliders;

            Get.RefreshTransformsList();

            #endregion


            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            DrawCategoryButton(TailAnimator2.ETailCategory.Setup, FGUI_Resources.Tex_GearSetup, "Setup");
            DrawCategoryButton(TailAnimator2.ETailCategory.Tweak, FGUI_Resources.Tex_Sliders, "Tweak");
            DrawCategoryButton(TailAnimator2.ETailCategory.Features, FGUI_Resources.Tex_Module, "Features");
            DrawCategoryButton(TailAnimator2.ETailCategory.Shaping, FGUI_Resources.Tex_Repair, "Shaping");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);

            switch (Get._Editor_Category)
            {
                case TailAnimator2.ETailCategory.Setup:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.7f - substr, .7f - substr, 0.7f - substr, bgAlpha), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(Lang("Main Setup"), true, FGUI_Resources.Tex_GearSetup, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawSetup();
                    GUILayout.EndVertical();
                    break;

                case TailAnimator2.ETailCategory.Tweak:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.3f - substr, .4f - substr, 1f - substr, bgAlpha), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(Lang("Tweak Animation"), true, FGUI_Resources.Tex_Sliders, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawTweaking();
                    GUILayout.EndVertical();
                    break;

                case TailAnimator2.ETailCategory.Features:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(.4f - substr, 1f - substr, .8f - substr, bgAlpha * 0.6f), Vector4.one * 3, 3));
                    //FGUI_Inspector.HeaderBox(Lang("Additional Modules"), true, FGUI_Resources.Tex_Module, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawAdditionalFeatures();
                    GUILayout.EndVertical();
                    break;

                case TailAnimator2.ETailCategory.Shaping:
                    GUILayout.BeginVertical(FGUI_Inspector.Style(zeroOff, zeroOff, new Color(1f - substr, .55f - substr, .55f - substr, bgAlpha * 0.5f), Vector4.one * 3, 3));
                    FGUI_Inspector.HeaderBox(Lang("Additional Shaping"), true, FGUI_Resources.Tex_Repair, headerHeight, headerHeight - 1, LangBig());
                    Tab_DrawShaping();
                    GUILayout.EndVertical();
                    break;
            }

            GUILayout.Space(2f);
        }

    }
}
