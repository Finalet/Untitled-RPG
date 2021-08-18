using UnityEngine;
using System.Collections.Generic;
using System;
using MalbersAnimations.Events;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Mesh/Active Meshes")]
    public class ActiveMeshes : MonoBehaviour
    { 
        public List<ActiveSMesh> Meshes = new List<ActiveSMesh>();
        public bool showMeshesList = true;

        public ActiveSMesh this[int index] { get => Meshes[index]; set => Meshes[index] = value; }

        public int Count => Meshes.Count;

        /// <summary> All Active Meshes Index stored on a string separated by a space ' '  </summary>
        public string AllIndex
        {
            set
            {
                string[] getIndex = value.Split(' ');

                for (int i = 0; i < Count; i++)
                {
                    if (getIndex.Length > i)
                    {

                        if (int.TryParse(getIndex[i], out int index))
                        {
                            if (index == -1) continue;

                            Meshes[i].ChangeMesh(index);
                        }
                    }
                }
            }

            get
            {
                string AllIndex = "";

                for (int i = 0; i < Count; i++)
                {
                    AllIndex += Meshes[i].Current.ToString() + " ";
                }

                AllIndex.Remove(AllIndex.Length - 1);   //Remove the last space }
                return AllIndex;
            }
        }


        public bool random;
        private void Awake()
        {
            if (random)
            {
                Randomize();
            }
        }


        /// <summary>  Randomize all the Active Meshes on the list </summary>
        public void Randomize()
        {
            foreach (var mat in Meshes)
            {
                mat.ChangeMesh(UnityEngine.Random.Range(0, mat.meshes.Length));
            }
        }


        /// <summary> Set All ActiveMeshes Index from the Meshes list. </summary>
        public void SetActiveMeshesIndex(int[] MeshesIndex)
        {
            if (MeshesIndex.Length != Count)
            {
                Debug.LogError("Meshes Index array Lenghts don't match");
                return;
            }

            for (int i = 0; i < MeshesIndex.Length; i++)
            {
                Meshes[i].ChangeMesh(MeshesIndex[i]);
            }
        }


        /// <summary>Select an Element from the List by its index, and change to the next variation.  </summary>
        public virtual void ChangeMesh(int index) => Meshes[index % Count].ChangeMesh();

        /// <summary>Select an Element from the List by the index, and change to a variation. using its index</summary>
        public virtual void ChangeMesh(int indexList, int IndexMesh) => Meshes[indexList % Count].ChangeMesh(IndexMesh - 1);


        /// <summary> Change to next mesh using the name</summary>
        public virtual void ChangeMesh(string name, bool next)
        {
            ActiveSMesh mesh = Meshes.Find(item => item.Name == name);

            mesh?.ChangeMesh(next);
        }

        public virtual void ChangeMesh(string name) => ChangeMesh(name, true);

        public virtual void ChangeMesh(string name, int CurrentIndex)
        {
            ActiveSMesh mesh = Meshes.Find(item => item.Name == name);
            mesh?.ChangeMesh(CurrentIndex);
        }

        public virtual void ChangeMesh(int index, bool next) => Meshes[index].ChangeMesh(next);

        /// <summary>Toggle all meshes on the list</summary>
        public virtual void ChangeMesh(bool next = true)
        {
            foreach (var mesh in Meshes)
                mesh.ChangeMesh(next);
        }

        /// <summary>  Get the Active mesh by their name  </summary>
        public virtual ActiveSMesh GetActiveMesh(string name)
        {
            if (Count == 0) return null;

            return Meshes.Find(item => item.Name == name);
        }

        /// <summary>  Get the Active Mesh by their Index </summary>
        public virtual ActiveSMesh GetActiveMesh(int index)
        {
            if (Count == 0) return null;

            if (index >= Count) index = 0;
            if (index < 0) index = Count - 1;

            return Meshes[index];
        }



#if UNITY_EDITOR
        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {
            MEventListener listener = this.FindComponent<MEventListener>();
            if (listener == null) listener = transform.root.gameObject.AddComponent<MEventListener>();
            if (listener.Events == null) listener.Events = new List<MEventItemListener>();

            MEvent BlendS = MTools.GetInstance<MEvent>("Change Mesh");


            if (listener.Events.Find(item => item.Event == BlendS) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = BlendS,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, ChangeMesh);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, ChangeMesh);
                listener.Events.Add(item);

                Debug.Log("<B>Change Mesh</B> Added to the Event Listeners");
                UnityEditor.EditorUtility.SetDirty(listener);

                MTools.SetDirty(listener);
            }
        }
#endif

    }




    [Serializable]
    public class ActiveSMesh
    {
        [HideInInspector]
        public string Name = "NameHere";
        public Transform[] meshes;
        [HideInInspector,SerializeField] public int Current;

        [Space,Header("Invoked when the Active mesh is changed")]
        public TransformEvent OnActiveMesh = new TransformEvent();

        /// <summary>Change mesh to the Next/Before</summary>
        public virtual void ChangeMesh(bool next = true)
        {
            if (next)
                Current++;
            else
                Current--;

            if (Current >= meshes.Length) Current = 0;
            if (Current < 0) Current = meshes.Length - 1;

            foreach (var item in meshes)
            {
                if (item) item.gameObject.SetActive(false);
            }

            if (meshes[Current])
            {
                meshes[Current].gameObject.SetActive(true);
                OnActiveMesh.Invoke(meshes[Current]);
            }
        }

        /// <summary>Returns the Active Transform</summary>
        public virtual Transform GetCurrentActiveMesh()
        {
            return meshes[Current];
        }

        /// <summary> Set a mesh by Index </summary>
        public virtual void ChangeMesh(int Index)
        {
            Current = Index-1;
            ChangeMesh();
        }

        public void Set_by_BinaryIndex(int binaryCurrent)
        {
            int current = 0; 

            for (int i = 0; i < meshes.Length; i++)
            {
                if (MTools.IsBitActive(binaryCurrent, i))
                {
                    current = i;   //Find the first active bit and use it as current
                    break;
                }
            }
            ChangeMesh(current);
        }
    }
}