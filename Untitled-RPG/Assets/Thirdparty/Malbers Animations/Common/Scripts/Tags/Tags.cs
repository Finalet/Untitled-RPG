using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    public class Tags : MonoBehaviour, ITag
    {
        /// <summary>Keep a Track of the game objects that has this component</summary>
        public static List<Tags> TagsHolders;

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
            tags_Dic = new Dictionary<int, Tag>();

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

        public bool HasTag(Tag tag)
        {
            return tags_Dic.ContainsValue(tag);
        }

        public bool HasTag(int key)
        {
            return tags_Dic.ContainsKey(key);
        }

        public bool HasTag(Tag[] enteringTags)
        {
            foreach (var tag in enteringTags)
            {
                if (tags_Dic.ContainsKey(tag.ID)) return true;
            }
            return false;
        }


        public bool HasTag(List<Tag> enteringTags)
        {
            foreach (var tag in enteringTags)
            {
                if (tags_Dic.ContainsKey(tag.ID))   return true;
                 
            }
            return false;
        }
    }
}