﻿using UnityEngine;

namespace UnityEditor.Experimental.TerrainAPI {
    [System.Serializable]
    public class ConcavityFilter : Filter {
        static readonly int RemapTexWidth = 1024;

        [SerializeField]
        private float m_ConcavityEpsilon = 1.0f; //kernel size
        [SerializeField]
        private float m_ConcavityScalar = 1.0f;  //toggles the compute shader between recessed (1.0f) & exposed (-1.0f) surfaces
        [SerializeField]
        private float m_ConcavityStrength = 1.0f;  //overall strength of the effect

        //We bake an AnimationCurve to a texture to control value remapping
        [SerializeField]
        private AnimationCurve m_ConcavityRemapCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        Texture2D m_ConcavityRemapTex = null;

        public enum ConcavityMode {
            Recessed = 0,
            Exposed = 1
        }

        Texture2D GetRemapTexture() {
            if (m_ConcavityRemapTex == null) {
                m_ConcavityRemapTex = new Texture2D(RemapTexWidth, 1, TextureFormat.RFloat, false, true);
                m_ConcavityRemapTex.wrapMode = TextureWrapMode.Clamp;

                TerrainTools.Utility.AnimationCurveToRenderTexture(m_ConcavityRemapCurve, ref m_ConcavityRemapTex);
            }
            
            return m_ConcavityRemapTex;
        }

        //Compute Shader resource helper
        ComputeShader m_ConcavityCS = null;
        ComputeShader GetComputeShader() {
            if (m_ConcavityCS == null) {
                m_ConcavityCS = (ComputeShader)Resources.Load("Concavity");
            }
            return m_ConcavityCS;
        }

        public override string GetDisplayName() {
            return "Concavity";
        }

        public override string GetToolTip()
        {
            return "Uses the concavity of a heightmap to mask the effect of a chosen Brush.";
        }

        public override void Eval(FilterContext fc) {
            ComputeShader cs = GetComputeShader();
            int kidx = cs.FindKernel("ConcavityMultiply");

            Texture2D remapTex = GetRemapTexture();

            cs.SetTexture(kidx, "In_BaseMaskTex", fc.sourceRenderTexture);
            cs.SetTexture(kidx, "In_HeightTex", fc.renderTextureCollection["heightMap"]);
            cs.SetTexture(kidx, "OutputTex", fc.destinationRenderTexture);
            cs.SetTexture(kidx, "RemapTex", remapTex);
            cs.SetInt("RemapTexRes", remapTex.width);
            cs.SetFloat("EffectStrength", m_ConcavityStrength);
            cs.SetVector("TextureResolution", new Vector4(fc.sourceRenderTexture.width, fc.sourceRenderTexture.height, m_ConcavityEpsilon, m_ConcavityScalar));

            //using 1s here so we don't need a multiple-of-8 texture in the compute shader (probably not optimal?)
            cs.Dispatch(kidx, fc.sourceRenderTexture.width, fc.sourceRenderTexture.height, 1);
        }

        public override void DoGUI(Rect rect) {

            //Precaculate dimensions
            float epsilonLabelWidth = GUI.skin.label.CalcSize(epsilonLabel).x;
            float modeLabelWidth = GUI.skin.label.CalcSize(modeLabel).x;
            float strengthLabelWidth = GUI.skin.label.CalcSize(strengthLabel).x;
            float curveLabelWidth = GUI.skin.label.CalcSize(curveLabel).x;
            float labelWidth = Mathf.Max(Mathf.Max(Mathf.Max(modeLabelWidth, epsilonLabelWidth), strengthLabelWidth), curveLabelWidth) + 4.0f;

            //Concavity Mode Drop Down
            Rect modeRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, EditorGUIUtility.singleLineHeight);
            ConcavityMode mode = m_ConcavityScalar > 0.0f ? ConcavityMode.Recessed : ConcavityMode.Exposed;
            switch(EditorGUI.EnumPopup(modeRect, mode)) {
                case ConcavityMode.Recessed:
                    m_ConcavityScalar = 1.0f;
                    break;
                case ConcavityMode.Exposed:
                    m_ConcavityScalar = -1.0f;
                    break;
            }

            //Strength Slider
            Rect strengthLabelRect = new Rect(rect.x, modeRect.yMax, labelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(strengthLabelRect, strengthLabel);
            Rect strengthSliderRect = new Rect(strengthLabelRect.xMax, strengthLabelRect.y, rect.width - labelWidth, strengthLabelRect.height);
            m_ConcavityStrength = EditorGUI.Slider(strengthSliderRect, m_ConcavityStrength, 0.0f, 1.0f);

            //Epsilon (kernel size) Slider
            Rect epsilonLabelRect = new Rect(rect.x, strengthSliderRect.yMax, labelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(epsilonLabelRect, epsilonLabel);
            Rect epsilonSliderRect = new Rect(epsilonLabelRect.xMax, epsilonLabelRect.y, rect.width - labelWidth, epsilonLabelRect.height);
            m_ConcavityEpsilon = EditorGUI.Slider(epsilonSliderRect, m_ConcavityEpsilon, 1.0f, 20.0f);

            //Value Remap Curve
            Rect curveLabelRect = new Rect(rect.x, epsilonSliderRect.yMax, labelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(curveLabelRect, curveLabel);
            Rect curveRect = new Rect(curveLabelRect.xMax, curveLabelRect.y, rect.width - labelWidth, curveLabelRect.height);

            EditorGUI.BeginChangeCheck();
            m_ConcavityRemapCurve = EditorGUI.CurveField(curveRect, m_ConcavityRemapCurve);
            if(EditorGUI.EndChangeCheck()) {
                Vector2 range = TerrainTools.Utility.AnimationCurveToRenderTexture(m_ConcavityRemapCurve, ref m_ConcavityRemapTex);
            }
        }

        public override float GetElementHeight() {
            return EditorGUIUtility.singleLineHeight * 5;
        }

        private static GUIContent strengthLabel = EditorGUIUtility.TrTextContent("Strength", "Controls the strength of the masking effect.");
        private static GUIContent epsilonLabel = EditorGUIUtility.TrTextContent("Feature Size", "Specifies the scale of Terrain features that affect the mask. This determines the size of features to which to apply the effect.");
        private static GUIContent modeLabel = EditorGUIUtility.TrTextContent("Mode");
        private static GUIContent curveLabel = EditorGUIUtility.TrTextContent("Remap Curve", "Remaps the concavity input before computing the final mask.");
    }
}