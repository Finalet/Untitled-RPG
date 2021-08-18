using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations
{
    [CanEditMultipleObjects,CustomEditor(typeof(Stats))]
    public class StatsEd : Editor
    {
        private ReorderableList list;
        private Stats M;
        private SerializedProperty statList;

        private void OnEnable()
        {
            M = (Stats)target;

            statList = serializedObject.FindProperty("stats");


            list = new ReorderableList(serializedObject, statList, false, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Stats Manager");

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {

                if (Application.isPlaying)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        if (M.PinnedStat != null)
                            EditorGUILayout.ObjectField("Pin Stat: ", (StatID)M.PinnedStat.ID, typeof(StatID), false);
                        else
                            EditorGUILayout.LabelField("Pin Stat: NULL ");
                    }
                    EditorGUI.EndDisabledGroup();
                }

                list.DoLayoutList();

                if (list.index != -1)
                {
                    var element = statList.GetArrayElementAtIndex(list.index);
                    var id = element.FindPropertyRelative("ID");
                    var ShowEvents = element.FindPropertyRelative("ShowEvents");
                    var BelowValue = element.FindPropertyRelative("Below");
                    var AboveValue = element.FindPropertyRelative("Above");

                    var Value = element.FindPropertyRelative("value");
                    var MaxValue = element.FindPropertyRelative("maxValue");
                    var MinValue = element.FindPropertyRelative("minValue");

                    var resetTo = element.FindPropertyRelative("resetTo");
                    var InmuneTime = element.FindPropertyRelative("InmuneTime");

                    var Regenerate = element.FindPropertyRelative("regenerate");
                    var RegenRate = element.FindPropertyRelative("RegenRate");
                    var RegenWaitTime = element.FindPropertyRelative("RegenWaitTime");

                    var Degenerate = element.FindPropertyRelative("degenerate");
                    var DegenRate = element.FindPropertyRelative("DegenRate");
                    var DegenWaitTime = element.FindPropertyRelative("DegenWaitTime");
                    var multiplier = element.FindPropertyRelative("multiplier");


                    var OnValueChange = element.FindPropertyRelative("OnValueChange");
                    var OnValueChangeNormalized = element.FindPropertyRelative("OnValueChangeNormalized");
                    var OnStatFull = element.FindPropertyRelative("OnStatFull");
                    var OnStatEmpty = element.FindPropertyRelative("OnStatEmpty");
                    var OnRegenerate = element.FindPropertyRelative("OnRegenerate");
                    var OnDegenerate = element.FindPropertyRelative("OnDegenerate");
                    var OnStatBelow = element.FindPropertyRelative("OnStatBelow");
                    var OnStatAbove = element.FindPropertyRelative("OnStatAbove");




                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(Value, new GUIContent("Value", "Current Value of the Stat"));
                        EditorGUILayout.PropertyField(multiplier, new GUIContent("Mult", "Stat Multiplier to be used when the value is modified"));
                        EditorGUIUtility.labelWidth = 0;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        EditorGUIUtility.labelWidth = 55;
                        EditorGUILayout.PropertyField(MinValue, new GUIContent("Min", "Minimun value of the Stat"));
                        EditorGUILayout.PropertyField(MaxValue, new GUIContent("Max", "Maximum value of the Stat"));
                        EditorGUIUtility.labelWidth = 0;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(Regenerate, new GUIContent("Regenerate", "Can the Stat Regenerate over time?"));
                        EditorGUILayout.PropertyField(RegenRate, new GUIContent("Rate", "Regeneration Rate, how fast/Slow the Stat will regenerate"));
                        EditorGUILayout.PropertyField(RegenWaitTime, new GUIContent("Wait Time", "After the Stat is modified, the time to wait to start regenerating"));
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(Degenerate, new GUIContent("Degenerate", "Can the Stat Degenerate over time?"));
                        EditorGUILayout.PropertyField(DegenRate, new GUIContent("Rate", "Degeneration Rate, how fast/Slow the Stat will Degenerate"));
                        EditorGUILayout.PropertyField(DegenWaitTime, new GUIContent("Wait Time", "After the Stat is modified, the time to wait to start degenerating"));

                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(resetTo, new GUIContent("Reset To", "When called the Funtion RESET()  it will reset to the Min Value or the Max Value"));
                        EditorGUILayout.PropertyField(InmuneTime, new GUIContent("Inmune Time", "If greater than zero, the Stat cannot be modify until the inmune time have passed"));

                        if (Application.isPlaying)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.Toggle("Is Inmune", M.stats[list.index].IsInmune);
                            EditorGUI.EndDisabledGroup();
                        }
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        if (MalbersEditor.Foldout(ShowEvents,"Events"))
                        {
                            string name = "Stat";

                            if (id.objectReferenceValue != null)
                            {
                                name = id.objectReferenceValue.name;
                            }

                            EditorGUILayout.PropertyField(OnValueChange, new GUIContent("On " + name + " change"));
                            EditorGUILayout.PropertyField(OnValueChangeNormalized, new GUIContent("On " + name + " change normalized"));
                            MalbersEditor.DrawSplitter();
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(OnStatFull, new GUIContent("On " + name + " full "));
                            EditorGUILayout.PropertyField(OnStatEmpty, new GUIContent("On " + name + " empty "));
                            EditorGUILayout.PropertyField(OnRegenerate, new GUIContent("On " + name + " Regenerate "));
                            EditorGUILayout.PropertyField(OnDegenerate, new GUIContent("On " + name + " Degenerate "));

                            MalbersEditor.DrawSplitter();
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUIUtility.labelWidth = 55;
                                EditorGUILayout.PropertyField(BelowValue, new GUIContent("Below", "Used to Check when the Stat is below this value"));
                                EditorGUILayout.PropertyField(AboveValue, new GUIContent("Above", "Used to Check when the Stat is Above this value"));
                                EditorGUIUtility.labelWidth = 0;
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.PropertyField(OnStatBelow, new GUIContent("On " + name + " Below " + BelowValue.floatValue.ToString("F1")));
                            EditorGUILayout.PropertyField(OnStatAbove, new GUIContent("On " + name + " Above " + AboveValue.floatValue.ToString("F1")));
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }


        void HeaderCallbackDelegate(Rect rect)
        { 
            Rect R_1 = new Rect(rect.x + 25, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.width / 2 + 25, rect.y, rect.x + (rect.width / 4) - 5, EditorGUIUtility.singleLineHeight); 

            EditorGUI.LabelField(R_1, "ID/Name", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "Value", EditorStyles.centeredGreyMiniLabel);   
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.x += 5;
             rect.width -= 15;

            var element = statList.GetArrayElementAtIndex(index);
            var ID = element.FindPropertyRelative("ID");
            var active = element.FindPropertyRelative("active");
            var Value = element.FindPropertyRelative("value");
          

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 40, rect.y, (rect.width) / 2  -22, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 40 + ((rect.width) / 2), rect.y, rect.width - ((rect.width) / 2) - 40, EditorGUIUtility.singleLineHeight); 

            EditorGUI.PropertyField(R_0, active, new GUIContent("", "Is the Stat Enabled? when Disable no modification can be done"));

            EditorGUI.PropertyField(R_1, ID, GUIContent.none);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(R_2, Value, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                var ConstantMAX = element.FindPropertyRelative("maxValue").FindPropertyRelative("ConstantValue");
                var ConstantValue = element.FindPropertyRelative("value").FindPropertyRelative("ConstantValue");

                if (ConstantMAX.floatValue < ConstantValue.floatValue)
                {
                    ConstantMAX.floatValue = ConstantValue.floatValue;
                }
            }

           // serializedObject.ApplyModifiedProperties();
        }


        void OnAddCallBack(ReorderableList list)
        {
            if (M.stats == null)
            {
                M.stats = new List<Stat>();
            }
            M.stats.Add(new Stat());
        }

    }
}