using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Controller.Reactions
{
    [AddComponentMenu("Malbers/Animal Controller/Reaction List")]

    public class MReactionsList : MonoBehaviour
    {
        [SerializeField] private MAnimal animal;
        public List<MReaction> reactions;

        public MAnimal Animal { get => animal; internal set => animal = value; }

        public void React()
        {
            if (animal == null)
            {
                Debug.LogWarning("There's no animal set to apply the reactions");
                return;
            }

            for (int i = 0; i < reactions.Count; i++)
                React(i);
        }

        public void React(int index)
        {
            var reaction = reactions[index];
            reaction.React(Animal);
        }

        public void React(Component newAnimal) { SetAnimal(newAnimal); React(); }

        public void React(GameObject newAnimal) { SetAnimal(newAnimal); React(); }

        public void React(Component newAnimal, int index) { SetAnimal(newAnimal); React(index); }

        public void React(GameObject newAnimal, int index) { SetAnimal(newAnimal); React(index); }

        public virtual void SetAnimal(Component newAnimal) => Animal = newAnimal?.FindComponent<MAnimal>();

        public virtual void SetAnimal(GameObject newAnimal) => Animal = newAnimal?.FindComponent<MAnimal>();

        private void Reset() => animal = GetComponentInParent<MAnimal>();
    }








    /// ----------------------------------------
    /// EDITOR
    /// ----------------------------------------


#if UNITY_EDITOR

    [CustomEditor(typeof(MReactionsList))]
    public class MAnimalReactionsEditor : Editor
    {
        private List<Type> ReactionType = new List<Type>();
        private GenericMenu addMenu;
        private UnityEditorInternal.ReorderableList Reo_List_Reactions;
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));

        SerializedProperty animal, reactions_List;
        //private MonoScript script;

        MReactionsList m;

        private void OnEnable()
        {
            //   script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            m = (MReactionsList)target;
            ReactionType = MTools.GetAllTypes<MReaction>();

            animal = serializedObject.FindProperty("animal");
            reactions_List = serializedObject.FindProperty("reactions");



            Reo_List_Reactions = new ReorderableList(serializedObject, reactions_List, true, true, true, true)
            {
                drawElementCallback = Draw_Element_Reaction,
                drawHeaderCallback = Draw_Header_Reaction,
                onAddCallback = OnAddCallback_Reaction,
                onRemoveCallback = OnRemove_Reaction
            };
        }

        //private void OnDestroy()
        //{
        //    foreach (var rea in m.reactions)
        //    {
        //        ScriptableObject.DestroyImmediate(rea);
        //    }
        //}

        private void OnRemove_Reaction(ReorderableList list)
        {
            var reaction = reactions_List.GetArrayElementAtIndex(list.index).objectReferenceValue as MReaction;
            DestroyImmediate(reaction);
            reactions_List.DeleteArrayElementAtIndex(list.index);
            reactions_List.DeleteArrayElementAtIndex(list.index); //HACK

            list.index = Mathf.Clamp(list.index, 0, list.index - 1);

            EditorUtility.SetDirty(target);
        }


        private void Draw_Header_Reaction(Rect rect)
        {
            EditorGUI.LabelField(rect, "    Reactions");

            var activeRect2 = new Rect(rect.width - 15, rect.y, 60, rect.height);

            EditorGUI.LabelField(activeRect2, "Delay");
        }

        private void Draw_Element_Reaction(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;

            var field = new Rect(rect);

            var activeRect1 = new Rect(rect.x, rect.y, 20, rect.height);
            var activeRect2 = new Rect(rect.width + 5, rect.y, 30, EditorGUIUtility.singleLineHeight);

            field.height = EditorGUIUtility.singleLineHeight;
            field.x += 20;
            field.width -= 20;

            var element = reactions_List.GetArrayElementAtIndex(index);
            var reaction = element.objectReferenceValue as MReaction;

            if (reaction != null)
            {
                reaction.active = EditorGUI.Toggle(activeRect1, GUIContent.none, reaction.active);
                reaction.delay = EditorGUI.FloatField(activeRect2, GUIContent.none, reaction.delay);

                EditorGUI.LabelField(field, new GUIContent("[" + index + "]  " + reaction.fullName));
            }
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //EditorGUILayout.BeginVertical(StyleBlue);
            //EditorGUILayout.HelpBox("Apply Reactions to an Animal when the method React()  is called", MessageType.None);
            //EditorGUILayout.EndVertical();
            //EditorGUI.BeginDisabledGroup(true);
            //EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            //EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(animal);

            Reo_List_Reactions.DoLayoutList();

            if (Reo_List_Reactions.index != -1)
            {
                var element = reactions_List.GetArrayElementAtIndex(Reo_List_Reactions.index);

                //EditorGUI.BeginDisabledGroup(true);
                //EditorGUILayout.PropertyField(element,GUIContent.none);
                //EditorGUI.EndDisabledGroup();
                DrawElement(element);
            }

            if (m.reactions != null && m.reactions.Count > 0 && Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(!m.Animal);
                for (int i = 0; i < m.reactions.Count; i++)
                {
                    if (GUILayout.Button("React [" + m.reactions[i].fullName + "]"))
                    {
                        m.React(i);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
            //base.OnInspectorGUI();
        }

        private void DrawElement(SerializedProperty element)
        {
            var reaction = element.objectReferenceValue as MReaction;
            if (reaction)
            {
                EditorGUILayout.BeginVertical(StyleBlue);
                EditorGUILayout.HelpBox(reaction.description, MessageType.None);
                EditorGUILayout.EndVertical();

                MTools.DrawScriptableObject(reaction, false, 3);
            }
        }

        private void OnAddCallback_Reaction(UnityEditorInternal.ReorderableList list)
        {
            addMenu = new GenericMenu();

            for (int i = 0; i < ReactionType.Count; i++)
            {
                Type st = ReactionType[i];

                var Rname = st.Name.Replace("Rea", " Rea"); 
                addMenu.AddItem(new GUIContent(Rname), false, () => AddReaction(st));

            }

            addMenu.ShowAsContext();
        }

        private void AddReaction(Type NewReaction)
        {
            MReaction reaction = (MReaction)CreateInstance(NewReaction);

            // Pull all the information from the target of the serializedObject.
            reactions_List.serializedObject.Update();
            reactions_List.InsertArrayElementAtIndex(0);
            reactions_List.GetArrayElementAtIndex(0).objectReferenceValue = reaction;
            reactions_List.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(reaction);
            EditorUtility.SetDirty(target);
        }
    }
#endif
}

 