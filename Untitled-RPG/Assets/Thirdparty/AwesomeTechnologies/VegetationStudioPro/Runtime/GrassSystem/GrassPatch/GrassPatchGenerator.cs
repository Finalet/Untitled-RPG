using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.Grass
{
    public enum GrassPatchLod
    {
        Lod0 = 0,
        Lod1 = 1,
        Lod2 = 2
    }

    [HelpURL("http://www.awesometech.no/index.php/grass-patch-generator")]
    public class GrassPatchGenerator : MonoBehaviour
    {

        public GrassPatchLod GrassPatchLod = GrassPatchLod.Lod0;

        public int PlaneCount = 15;
        public float Size = 0.40f;
        public float MinScale = 0.8f;
        public float MaxScale = 1.2f;
        public float PlaneHeight = 0.4f;
        public float PlaneMaxHeight = 0.5f;
        public float PlaneWidth = 0.4f;
        public float PlaneMaxWidth = 0.5f;
        public int PlaneWidthSegments = 2;
        public int PlaneHeightSegments = 2;
        public int RandomSeed = 1;
        public float MaxBendDistance = 0.25f;
        public float CurveOffset = 0.25f;
        public Material GrassMaterial;
        public Texture2D GrassTexture;

        public Material CustomMaterial;
        public float MinBendHeight = 0.05f;
        public AnimationCurve WindBend = new AnimationCurve();
        public AnimationCurve AmbientOcclusion = new AnimationCurve();

        public bool BakePhase = true;
        public bool BakeBend = true;
        public bool BakeAo = true;
        public bool ShowVertexColors = false;
        public bool GenerateBackside = false;

        public Color ColorTint1 = Color.white;
        public Color ColorTint2 = Color.white;
        public float RandomDarkening = 0.31f;
        public float RootAmbient = 0.63f;
        public float TextureCutoff = 0.1f;

        private Material _vertexColorMaterial;

        public List<ProceduralGrassPlane> GrassPlaneList;

        // ReSharper disable once UnusedMember.Local
        private void Reset()
        {
            WindBend.AddKey(0f, 0f);
            WindBend.AddKey(1f, 1f);

            AmbientOcclusion.AddKey(0f, 0f);
            AmbientOcclusion.AddKey(1f, 1f);

            if (GrassTexture == null)
            {
                GrassTexture = Resources.Load("GrassTextures/GrassFrond01") as Texture2D;

                UpdateTexture();
            }

            GenerateGrassPatch();
        }

        public void UpdateTexture()
        {
            if (CustomMaterial == null)
            {
                Material material = new Material(Shader.Find("AwesomeTechnologies/Grass/Grass"));
                material.SetTexture("_MainTex", GrassTexture);
                //material.SetFloat("_Cutoff", 0.2f);
                material.SetVector("_AG_ColorNoiseArea", new Vector4(0, 30, 0, 1));
                material.SetTexture("_AG_ColorNoiseTex", Resources.Load("PerlinSeamless") as Texture2D);

                material.SetColor("_Color", ColorTint1);
                material.SetColor("_ColorB", ColorTint2);
                material.SetFloat("_Cutoff", TextureCutoff);
                material.SetFloat("_RandomDarkening", RandomDarkening);
                material.SetFloat("_RootAmbient", RootAmbient);
#if UNITY_5_6_OR_NEWER
                material.enableInstancing = true;
#endif
                material.EnableKeyword("_ALPHATEST_ON");
                GrassMaterial = material;
            }
            else
            {
                Material material = new Material(CustomMaterial);
                material.SetTexture("_MainTex", GrassTexture);
                GrassMaterial = material;
            }

            GenerateGrassPatch();
        }

        private void ClearGrassPlanes()
        {
            if (GrassPlaneList == null) GrassPlaneList = new List<ProceduralGrassPlane>();
            for (int i = 0; i <= GrassPlaneList.Count - 1; i++)
            {
                DestroyImmediate(GrassPlaneList[i].gameObject);
            }
            GrassPlaneList.Clear();

            Transform[] ts = transform.GetComponentsInChildren<Transform>();
            foreach (Transform t in ts)
            {
                if (t)
                {
                    if (t.gameObject.name.StartsWith("Plane_"))
                    {
                        DestroyImmediate(t.gameObject);
                    }
                }

            }
        }

        public int GetMeshVertexCount()
        {
            int vertexCount = 0;

            for (int i = 0; i <= GrassPlaneList.Count - 1; i++)
            {
                MeshFilter meshFilter = GrassPlaneList[i].gameObject.GetComponent<MeshFilter>();
                if (meshFilter) vertexCount += meshFilter.sharedMesh.vertexCount;
            }

            return vertexCount;
        }

        public int GetMeshTriangleCount()
        {
            int triangleCount = 0;

            for (int i = 0; i <= GrassPlaneList.Count - 1; i++)
            {
                MeshFilter meshFilter = GrassPlaneList[i].gameObject.GetComponent<MeshFilter>();
                if (meshFilter) triangleCount += meshFilter.sharedMesh.triangles.Length / 3;
            }

            return triangleCount;
        }
        public void GenerateGrassPatch()
        {
            _vertexColorMaterial = Resources.Load("GrassPatchVertexColor") as Material;

            ClearGrassPlanes();

            Random.InitState(RandomSeed);

            for (int i = 0; i <= PlaneCount - 1; i++)
            {
                //

                GameObject go = new GameObject
                {
                    hideFlags = HideFlags.HideInHierarchy,
                    name = "Plane_" + i.ToString()
                };
                go.transform.SetParent(transform);

                float scale = Random.Range(MinScale, MaxScale);

                float selectedPlaneWidth = PlaneWidth * scale;
                float selectedPlaneHeight = PlaneHeight * scale; 

                ProceduralGrassPlane proceduralGrassPlane = go.AddComponent<ProceduralGrassPlane>();
                proceduralGrassPlane.CurveOffset = Random.Range(-CurveOffset, CurveOffset);
                proceduralGrassPlane.Offset1 = Random.Range(-MaxBendDistance, MaxBendDistance);
                proceduralGrassPlane.Offset2 = Random.Range(-MaxBendDistance, MaxBendDistance);
                proceduralGrassPlane.height = selectedPlaneHeight;
                proceduralGrassPlane.width = selectedPlaneWidth;
                proceduralGrassPlane.BakeBend = BakeBend;
                proceduralGrassPlane.BakePhase = BakePhase;
                proceduralGrassPlane.BakeAO = BakeAo;
                proceduralGrassPlane.BendCurve = WindBend;
                proceduralGrassPlane.AmbientOcclusionCurve = AmbientOcclusion;
                proceduralGrassPlane.Phase = i * (1f /PlaneCount);
                proceduralGrassPlane.GenerateBackside = GenerateBackside;

                if (i % 4 == 1)
                {
                    proceduralGrassPlane.LODLevel = 2;
                }
                else if (i % 2 == 1)
                {
                    proceduralGrassPlane.LODLevel = 1;
                }
                else
                {
                    proceduralGrassPlane.LODLevel = 0;
                }

                if (ShowVertexColors)
                {
                    proceduralGrassPlane.Material = _vertexColorMaterial;
                }
                else
                {
                    proceduralGrassPlane.Material = GrassMaterial;
                }

                proceduralGrassPlane.MinimumBendHeight = MinBendHeight;
                proceduralGrassPlane.heightSegments = PlaneHeightSegments;
                proceduralGrassPlane.widthSegments = PlaneWidthSegments;


                go.transform.localRotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 364), 0));
                go.transform.localPosition = new Vector3(Random.Range(-Size / 2f, Size / 2f), selectedPlaneHeight / 2f, Random.Range(-Size / 2f, Size / 2f));
                GrassPlaneList.Add(proceduralGrassPlane);

                proceduralGrassPlane.CreateGrassPlane();
            }
        }

        Mesh GetCombinedMesh(int lod)
        {
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
           
            List<MeshFilter> filterList = new List<MeshFilter>();
            for (int i = 0; i <= meshFilters.Length - 1; i++)
            {
                ProceduralGrassPlane proceduralPlane = meshFilters[i].gameObject.GetComponent<ProceduralGrassPlane>();
                if (proceduralPlane.LODLevel >= lod) filterList.Add(meshFilters[i]);
            }

            CombineInstance[] combine = new CombineInstance[filterList.Count];
            for (int i = 0; i <= filterList.Count - 1; i++)
            {
                combine[i].mesh = filterList[i].sharedMesh;
                combine[i].transform = filterList[i].transform.localToWorldMatrix;
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);

            return mesh;
        }

        public void BuildPrefab()
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanelInProject("Save prefab", "", "prefab",
              "Please enter a file name to save the prefab to");
            if (path.Length != 0)
            {
                string prefabName = Path.GetFileNameWithoutExtension(path);
                string directory = Path.GetDirectoryName(path);
                string prefabFilename = path;
                string meshFilenameLod0 = directory + "/" + prefabName + "_LOD0.asset";
                string materialFilename = Path.ChangeExtension(prefabFilename, ".mat");

                Vector3 oldPosition = transform.position;
                Quaternion oldRotation = transform.rotation;

                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;

                Material material;

                if (CustomMaterial)
                {
                    material = new Material(CustomMaterial);
                    material.SetTexture("_MainTex", GrassTexture);
                }
                else
                {
                    material = new Material(Shader.Find("AwesomeTechnologies/Grass/Grass"));
                    material.SetTexture("_MainTex", GrassTexture);
                    material.SetColor("_Color", ColorTint1);
                    material.SetColor("_ColorB", ColorTint2);
                    material.SetFloat("_Cutoff", TextureCutoff);
                    material.SetFloat("_RandomDarkening", RandomDarkening);
                    material.SetFloat("_RootAmbient", RootAmbient);
                    material.SetVector("_AG_ColorNoiseArea", new Vector4(0, 30, 0, 1));
                    material.SetTexture("_AG_ColorNoiseTex", Resources.Load("PerlinSeamless") as Texture2D);
                    material.EnableKeyword("_ALPHATEST_ON");
#if UNITY_5_6_OR_NEWER
                    material.enableInstancing = true;
#endif
                }

                Mesh meshLod0 = GetCombinedMesh(0);

                AssetDatabase.CreateAsset(material, materialFilename);
                GameObject go = new GameObject {name = prefabName};
                // "New grass patch_" + patchID;
                go.transform.position = transform.position;
                go.transform.rotation = transform.localRotation;
                go.AddComponent<MeshFilter>().sharedMesh = meshLod0;
                go.AddComponent<MeshRenderer>().sharedMaterial = material;

                AssetDatabase.CreateAsset(meshLod0, meshFilenameLod0);
                
                transform.position = oldPosition;
                transform.rotation = oldRotation;
                
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAsset(go, prefabFilename);
#else
                PrefabUtility.CreatePrefab(prefabFilename, go);
#endif
                
                AssetDatabase.Refresh();
            }
