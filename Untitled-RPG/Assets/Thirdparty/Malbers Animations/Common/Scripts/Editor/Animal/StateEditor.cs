using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations.Controller
{ }/*
    [CustomEditor(typeof(State),true)]
    public class StateEditor : Editor
    {
        public SerializedProperty stateProperty;    // Represents the SerializedProperty of the array the target belongs to.
        private State m;                      // The target Reaction.
        private MonoScript script;

        private ReorderableList Reo_List_SpeedM;

        SerializedProperty   S_SpeedModifiers;


        private void OnEnable()
        {
            m = (State)target;// Cache the target reference.
            script = MonoScript.FromScriptableObject(target as ScriptableObject);  

            S_SpeedModifiers = serializedObject.FindProperty("speedModifiers");

            Reo_List_SpeedM = new ReorderableList(serializedObject, S_SpeedModifiers, true, true, false, false)
            {
                drawElementCallback = DrawElement_MSpeeds,
                onAddCallback = OnAddCallback_MSpeeds,
                drawHeaderCallback = DrawHeader_MSpeeds,
            };

            Init(); // Call an initialisation method for inheriting classes.
        }

        public override void OnInspectorGUI()
        { 
            // Pull data from the target into the serializedObject.
            serializedObject.Update();

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            DrawBaseState();
            EditorGUILayout.EndVertical();

            // Push data back from the serializedObject to the target.
            serializedObject.ApplyModifiedProperties();
        }


        #region Draw MSpeeds
        private void DrawHeader_MSpeeds(Rect rect)
        {
            var height = EditorGUIUtility.singleLineHeight;

            var nameRect = new Rect(rect.x + 30, rect.y, rect.width / 2 - 22, height);
           // var stateRect = new Rect(rect.x + rect.width / 2 - 10, rect.y, rect.width / 2 - 30, height);
           // var IDrect = new Rect(rect.width - 55, rect.y, 60, height);


            EditorGUI.LabelField(nameRect, new GUIContent("Name", "Name of the Speed"), EditorStyles.miniLabel);
           // EditorGUI.LabelField(stateRect, new GUIContent("SpeedID", "This is the Speed that it will be used on the SetSpeed Method"), EditorStyles.miniLabel);
           // EditorGUI.LabelField(IDrect, new GUIContent("StateID", "This Speed only works on that State"), EditorStyles.miniLabel);


            Rect R_2 = new Rect(rect.width - 7, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);
            Rect R_3 = new Rect(rect.width + 13, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);


            if (GUI.Button(R_2, new GUIContent("+", "Add a new Speed"), EditorStyles.miniButton))
            {
                OnAddCallback_MSpeeds(Reo_List_SpeedM);
            }

            if (GUI.Button(R_3, new GUIContent("-", "Remove the selected Speed"), EditorStyles.miniButton))
            {
                if (Reo_List_SpeedM.index != -1) //If there's a selected Ability
                {
                    OnRemoveCallback_MSpeed(Reo_List_SpeedM);
                }
            }
        }

        private void DrawElement_MSpeeds(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (S_SpeedModifiers.arraySize <= index) return;

            rect.y += 2;

            var rectActive = new Rect(rect.x - 2, rect.y, 10, EditorGUIUtility.singleLineHeight);
            var rectName = new Rect(rect.x + 14, rect.y, rect.width-64, EditorGUIUtility.singleLineHeight);
            var rectID = new Rect(rect.x+ rect.width - 34, rect.y,  34, EditorGUIUtility.singleLineHeight);
          //  var rectID = new Rect(rect.x + rect.width / 2 - 15, rect.y, rect.width / 2 - 70, EditorGUIUtility.singleLineHeight);
          //  var rectState = new Rect(rect.width - 38, rect.y, 78, EditorGUIUtility.singleLineHeight);

            var mspeed = S_SpeedModifiers.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(rectActive, mspeed.FindPropertyRelative("active"), GUIContent.none);
            EditorGUI.PropertyField(rectName, mspeed.FindPropertyRelative("name"), GUIContent.none);

            var ind = index + 1;

            EditorGUI.LabelField(rectID, "("+ ind + ")");

            // EditorGUI.PropertyField(rectID, mspeed.FindPropertyRelative("ID"), GUIContent.none);
            //  EditorGUI.PropertyField(rectState, mspeed.FindPropertyRelative("state"), GUIContent.none);
        }

        private void OnAddCallback_MSpeeds(ReorderableList list)
        {
            if (m.speedModifiers == null) m.speedModifiers = new List<MSpeed>();
            m.speedModifiers.Add(new MSpeed("SpeedName"));

            EditorUtility.SetDirty(m);
        }

        private void OnRemoveCallback_MSpeed(ReorderableList list)
        {
         //   var state = S_SpeedModifiers.GetArrayElementAtIndex(list.index);

            // The reference value must be null in order for the element to be removed from the SerializedProperty array.
            S_SpeedModifiers.DeleteArrayElementAtIndex(list.index);
            list.index = list.index - 1;

            if (list.index == -1 && S_SpeedModifiers.arraySize > 0)  //In Case you remove the first one
            {
                list.index = 0;
            }

            EditorUtility.SetDirty(m);
        }
        #endregion



        // This function should be overridden by inheriting classes that need initialisation.
        protected virtual void Init() { }

        protected virtual void DrawBaseState()
        {
            EditorGUI.BeginDisabledGroup(true);
            script = (MonoScript)EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));
         //   EditorGUILayout.PropertyField(serializedObject.FindProperty("Input"));
          //  EditorGUILayout.PropertyField(serializedObject.FindProperty("RootMotion"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CanEnterFrom"),true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            //EditorGUI.indentLevel++;
            //var showSpeedModifiers = serializedObject.FindProperty("showSpeedModifiers");
            //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //showSpeedModifiers.boolValue = EditorGUILayout.Foldout(showSpeedModifiers.boolValue, "Speeds Modifiers");
            //EditorGUI.indentLevel--;

            //if (showSpeedModifiers.boolValue)
            //{
            //    Reo_List_SpeedM.DoLayoutList();        //Paint the Reordable List
            //    if (Reo_List_SpeedM.index != -1)
            //    {
            //        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //        var selectedSpeed = S_SpeedModifiers.GetArrayElementAtIndex(Reo_List_SpeedM.index);
            //        EditorGUILayout.PropertyField(selectedSpeed);
            //        EditorGUILayout.EndVertical();
            //    }
            //}
            //EditorGUILayout.EndVertical();

            //var show_Events = serializedObject.FindProperty("showEvents");

            //EditorGUI.indentLevel++;
            //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //show_Events.boolValue = EditorGUILayout.Foldout(show_Events.boolValue, "Events");
            //EditorGUI.indentLevel--;

            //if (show_Events.boolValue)
            //{
            //    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStateEnter"));
            //    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStateExit"));
            //}
            //EditorGUILayout.EndVertical();

            //EditorGUI.indentLevel++;
            //DrawDefaultInspector();
            //EditorGUI.indentLevel--;
        }

    }

}*/