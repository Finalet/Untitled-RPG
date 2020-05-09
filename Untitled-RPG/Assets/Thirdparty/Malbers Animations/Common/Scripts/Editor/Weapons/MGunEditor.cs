using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MalbersAnimations.Weapons
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MGun))]
    public class MGunEditor : MWeaponEditor
    {
        MGun myGun;
        SerializedProperty bulletHole, isAutomatic,ammoInChamber, Ammo, ClipSize, BulletHoleTime;

        private void OnEnable()
        {
            SetOnEnable();
            myGun = (MGun)target;
            bulletHole = serializedObject.FindProperty("bulletHole");
            isAutomatic = serializedObject.FindProperty("isAutomatic");
            ammoInChamber = serializedObject.FindProperty("ammoInChamber");
            Ammo = serializedObject.FindProperty("Ammo");
            ClipSize = serializedObject.FindProperty("ClipSize");
            BulletHoleTime = serializedObject.FindProperty("BulletHoleTime");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Guns Weapons Properties");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                CommonWeaponProperties();

                GunCustomInspector();

                SoundsList();
                EventList();
                CheckWeaponID();
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Gun Inspector");
              EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }

        public override void UpdateSoundHelp()
        {
            SoundHelp = "0:Draw   1:Store   2:Shoot   3:Reload   4:Empty";
        }

        protected override string CustomEventsHelp()
        {
            return "\n\n On Fire Gun: Invoked when the weapon is fired \n(Vector3: the Aim direction of the rider), \n\n On Hit: Invoked when the Weapon Fired and hit something \n(Transform: the gameobject that was hitted) \n\n On Aiming: Invoked when the Rider is Aiming or not \n\n On Reload: Invoked when Reload";
        }

        protected virtual void GunCustomInspector()
        {

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(isAutomatic, new GUIContent("Is Automatic", "one shot at the time or Automatic"));
            EditorGUILayout.PropertyField(ammoInChamber, new GUIContent("Ammo in Chamber", "Current ammo in the chamber"));
            EditorGUILayout.PropertyField(Ammo, new GUIContent("Total Ammo", "Total ammo for the wapon"));
            EditorGUILayout.PropertyField(ClipSize, new GUIContent("Clip Size", "Total of Ammo that can be shoot before reloading"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(bulletHole, new GUIContent("Bullet Hole ", "Bullet Hole Prefab"));
            EditorGUILayout.PropertyField(BulletHoleTime, new GUIContent("Bulle Hole Time", "Time before destroying the decal"));
            EditorGUILayout.EndVertical();

        }


        public override void CustomEvents()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFire"), new GUIContent("On Fire Gun"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHit"), new GUIContent("On Hit Something"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAiming"), new GUIContent("On Aiming"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnReload"), new GUIContent("On Reload"));
        }

    }
}