using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AwesomeTechnologies.External.Octree;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;


namespace AwesomeTechnologies.Utility
{
    public class MeshRendererRaycastInfo
    {
        public MeshRenderer MeshRenderer;
        public Mesh Mesh;
        public Matrix4x4 LocalToWorldMatrix4X4;
        public Bounds Bounds;
    }

    public class SceneMeshRaycaster
    {
#if UNITY_EDITOR
        private readonly MethodInfo _methIntersectRayMesh;
#endif
        
        public List<MeshRendererRaycastInfo> MeshRendererRaycastInfoList = new List<MeshRendererRaycastInfo>();
        public List<TerrainCollider> SceneTerrainColliderList = new List<TerrainCollider>();

        public BoundsOctree<MeshRendererRaycastInfo> BoundsOctree;

        public SceneMeshRaycaster()
        {
#if UNITY_EDITOR
            var editorTypes = typeof(Editor).Assembly.GetTypes();

            var typeHandleUtility = editorTypes.FirstOrDefault(t => t.Name == "HandleUtility");
            if (typeHandleUtility != null)
            {
                _methIntersectRayMesh = typeHandleUtility.GetMethod("IntersectRayMesh", (BindingFlags.Static | BindingFlags.NonPublic));

            }
#endif
            FindMeshRenderers();
            SetupOctree();
        }

        private Bounds GetSceneBounds()
        {
            var sceneBounds = MeshRendererRaycastInfoList.Count > 0 ? MeshRendererRaycastInfoList[0].Bounds : new Bounds();
                       
            for (int i = 0; i <= MeshRendererRaycastInfoList.Count - 1; i++)
            {
                sceneBounds.Encapsulate(MeshRendererRaycastInfoList[i].Bounds);
            }

            return sceneBounds;
        }

        private void SetupOctree()
        {
            Bounds sceneBounds = GetSceneBounds();
            BoundsOctree = new BoundsOctree<MeshRendererRaycastInfo>(sceneBounds.size.magnitude,sceneBounds.center,1,1.2f);

            for (int i = 0; i <= MeshRendererRaycastInfoList.Count - 1; i++)
            {
                BoundsOctree.Add(MeshRendererRaycastInfoList[i], MeshRendererRaycastInfoList[i].Bounds);
            }
        }

        private void FindMeshRenderers()
        {
            MeshRendererRaycastInfoList.Clear();

            MeshRenderer[] meshRenderers = Object.FindObjectsOfType<MeshRenderer>();
            for (int i = 0; i <= meshRenderers.Length - 1; i++)
            {
                MeshRendererRaycastInfo meshRendererRaycastInfo =
                    new MeshRendererRaycastInfo
                    {
                        MeshRenderer = meshRenderers[i],
                        Bounds = meshRenderers[i].bounds,
                        LocalToWorldMatrix4X4 = meshRenderers[i].localToWorldMatrix,                       
                    };
                MeshFilter meshFilter = meshRenderers[i].gameObject.GetComponent<MeshFilter>();
                if (meshFilter)
                {
                    meshRendererRaycastInfo.Mesh = meshFilter.sharedMesh;
                }

                if (meshRendererRaycastInfo.Mesh)
                {
                    MeshRendererRaycastInfoList.Add(meshRendererRaycastInfo);
                }
            }

            SceneTerrainColliderList.Clear();
            TerrainCollider[] terrainColliders = Object.FindObjectsOfType<TerrainCollider>();
            SceneTerrainColliderList.AddRange(terrainColliders);         
        }

        // ReSharper disable once UnusedMember.Local
        private bool IntersectRayMesh(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
        {
            return IntersectRayMesh(ray, meshFilter.mesh, meshFilter.transform.localToWorldMatrix, out hit);
        }

        private bool IntersectRayMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit)
        {
#if UNITY_EDITOR
            var parameters = new object[] { ray, mesh, matrix, null };
            bool result = (bool)_methIntersectRayMesh.Invoke(null, parameters);
            hit = (RaycastHit)parameters[3];
            return result;
#else
            hit = new RaycastHit();
            return false;
#endif
        }

        public bool RaycastSceneMeshes(Ray ray, out RaycastHit hit, bool includeTerrain, bool includeColliders, bool includeMeshes)
        {
            hit = new RaycastHit();
            RaycastHit tempRaycastHit;
            bool hitSomething = false;

            float minDistance = float.PositiveInfinity;

            if (includeTerrain && !includeColliders)
            for (int i = 0; i <= SceneTerrainColliderList.Count - 1; i++)
            {
                if (!SceneTerrainColliderList[i].Raycast(ray, out tempRaycastHit, float.PositiveInfinity)) continue;
                float distance = Vector3.Distance(ray.origin, tempRaycastHit.point);
                if (!(distance < minDistance)) continue;
                minDistance = distance;
                hitSomething = true;
                hit = tempRaycastHit;
            }

            if (includeColliders && !includeTerrain)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray, float.PositiveInfinity);
                for (int i = 0; i <= hits.Length - 1; i++)
                {
                    if (!(hits[i].collider is TerrainCollider))
                    {
                        float distance = Vector3.Distance(ray.origin, hits[i].point);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            hitSomething = true;
                            hit = hits[i];
                        }
                    }
                }
            }

            if (includeTerrain && includeColliders)
            {
                if (Physics.Raycast(ray,out tempRaycastHit,float.PositiveInfinity))
                {
                    float distance = Vector3.Distance(ray.origin, tempRaycastHit.point);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        hitSomething = true;
                        hit = tempRaycastHit;
                    }
                }
            }

            if (includeMeshes)
            {
                List<MeshRendererRaycastInfo> collidingWith = new List<MeshRendererRaycastInfo>();
                BoundsOctree.GetColliding(collidingWith, ray);

                for (int i = 0; i <= collidingWith.Count - 1; i++)
                {
                    //enable for debug
                    //Gizmos.color = Color.white;
                    //Handles.DrawWireCube(collidingWith[i].Bounds.center, collidingWith[i].Bounds.size);

                    if (!IntersectRayMesh(ray, collidingWith[i].Mesh,
                        collidingWith[i].LocalToWorldMatrix4X4, out tempRaycastHit)) continue;
                    float distance = Vector3.Distance(ray.origin, tempRaycastHit.point);
                    if (!(distance < minDistance)) continue;
                    minDistance = distance;
                    hitSomething = true;
                    hit = tempRaycastHit;
                }
            }
           
            return hitSomething;
        }
    }
}