#endif
        }

        public void BuildPrefabLod()
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanelInProject("Save prefab", "", "prefab",
              "Please enter a file name to save the prefab to");
            if (path.Length == 0) return;

            string prefabName = Path.GetFileNameWithoutExtension(path);
            string directory = Path.GetDirectoryName(path);
            string prefabFilename = path;
            string meshFilenameLod0 = directory + "/" + prefabName + "_LOD0.asset";
            string meshFilenameLod1 = directory + "/" + prefabName + "_LOD1.asset";
            string meshFilenameLod2 = directory + "/" + prefabName + "_LOD2.asset";
            string materialFilename = Path.ChangeExtension(prefabFilename, ".mat");

            Vector3 oldPosition = transform.position;
            Quaternion oldRotation = transform.rotation;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            //string patchID = Time.realtimeSinceStartup.ToString();
            Material material;

            if (CustomMaterial)
            {
                material = new Material(CustomMaterial);
                material.SetTexture("_MainTex", GrassTexture);
            }
            else
            {
                material = new Material(Shader.Find("AwesomeTechnologies/Grass/Grass"));
                material.SetTexture("_MainTex", GrassTexture);
                material.SetColor("_Color", ColorTint1);
                material.SetColor("_ColorB", ColorTint2);
                material.SetFloat("_Cutoff", TextureCutoff);
                material.SetFloat("_RandomDarkening", RandomDarkening);
                material.SetFloat("_RootAmbient", RootAmbient);
                material.SetVector("_AG_ColorNoiseArea", new Vector4(0, 30, 0, 1));
                material.SetTexture("_AG_ColorNoiseTex", Resources.Load("PerlinSeamless") as Texture2D);
                material.EnableKeyword("_ALPHATEST_ON");
            }

            AssetDatabase.CreateAsset(material, materialFilename);
            GameObject go = new GameObject {name = prefabName};
            // "New grass patch_" + patchID;
            go.transform.position = transform.position;
            go.transform.rotation = transform.localRotation;

            Mesh meshLod0 = GetCombinedMesh(0);
            Mesh meshLod1 = GetCombinedMesh(1);
            Mesh meshLod2 = GetCombinedMesh(2);

            GameObject goLod0 = new GameObject {name = prefabName + "_LOD0"};
            goLod0.transform.SetParent(go.transform, false);
            goLod0.AddComponent<MeshFilter>().sharedMesh = meshLod0;
            goLod0.AddComponent<MeshRenderer>().sharedMaterial = material;

            GameObject goLod1 = new GameObject {name = prefabName + "_LOD1"};
            goLod1.transform.SetParent(go.transform, false);
            goLod1.AddComponent<MeshFilter>().sharedMesh = meshLod1;
            goLod1.AddComponent<MeshRenderer>().sharedMaterial = material;

            GameObject goLod2 = new GameObject {name = prefabName + "_LOD2"};
            goLod2.transform.SetParent(go.transform, false);
            goLod2.AddComponent<MeshFilter>().sharedMesh = meshLod2;
            goLod2.AddComponent<MeshRenderer>().sharedMaterial = material;

            AssetDatabase.CreateAsset(meshLod0, meshFilenameLod0);
            AssetDatabase.CreateAsset(meshLod1, meshFilenameLod1);
            AssetDatabase.CreateAsset(meshLod2, meshFilenameLod2);

            LODGroup lodGroup = go.AddComponent<LODGroup>();
            LOD[] lods = new LOD[4];
            lods[0] = CreateLOD(goLod0, 1f);
            lods[1] = CreateLOD(goLod0, 0.50f);
            lods[2] = CreateLOD(goLod1, 0.15f);
            lods[3] = CreateLOD(goLod2, 0.05f);
            lodGroup.SetLODs(lods);

            transform.position = oldPosition;
            transform.rotation = oldRotation;
            
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAsset(go, prefabFilename);
#else
            PrefabUtility.CreatePrefab(prefabFilename, go);
#endif

            AssetDatabase.Refresh();
#endif
        }

        private LOD CreateLOD(GameObject go, float screenRelativeTransitionHeight)
        {
            Renderer[] renderers;
            if (go)
            {
                renderers = new Renderer[1];
                renderers[0] = go.GetComponent<Renderer>();
            }
            else
            {
                renderers = new Renderer[0];
            }

            return new LOD(screenRelativeTransitionHeight, renderers);
        }
    }
}
