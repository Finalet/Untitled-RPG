using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MalbersAnimations.Weapons
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MMelee))]
    public class MMeleeEditor : MWeaponEditor
    {
        SerializedProperty meleeCollider, OnCauseDamage, OnHit;

        void OnEnable()
        {
            SetOnEnable();
            meleeCollider = serializedObject.FindProperty("meleeCollider");
            OnCauseDamage = serializedObject.FindProperty("OnCauseDamage");
            OnHit = serializedObject.FindProperty("OnHit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Melee Weapons Properties");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                CommonWeaponProperties();
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(meleeCollider, new GUIContent("Melee Collider", "Gets the reference of where is the Melee Collider of this weapon (Not Always is in the same gameobject level)"));
                EditorGUILayout.EndVertical();

                SoundsList();

                EventList();

                CheckWeaponID();
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        public override void UpdateSoundHelp()
        {
            SoundHelp = "0:Draw   1:Store   2:Swing   3:Hit \n (Leave 3 Empty, add SoundByMaterial and Invoke 'PlayMaterialSound' for custom Hit sounds)";
        }

        public override void CustomEvents()
        {
            EditorGUILayout.PropertyField(OnCauseDamage, new GUIContent("On Cause Damage"));
            EditorGUILayout.PropertyField(OnHit, new GUIContent("On Hit Something"));
        }

    }
}