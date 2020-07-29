using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.Utility.MeshTools
{
    public class VegetationMeshCombiner : MonoBehaviour
    {
        public GameObject TargetGameObject;
        public bool MergeSubmeshesWitEquialMaterial = true;

        void Reset()
        {
            TargetGameObject = gameObject;
        }
        
        public static GameObject CombineMeshes(GameObject sourceGameObject, bool mergeSubmeshesWitEquialMaterial)
        {
            MeshFilter[] meshFilters = sourceGameObject.GetComponentsInChildren<MeshFilter>();
            MeshRenderer[] meshRenderers = sourceGameObject.GetComponentsInChildren<MeshRenderer>();

            Vector3 targetPosition = sourceGameObject.transform.position;
            Quaternion targetRotation = sourceGameObject.transform.rotation;
            Vector3 targetScale = sourceGameObject.transform.localScale;

            sourceGameObject.transform.position = new Vector3(0, 0, 0);
            sourceGameObject.transform.rotation = Quaternion.identity;
            sourceGameObject.transform.localScale = Vector3.one;

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                i++;
            }

            GameObject mergedMesh = new GameObject(sourceGameObject.name + "_Merged");

            mergedMesh.transform.position = new Vector3(0, 0, 0);
            mergedMesh.transform.rotation = Quaternion.identity;
            mergedMesh.transform.localScale = Vector3.one;

            MeshFilter meshFilter = mergedMesh.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            meshFilter.sharedMesh.CombineMeshes(combine, false, true);

            MeshRenderer meshRenderer = mergedMesh.AddComponent<MeshRenderer>();


            List<Material> materialList = new List<Material>();
            for (int j = 0; j <= meshRenderers.Length - 1; j++)
            {
                materialList.AddRange(meshRenderers[j].sharedMaterials);
            }

            Material[] materials = materialList.ToArray();
            meshRenderer.sharedMaterials = materials;

            if (mergeSubmeshesWitEquialMaterial)
            {
                SubmeshCombiner submeshCombiner = new SubmeshCombiner();
                for (int j = 0; j <= meshFilter.sharedMesh.subMeshCount - 1; j++)
                {
                    submeshCombiner.AddSubmesh(meshFilter.sharedMesh.GetIndices(j), materials[j]);
                }

                submeshCombiner.UpdateMesh(meshFilter.sharedMesh);
                meshRenderer.sharedMaterials = submeshCombiner.GetMaterials();
            }

            sourceGameObject.transform.position = targetPosition;
            sourceGameObject.transform.rotation = targetRotation;
            sourceGameObject.transform.localScale = targetScale;

            mergedMesh.transform.position = targetPosition;
            mergedMesh.transform.rotation = targetRotation;
            mergedMesh.transform.localScale = targetScale;

            return mergedMesh;
        }
    }


    public class SubmeshInfo
    {
        public Material Material;
        public readonly List<int> IndicesList = new List<int>();
    }

    public class SubmeshCombiner
    {
        public readonly List<SubmeshInfo> SubmeshInfoList = new List<SubmeshInfo>();

        public void AddSubmesh(int[] indices, Material material)
        {
            SubmeshInfo submeshInfo = GetSubmeshInfo(material);
            if (submeshInfo == null)
            {
                submeshInfo = new SubmeshInfo {Material = material};
                SubmeshInfoList.Add(submeshInfo);
            }

            submeshInfo.IndicesList.AddRange(indices);
        }

        private SubmeshInfo GetSubmeshInfo(Material material)
        {
            for (int i = 0; i <= SubmeshInfoList.Count - 1; i++)
            {
                if (SubmeshInfoList[i].Material == material)
                {
                    return SubmeshInfoList[i];
                }
            }

            return null;
        }

        public void UpdateMesh(Mesh mesh)
        {
            mesh.subMeshCount = SubmeshInfoList.Count;
            for (int i = 0; i <= SubmeshInfoList.Count - 1; i++)
            {
                mesh.SetIndices(SubmeshInfoList[i].IndicesList.ToArray(), mesh.GetTopology(i), i);
            }
        }

        public Material[] GetMaterials()
        {
            Material[] materials = new Material[SubmeshInfoList.Count];
            for (int i = 0; i <= SubmeshInfoList.Count - 1; i++)
            {
                materials[i] = SubmeshInfoList[i].Material;
            }

            return materials;
        }
    }
}