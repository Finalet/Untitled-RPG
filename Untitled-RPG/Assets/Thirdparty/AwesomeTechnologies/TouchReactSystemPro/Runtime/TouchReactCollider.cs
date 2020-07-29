using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.TouchReact
{
    [ExecuteInEditMode]
    public class TouchReactCollider : MonoBehaviour
    {
        public List<TouchColliderInfo> ColliderList = new List<TouchColliderInfo>();
        public bool AddChildColliders = true;
        public float ColliderScale = 1f;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            ColliderList.Clear();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            AddCollidersToManager();
        }

        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
            AddCollidersToManager();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            RemoveCollidersFromManager();
        }

        public void RefreshColliders()
        {
            RemoveCollidersFromManager();
            AddCollidersToManager();
        }

        private void AddCollidersToManager()
        {
            var colliders = AddChildColliders ? gameObject.GetComponentsInChildren<Collider>() : gameObject.GetComponents<Collider>();
            foreach (Collider thisCollider in colliders)
            {
                if (thisCollider is TerrainCollider) continue;

                TouchColliderInfo touchColliderInfo = new TouchColliderInfo
                {
                    Collider = thisCollider,
                    Scale = ColliderScale
                };
                ColliderList.Add(touchColliderInfo);
                TouchReactSystem.AddCollider(touchColliderInfo);
            }
        }

        private void RemoveCollidersFromManager()
        {
            for (int i = 0; i <= ColliderList.Count - 1; i++)
            {
                TouchReactSystem.RemoveCollider(ColliderList[i]);
            }
            ColliderList.Clear();
        }
    }
}
