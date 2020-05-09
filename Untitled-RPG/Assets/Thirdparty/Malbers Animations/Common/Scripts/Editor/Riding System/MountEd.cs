using UnityEngine;
using UnityEditor;
using System;

namespace MalbersAnimations.HAP
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Mount))]
    public class MountEd : Editor
    {
        bool helpUseSpeeds;
        bool helpEvents;
        Mount M;
        private MonoScript script;
        SerializedProperty UseSpeedModifiers, MountOnly, DismountOnly, active, mountIdle, instantMount, straightSpine, ID, StraightSpineOffsetTransform,
            pointOffset,Animal,  smoothSM, mountPoint, rightIK, 
            rightKnee, leftIK, leftKnee, SpeedMultipliers,  OnMounted, Editor_Tabs1, Editor_Tabs2,
            OnDismounted, OnCanBeMounted, MountOnlyStates, DismountOnlyStates, ForceDismountStates, ForceDismount, debug;
        private void OnEnable()
        {
            M = (Mount)target;
            script = MonoScript.FromMonoBehaviour(M);

            UseSpeedModifiers = serializedObject.FindProperty("UseSpeedModifiers");
            //syncAnimators = serializedObject.FindProperty("syncAnimators");
            Animal = serializedObject.FindProperty("Animal");
           // ShowLinks = serializedObject.FindProperty("ShowLinks");
            debug = serializedObject.FindProperty("debug");
            ID = serializedObject.FindProperty("ID");

            MountOnly = serializedObject.FindProperty("MountOnly");
            DismountOnly = serializedObject.FindProperty("DismountOnly");
            active = serializedObject.FindProperty("active");
            mountIdle = serializedObject.FindProperty("mountIdle");
            instantMount = serializedObject.FindProperty("instantMount");
            straightSpine = serializedObject.FindProperty("straightSpine");
            //HighLimit = serializedObject.FindProperty("HighLimit");
            //LowLimit = serializedObject.FindProperty("LowLimit");
            smoothSM = serializedObject.FindProperty("smoothSM");

            mountPoint = serializedObject.FindProperty("MountPoint");
            rightIK = serializedObject.FindProperty("FootRightIK");
            rightKnee = serializedObject.FindProperty("KneeRightIK");
            leftIK = serializedObject.FindProperty("FootLeftIK");
            leftKnee = serializedObject.FindProperty("KneeLeftIK");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");

            SpeedMultipliers = serializedObject.FindProperty("SpeedMultipliers");
          //  DebugSync = serializedObject.FindProperty("DebugSync");
            OnMounted = serializedObject.FindProperty("OnMounted");
            pointOffset = serializedObject.FindProperty("pointOffset");
            StraightSpineOffsetTransform = serializedObject.FindProperty("StraightSpineOffsetTransform");

            OnDismounted = serializedObject.FindProperty("OnDismounted");
            OnCanBeMounted = serializedObject.FindProperty("OnCanBeMounted");
            MountOnlyStates = serializedObject.FindProperty("MountOnlyStates");
            DismountOnlyStates = serializedObject.FindProperty("DismountOnlyStates");

            ForceDismountStates = serializedObject.FindProperty("ForceDismountStates");
            ForceDismount = serializedObject.FindProperty("ForceDismount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Makes this GameObject mountable. Requires Mount Triggers and Moint Points");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUI.BeginChangeCheck();
                {
                    Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Links", "Custom Mount" });
                    if (Editor_Tabs1.intValue != 3) Editor_Tabs2.intValue = 3;

                    Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, new string[] { "M/D States", "Events", "Debug" });
                    if (Editor_Tabs2.intValue != 3) Editor_Tabs1.intValue = 3;
                   

                    //First Tabs
                    int Selection = Editor_Tabs1.intValue;

                    if (Selection == 0) ShowGeneral();
                    else if (Selection == 1) ShowLinks();
                    else if (Selection == 2) ShowCustom();

                    //2nd Tabs
                    Selection = Editor_Tabs2.intValue;

                    if (Selection == 0) ShowStates();
                    else if (Selection == 1) ShowEvents();
                    else if (Selection == 2) ShowDebug();
                }
              
                EditorGUILayout.EndVertical();


                if (M.MountPoint == null)
                {
                    EditorGUILayout.HelpBox("'Mount Point'  is empty, please set a reference", MessageType.Warning);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Mount Inspector");
                //EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowDebug()
        {
            EditorGUILayout.PropertyField(debug);
        }

        
        private void ShowEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
                    helpEvents = GUILayout.Toggle(helpEvents, "?", EditorStyles.miniButton, GUILayout.Width(18));
                }
                EditorGUILayout.EndHorizontal();
                if (helpEvents) EditorGUILayout.HelpBox("On Mounted: Invoked when the rider start to mount the animal\nOn Dismounted: Invoked when the rider start to dismount the animal\nInvoked when the Mountable has an available Rider Nearby", MessageType.None);

                EditorGUILayout.PropertyField(OnMounted);
                EditorGUILayout.PropertyField(OnDismounted);
                EditorGUILayout.PropertyField(OnCanBeMounted);
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowStates()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Mount/Dismount States", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(MountOnly, new GUIContent("Mount Only", "The Rider can only Mount when the Animal is on any of these states"));

                if (MountOnly.boolValue) MalbersEditor.Arrays(MountOnlyStates);

                EditorGUILayout.PropertyField(DismountOnly, new GUIContent("Dismount Only", "The Rider can only Dismount when the Animal is on any of these states"));

                if (DismountOnly.boolValue) MalbersEditor.Arrays(DismountOnlyStates);


                EditorGUILayout.PropertyField(ForceDismount, new GUIContent("Force Dismount", "The Rider is forced to dismount when the Animal is on any of these states"));

                if (ForceDismount.boolValue) MalbersEditor.Arrays(ForceDismountStates);
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowCustom()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(straightSpine, new GUIContent("Straight Spine", "Straighten the Mount Point to fix the Rider Animation"));

                if (M.StraightSpine)
                {
                    EditorGUILayout.PropertyField(StraightSpineOffsetTransform, new GUIContent("Transf Ref", "Transform to use for the Point Offset Calculation"));
                    EditorGUILayout.PropertyField(pointOffset, new GUIContent("Point Offset", "Point in front of the Mount to Straight the Spine of the Rider"));
                    EditorGUILayout.PropertyField(smoothSM, new GUIContent("Smoothness", "Smooth changes between the rotation and the straight Mount"));
                }
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(UseSpeedModifiers,new GUIContent("Animator Speeds", "Use this for other animals but the horse"));
                    helpUseSpeeds = GUILayout.Toggle(helpUseSpeeds, "?", EditorStyles.miniButton, GUILayout.Width(18));
                }
                EditorGUILayout.EndHorizontal();

                if (M.UseSpeedModifiers)
                {
                    if (helpUseSpeeds) EditorGUILayout.HelpBox("Changes the Speed on the Rider's Animator to Sync with the Animal Animator.\nThe Original Riding Animations are meant for the Horse. Only change the Speeds for other creatures", MessageType.None);
                    MalbersEditor.Arrays(SpeedMultipliers, new GUIContent("Animator Speed Multipliers", "Velocity changes for diferent Animation Speeds... used on other animals"));
                }
            }
            EditorGUILayout.EndVertical();

        }

        private void ShowLinks()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.HelpBox("'Mount Point' is obligatory, the rest are recommended", MessageType.None);

                EditorGUILayout.PropertyField(mountPoint, new GUIContent("Mount Point", "Reference for the Mount Point"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(rightIK, new GUIContent("Right Foot", "Reference for the Right Foot correct position on the mount"));
                EditorGUILayout.PropertyField(rightKnee, new GUIContent("Right Knee", "Reference for the Right Knee correct position on the mount"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(leftIK, new GUIContent("Left Foot", "Reference for the Left Foot correct position on the mount"));
                EditorGUILayout.PropertyField(leftKnee, new GUIContent("Left Knee", "Reference for the Left Knee correct position on the mount"));
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(active, new GUIContent("Active", "If the animal can be mounted. Deactivate if the mount is death or destroyed or is not ready to be mountable"));
                EditorGUILayout.PropertyField(Animal, new GUIContent("Animal", "Animal Reference for the Mounting System"));
                EditorGUILayout.PropertyField(ID, new GUIContent("ID", "Default should be 0.... change this and the Stance parameter on the Rider will change to that value... alowing other types of mounts like Wagon"));
                EditorGUILayout.PropertyField(instantMount, new GUIContent("Instant Mount", "Ignores the Mounting Animations"));
                EditorGUILayout.PropertyField(mountIdle, new GUIContent("Mount Idle", "Animation to Play directly when instant mount is enabled"));
            }
            EditorGUILayout.EndVertical();
        }
    }
}