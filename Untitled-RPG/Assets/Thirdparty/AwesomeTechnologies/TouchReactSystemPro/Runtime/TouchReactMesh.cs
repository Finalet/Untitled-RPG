using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.TouchReact
{
    [ExecuteInEditMode]
    public class TouchReactMesh : MonoBehaviour
    {
        public List<MeshFilter> MeshFilterList = new List<MeshFilter>();

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            MeshFilterList.Clear();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            AddMeshToManager();
        } 

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            AddMeshToManager();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            RemoveMeshFromManager();
        }

        private void AddMeshToManager()
        {
            MeshFilter[] meshes = gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mesh in meshes)
            {
                MeshFilterList.Add(mesh);
                TouchReactSystem.AddMeshFilter(mesh);
            }
        }

        private void RemoveMeshFromManager()
        {
            for (int i = 0; i <= MeshFilterList.Count - 1; i++)
            {
                TouchReactSystem.RemoveMeshFilter(MeshFilterList[i]);
            }
            MeshFilterList.Clear();
        }

    }
}
