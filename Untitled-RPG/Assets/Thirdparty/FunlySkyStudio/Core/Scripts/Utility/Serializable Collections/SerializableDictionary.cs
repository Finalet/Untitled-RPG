using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Unity can't serialize generics other than List<T>. So we use this serializable dict,
  // You need to subclass this so that your top level class doesn't have templating. The base
  // class will handle storing and restoring the dict content.
  [Serializable]
  public class SerializableDictionary<K, V> : System.Object, ISerializationCallbackReceiver
  {
    [NonSerialized]
    public Dictionary<K, V> dict = new Dictionary<K, V>();

    [SerializeField]
    public List<K> m_Keys = new List<K>();

    [SerializeField]
    public List<V> m_Values = new List<V>();

    public void Clear()
    {
      dict.Clear();
    }

    public V this[K aKey]
    {
      get { return dict[aKey]; }
      set { dict[aKey] = value; }
    }
     
    // Move dict into the list which is serializable.
    public void OnBeforeSerialize()
    {
      m_Keys.Clear();
      m_Values.Clear();

      foreach (K aKey in dict.Keys) {
        m_Keys.Add(aKey);
        m_Values.Add(dict[aKey]);
      }
    }

    // Restore the dict using the list contents.
    public void OnAfterDeserialize()
    {
      if (m_Keys.Count != m_Values.Count) {
        Debug.LogError("Can't restore dictionry with unbalaned key/values");
        return;
      }

      dict.Clear();

      for (int i = 0; i < m_Keys.Count; i++) {
        dict[m_Keys[i]] = m_Values[i];
      }
    }
  }

}
