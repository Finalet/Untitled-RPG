using MalbersAnimations.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations
{
    [CustomEditor(typeof(Dismember))]
    public class DismemberEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty LimbList;
        private SerializedObject MemberScript;
        private MaterialChanger changer;
        private Dismember M;
        private MonoScript script;



        private void OnEnable()
        {
            M = ((Dismember)target);
            script = MonoScript.FromMonoBehaviour(M);
            changer = M.GetComponent<MaterialChanger>();
            LimbList = serializedObject.FindProperty("bodyParts");

            list = new ReorderableList(serializedObject, LimbList, true, true, true, true);
            list.drawElementCallback = DrawElementCallback;
            list.drawHeaderCallback = HeaderCallbackDelegate;
            list.onAddCallback = OnAddCallBack;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Dismember Limbs... To enable it, call the public methods for 'Dismember'");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {
                    MalbersEditor.DrawScript(script);

                    list.DoLayoutList();


                    if (list.index != -1 && M.bodyParts.Count > list.index)
                    {
                        BodyPart item = M.bodyParts[list.index];
                        SerializedProperty Element = LimbList.GetArrayElementAtIndex(list.index);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUIUtility.labelWidth = 20;

                                EditorGUILayout.LabelField(item.name, EditorStyles.boldLabel);
                                EditorGUIUtility.labelWidth = 0;

                                EditorGUI.BeginDisabledGroup(true);
                                item.dismembered = GUILayout.Toggle(item.dismembered, new GUIContent(!item.dismembered ? "Not Used" : "Used", "This variable gets enabled when the limb is intantiated... that way the dismembering will be used one time"), EditorStyles.miniButton);
                                EditorGUI.EndDisabledGroup();

                                if (M.bodyParts[list.index].member != null)
                                {
                                    item.Instantiate = GUILayout.Toggle(item.Instantiate, new GUIContent("Instantiate", "if True the separated limb will be instantiated... else is already on scene and it will be enabled"), EditorStyles.miniButton);
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            Limb member = M.bodyParts[list.index].member;

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            {
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("AttachedLimb"), new GUIContent("Attached Limb", "The Attached limb on the Animal.. this gameobject will be hide after is dismemered"), true);
                                if (member != null) //If theres a member on it
                                {
                                    EditorGUILayout.PropertyField(Element.FindPropertyRelative("life"), new GUIContent("Destroy Limb", "Seconds before destroy the limb object"), true);
                                    if (Element.FindPropertyRelative("life").floatValue <= 0)
                                    {
                                        EditorGUILayout.HelpBox("Life <= 0 , the limb wont be destroy", MessageType.Warning);
                                    }
                                }
                            }
                            EditorGUILayout.EndVertical();



                            if (member != null) //If theres a member on it
                            {
                                EditorGUILayout.BeginVertical(MalbersEditor.StyleGreen);
                                EditorGUILayout.HelpBox("[Attached Bones] Bones on the animal \n[Dismember Bones] Bones on the Detached Limb\n\n The order of elements of both List must match", MessageType.None);
                                EditorGUILayout.EndVertical();


                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                MalbersEditor.Arrays(Element.FindPropertyRelative("AttachedLimbBones"), new GUIContent("Attached Bones", "Attached Limb Bones... They are use for alinging the Dismembered bones"));
                                EditorGUILayout.EndVertical();


                                MemberScript = new SerializedObject(member);


                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                {
                                    EditorGUI.BeginDisabledGroup(true);
                                    MalbersEditor.Arrays(MemberScript.FindProperty("Bones"), new GUIContent("Dismember Bones", "The bones has to be on the same order than the Attached bones on the Dismember Script"));
                                    EditorGUI.EndDisabledGroup();
                                }
                                EditorGUILayout.EndVertical();
                            }
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnDismember"), new GUIContent("On Dismember", "Invoked when Dismember is called"), true);
                        }
                        EditorGUILayout.EndVertical();
                    }
                }

                if (changer)
                {
                    EditorGUILayout.BeginVertical(MalbersEditor.StyleGreen);
                    {
                        EditorGUILayout.HelpBox("This animal has [MaterialChanger] component.\nSet a Material-Item name to match the materials on the Dismembered Limbs ", MessageType.None);
                    }
                    EditorGUILayout.EndVertical();


                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MaterialItemName"), new GUIContent("Material-Item", "The Current Material Item on the Animal to Update the Limbs Material Items"));
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Dismember ChangeChanged");
                //EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_01 = new Rect(rect.x + 14, rect.y, 35, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 14 + 25, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 10 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 25, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_01, new GUIContent(" #", "Index"), EditorStyles.miniLabel);
            EditorGUI.LabelField(R_1, "Body Part", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "Detached Limb", EditorStyles.centeredGreyMiniLabel);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = M.bodyParts[index];
            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, (rect.width - 65) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 25, rect.y, (rect.width - 45) / 2, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_0, "(" + index.ToString() + ")", EditorStyles.label);

            element.name = EditorGUI.TextField(R_1, element.name, EditorStyles.label);

            Rect R_2 = new Rect(rect.x + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width - 30) / 2), EditorGUIUtility.singleLineHeight);
            element.member = (Limb)EditorGUI.ObjectField(R_2, element.member, typeof(Limb), true);
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (M.bodyParts == null)
            {
                M.bodyParts = new List<BodyPart>();
            }

            M.bodyParts.Add(new BodyPart());
        }
    }
}
