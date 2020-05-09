using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace MalbersAnimations.Utilities
{
    [CustomEditor(typeof(EffectManager))]
    public class EffectManagerEditor : Editor
    {

        private ReorderableList list;
        private SerializedProperty EffectList;
        private EffectManager M;
        private MonoScript script;
        bool eventss = true, offsets = true, parent = true, general = true;

        private void OnEnable()
        {
            M = ((EffectManager)target);
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            EffectList = serializedObject.FindProperty("Effects");

            list = new ReorderableList(serializedObject, EffectList, true, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Manage all the Effects using the function (PlayEffect(int ID))");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                    MalbersEditor.DrawScript(script);

                    list.DoLayoutList();

                    if (list.index != -1)
                    {
                        Effect effect = M.Effects[list.index];

                        SerializedProperty Element = EffectList.GetArrayElementAtIndex(list.index);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUILayout.LabelField("* " + effect.Name + " *", EditorStyles.boldLabel);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUI.indentLevel++;
                            general = EditorGUILayout.Foldout(general, "General");
                            EditorGUI.indentLevel--;

                            if (general)
                            {
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("effect"), new GUIContent("Effect", "The Prefab or gameobject which holds the Effect(Particles, transforms)"));

                                //  EditorGUILayout.PropertyField(Element.FindPropertyRelative("instantiate"), new GUIContent("Instantiate", "True you want to make a copy of the effect, or if the effect is a Prefab!"));

                                //  if (Element.FindPropertyRelative("instantiate").boolValue) 

                                if (effect.effect != null && !effect.effect.activeInHierarchy)
                                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("life"), new GUIContent("Life", "Duration of the Effect. The Effect will be destroyed after the Life time has passed"));

                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("delay"), new GUIContent("Delay", "Time before playing the Effect"));

                                if (Element.FindPropertyRelative("life").floatValue <= 0)
                                {
                                    EditorGUILayout.HelpBox("Life = 0  the effect will not be destroyed by this Script", MessageType.Info);
                                }
                            }

                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUI.indentLevel++;
                            parent = EditorGUILayout.Foldout(parent, "Parent");
                            EditorGUI.indentLevel--;
                            if (parent)
                            {
                                SerializedProperty root = Element.FindPropertyRelative("root");
                                EditorGUILayout.PropertyField(root, new GUIContent("Root", "Uses the Root transform to position the Effect"));

                                if (root.objectReferenceValue != null)
                                {
                                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("isChild"), new GUIContent("is Child", "Set the Effect as a child of the Root transform"));
                                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("useRootRotation"), new GUIContent("Use Root Rotation", "Orient the Effect using the root rotation."));
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUI.indentLevel++;
                            offsets = EditorGUILayout.Foldout(offsets, "Offsets");
                            EditorGUI.indentLevel--;
                            if (offsets)
                            {
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("PositionOffset"), new GUIContent("Position", "Add additional offset to the Effect rotation"));
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("RotationOffset"), new GUIContent("Rotation", "Add additional offset to the Effect position"));
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("ScaleMultiplier"), new GUIContent("Scale", "Add additional offset to the Effect Scale"));
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("Modifier"), new GUIContent("Modifier", ""));

                            if (effect.Modifier != null && effect.Modifier.Description != string.Empty)
                            {
                                SerializedObject modifier = new SerializedObject(effect.Modifier);
                                var property = modifier.GetIterator();

                                property.NextVisible(true);                 //Don't Paint the first "Base thing"
                                property.NextVisible(true);                 //Don't Paint the script
                                property.NextVisible(true);                 //Don't Paint the Description I already painted

                                EditorGUILayout.HelpBox(effect.Modifier.Description, MessageType.None);

                                EditorGUI.BeginChangeCheck();
                                {
                                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                    {
                                        do
                                        {
                                            EditorGUILayout.PropertyField(property, true);
                                        } while (property.NextVisible(false));
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(effect.Modifier, "Ability Changed");
                                    modifier.ApplyModifiedProperties();
                                    if (modifier != null)
                                    {
                                        EditorUtility.SetDirty(effect.Modifier);
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUI.indentLevel++;
                            eventss = EditorGUILayout.Foldout(eventss, "Events");
                            EditorGUI.indentLevel--;
                            if (eventss)
                            {
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnPlay"), new GUIContent("On Play"));
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnStop"), new GUIContent("On Stop"));
                            }
                        }
                        EditorGUILayout.EndVertical();

                    }
                }
                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Effect Manager");
                //  EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_1 = new Rect(rect.x + 14, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 14 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2), EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_1, "Effect List", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "ID", EditorStyles.centeredGreyMiniLabel);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = EffectList.GetArrayElementAtIndex(index);

            var e_active = element.FindPropertyRelative("active");
            var e_Name = element.FindPropertyRelative("Name");
            var e_ID = element.FindPropertyRelative("ID");

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 16, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 16 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2), EditorGUIUtility.singleLineHeight);

            e_active.boolValue = EditorGUI.Toggle(R_0, e_active.boolValue);
            e_Name.stringValue = EditorGUI.TextField(R_1, e_Name.stringValue, EditorStyles.label);
            e_ID.intValue = EditorGUI.IntField(R_2, e_ID.intValue);
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (M.Effects == null)
            {
                M.Effects = new System.Collections.Generic.List<Effect>();
            }
            M.Effects.Add(new Effect());
        }
    }
}