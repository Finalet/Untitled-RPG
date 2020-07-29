using System.Collections.Generic;
using System.IO;
using AwesomeTechnologies.Billboards;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AwesomeTechnologies
{
    public class BillboardObject
    {
        public GameObject BillboardPrefab;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public GameObject Go;
    }

    public class BillboardAtlasRenderer : MonoBehaviour
    {
        public List<BillboardObject> BillboardObjectList = new List<BillboardObject>();

        public static Texture2D GenerateBillboardTexture(GameObject prefab, BillboardQuality billboardQuality,
            LODLevel billboardSourceLODLevel, VegetationShaderType vegetationShaderType, Quaternion rotationOffset,
            Color backgroundColor, string overrideBillboardAtlasShader, bool recalculateNormals, float normalBlendFactor)
        {
            Shader diffuseShader = BillboardShaderDetector.GetDiffuceBillboardAtlasShader(prefab);

            if (overrideBillboardAtlasShader != "")
            {
                diffuseShader = Shader.Find(overrideBillboardAtlasShader);
            }

            //if (vegetationShaderType == VegetationShaderType.Speedtree)
            //{

            //    diffuseShader = Shader.Find("AwesomeTechnologies/Billboards/RenderDiffuseAtlas");
            //}
            //else
            //{
            //    diffuseShader = Shader.Find("AwesomeTechnologies/Billboards/RenderDiffuseAtlasBright");
            //}

            Material minPostfilter = (Material) Resources.Load("MinPostFilter/MinPostFilter", typeof(Material));
            Texture2D texture = GenerateBillboardNew(prefab, GetBillboardQualityTileWidth(billboardQuality),
                GetBillboardQualityTileWidth(billboardQuality), GetBillboardQualityColumnCount(billboardQuality),
                GetBillboardQualityRowCount(billboardQuality), diffuseShader, backgroundColor, minPostfilter,
                billboardSourceLODLevel, rotationOffset,false,recalculateNormals,normalBlendFactor);
            return texture;
        }

        public static Texture2D GenerateBillboardNormalTexture(GameObject prefab, BillboardQuality billboardQuality,
            LODLevel billboardSourceLODLevel, Quaternion rotationOffset, string overrideBillboardAtlasNormalShader, bool recalculateNormals,float normalBlendFactor, bool flipBackNormals)
        {
            Shader normalShader = BillboardShaderDetector.GetNormalBillboardAtlasShader(prefab);

            if (overrideBillboardAtlasNormalShader != "")
            {
                normalShader = Shader.Find(overrideBillboardAtlasNormalShader);
            }

            if (flipBackNormals)
            {
                Shader.SetGlobalInt("_FlipBackNormals", 1);
            }
            else
            {
                Shader.SetGlobalInt("_FlipBackNormals", 0); 
            }

            Material minPostfilter = (Material) Resources.Load("MinPostFilter/MinPostFilter", typeof(Material));
            Texture2D texture = GenerateBillboardNew(prefab, GetBillboardQualityTileWidth(billboardQuality),
                GetBillboardQualityTileWidth(billboardQuality), GetBillboardQualityColumnCount(billboardQuality),
                GetBillboardQualityRowCount(billboardQuality), normalShader, new Color(0.5f,0.5f,1,0), minPostfilter,
                billboardSourceLODLevel, rotationOffset,false,recalculateNormals,normalBlendFactor);
            Shader.SetGlobalInt("_FlipBackNormals", 0);
            return texture;
        }

        public static Texture2D GenerateBillboardAOTexture(GameObject prefab, BillboardQuality billboardQuality,
            LODLevel billboardSourceLODLevel, Quaternion rotationOffset, bool recalculateNormals,float normalBlendFactor)
        {
            Shader normalShader = Shader.Find("AwesomeTechnologies/Billboards/AOBackground");
            Material minPostfilter = (Material)Resources.Load("MinPostFilter/MinPostFilter", typeof(Material));
            Texture2D texture = GenerateBillboardNew(prefab, GetBillboardQualityTileWidth(billboardQuality),
                GetBillboardQualityTileWidth(billboardQuality), GetBillboardQualityColumnCount(billboardQuality),
                GetBillboardQualityRowCount(billboardQuality), normalShader, new Color(0.5f, 0.5f, 1, 0), minPostfilter,
                billboardSourceLODLevel, rotationOffset,true,recalculateNormals,normalBlendFactor);
            return texture;
        }

        public static int GetBillboardQualityTileWidth(BillboardQuality billboardQuality)
        {
            switch (billboardQuality)
            {
                case BillboardQuality.Normal:
                case BillboardQuality.Normal3D:
                case BillboardQuality.NormalSingle:
                case BillboardQuality.NormalQuad:
                    return 128;
                case BillboardQuality.High:
                case BillboardQuality.High3D:
                case BillboardQuality.HighSingle:
                case BillboardQuality.HighQuad:
                    return 256;
                case BillboardQuality.Max:
                case BillboardQuality.Max3D:
                case BillboardQuality.MaxSingle:
                case BillboardQuality.MaxQuad:
                    return 512;
                case BillboardQuality.HighSample3D:
                case BillboardQuality.HighSample2D:
                    return 256;
                default:
                    return 128;
            }
        }        

        public static int GetBillboardQualityRowCount(BillboardQuality billboardQuality)
        {
            switch (billboardQuality)
            {
                case BillboardQuality.Normal:
                case BillboardQuality.High:
                case BillboardQuality.Max:
                case BillboardQuality.HighSample2D:
                case BillboardQuality.NormalSingle:
                case BillboardQuality.HighSingle:
                case BillboardQuality.MaxSingle:
                    return 1;
                case BillboardQuality.NormalQuad:
                case BillboardQuality.HighQuad:
                case BillboardQuality.MaxQuad:
                    return 1;
                case BillboardQuality.Normal3D:
                case BillboardQuality.High3D:
                case BillboardQuality.Max3D:
                    return 8;
                case BillboardQuality.HighSample3D:
                    return 16;
            }
            return 1;
        }

        public static int GetBillboardQualityColumnCount(BillboardQuality billboardQuality)
        {
            switch (billboardQuality)
            {
                case BillboardQuality.HighSample3D:
                case BillboardQuality.HighSample2D:
                    return 16;
                case BillboardQuality.NormalSingle:
                case BillboardQuality.HighSingle:
                case BillboardQuality.MaxSingle:
                    return 1;
                case BillboardQuality.NormalQuad:
                case BillboardQuality.HighQuad:
                case BillboardQuality.MaxQuad:
                    return 4;
            }
            return 8;
        }

        public static Texture2D GenerateBillboardNew(GameObject prefab, int width, int height, int gridSizeX,
            int gridSizeY, Shader replacementShader, Color backgroundColor, Material minPostfilter,
            LODLevel billboardSourceLODLevel, Quaternion rotationOffset, bool generateAo, bool recalculateNormals, float normalBlendFactor)
        {
#if UNITY_EDITOR
            bool fog = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);
#endif

            Vector3 renderPosition = new Vector3(0, 0, 0);
            const int invisibleLayer = 31; // used for instance and camera cull

            // atlas size
            var w = width * gridSizeX;
            var h = height * gridSizeY;

            var result = new Texture2D(w, h);
            var frameBuffer = new RenderTexture(width, height, 24);

            var filteredFrameBuffer = new RenderTexture(width, height, 24);

            var camGo = new GameObject("TempCamera");
            var cam = camGo.AddComponent<Camera>();

            //if (generateAo)
            //{
            //    ScreenSpaceAmbientOcclusion ao = camGo.AddComponent<ScreenSpaceAmbientOcclusion>();
            //    ao.m_Radius = 0.874f;
            //    ao.m_SampleCount = ScreenSpaceAmbientOcclusion.SSAOSamples.High;
            //    ao.m_OcclusionIntensity = 4;
            //    ao.m_Blur = 4;
            //    ao.m_Downsampling = 6;
            //    ao.m_OcclusionAttenuation = 2;
            //    ao.m_MinZ = 0.0001f;
            //}

            cam.clearFlags = CameraClearFlags.Color;

            backgroundColor.a = 0;
            cam.backgroundColor = backgroundColor;

            cam.renderingPath = RenderingPath.Forward;


            
            if (replacementShader != null)
                cam.SetReplacementShader(replacementShader, "");
            //cam.renderingPath = RenderingPath.DeferredShading;
            //cam.gameObject.AddComponent<ReadSpecularOcclusion>();                        

            var go = Instantiate(prefab, renderPosition, rotationOffset);
            if (recalculateNormals) RecalculateMeshNormals(go,normalBlendFactor);
            
            go.hideFlags = HideFlags.DontSave;

            LODGroup lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup)
            {
                if (lodGroup.fadeMode == LODFadeMode.SpeedTree)
                {                   
                    lodGroup.ForceLOD((int)billboardSourceLODLevel);
                }
            }

            //MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            //RecalculateMeshNormals(meshFilter.mesh,0);


            var children = go.GetComponentsInChildren<Transform>();
            foreach (var t in children)
                t.gameObject.layer = invisibleLayer;

            var bounds = CalculateBounds(go);
            float yOffset = FindLowestMeshYposition(go);

            cam.cullingMask = 1 << invisibleLayer;
            cam.orthographic = true;
            

            //var boundsSize = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) * 2;
            var boundsSize = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            cam.orthographicSize = boundsSize;
            cam.nearClipPlane = -boundsSize * 2;
            cam.farClipPlane = boundsSize * 2;


            //cam.renderingPath = RenderingPath.DeferredShading;
            //float aspect = (float) width/(float)height;
            //float orthographicSize = boundsSize;
            //cam.projectionMatrix = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, 0, cam.farClipPlane);
            
            
            cam.targetTexture = frameBuffer;

            cam.transform.position = renderPosition + new Vector3(0, bounds.extents.y - yOffset/2, 0); // + yOffset/2

            var xAngleStep = 360f / gridSizeY / 4;
            var yAngleStep = 360f / gridSizeX;


            minPostfilter.SetInt("_UseGammaCorrection", 0);

#if UNITY_EDITOR
#if UNITY_2018_1_OR_NEWER

#else
            if (PlayerSettings.colorSpace == ColorSpace.Linear)
                minPostfilter.SetInt("_UseGammaCorrection", 1);
#endif

#endif


            for (int i = 0; i < gridSizeX; i++)
            for (int j = 0; j < gridSizeY; j++)
            {
                cam.transform.rotation = Quaternion.AngleAxis(yAngleStep * i, Vector3.up) *
                                         Quaternion.AngleAxis(xAngleStep * j, Vector3.right);
                cam.Render();

                RenderTexture.active = filteredFrameBuffer;

                Graphics.Blit(frameBuffer, minPostfilter);

                result.ReadPixels(new Rect(0, 0, frameBuffer.width, frameBuffer.height), i * width, j * height);

                RenderTexture.active = null;
            }

            DestroyImmediate(go);
            DestroyImmediate(camGo);

            result.Apply();

#if UNITY_EDITOR
            Unsupported.SetRenderSettingsUseFogNoDirty(fog);
#endif

            return result;
        }

        public static void RecalculateMeshNormals(GameObject go, float normalBlendfactor)
        {
            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();

            for (int i = 0; i <= meshFilters.Length - 1; i++)
            {
                Mesh newMesh = Instantiate(meshFilters[i].sharedMesh);
                newMesh.RecalculateNormals();
               

                Vector3[] originalNormals = meshFilters[i].sharedMesh.normals;
                Vector3[] newNormals = newMesh.normals;
                
                for (int j = 0; j <= newNormals.Length  - 1; j++)
                {
                    newNormals[j] = Vector3.Slerp(originalNormals[j], newNormals[j], normalBlendfactor);
                }

                newMesh.normals = newNormals;
                newMesh.UploadMeshData(false);
                meshFilters[i].mesh = newMesh;
            }
        }

        public static void RecalculateMeshNormals(Mesh mesh, int subMeshIndex)
        {
            Vector3[] normals = mesh.normals;
            Vector3[] vertices = mesh.vertices;

            int[] indexes = mesh.GetIndices(subMeshIndex);

            Vector3 center = vertices[indexes[0]];
            for (int i = 1; i <= indexes.Length - 1; i++)
            {
                center += vertices[indexes[i]];
            }
            center = center / indexes.Length;

            for (int i = 0; i <= normals.Length - 1; i++)
            {
                Vector3 newNormal = vertices[i] - center;
                normals[i] = newNormal.normalized;
            }

            mesh.normals = normals;
        }

        public static Bounds CalculateBounds(GameObject go)
        {
            Bounds combinedbounds = new Bounds(go.transform.position, Vector3.zero);
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
                if (renderer is SkinnedMeshRenderer)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                    Mesh mesh = new Mesh();
                    skinnedMeshRenderer.BakeMesh(mesh);
                    Vector3[] vertices = mesh.vertices;

                    for (int i = 0; i <= vertices.Length - 1; i++)
                        vertices[i] = skinnedMeshRenderer.transform.TransformPoint(vertices[i]);
                    mesh.vertices = vertices;
                    mesh.RecalculateBounds();
                    Bounds meshBounds = mesh.bounds;
                    combinedbounds.Encapsulate(meshBounds);
                }
                else
                {
                    combinedbounds.Encapsulate(renderer.bounds);
                }
            return combinedbounds;
        }

        public static float FindLowestMeshYposition(GameObject go)
        {
            float lowestY = float.PositiveInfinity;

            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                if (meshFilter && meshFilter.sharedMesh)
                {
                    Vector3[] vertices = meshFilter.sharedMesh.vertices;
                    for (int i = 0; i <= vertices.Length - 1; i++)
                    {
                        if (vertices[i].y < lowestY) lowestY = vertices[i].y;
                    }
                }
            }
            return lowestY;
        }
    

        public static void SaveTexture(Texture2D tex, string name)
        {
#if UNITY_EDITOR
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(name + ".png", bytes);
#endif
        }

        

        public static void SetTextureImportSettings(Texture2D texture, bool normalMap)
        {
#if UNITY_EDITOR
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;

                if (normalMap)
                {
                    tImporter.textureType = TextureImporterType.Default;
                    tImporter.maxTextureSize = 4096;
                    tImporter.wrapMode = TextureWrapMode.Repeat;
                    tImporter.SaveAndReimport();
                }
                else
                {
                    tImporter.mipmapEnabled = true;
                    tImporter.maxTextureSize = 4096;
                    tImporter.wrapMode = TextureWrapMode.Repeat;
                    tImporter.SaveAndReimport();
                }
            }
#endif
        }
    }
}
