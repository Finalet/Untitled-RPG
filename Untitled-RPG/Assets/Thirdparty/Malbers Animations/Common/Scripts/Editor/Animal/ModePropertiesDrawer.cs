using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(ModeProperties))]
    public class ModePropertiesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var indent = EditorGUI.indentLevel;

            var Light = new Color(0.6f, 0.6f, 0.6f, 0.333f);
            var Dark = new Color(0.12f, 0.12f, 0.12f, 0.333f);

            EditorGUI.indentLevel = 0;


            #region Serialized Properties
            var Status = property.FindPropertyRelative("Status");
            var HoldByTime = property.FindPropertyRelative("HoldByTime");
            //var ModifyOnEnter = property.FindPropertyRelative("ModifyOnEnter");
            //var ModifyOnExit = property.FindPropertyRelative("ModifyOnExit");
            var affect = property.FindPropertyRelative("affect");
            var affectStates = property.FindPropertyRelative("affectStates");
            var affectSt = property.FindPropertyRelative("affect_Stance");
            var Stances = property.FindPropertyRelative("Stances");
            var OnEnter = property.FindPropertyRelative("OnEnter");
            var OnExit = property.FindPropertyRelative("OnExit");
            var TransitionFrom = property.FindPropertyRelative("TransitionFrom");
            var ShowEvents = property.FindPropertyRelative("ShowEvents");

            #endregion

            var height = EditorGUIUtility.singleLineHeight;
            var line = new Rect(position);
            line.height = height;

            line.x += 4;
            line.width -= 8;

            EditorGUI.LabelField(line, label, EditorStyles.boldLabel);

            line.y += height + 2;



            EditorGUI.PropertyField(line, Status);
            line.y += height + 2;

            if (Status.intValue == 2)
            {
                EditorGUI.PropertyField(line, HoldByTime);
                line.y += height + 2;
            }

            line.y += 2;
            var SplRect = new Rect(line) { height = 1, };
            EditorGUI.DrawRect(SplRect, !EditorGUIUtility.isProSkin   ? Light : Dark);
            line.y += 2;


            line.y += 2;
            EditorGUI.PropertyField(line, affect, new GUIContent("Affect States (" + affectStates.arraySize + ")"));
            line.y += height + 2;

            if (affect.intValue != 0)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(line, affectStates, new GUIContent("States"), true);
                // line.y += 7;
                EditorGUI.indentLevel--;

                line.y += EditorGUI.GetPropertyHeight(affectStates);
            }




            line.y += 4;
            SplRect = new Rect(line) { height = 1, };
            EditorGUI.DrawRect(SplRect, !EditorGUIUtility.isProSkin ? Light : Dark);
            line.y += 2;


            line.y += 2;
            EditorGUI.PropertyField(line, affectSt, new GUIContent("Affect Stances ("+ Stances.arraySize  +")"));
            line.y += height + 2;

            if (affectSt.intValue != 0)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(line, Stances, true);
                // line.y += 7;
                EditorGUI.indentLevel--;

                line.y += EditorGUI.GetPropertyHeight(Stances); ;
            }



            float TransitionfromHeight = EditorGUI.GetPropertyHeight(TransitionFrom);

            line.y += 4;
            SplRect = new Rect(line) { height = 1, };
            EditorGUI.DrawRect(SplRect, !EditorGUIUtility.isProSkin ? Light : Dark);
            line.y += 2;


            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(line, TransitionFrom, new GUIContent("Can Transition from Ability (" + TransitionFrom.arraySize + ")"), true);
            EditorGUI.indentLevel--;

            line.y += TransitionfromHeight;


            line.y += 4;
            SplRect = new Rect(line) { height = 1, };
            EditorGUI.DrawRect(SplRect, !EditorGUIUtility.isProSkin ? Light : Dark);
            line.y += 2;


            EditorGUI.indentLevel++;
            ShowEvents.boolValue = EditorGUI.Foldout(line, ShowEvents.boolValue, new GUIContent("Events"), true);
            EditorGUI.indentLevel--;

            line.y += height + 2;

            if (ShowEvents.boolValue)
            {
                EditorGUI.PropertyField(line, OnEnter, true);
                line.y += EditorGUI.GetPropertyHeight(OnEnter);
                EditorGUI.PropertyField(line, OnExit, true);
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var Status = property.FindPropertyRelative("Status");
            var ShowEvents = property.FindPropertyRelative("ShowEvents");
            var OnEnter = property.FindPropertyRelative("OnEnter");
            var OnExit = property.FindPropertyRelative("OnExit");
            var TransitionFrom = property.FindPropertyRelative("TransitionFrom");

            var affect = property.FindPropertyRelative("affect");
            var affectSt = property.FindPropertyRelative("affect_Stance");

            var Stances = property.FindPropertyRelative("Stances");
            var affectStates = property.FindPropertyRelative("affectStates");

            var height = EditorGUIUtility.singleLineHeight;

            float TotalHeight = (height + 2) * 6 + 4; //Label and Status

            if (Status.intValue == 2) TotalHeight += height + 2;

            if (affect.intValue != 0)  TotalHeight += EditorGUI.GetPropertyHeight(affectStates);
            if (affectSt.intValue != 0)  TotalHeight += EditorGUI.GetPropertyHeight(Stances);


            TotalHeight += EditorGUI.GetPropertyHeight(TransitionFrom);

            if (ShowEvents.boolValue)
            {
                TotalHeight += EditorGUI.GetPropertyHeight(OnEnter);
                TotalHeight += EditorGUI.GetPropertyHeight(OnExit);
            }
            return TotalHeight;
        }


        
    }
}