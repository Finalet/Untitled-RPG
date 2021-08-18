using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/scriptables/tags")]
    [AddComponentMenu("Malbers/Utilities/Tools/Tags")]
    public class Tags : MonoBehaviour, ITag
    {
        /// <summary>Keep a Track of the game objects that has this component</summary>
        public static List<Tags> TagsHolders;

        /// <summary>List of tags for this component</summary>
        public List<Tag> tags = new List<Tag>();
        protected Dictionary<int, Tag> tags_Dic;

        void OnEnable()
        {
            if (TagsHolders == null) TagsHolders = new List<Tags>();
            TagsHolders.Add(this);                                                  //Save the GameObject who has this Tags on the global TagsHolders list //Better for saving performance
        }
        void OnDisable()
        {
            TagsHolders.Remove(this);                                              //Remove the GameObject who has this Tags on the global TagsHolders list //Better for saving performance
        }

        private void Start()
        {
            tags_Dic = new Dictionary<int, Tag>(); //Convert the list into a dictionary

            foreach (var tag in tags)
            {
                if (tag == null) continue;

                if (!tags_Dic.ContainsValue(tag))
                {
                    tags_Dic.Add(tag.ID, tag);
                }
            }

            tags = new List<Tag>();

            foreach (var item in tags_Dic)
            {
                tags.Add(item.Value);
            }
        }

        /// <summary>Return all the Gameobjects that use a tag</summary>
        public static List<GameObject> GambeObjectbyTag(Tag tag) { return GambeObjectbyTag(tag.ID); }


        /// <summary>Return all the Gameobjects that use a tag ID</summary>
        public static List<GameObject> GambeObjectbyTag(int tag)
        {
            List<GameObject> go = new List<GameObject>();

            if (Tags.TagsHolders == null || TagsHolders.Count == 0) return null;

            foreach (var item in TagsHolders)
            {
                if (item.HasTag(tag))
                {
                    go.Add(item.gameObject);
                }
            }

            if (go.Count == 0) return null;
            return go;
        }

        /// <summary>Return all the Gameobjects that use a tag ID</summary>
        public bool HasTag(Tag tag) { return HasTag(tag.ID); }

        /// <summary>Return all the Gameobjects that use a tag ID</summary>
        public bool HasTag(int key) { return tags_Dic.ContainsKey(key); }

        /// <summary>Check if this component has a more than one tag</summary>
        public bool HasTag(params Tag[] enteringTags)
        {
            foreach (var tag in enteringTags)
            {
                if (tags_Dic.ContainsKey(tag.ID))
                    return true;
            }
            return false;
        }

        /// <summary>Add a new Tag</summary>
        public void AddTag(Tag t)
        {
            if (!tags_Dic.ContainsValue(t))
            {
                tags.Add(t);
                tags_Dic.Add(t.ID, t);
            }
        }


        /// <summary>Check if this component has a more than one tag integer value</summary>
        public bool HasTag(params int[] enteringTags)
        {
            foreach (var tag in enteringTags)
            {
                if (!tags_Dic.ContainsKey(tag))
                    return false;
            }
            return true;
        }
    }

    public static class Tag_Transform_Extension
    {
        /// <summary> Returns if the Transform has a malbers Tag</summary>
        public static bool HasMalbersTag(this Transform t, Tag tag)
        {
            var tagC = t.GetComponent<Tags>();
            return tag != null ? tagC.HasTag(tag) : false;
        }

        /// <summary> Returns if the Gameobject has a Malbers Tag</summary>
        public static bool HasMalbersTag(this GameObject t, Tag tag)
        {
            var tagC = t.GetComponent<Tags>();
            return tagC != null ? tagC.HasTag(tag) : false;
        }

        /// <summary> Returns if the Gameobject has a Malbers Tag</summary>
        public static bool HasMalbersTag(this GameObject t, params Tag[] tags)
        {
            var tagC = t.GetComponent<Tags>();
            return tagC != null ? tagC.HasTag(tags) : false;
        }


        /// <summary> Returns if the Transform has a malbers Tag in one of its parents</summary>
        public static bool HasMalbersTagInParent(this Transform t, Tag tag)
        {
            var tagC = t.GetComponentInParent<Tags>();
            return tagC != null ? tagC.HasTag(tag) : false;
        }

        public static bool HasMalbersTagInParent(this Transform t, params Tag[] tags)
        {
            var tagC = t.GetComponentInParent<Tags>();
            return tagC != null ? tagC.HasTag(tags) : false;
        }

        /// <summary> Returns if the Gameobject has a Malbers Tag in one of its parents</summary>
        public static bool HasMalbersTagInParent(this GameObject t, Tag tag)
        {
            var tagC = t.GetComponentInParent<Tags>();
            return tagC != null ? tagC.HasTag(tag) : false;
        }

        public static bool HasMalbersTagInParent(this GameObject t, params Tag[] tags)
        {
            var tagC = t.GetComponentInParent<Tags>();
            return tagC != null ? tagC.HasTag(tags) : false;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Tags)), CanEditMultipleObjects]
    public class TagsEd : Editor
    {
        SerializedProperty tags;
        private void OnEnable()
        {
            tags = serializedObject.FindProperty("tags");
        }

        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Dupicated Tags will cause errors. Keep unique tags on the list");
            serializedObject.Update();
            EditorGUILayout.PropertyField(tags, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}