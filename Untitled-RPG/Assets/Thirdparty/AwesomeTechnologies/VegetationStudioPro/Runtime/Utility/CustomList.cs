using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace AwesomeTechnologies.Utility
{
    
    
    
    [System.Serializable]
    public class CustomList<T>
    {
        public CustomList()
        {
                        
        }

        public CustomList(int capacity)
        {
            if (Data == null)
            {
                Data = new T[capacity];
            }
        }       

        [SerializeField]
        public T[] Data;

        public int Count;
        public T this[int i]
        {
            get { return Data[i]; }
            set { Data[i] = value; }
        }

        private void ResizeArray()
        {
            var newData = Data != null ? new T[Mathf.Max(Data.Length << 1, 64)] : new T[64];

            if (Data != null && Count > 0)
                Data.CopyTo(newData, 0);

            Data = newData;
        }

        public void Clear()
        {
            Count = 0;
        }

        public T First()
        {
            if (Data == null || Count == 0) return default(T);
            return Data[0];
        }

        public T Last()
        {
            if (Data == null || Count == 0) return default(T);
            return Data[Count - 1];
        }

        public void Add(T item)
        {
            if (Data == null || Count == Data.Length)
                ResizeArray();

            Data[Count] = item;
            Count++;
        }

        public void AddStart(T item)
        {
            Insert(item, 0);
        }

        public void Insert(T item, int index)
        {
            if (Data == null || Count == Data.Length)
                ResizeArray();

            for (var i = Count; i > index; i--)
                Data[i] = Data[i - 1];

            Data[index] = item;
            Count++;
        }

        public T RemoveStart()
        {
            return RemoveAt(0);
        }

        public T RemoveAt(int index)
        {
            if (Data != null && Count != 0)
            {
                T val = Data[index];

                for (var i = index; i < Count - 1; i++)
                    Data[i] = Data[i + 1];

                Count--;
                Data[Count] = default(T);
                return val;
            }
            return default(T);
        }

        public T Remove(T item)
        {
            if (Data != null && Count != 0)
                for (var i = 0; i < Count; i++)
                    if (Data[i].Equals(item))
                        return RemoveAt(i);

            return default(T);
        }

        public T RemoveEnd()
        {
            if (Data != null && Count != 0)
            {
                Count--;
                T val = Data[Count];
                Data[Count] = default(T);

                return val;
            }
            return default(T);
        }

        public bool Contains(T item)
        {
            if (Data == null)
                return false;

            for (var i = 0; i < Count; i++)
                if (Data[i].Equals(item))
                    return true;

            return false;
        }
    }      
}