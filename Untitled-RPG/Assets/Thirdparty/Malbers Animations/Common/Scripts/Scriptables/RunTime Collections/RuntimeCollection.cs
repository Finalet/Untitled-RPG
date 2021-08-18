using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Scriptables
{
    public abstract class RuntimeCollection<T> : ScriptableObject where T : Object
    {
        public List<T> items = new List<T>();

        [TextArea(3,5)]
        public string Description;

        public UnityEvent OnSetEmpty = new UnityEvent();

        /// <summary>Ammount of object on the list</summary>
        public int Count => items.Count;

        public List<T> Items { get => items; set => items = value; }

        /// <summary> Clears the list of objects </summary>
        public virtual void Clear()
        {
            items.Clear();
            OnSetEmpty.Invoke();
        }

        /// <summary>Gets an object on the list by an index </summary>
        public virtual T Item_Get(int index) => items[index % items.Count];

        /// <summary>Gets the first object of the list</summary>
        public virtual T Item_GetFirst() => items[0];

        public virtual T Item_Get(string name) => items.Find(x => x.name == name);

       
        /// <summary>Gets a rando first object of the list</summary>
        public virtual T Item_GetRandom()
        {
            if (items != null && items.Count > 0)
            {  
                return items[Random.Range(0,items.Count)];
            }
            return default;
        }

        public virtual void Item_Add(T newItem)
        {
            if (!items.Contains(newItem))    items.Add(newItem);
        }

        public virtual void Item_Remove(T newItem)
        {
            if (items.Contains(newItem))
            {
                items.Remove(newItem);
            }

            if (items == null || items.Count == 0) OnSetEmpty.Invoke();
        }
    }
}