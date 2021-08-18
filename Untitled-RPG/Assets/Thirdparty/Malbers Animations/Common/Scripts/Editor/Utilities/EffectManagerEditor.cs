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
        private SerializedProperty EffectList, Owner;
        private EffectManager M;
        //private MonoScript script;
        bool eventss = true, offsets = true, parent = true, general = true;

        private void OnEnable()
        {
            M = ((EffectManager)target);
            //script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

            Owner = serializedObject.FindProperty("Owner");
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
                    //MalbersEditor.DrawScript(script);
                    EditorGUILayout.PropertyField(Owner);
                    list.DoLayoutList();

                    if (list.index != -1)
                    {
                        Effect effect = M.Effects[list.index];

                        SerializedProperty Element = EffectList.GetArrayElementAtIndex(list.index);

                        EditorGUILayout.LabelField("[" + effect.Name + "]", EditorStyles.boldLabel);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            if (MalbersEditor.Foldout(general, "General"))
                            {
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("effect"), new GUIContent("Effect", "The Prefab or gameobject which holds the Effect(Particles, transforms)"));


                                if (effect.effect != null)
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
                            if (MalbersEditor.Foldout(parent, "Parent"))
                            {
                                var root = Element.FindPropertyRelative("root");

                                EditorGUILayout.PropertyField(root, new GUIContent("Root", "Uses this transform to position the Effect"));

                                if (root.objectReferenceValue != null)
                                {
                                    var isChild = Element.FindPropertyRelative("isChild");
                                    var useRootRotation = Element.FindPropertyRelative("useRootRotation");

                                    EditorGUILayout.PropertyField(isChild, new GUIContent("is Child", "Set the Effect as a child of the Root transform"));
                                    EditorGUILayout.PropertyField(useRootRotation, new GUIContent("Use Root Rotation", "Orient the Effect using the root rotation."));
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            if (MalbersEditor.Foldout(offsets, "Offsets"))
                            {
                                var PositionOffset = Element.FindPropertyRelative("PositionOffset");
                                var RotationOffset = Element.FindPropertyRelative("RotationOffset");
                                var ScaleMultiplier = Element.FindPropertyRelative("ScaleMultiplier");


                                EditorGUILayout.PropertyField(PositionOffset, new GUIContent("Position", "Add additional offset to the Effect rotation"));
                                EditorGUILayout.PropertyField(RotationOffset, new GUIContent("Rotation", "Add additional offset to the Effect position"));
                                EditorGUILayout.PropertyField(ScaleMultiplier, new GUIContent("Scale", "Add additional offset to the Effect Scale"));
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            var mod = Element.FindPropertyRelative("Modifier");

                            EditorGUILayout.PropertyField(mod, new GUIContent("Modifier", ""));

                            if (effect.Modifier != null)
                            {
                                if (effect.Modifier.Description != string.Empty)
                                    EditorGUILayout.HelpBox(effect.Modifier.Description, MessageType.None);

                                MTools.DrawScriptableObject(effect.Modifier, false, 1);
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            if (MalbersEditor.Foldout(eventss, "Events"))
                            {
                                var OnPlay = Element.FindPropertyRelative("OnPlay");
                                var OnStop = Element.FindPropertyRelative("OnStop");

                                EditorGUILayout.PropertyField(OnPlay);
                                EditorGUILayout.PropertyField(OnStop);
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