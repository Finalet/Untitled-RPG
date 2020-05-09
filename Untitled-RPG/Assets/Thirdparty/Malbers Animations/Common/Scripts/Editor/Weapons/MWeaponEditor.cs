using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MalbersAnimations.Weapons
{

    public abstract class MWeaponEditor : Editor
    {
        protected MWeapon M;
        protected string SoundHelp;
        protected SerializedProperty 
            Sounds, WeaponSound, weaponType, weaponID, rightHand, AffectStat,
            minDamage, maxDamage, minForce, maxForce, holder, rotationOffset, positionOffset;
        bool offsets = true;

        protected void SetOnEnable()
        {
            M = (MWeapon)target;

            Sounds = serializedObject.FindProperty("Sounds");
            WeaponSound = serializedObject.FindProperty("WeaponSound");
            AffectStat = serializedObject.FindProperty("AffectStat");
            weaponType = serializedObject.FindProperty("weaponType");
            weaponID = serializedObject.FindProperty("weaponID");
            rightHand = serializedObject.FindProperty("rightHand");
            minDamage = serializedObject.FindProperty("minDamage");
            maxDamage = serializedObject.FindProperty("maxDamage");
            minForce = serializedObject.FindProperty("minForce");
            maxForce = serializedObject.FindProperty("maxForce");
            holder = serializedObject.FindProperty("holder");
            rotationOffset = serializedObject.FindProperty("rotationOffset");
            positionOffset = serializedObject.FindProperty("positionOffset");

        }

        protected void CommonWeaponProperties()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("active"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(weaponType, new GUIContent("Weapon Type", "Gets the Weapon Type to match the RiderAbility"));

                EditorGUILayout.BeginHorizontal();
                {
                    // EditorGUILayout.LabelField("Weapon ID", EditorStyles.label, (GUILayout.MinWidth(1)));
                    weaponID.intValue = EditorGUILayout.IntField("Weapon ID", weaponID.intValue, GUILayout.MinWidth(1));

                    if (GUILayout.Button("Generate", EditorStyles.miniButton, GUILayout.MinWidth(55)))
                    {
                        weaponID.intValue = Random.Range(10000, 99999);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(rightHand, new GUIContent(rightHand.boolValue ? "Right Hand" : "Left Hand", "Which Hand the weapon uses"));

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Damage Range", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 45;
                    EditorGUILayout.PropertyField(minDamage, new GUIContent("Min", "Minimun Damage"));
                    EditorGUILayout.PropertyField(maxDamage, new GUIContent("Max", "Minimun Damage"));
                    EditorGUIUtility.labelWidth = 0;
                    if (M.MaxDamage < M.MinDamage)
                    {
                        M.MaxDamage = M.MinDamage;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(AffectStat, new GUIContent("Affect Stat", "Which Stat should Affect when the Weapon Hit something"),true);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Force Range", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 45;
                    EditorGUILayout.PropertyField(minForce, new GUIContent("Min", "Minimun Force"));
                    EditorGUILayout.PropertyField(maxForce, new GUIContent("Max", "Maximun Force"));
                    EditorGUIUtility.labelWidth = 0;

                    if (M.MaxForce < M.MinForce)
                    {
                        M.MaxForce = M.MinForce;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }

                }
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(holder, new GUIContent("Holder", "The Side where the weapon Draw/Store from"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUI.indentLevel++;
                offsets = EditorGUILayout.Foldout(offsets, "Offsets");
                EditorGUI.indentLevel--;
                if (offsets)
                {
                    EditorGUILayout.PropertyField(positionOffset, new GUIContent("Position"));
                    EditorGUILayout.PropertyField(rotationOffset, new GUIContent("Rotation"));
                }
            }
            EditorGUILayout.EndVertical();
        }

       
      

        public virtual void SoundsList()
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            UpdateSoundHelp();
            EditorGUILayout.PropertyField(WeaponSound, new GUIContent("Weapon Source", "Audio Source for the wapons"));
            EditorGUILayout.PropertyField(Sounds, new GUIContent("Sounds", "Sounds Played by the weapon"), true);
            EditorGUI.indentLevel--;

            EditorGUILayout.HelpBox(SoundHelp, MessageType.None);
            EditorGUILayout.EndVertical();
        }

        protected bool EventHelp;

        public virtual void EventList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                M.ShowEventEditor = EditorGUILayout.Foldout(M.ShowEventEditor, "Events");
                EventHelp = GUILayout.Toggle(EventHelp, "?", EditorStyles.miniButton, GUILayout.Width(18));
                EditorGUILayout.EndHorizontal();


                EditorGUI.indentLevel--;
                if (M.ShowEventEditor)
                {
                    if (EventHelp)
                    {
                        EditorGUILayout.HelpBox("On Equip Weapon: Invoked when the rider equip this weapon. \n\nOn Unequip Weapon: Invoked when the rider unequip this weapon" + CustomEventsHelp(), MessageType.None);
                    }

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEquiped"), new GUIContent("On Equiped"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnUnequiped"), new GUIContent("On Unequiped"));
                    CustomEvents();
                }
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual string CustomEventsHelp() { return ""; }
        public virtual void CustomEvents() { }
        public virtual void UpdateSoundHelp()  {}

        protected void CheckWeaponID()
        {
            if (weaponID.intValue == 0) 
            {
                EditorGUILayout.HelpBox("Weapon ID needs cant be Zero, Please Set an ID number ", MessageType.Warning);
            }
        }
    }
}