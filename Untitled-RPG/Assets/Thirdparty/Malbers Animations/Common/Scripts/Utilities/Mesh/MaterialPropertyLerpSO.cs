using UnityEngine;
using System.Collections;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Extras/Material Property Lerp", order = 2100)]
    public class MaterialPropertyLerpSO : ScriptableCoroutine
    {
        [Tooltip("Index of the Material")]
        public IntReference materialIndex = new IntReference();
        public FloatReference time = new FloatReference(1f);
        public AnimationCurve curve = new AnimationCurve(MTools.DefaultCurve);


        public StringReference propertyName;
        public MaterialPropertyType propertyType = MaterialPropertyType.Float;
       
        public FloatReference FloatValue = new FloatReference(1f);
        public Color ColorValue = Color.white;
        [ColorUsage(true, true)]
        public Color ColorHDRValue = Color.white;


        internal Dictionary<Renderer, Material> StartValue;

        public virtual void LerpSharedMaterial(Renderer mesh)
        {
            IEnumerator ICoroutine = null;
            switch (propertyType)
            {
                case MaterialPropertyType.Float:
                    ICoroutine = LerperFloat(mesh,true);
                    break;
                case MaterialPropertyType.Color:
                   ICoroutine = LerperColor(mesh,true, ColorValue);
                    break;
                case MaterialPropertyType.HDRColor:
                    ICoroutine = LerperColor(mesh,true, ColorHDRValue);
                    break;
                default:
                    break;

            }
            StartCoroutine(mesh, ICoroutine);
        }

        [System.Obsolete("Lerp is Obsolete, use LerpMaterial(Renderer) instead")]
        public virtual void Lerp(Renderer mesh) => LerpMaterial(mesh);


        public virtual void LerpMaterial(Renderer mesh)
        {
            IEnumerator ICoroutine = null;
            switch (propertyType)
            {
                case MaterialPropertyType.Float:
                    ICoroutine = LerperFloat(mesh, false);
                    break;
                case MaterialPropertyType.Color:
                    ICoroutine = LerperColor(mesh, false, ColorValue);
                    break;
                case MaterialPropertyType.HDRColor:
                    ICoroutine = LerperColor(mesh, false, ColorHDRValue);
                    break;
                default:
                    break;

            }
            StartCoroutine(mesh, ICoroutine);
        }

        IEnumerator LerperFloat(Renderer mesh, bool shared)
        {
            float elapsedTime = 0;
            var mat = shared ? mesh.sharedMaterials[materialIndex] :  mesh.materials[materialIndex];


            while (elapsedTime <= time)
            {
                float value = curve.Evaluate(elapsedTime / time);
                mat.SetFloat(propertyName, value * FloatValue);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            mat.SetFloat(propertyName, curve.Evaluate(curve.keys[curve.keys.Length - 1].time));

            yield return null;
            Stop(mesh);
        }


        IEnumerator LerperColor(Renderer mesh, bool shared, Color FinalColor)
        {
            float elapsedTime = 0;

            var mat = shared ? mesh.sharedMaterials[materialIndex] : mesh.materials[materialIndex];


            Color StartingColor = mat.GetColor(propertyName);
            Color ElapsedColor;

            while (elapsedTime <= time)
            {
                float value = curve.Evaluate(elapsedTime / time);

                ElapsedColor = Color.LerpUnclamped(StartingColor, FinalColor, value);

                mat.SetColor(propertyName, ElapsedColor);
               
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            mat.SetColor(propertyName, Color.LerpUnclamped(StartingColor, FinalColor, curve.Evaluate(curve.keys[curve.keys.Length-1].time)));

            yield return null;

            Stop(mesh);
        }

        internal override void CleanCoroutine()
        {
            base.CleanCoroutine();
            StartValue = null;
        }

    }

    [System.Serializable]
    public class MaterialPropertyInternal
    {
        public string propertyName;
        public MaterialPropertyType propertyType = MaterialPropertyType.Float;
        public float FloatValue = 1f;
        public Color ColorValue = Color.white;
        [ColorUsage(true, true)]
        public Color ColorHDRValue = Color.white;

        [HideInInspector] public bool isFloat; 
        [HideInInspector] public bool isColor; 
        [HideInInspector] public bool isColorHDR; 
    }

    public enum MaterialPropertyType
    {
        Float,
        Color,
        HDRColor
    }

    //Inspector

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(MaterialPropertyLerpSO)),UnityEditor.CanEditMultipleObjects]
    public class MaterialPropertyLerpSOEditor : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty propertyName, materialIndex, propertyType, time, FloatValue, ColorValue, ColorHDRValue, curve;//, UseMaterialPropertyBlock, shared;

        private void OnEnable()
        {
            propertyName = serializedObject.FindProperty("propertyName");
            materialIndex = serializedObject.FindProperty("materialIndex");
            propertyType = serializedObject.FindProperty("propertyType");
            time = serializedObject.FindProperty("time");
            FloatValue = serializedObject.FindProperty("FloatValue");
            ColorValue = serializedObject.FindProperty("ColorValue");
            ColorHDRValue = serializedObject.FindProperty("ColorHDRValue");
            curve = serializedObject.FindProperty("curve");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(propertyName);
            UnityEditor.EditorGUILayout.PropertyField(materialIndex);
            UnityEditor.EditorGUILayout.PropertyField(time);

            UnityEditor.EditorGUILayout.PropertyField(propertyType);

            var pType = (MaterialPropertyType)propertyType.intValue;

            switch (pType)
            {
                case MaterialPropertyType.Float:
                    UnityEditor.EditorGUILayout.PropertyField(FloatValue);
                    break;
                case MaterialPropertyType.Color:
                    UnityEditor.EditorGUILayout.PropertyField(ColorValue);
                    break;
                case MaterialPropertyType.HDRColor:
                    UnityEditor.EditorGUILayout.PropertyField(ColorHDRValue);
                    break;
                default:
                    break;
            }


            UnityEditor.EditorGUILayout.PropertyField(curve);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

  
}

