using System;
using System.Collections.Generic;
using System.IO;
using AwesomeTechnologies.Shaders;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Extentions;
using AwesomeTechnologies.Vegetation.Masks;
using Unity.Collections;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    
    [Serializable]
    public struct VegetationItemIndexes
    {
        public int VegetationPackageIndex;
        public int VegetationItemIndex;
    }
    
    public class VegetationInfoComparer : IComparer<int>
    {
        public List<VegetationItemInfoPro> VegetationInfoList;
        public int Compare(int a, int b)
        {
            var aTypeValue = (int)VegetationInfoList[a].VegetationType;
            var bTypeValue = (int)VegetationInfoList[b].VegetationType;
            return bTypeValue.CompareTo(aTypeValue);
        }
    }  

    public class BiomeSortOrderComparer : IComparer<VegetationPackagePro>
    {
        public int Compare(VegetationPackagePro x, VegetationPackagePro y)
        {
            if (x != null && y != null)
            {
                return x.BiomeSortOrder.CompareTo(y.BiomeSortOrder);
            }
            else
            {
                return 0;
            }
        }
    }


    public class VegetationInfoIDComparer : IComparer<string>
    {
        public List<VegetationItemInfoPro> VegetationInfoList;

        public int Compare(string a, string b)
        {
            var indexA = GetIndexFromID(a);
            var indexB = GetIndexFromID(b);

            if (indexA < 0 || indexB < 0) return -1;

            var aTypeValue = (int)VegetationInfoList[indexA].VegetationType;
            var bTypeValue = (int)VegetationInfoList[indexB].VegetationType;
            return bTypeValue.CompareTo(aTypeValue);
        }

        private int GetIndexFromID(string id)
        {
            for (var i = 0; i <= VegetationInfoList.Count - 1; i++)
                if (VegetationInfoList[i].VegetationItemID == id) return i;

            return -1;
        }
    }

    [Serializable]
    public class TerrainTextureRule
    {
        public int TextureIndex;
        public float MinimumValue;
        public float MaximumValue;

        public TerrainTextureRule()
        {

        }

        public TerrainTextureRule(TerrainTextureRule sourceItem)
        {
            TextureIndex = sourceItem.TextureIndex;
            MinimumValue = sourceItem.MinimumValue;
            MaximumValue = sourceItem.MaximumValue;
        }
    }

    [Serializable]
    public enum BiomeType
    {
        Default = 0,
        BorealForest = 1,
        TemperateDeciduousForest = 2,
        TropicalRainForest = 3,
        TemperateRainForest = 4,
        ScrubForest = 5,
        DeadForest = 6,
        FantasyForest = 7,
        Grassland = 8,
        Desert = 9,
        Swamp = 10,
        Tundra = 11,
        Oasis = 12,
        Underwater = 13,
        FrozenForest = 14,
        Volcano = 15,
        River = 16,
        Seaside = 17,
        Meadow = 18,
        Lake = 19, 
        Road = 20,
        Biome1 = 21,
        Biome2 = 22,
        Biome3 = 23,
        Biome4 = 24,
        Biome5 = 25,
        Biome6 = 26,
        Biome7 = 27,
        Biome8 = 28
    }

    [Serializable]
    public enum BillboardRenderMode
    {
        Standard = 0,
        Specular = 1
    }

    [Serializable]
    public enum TerrainSourceID
    {
        TerrainSourceID1 = 0,
        TerrainSourceID2 = 1,
        TerrainSourceID3 = 2,
        TerrainSourceID4 = 3,
        TerrainSourceID5 = 4,
        TerrainSourceID6 = 5,
        TerrainSourceID7 = 6,
        TerrainSourceID8 = 7
    }

    [Serializable]
    public class RuntimePrefabRule
    {
        public GameObject RuntimePrefab;
        public float DistanceFactor = 0.15f;
        public float SpawnFrequency = 1f;
        public int Seed;
        public bool UsePool = true;
        public Vector3 PrefabOffset = new Vector3(0, 0, 0);
        public Vector3 PrefabRotation = new Vector3(0, 0, 0);
        public Vector3 PrefabScale = new Vector3(1, 1, 1);
        public LayerMask PrefabLayer = 0;
        public bool UseVegetationItemScale;

        public void SetSeed()
        {
            Seed = UnityEngine.Random.Range(0, 99);
        }

        public RuntimePrefabRule(RuntimePrefabRule sourceItem)
        {
            RuntimePrefab = sourceItem.RuntimePrefab;
            SpawnFrequency = sourceItem.SpawnFrequency;
            Seed = sourceItem.Seed;
            PrefabOffset = sourceItem.PrefabOffset;
            PrefabRotation = sourceItem.PrefabRotation;
            PrefabScale = sourceItem.PrefabScale;
            PrefabLayer = sourceItem.PrefabLayer;
            UseVegetationItemScale = sourceItem.UseVegetationItemScale;
            UsePool = sourceItem.UsePool;
        }
        
        public RuntimePrefabRule()
        {
           
        }
    }
    
    [Serializable]
    public struct TerrainSourceRule
    {
        public bool UseTerrainSourceID1;
        public bool UseTerrainSourceID2;
        public bool UseTerrainSourceID3;
        public bool UseTerrainSourceID4;
        public bool UseTerrainSourceID5;
        public bool UseTerrainSourceID6;
        public bool UseTerrainSourceID7;
        public bool UseTerrainSourceID8;

        public bool this[int index]
        {
            get
            {
                // ReSharper disable once ArrangeAccessorOwnerBody
                return UseTerrainSource(index);
            }

            set
            {
                // ReSharper disable once ArrangeAccessorOwnerBody
                SetUseTerrainSource(index, value);
            }
        }

        public void SetUseTerrainSource(int index,bool value)
        {
            switch (index)
            {
                case 0:
                    UseTerrainSourceID1 = value;
                    break;
                case 1:
                    UseTerrainSourceID2 = value;
                    break;
                case 2:
                    UseTerrainSourceID3 = value;
                    break;
                case 3:
                    UseTerrainSourceID4 = value;
                    break;
                case 4:
                    UseTerrainSourceID5 = value;
                    break;
                case 5:
                    UseTerrainSourceID6 = value;
                    break;
                case 6:
                    UseTerrainSourceID7 = value;
                    break;
                case 7:
                    UseTerrainSourceID8 = value;
                    break;
            }
        }

        public bool UseTerrainSource(int index)
        {
            switch (index)
            {
                case 0:
                    return UseTerrainSourceID1;
                case 1:
                    return UseTerrainSourceID2;
                case 2:
                    return UseTerrainSourceID3;
                case 3:
                    return UseTerrainSourceID4;
                case 4:
                    return UseTerrainSourceID5;
                case 5:
                    return UseTerrainSourceID6;
                case 6:
                    return UseTerrainSourceID7;
                case 7:
                    return UseTerrainSourceID8;
                default:
                    return false;
            }
        }
    }

    [Serializable]
    public enum VegetationRotationType
    {
        RotateY = 0,
        // ReSharper disable once InconsistentNaming
        RotateXYZ = 1,
        FollowTerrain = 2,
        FollowTerrainScale = 3,
        NoRotation = 4
    }

    [Serializable]
    public enum VegetationShaderType
    {
        Standard = 0,
        Other = 1,
        Speedtree = 2,
        VegetationStudioGrass = 3,
    }

    [Serializable]
    public enum VegetationRenderMode
    {
        Instanced = 0,
        Normal = 1,
        InstancedIndirect = 2
    }

    [Serializable]
    public enum LODLevel
    {
        LOD0 = 0,
        LOD1 = 1,
        LOD2 = 2,
        LOD3 = 3
    }

    [Serializable]
    public enum TerrainTextureType
    {
        Texture1 = 0,
        Texture2 = 1,
        Texture3 = 2,
        Texture4 = 3,
        Texture5 = 4,
        Texture6 = 5,
        Texture7 = 6,
        Texture8 = 7,
        Texture9 = 8,
        Texture10 = 9,
        Texture11 = 10,
        Texture12 = 11,
        Texture13 = 12,
        Texture14 = 13,
        Texture15 = 14,
        Texture16 = 15,
        Texture17 = 16,
        Texture18 = 17,
        Texture19 = 18,
        Texture20 = 19,
        Texture21 = 20,
        Texture22 = 21,
        Texture23 = 22,
        Texture24 = 23,
        Texture25 = 24,
        Texture26 = 25,
        Texture27 = 26,
        Texture28 = 27,
        Texture29 = 28,
        Texture30 = 29,
        Texture31 = 30,
        Texture32 = 31
    }


    [Serializable]
    public enum VegetationTypeIndex
    {
        VegetationType1 = 1,
        VegetationType2 = 2,
        VegetationType3 = 3,
        VegetationType4 = 4,
        VegetationType5 = 5,
        VegetationType6 = 6,
        VegetationType7 = 7,
        VegetationType8 = 8,
        VegetationType9 = 9,
        VegetationType10 = 10,
        VegetationType11 = 11,
        VegetationType12 = 12,
        VegetationType13 = 13,
        VegetationType14 = 14,
        VegetationType15 = 15,
        VegetationType16 = 16,
        VegetationType17 = 17,
        VegetationType18 = 18,
        VegetationType19 = 19,
        VegetationType20 = 20,
        VegetationType21 = 21,
        VegetationType22 = 22,
        VegetationType23 = 23,
        VegetationType24 = 24,
        VegetationType25 = 25,
        VegetationType26 = 26,
        VegetationType27 = 27,
        VegetationType28 = 28,
        VegetationType29 = 29,
        VegetationType30 = 30,
        VegetationType31 = 31,
        VegetationType32 = 32
    }

    [Serializable]
    public enum BillboardQuality
    {
        Normal = 0,
        High = 1,
        Max = 2,
        Normal3D = 4,
        High3D = 5,
        Max3D = 6,
        HighSample3D = 7,
        HighSample2D = 8,
        NormalSingle = 9,
        HighSingle = 10,
        MaxSingle = 11,
        NormalQuad = 12,
        HighQuad = 13,
        MaxQuad = 14,

    }

    [Serializable]
    public enum VegetationType
    {
        Grass = 0,
        Plant = 1,
        Tree = 2,
        Objects = 3,
        LargeObjects = 4
    }

    [Serializable]
    public enum VegetationPrefabType
    {
        Mesh = 0,
        Texture = 1
    }
    
    [Serializable]
    public enum NavMeshObstacleType
    {
        Disabled = 0,
        Capsule = 1,
        Box = 2
    }
    
    [Serializable]
    public enum ConcaveMode
    {
        Additive = 0,
        Blended = 1
    }

    [Serializable]
    public class TerrainTextureSettings
    {
        public AnimationCurve TextureHeightCurve;
        public AnimationCurve TextureSteepnessCurve;
        public bool UseNoise;
        public float NoiseScale = 5;
        public float TextureWeight = 1;
        public Vector2 NoiseOffset = Vector2.zero;
        public bool InverseNoise;
        public int TextureLayer;
        public bool Enabled = true;
        public bool LockTexture;
        public NativeArray<float> HeightCurveArray;
        public NativeArray<float> SteepnessCurveArray;
        
        public bool ConcaveEnable;
        public bool ConvexEnable;   
        public bool ConcaveAverage = true;
        public float ConcaveMinHeightDifference = 5;
        public float ConcaveDistance = 10;
        public ConcaveMode ConcaveMode = ConcaveMode.Additive;
    }

    [Serializable]
    public class TerrainTextureInfo
    {
        public Texture2D Texture;
        public Texture2D TextureNormals;
        public Texture2D TextureOcclusion;
        public Texture2D TextureHeightMap;
        public Vector2 TileSize = new Vector2(15, 15);
        public Vector2 Offset;
#if UNITY_2018_3_OR_NEWER
        public TerrainLayer TerrainLayer;
#endif
    }

    [Serializable]
    public enum ColliderType
    {
        Disabled = 0,
        Capsule = 1,
        Sphere = 2,
        Box = 3,
        Mesh = 4,
        CustomMesh = 5,
        FromPrefab = 6
     }

    [Serializable]
    public class TextureMaskRule
    {
        public String TextureMaskGroupID;
        public float MinDensity = 0.1f;
        public float MaxDensity = 1;
        public float ScaleMultiplier = 1;
        public float DensityMultiplier = 1;
        public List<SerializedControllerProperty> TextureMaskPropertiesList = new List<SerializedControllerProperty>();

        public TextureMaskRule(TextureMaskRule sourceItem)
        {
            TextureMaskGroupID = sourceItem.TextureMaskGroupID;
            MinDensity = sourceItem.MinDensity;
            MaxDensity = sourceItem.MaxDensity;
            ScaleMultiplier = sourceItem.ScaleMultiplier;
            DensityMultiplier = sourceItem.DensityMultiplier;

            for (var i = 0; i <= sourceItem.TextureMaskPropertiesList.Count - 1; i++)
                TextureMaskPropertiesList.Add(new SerializedControllerProperty(sourceItem.TextureMaskPropertiesList[i]));
        }

        public TextureMaskRule(TextureMaskSettings textureMaskSettings)
        {
            for (var i = 0; i <= textureMaskSettings.ControlerPropertyList.Count - 1; i++)
            {
                TextureMaskPropertiesList.Add(new SerializedControllerProperty(textureMaskSettings.ControlerPropertyList[i]));
            }               
        }


        public bool GetBooleanPropertyValue(string propertyName)
        {
            for (int i = 0;
                i <= TextureMaskPropertiesList.Count - 1; i++)
            {
                if (TextureMaskPropertiesList[i].PropertyName == propertyName)
                {
                    return TextureMaskPropertiesList[i].BoolValue;
                }
            }
            return false;
        }

        public int GetIntPropertyValue(string propertyName)
        {
            for (int i = 0;
                i <= TextureMaskPropertiesList.Count - 1; i++)
            {
                if (TextureMaskPropertiesList[i].PropertyName == propertyName)
                {
                    return TextureMaskPropertiesList[i].IntValue;
                }
            }

            return 0;
        }
    }

    [Serializable]
    public class VegetationItemInfoPro
    {
        public string VegetationItemID;
        public string Name;
        public VegetationType VegetationType = VegetationType.Tree;
        public VegetationPrefabType PrefabType = VegetationPrefabType.Mesh;
        public VegetationRenderMode VegetationRenderMode = VegetationRenderMode.Instanced;

        public bool EnableRuntimeSpawn = true;

        public bool DisableShadows;

        public GameObject VegetationPrefab;
        public Texture2D VegetationTexture;
        public string VegetationGuid = "";
        
        public float SampleDistance = 1.5f;
        public float Density = 1f;
        public bool RandomizePosition = true;

        public bool UseVegetationMasksOnStorage;

        public bool UseSamplePointOffset;
        public float SamplePointMinOffset = 3;
        public float SamplePointMaxOffset = 5;

        public Vector3 Offset = new Vector3(0, 0, 0);
        public Vector3 RotationOffset = new Vector3(0, 0, 0);
        public VegetationRotationType Rotation = VegetationRotationType.RotateY;
        public float MinUpOffset = 0;
        public float MaxUpOffset = 0;

        public Bounds Bounds;
        public Vector3 ScaleMultiplier = new Vector3(1,1,1);

        public float RenderDistanceFactor = 1;
        public int Seed;

        public float MinScale = 0.8f;
        public float MaxScale = 1.2f;
        public float YScale = 1;

        public ColliderType ColliderType = ColliderType.Disabled;
        public float ColliderRadius = 0.25f;
        public float ColliderHeight = 2f;
        public Vector3 ColliderOffset = Vector3.zero;
        public Vector3 ColliderSize = Vector3.one;
        public bool ColliderTrigger;
        public Mesh ColliderMesh;
        public bool ColliderUseForBake = true;
        public float ColliderDistanceFactor = 0.15f;
        public string ColliderTag = "";
        public bool ColliderConvex;
        
        public NavMeshObstacleType NavMeshObstacleType = NavMeshObstacleType.Disabled;
        public Vector3 NavMeshObstacleCenter;
        public Vector3 NavMeshObstacleSize = Vector3.one;
        public float NavMeshObstacleRadius = 0.5f;
        public float NavMeshObstacleHeight = 2f;
        public bool NavMeshObstacleCarve = true;
        public int NavMeshArea;
        
        public bool UseBillboards = true;
        public BillboardQuality BillboardQuality = BillboardQuality.Normal;
        public Texture2D BillboardTexture;
        public Texture2D BillboardNormalTexture;
        public Texture2D BillboardAoTexture;
        public LODLevel BillboardSourceLODLevel;
        public ColorSpace BillboardColorSpace = ColorSpace.Uninitialized;
        public float BillboardBrightness = 1;
        public float BillboardCutoff = 0.2f;
        public Color BillboardTintColor = Color.white;
        public Color BillboardAtlasBackgroundColor = new Color(80 / 255f, 80 / 255f, 20 / 255f);
        public float BillboardMipmapBias = -2;
        public float BillboardWindSpeed = 1;
        public float BillboardSmoothness = 0.2f;
        public float BillboardMetallic = 0.5f;
        public float BillboardSpecular;
        public float BillboardOcclusion = 1f;
        public float BillboardNormalStrength = 1f;
        public bool BillboardRecalculateNormals;
        public float BillboardNormalBlendFactor = 1f;
        public BillboardRenderMode BillboardRenderMode = BillboardRenderMode.Specular;
        public bool BillboardFlipBackNormals;

        public bool UseHeightRule = true;
        public float MinHeight;
        public float MaxHeight = 1500;

        public bool UseAdvancedHeightRule;
        public float MaxCurveHeight = 500;
        public AnimationCurve HeightRuleCurve = new AnimationCurve();

        public bool UseSteepnessRule = true;
        public float MinSteepness;
        public float MaxSteepness = 30f;

        public bool UseAdvancedSteepnessRule;
        public AnimationCurve SteepnessRuleCurve = new AnimationCurve();

        public bool UseNoiseCutoff = true;
        public float NoiseCutoffValue = 0.5f;
        public float NoiseCutoffScale = 5;
        public bool NoiseCutoffInverse;
        public Vector2 NoiseCutoffOffset = new Vector2(0, 0);

        public bool UseNoiseDensity = true;
        public float NoiseDensityScale = 5;
        public bool NoiseDensityInverse;
        public Vector2 NoiseDensityOffset = new Vector2(0, 0);

        public bool UseNoiseScaleRule;
        public float NoiseScaleMinScale = 0.7f;
        public float NoiseScaleMaxScale = 1.3f;
        public float NoiseScaleScale = 5;
        public bool NoiseScaleInverse;
        public Vector2 NoiseScaleOffset = new Vector2(0, 0);

        public bool UseBiomeEdgeScaleRule;
        public float BiomeEdgeScaleDistance = 10f;
        public float BiomeEdgeScaleMinScale = 0.3f;
        public float BiomeEdgeScaleMaxScale = 1;
        public bool BiomeEdgeScaleInverse;

        public bool UseBiomeEdgeIncludeRule;
        public float BiomeEdgeIncludeDistance = 10f;
        public bool BiomeEdgeIncludeInverse;

        public bool UseConcaveLocationRule;
        public bool ConcaveLocationInverse;
        public float ConcaveLoactionMinHeightDifference = 1f;
        public float ConcaveLoactionDistance = 3f;
        public bool ConcaveLoactionAverage = true;

        public bool UseTerrainTextureIncludeRules;
        public List<TerrainTextureRule> TerrainTextureIncludeRuleList = new List<TerrainTextureRule>();

        public bool UseTerrainTextureExcludeRules;
        public List<TerrainTextureRule> TerrainTextureExcludeRuleList = new List<TerrainTextureRule>();


        public bool UseTextureMaskIncludeRules;
        public List<TextureMaskRule> TextureMaskIncludeRuleList = new List<TextureMaskRule>();

        public bool UseTextureMaskExcludeRules;
        public List<TextureMaskRule> TextureMaskExcludeRuleList = new List<TextureMaskRule>();

        public bool UseTextureMaskScaleRules;
        public List<TextureMaskRule> TextureMaskScaleRuleList = new List<TextureMaskRule>();

        public bool UseTextureMaskDensityRules;
        public List<TextureMaskRule> TextureMaskDensityRuleList = new List<TextureMaskRule>();

        public string ShaderName;
        public ShaderControllerSettings ShaderControllerSettings;

        public bool UseTerrainSourceIncludeRule ;
        public TerrainSourceRule TerrainSourceIncludeRule;
        public bool UseTerrainSourceExcludeRule;
        public TerrainSourceRule TerrainSourceExcludeRule;

        public bool DisableLOD;
        public float LODFactor = 1;

        public bool UseDistanceFalloff;

        public float DistanceFalloffStartDistance = 0.4f;

        public bool UseVegetationMask;
        public VegetationTypeIndex VegetationTypeIndex = VegetationTypeIndex.VegetationType1;
        
        public List<RuntimePrefabRule> RuntimePrefabRuleList = new List<RuntimePrefabRule>();
        
        public VegetationItemInfoPro()
        {

        }

        public VegetationItemInfoPro(VegetationItemInfoPro sourceItem)
        {
            VegetationItemID = Guid.NewGuid().ToString();           
            CopySettingValues(sourceItem);
            Seed = UnityEngine.Random.Range(0, 99);
        }

        public void CopySettingValues(VegetationItemInfoPro sourceItem)
        {
            Name = sourceItem.Name;
            VegetationType = sourceItem.VegetationType;
            VegetationType = sourceItem.VegetationType;
            VegetationRenderMode = sourceItem.VegetationRenderMode;
            VegetationPrefab = sourceItem.VegetationPrefab;
            VegetationTexture = sourceItem.VegetationTexture;
            SampleDistance = sourceItem.SampleDistance;

            EnableRuntimeSpawn = sourceItem.EnableRuntimeSpawn;

            UseSamplePointOffset = sourceItem.UseSamplePointOffset;
            SamplePointMinOffset = sourceItem.SamplePointMinOffset;
            SamplePointMaxOffset = sourceItem.SamplePointMaxOffset;
            VegetationGuid = sourceItem.VegetationGuid;

            Offset = sourceItem.Offset;
            RotationOffset = sourceItem.RotationOffset;
            Rotation = sourceItem.Rotation;
            Bounds = new Bounds();

            RenderDistanceFactor = sourceItem.RenderDistanceFactor;
            Seed = sourceItem.Seed; 

            MinScale = sourceItem.MinScale;
            MaxScale = sourceItem.MaxScale;
            YScale = sourceItem.YScale;

            UseVegetationMasksOnStorage = sourceItem.UseVegetationMasksOnStorage;

            ColliderType = sourceItem.ColliderType;
            ColliderRadius = sourceItem.ColliderRadius;
            ColliderHeight = sourceItem.ColliderHeight;
            ColliderOffset = sourceItem.ColliderOffset;
            ColliderTrigger = sourceItem.ColliderTrigger;
            ColliderMesh = sourceItem.ColliderMesh;
            ColliderUseForBake = sourceItem.ColliderUseForBake;
            ColliderDistanceFactor = sourceItem.ColliderDistanceFactor;
            ColliderSize = sourceItem.ColliderSize;
            ColliderConvex = sourceItem.ColliderConvex;

            NavMeshObstacleType = sourceItem.NavMeshObstacleType;
            NavMeshObstacleCenter = sourceItem.NavMeshObstacleCenter;
            NavMeshObstacleSize = sourceItem.NavMeshObstacleSize;
            NavMeshObstacleRadius = sourceItem.NavMeshObstacleRadius;
            NavMeshObstacleHeight = sourceItem.NavMeshObstacleHeight;
            NavMeshObstacleCarve = sourceItem.NavMeshObstacleCarve;                 
            
            UseBillboards = sourceItem.UseBillboards;
            BillboardQuality = sourceItem.BillboardQuality;
            BillboardTexture = sourceItem.BillboardTexture;
            BillboardAoTexture = sourceItem.BillboardAoTexture;
            BillboardNormalTexture = sourceItem.BillboardNormalTexture;
            BillboardSourceLODLevel = sourceItem.BillboardSourceLODLevel;
            BillboardColorSpace = sourceItem.BillboardColorSpace;
            BillboardBrightness = sourceItem.BillboardBrightness;
            BillboardCutoff = sourceItem.BillboardCutoff;
            BillboardTintColor = sourceItem.BillboardTintColor;
            BillboardAtlasBackgroundColor = sourceItem.BillboardAtlasBackgroundColor;
            BillboardMipmapBias = sourceItem.BillboardMipmapBias;
            BillboardWindSpeed = sourceItem.BillboardWindSpeed;
            BillboardMetallic = sourceItem.BillboardMetallic;
            BillboardSmoothness = sourceItem.BillboardSmoothness;
            BillboardSpecular = sourceItem.BillboardSpecular;
            BillboardOcclusion = sourceItem.BillboardOcclusion;
            BillboardRenderMode = sourceItem.BillboardRenderMode;
            BillboardNormalStrength = sourceItem.BillboardNormalStrength;
            BillboardRecalculateNormals = sourceItem.BillboardRecalculateNormals;
            BillboardNormalBlendFactor = sourceItem.BillboardNormalBlendFactor;
            BillboardFlipBackNormals = sourceItem.BillboardFlipBackNormals;

            UseHeightRule = sourceItem.UseHeightRule;
            MinHeight = sourceItem.MinHeight;
            MaxHeight = sourceItem.MaxHeight;

            UseSteepnessRule = sourceItem.UseSteepnessRule;
            MinSteepness = sourceItem.MinSteepness;
            MaxSteepness = sourceItem.MaxSteepness;

            UseConcaveLocationRule = sourceItem.UseConcaveLocationRule;
            ConcaveLocationInverse = sourceItem.ConcaveLocationInverse;
            ConcaveLoactionMinHeightDifference = sourceItem.ConcaveLoactionMinHeightDifference;
            ConcaveLoactionDistance = sourceItem.ConcaveLoactionDistance;
            ConcaveLoactionAverage = sourceItem.ConcaveLoactionAverage;

            UseNoiseCutoff = sourceItem.UseNoiseCutoff;
            NoiseCutoffValue = sourceItem.NoiseCutoffValue;
            NoiseCutoffScale = sourceItem.NoiseCutoffScale;
            NoiseCutoffInverse = sourceItem.NoiseCutoffInverse;
            NoiseCutoffOffset = sourceItem.NoiseCutoffOffset;

            UseNoiseDensity = sourceItem.UseNoiseDensity;
            NoiseDensityScale = sourceItem.NoiseDensityScale;
            NoiseDensityInverse = sourceItem.NoiseDensityInverse;
            NoiseDensityOffset = sourceItem.NoiseDensityOffset;

            UseNoiseScaleRule = sourceItem.UseNoiseScaleRule;
            NoiseScaleMinScale = sourceItem.NoiseScaleMinScale;
            NoiseScaleMaxScale = sourceItem.NoiseScaleMaxScale;
            NoiseScaleScale = sourceItem.NoiseScaleScale;
            NoiseScaleInverse = sourceItem.NoiseScaleInverse;
            NoiseScaleOffset = sourceItem.NoiseScaleOffset;

            UseBiomeEdgeScaleRule = sourceItem.UseBiomeEdgeScaleRule;
            BiomeEdgeScaleDistance = sourceItem.BiomeEdgeScaleDistance;
            BiomeEdgeScaleMinScale = sourceItem.BiomeEdgeScaleMinScale;
            BiomeEdgeScaleMaxScale = sourceItem.BiomeEdgeScaleMaxScale;
            BiomeEdgeScaleInverse = sourceItem.BiomeEdgeScaleInverse;

            UseBiomeEdgeIncludeRule = sourceItem.UseBiomeEdgeIncludeRule;
            BiomeEdgeIncludeDistance = sourceItem.BiomeEdgeIncludeDistance;
            BiomeEdgeIncludeInverse = sourceItem.BiomeEdgeIncludeInverse;

            UseTerrainTextureIncludeRules = sourceItem.UseTerrainTextureIncludeRules;
            UseTerrainTextureExcludeRules = sourceItem.UseTerrainTextureExcludeRules;

            for (var i = 0; i <= sourceItem.TerrainTextureIncludeRuleList.Count - 1; i++)
                TerrainTextureIncludeRuleList.Add(new TerrainTextureRule(sourceItem.TerrainTextureIncludeRuleList[i]));

            for (var i = 0; i <= sourceItem.TerrainTextureExcludeRuleList.Count - 1; i++)
                TerrainTextureExcludeRuleList.Add(new TerrainTextureRule(sourceItem.TerrainTextureExcludeRuleList[i]));

            UseTextureMaskIncludeRules = sourceItem.UseTextureMaskIncludeRules;
            for (var i = 0; i <= sourceItem.TextureMaskIncludeRuleList.Count - 1; i++)
                TextureMaskIncludeRuleList.Add(new TextureMaskRule(sourceItem.TextureMaskIncludeRuleList[i]));

            UseTextureMaskExcludeRules = sourceItem.UseTextureMaskExcludeRules;
            for (var i = 0; i <= sourceItem.TextureMaskExcludeRuleList.Count - 1; i++)
                TextureMaskExcludeRuleList.Add(new TextureMaskRule(sourceItem.TextureMaskExcludeRuleList[i]));

            UseTextureMaskScaleRules = sourceItem.UseTextureMaskScaleRules;
            for (var i = 0; i <= sourceItem.TextureMaskScaleRuleList.Count - 1; i++)
                TextureMaskScaleRuleList.Add(new TextureMaskRule(sourceItem.TextureMaskScaleRuleList[i]));

            UseTextureMaskDensityRules = sourceItem.UseTextureMaskDensityRules;
            for (var i = 0; i <= sourceItem.TextureMaskDensityRuleList.Count - 1; i++)
                TextureMaskDensityRuleList.Add(new TextureMaskRule(sourceItem.TextureMaskDensityRuleList[i]));

            ShaderName = sourceItem.ShaderName;

            ShaderControllerSettings = new ShaderControllerSettings(sourceItem.ShaderControllerSettings);

            DisableLOD = sourceItem.DisableLOD;
            LODFactor = sourceItem.LODFactor;
            UseDistanceFalloff = sourceItem.UseDistanceFalloff;
            DistanceFalloffStartDistance = sourceItem.DistanceFalloffStartDistance;
            //DistanceFallofffAnimationCurve = new AnimationCurve(sourceItem.DistanceFallofffAnimationCurve.keys);

            UseVegetationMask = sourceItem.UseVegetationMask;
            VegetationTypeIndex = sourceItem.VegetationTypeIndex;

            UseTerrainSourceIncludeRule  = sourceItem.UseTerrainSourceIncludeRule;
            TerrainSourceIncludeRule = sourceItem.TerrainSourceIncludeRule;

            UseTerrainSourceExcludeRule = sourceItem.UseTerrainSourceExcludeRule;
            TerrainSourceExcludeRule = sourceItem.TerrainSourceExcludeRule;


            UseAdvancedHeightRule = sourceItem.UseAdvancedHeightRule;
            MaxCurveHeight = sourceItem.MaxCurveHeight;
            HeightRuleCurve = new AnimationCurve(sourceItem.SteepnessRuleCurve.keys);

            UseAdvancedSteepnessRule = sourceItem.UseAdvancedSteepnessRule;
            SteepnessRuleCurve = new AnimationCurve(sourceItem.SteepnessRuleCurve.keys);
            
            for (var i = 0; i <= sourceItem.RuntimePrefabRuleList.Count - 1; i++)
                RuntimePrefabRuleList.Add(new RuntimePrefabRule(sourceItem.RuntimePrefabRuleList[i]));
        }    

        public void Init()
        {
            HeightRuleCurve.AddKey(0, 1);
            HeightRuleCurve.AddKey(1, 1);

            SteepnessRuleCurve.AddKey(0, 0);
            SteepnessRuleCurve.AddKey(0.5f, 1);

            //DistanceFallofffAnimationCurve.AddKey(0, 1);
            //DistanceFallofffAnimationCurve.AddKey(1, 0);
        }

        public int GetDistanceBand()
        {
            if (VegetationType == VegetationType.Tree || VegetationType == VegetationType.LargeObjects) //|| VegetationType == VegetationType.Objects
            {
                return 1;
            }
            return 0;
        }
    }

    [Serializable]
    public class VegetationPackagePro : ScriptableObject
    {
        public string PackageName = "No name";
        [SerializeField]
        public List<VegetationItemInfoPro> VegetationInfoList = new List<VegetationItemInfoPro>();
        public List<TerrainTextureSettings> TerrainTextureSettingsList = new List<TerrainTextureSettings>();
        public List<TerrainTextureInfo> TerrainTextureList = new List<TerrainTextureInfo>();

        public List<TextureMaskGroup> TextureMaskGroupList = new List<TextureMaskGroup>();

        public int TerrainTextureCount;
        public BiomeType BiomeType = BiomeType.Default;
        public int BiomeSortOrder = 1;
        public bool GenerateBiomeSplamap = true;

        public void InitPackage()
        {

        }
        
        /// <summary>
        /// Get the VegetationItemID from the assetGUID. an empty string will be returned if not found
        /// </summary>
        /// <param name="assetGuid"></param>
        /// <returns></returns>
        public string GetVegetationItemID(string assetGuid)
        {
            for (var i = 0; i <= VegetationInfoList.Count - 1; i++)
            {
                if (VegetationInfoList[i].VegetationGuid == assetGuid)
                {
                    return VegetationInfoList[i].VegetationItemID;
                }
            }

            return "";
        }
        
        public void ResizeTerrainTextureList(int newCount)
        {
            if (newCount <= 0)
                TerrainTextureList.Clear();
            else
                while (TerrainTextureList.Count > newCount) TerrainTextureList.RemoveAt(TerrainTextureList.Count - 1);
        }

        public void RegenerateVegetationItemIDs()
        {
            for (int i = 0; i <= VegetationInfoList.Count - 1; i++)
            {
                VegetationInfoList[i].VegetationItemID = Guid.NewGuid().ToString();
            }
            
            
        }

        public void ResizeTerrainTextureSettingsList(int newCount)
        {
            if (newCount <= 0)
                TerrainTextureSettingsList.Clear();
            else
                while (TerrainTextureSettingsList.Count > newCount) TerrainTextureSettingsList.RemoveAt(TerrainTextureSettingsList.Count - 1);
        }
        
      

        public void DeleteTextureMaskGroup(TextureMaskGroup textureMaskGroup)
        {
            TextureMaskGroupList.Remove(textureMaskGroup);
            for (var i = 0; i <= VegetationInfoList.Count - 1; i++)
            {
                VegetationItemInfoPro vegetationItemInfoPro = VegetationInfoList[i];

                for (int j = vegetationItemInfoPro.TextureMaskIncludeRuleList.Count - 1; j >= 0; j--)
                {
                    if (vegetationItemInfoPro.TextureMaskIncludeRuleList[j].TextureMaskGroupID == textureMaskGroup.TextureMaskGroupID)
                    {
                        vegetationItemInfoPro.TextureMaskIncludeRuleList.RemoveAt(j);
                    }
                }

                if (vegetationItemInfoPro.TextureMaskIncludeRuleList.Count == 0)
                {
                    vegetationItemInfoPro.UseTextureMaskIncludeRules = false;
                }
            }
            //TODO delete other rules also when added
        }

        public TextureMaskGroup GetTextureMaskGroup(string textureMaskGroupID)
        {
            for (var i = 0; i <= TextureMaskGroupList.Count - 1; i++)
            {
                if (TextureMaskGroupList[i].TextureMaskGroupID == textureMaskGroupID) return TextureMaskGroupList[i];
            }

            return null;
        }

        public VegetationItemInfoPro GetVegetationInfo(string id)
        {
            for (var i = 0; i <= VegetationInfoList.Count - 1; i++)
                if (VegetationInfoList[i].VegetationItemID == id) return VegetationInfoList[i];
            return null;
        }

        public void PrepareNativeArrayTextureCurves()
        {
            for (var i = 0; i <= TerrainTextureSettingsList.Count - 1; i++)
            {
                if (TerrainTextureSettingsList[i].HeightCurveArray.IsCreated)
                {
                    TerrainTextureSettingsList[i].HeightCurveArray.Dispose();
                }
                
                TerrainTextureSettingsList[i].HeightCurveArray = new NativeArray<float>(4096, Allocator.Persistent);

                if (!ValidateAnimationCurve(TerrainTextureSettingsList[i].TextureHeightCurve))
                {
                    TerrainTextureSettingsList[i].TextureHeightCurve = CreateResetAnimationCurve();
                }
                
                TerrainTextureSettingsList[i].HeightCurveArray.CopyFrom(TerrainTextureSettingsList[i].TextureHeightCurve.GenerateCurveArray(4096));
                               
                if (TerrainTextureSettingsList[i].SteepnessCurveArray.IsCreated)
                {
                    TerrainTextureSettingsList[i].SteepnessCurveArray.Dispose();
                }
                
                TerrainTextureSettingsList[i].SteepnessCurveArray = new NativeArray<float>(4096, Allocator.Persistent);
                
                if (!ValidateAnimationCurve(TerrainTextureSettingsList[i].TextureSteepnessCurve))
                {
                    TerrainTextureSettingsList[i].TextureSteepnessCurve = CreateResetAnimationCurve();
                }
                
                TerrainTextureSettingsList[i].SteepnessCurveArray.CopyFrom(TerrainTextureSettingsList[i].TextureSteepnessCurve.GenerateCurveArray(4096));
            }
        }

        public bool ValidateAnimationCurve(AnimationCurve curve)
        {
            float sample = curve.Evaluate(0.5f);
            if (float.IsNaN(sample))
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Curve error",
                    "There is a problem with one of the splatmap curves. It will be reset", "OK");
#endif
                return false;
            }
            return true;
        }

        AnimationCurve CreateResetAnimationCurve()
        {           
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0.5f);
            curve.AddKey(1, 0.5f);
            return curve;
        }

        public void DisposeNativeArrayTextureCurves()
        {
            for (var i = 0; i <= TerrainTextureSettingsList.Count - 1; i++)
            {
                if (TerrainTextureSettingsList[i].HeightCurveArray.IsCreated) TerrainTextureSettingsList[i].HeightCurveArray.Dispose();
                if (TerrainTextureSettingsList[i].SteepnessCurveArray.IsCreated) TerrainTextureSettingsList[i].SteepnessCurveArray.Dispose();
            }
        }

        public void LoadDefaultTextures()
        {
            if (TerrainTextureCount == 0) return;

            if (TerrainTextureList.Count == 0)
            {
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture1",
                    "TerrainTextures/TerrainTexture1_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture2",
                    "TerrainTextures/TerrainTexture2_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture3",
                    "TerrainTextures/TerrainTexture3_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture4",
                    "TerrainTextures/TerrainTexture4_n", new Vector2(15, 15)));
            }

            if (TerrainTextureCount == 4) return;
            if (TerrainTextureList.Count == 4)
            {
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture5",
                    "TerrainTextures/TerrainTexture5_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture6",
                    "TerrainTextures/TerrainTexture6_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture7",
                    "TerrainTextures/TerrainTexture7_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture8",
                    "TerrainTextures/TerrainTexture8_n", new Vector2(15, 15)));
            }

            if (TerrainTextureCount == 8) return;
            if (TerrainTextureList.Count == 8)
            {
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture9",
                    "TerrainTextures/TerrainTexture9_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture10",
                    "TerrainTextures/TerrainTexture10_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture11",
                    "TerrainTextures/TerrainTexture11_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture12",
                    "TerrainTextures/TerrainTexture12_n", new Vector2(15, 15)));
            }

            if (TerrainTextureCount == 12) return;
            if (TerrainTextureList.Count == 12)
            {
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture13",
                    "TerrainTextures/TerrainTexture13_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture14",
                    "TerrainTextures/TerrainTexture14_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture15",
                    "TerrainTextures/TerrainTexture15_n", new Vector2(15, 15)));
                TerrainTextureList.Add(LoadTexture("TerrainTextures/TerrainTexture16",
                    "TerrainTextures/TerrainTexture16_n", new Vector2(15, 15)));
            }
        }

        public void SetupTerrainTextureSettings()
        {
            if (TerrainTextureSettingsList == null)
                TerrainTextureSettingsList = new List<TerrainTextureSettings>();
            if (TerrainTextureSettingsList.Count < TerrainTextureCount)
            {
                var currentTerrainTextureCount = TerrainTextureSettingsList.Count;

                for (var i = currentTerrainTextureCount; i <= TerrainTextureCount - 1; i++)
                {
                    var terrainTextureSettings =
                        new TerrainTextureSettings { TextureHeightCurve = new AnimationCurve() };
                    terrainTextureSettings.TextureHeightCurve.AddKey(0f, 1f);
                    terrainTextureSettings.TextureHeightCurve.AddKey(1f, 1f);

                    terrainTextureSettings.TextureSteepnessCurve = new AnimationCurve();
                    terrainTextureSettings.TextureSteepnessCurve.AddKey(0f, 0.5f);
                    terrainTextureSettings.TextureSteepnessCurve.AddKey(1f, 0.5f);

                    terrainTextureSettings.UseNoise = false;
                    terrainTextureSettings.NoiseScale = 5;
                    terrainTextureSettings.TextureWeight = 1;
                    terrainTextureSettings.Enabled = i < 4;
                    terrainTextureSettings.TextureLayer = i;

                    TerrainTextureSettingsList.Add(terrainTextureSettings);
                }

                var defaultPackage = (VegetationPackagePro)Resources.Load("DefaultSplatmapRulesPackage", typeof(VegetationPackagePro));
                if (defaultPackage)
                    if (TerrainTextureSettingsList.Count > 3 && defaultPackage.TerrainTextureSettingsList.Count > 3)
                    {
                        TerrainTextureSettingsList[0].TextureHeightCurve = new AnimationCurve(defaultPackage.TerrainTextureSettingsList[0].TextureHeightCurve.keys);
                        TerrainTextureSettingsList[0].TextureSteepnessCurve = new AnimationCurve(defaultPackage.TerrainTextureSettingsList[0].TextureSteepnessCurve.keys);

                        TerrainTextureSettingsList[1].TextureHeightCurve = new AnimationCurve(defaultPackage.TerrainTextureSettingsList[1].TextureHeightCurve.keys);
                        TerrainTextureSettingsList[1].TextureSteepnessCurve = new AnimationCurve(defaultPackage.TerrainTextureSettingsList[1].TextureSteepnessCurve.keys);

                        TerrainTextureSettingsList[2].TextureHeightCurve = new AnimationCurve(defaultPackage.TerrainTextureSettingsList[2].TextureHeightCurve.keys);
                        TerrainTextureSettingsList[2].TextureSteepnessCurve = new AnimationCurve(defaultPackage.TerrainTextureSettingsList[2].TextureSteepnessCurve.keys);

                        TerrainTextureSettingsList[3].TextureHeightCurve = new AnimationCurve(defaultPackage.TerrainTextureSettingsList[3].TextureHeightCurve.keys);
                        TerrainTextureSettingsList[3].TextureSteepnessCurve = new AnimationCurve(defaultPackage.TerrainTextureSettingsList[3].TextureSteepnessCurve.keys);
                    }
            }
        }

        private static TerrainTextureInfo LoadTexture(string textureName, string normalTextureName, Vector2 uv)
        {
            var newInfo = new TerrainTextureInfo
            {
                TileSize = uv,
                Offset = new Vector2(0, 0)
            };
            if (textureName != "") newInfo.Texture = Resources.Load(textureName) as Texture2D;
            if (normalTextureName != "") newInfo.TextureNormals = Resources.Load(normalTextureName) as Texture2D;
            return newInfo;
        }

        public void RefreshVegetationItemPrefab(VegetationItemInfoPro vegetationItemInfoPro)
        {
            GameObject vegetationPrefab = vegetationItemInfoPro.VegetationPrefab;
            if (vegetationItemInfoPro.PrefabType == VegetationPrefabType.Texture)
            {
                vegetationPrefab = Resources.Load("DefaultGrassPatch") as GameObject;
                if (vegetationItemInfoPro.VegetationTexture != null) vegetationItemInfoPro.Name = vegetationItemInfoPro.VegetationTexture.name;
            }
            else
            {
                if (vegetationItemInfoPro.VegetationPrefab != null) vegetationItemInfoPro.Name = vegetationItemInfoPro.VegetationPrefab.name;
            }

            string shaderName = ShaderSelector.GetShaderName(vegetationPrefab);
                                   
            Material[] vegetationItemMaterials = ShaderSelector.GetVegetationItemMaterials(vegetationPrefab);
            IShaderController shaderController = ShaderSelector.GetShaderControler(shaderName);
            shaderController.CreateDefaultSettings(vegetationItemMaterials);


            vegetationItemInfoPro.BillboardRenderMode = shaderController.Settings.BillboardRenderMode;
            vegetationItemInfoPro.ShaderName = shaderName;
            vegetationItemInfoPro.ShaderControllerSettings = shaderController.Settings;

            
            
            if (vegetationItemInfoPro.VegetationType == VegetationType.Tree)
            {
                GenerateBillboard(vegetationItemInfoPro.VegetationItemID);
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (shaderController.Settings.SupportsInstantIndirect)
                {
                    vegetationItemInfoPro.VegetationRenderMode = VegetationRenderMode.InstancedIndirect;
                }
            }

        }

        public void AddVegetationItem(Texture2D texture, VegetationType vegetationType, bool enableRuntimeSpawn = true,string newVegetationItemID = "")
        {
            VegetationItemInfoPro vegetationItemInfoPro = new VegetationItemInfoPro
            {
                VegetationPrefab = null,
                VegetationTexture = texture,
                PrefabType = VegetationPrefabType.Texture,
                VegetationType = vegetationType
            };

            if (texture != null) vegetationItemInfoPro.Name = texture.name;
            
            vegetationItemInfoPro.VegetationItemID = newVegetationItemID == "" ? Guid.NewGuid().ToString() : newVegetationItemID;

            vegetationItemInfoPro.Init();
            vegetationItemInfoPro.EnableRuntimeSpawn = enableRuntimeSpawn;            

            bool inverseNoise = (UnityEngine.Random.Range(0f, 1f) > 0.5f);

            vegetationItemInfoPro.Seed = UnityEngine.Random.Range(0, 100);
            vegetationItemInfoPro.UseNoiseCutoff = false;
            vegetationItemInfoPro.NoiseDensityInverse = inverseNoise;
            vegetationItemInfoPro.NoiseCutoffInverse = inverseNoise;
            vegetationItemInfoPro.NoiseScaleInverse = inverseNoise;

#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(texture);
            vegetationItemInfoPro.VegetationGuid = AssetDatabase.AssetPathToGUID(assetPath);
#endif
            
            
            switch (vegetationType)
            {
                case VegetationType.Grass:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(0.9f, 1.3f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.FollowTerrainScale;
                    break;
                case VegetationType.Plant:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(1.7f, 3f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.RotateY;
                    break;
                case VegetationType.Objects:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(3.3f, 4.5f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.RotateY;
                    break;
                case VegetationType.Tree:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(7f, 9f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.RotateY;
                    break;
                case VegetationType.LargeObjects:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(8f, 13f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.RotateY;
                    break;
            }

            GameObject vegetationPrefab = Resources.Load("DefaultGrassPatch") as GameObject;
            string shaderName = ShaderSelector.GetShaderName(vegetationPrefab);
            Material[] vegetationItemMaterials = ShaderSelector.GetVegetationItemMaterials(vegetationPrefab);
            IShaderController shaderController = ShaderSelector.GetShaderControler(shaderName);
            shaderController.CreateDefaultSettings(vegetationItemMaterials);

            
            vegetationItemInfoPro.BillboardRenderMode = shaderController.Settings.BillboardRenderMode;
            vegetationItemInfoPro.ShaderName = shaderName;
            vegetationItemInfoPro.ShaderControllerSettings = shaderController.Settings;
            VegetationInfoList.Add(vegetationItemInfoPro);

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (shaderController.Settings.SupportsInstantIndirect)
                {
                    vegetationItemInfoPro.VegetationRenderMode = VegetationRenderMode.InstancedIndirect;
                }
            }
        }

        public void AddVegetationItem(GameObject go, VegetationType vegetationType, bool enableRuntimeSpawn = true,string newVegetationItemID = "")
        {
            VegetationItemInfoPro vegetationItemInfoPro = new VegetationItemInfoPro
            {
                VegetationPrefab = go,
                PrefabType = VegetationPrefabType.Mesh,
                VegetationType = vegetationType
            };

            if (go != null) vegetationItemInfoPro.Name = go.name;


            vegetationItemInfoPro.VegetationItemID = newVegetationItemID == "" ? Guid.NewGuid().ToString() : newVegetationItemID;

            vegetationItemInfoPro.Init();
            vegetationItemInfoPro.EnableRuntimeSpawn = enableRuntimeSpawn;
            
            
            bool inverseNoise = (UnityEngine.Random.Range(0f, 1f) > 0.5f);

            vegetationItemInfoPro.Seed = UnityEngine.Random.Range(0, 100);
            vegetationItemInfoPro.UseNoiseCutoff = false;
            vegetationItemInfoPro.NoiseDensityInverse = inverseNoise;
            vegetationItemInfoPro.NoiseCutoffInverse = inverseNoise;
            vegetationItemInfoPro.NoiseScaleInverse = inverseNoise;
            
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(go);
            vegetationItemInfoPro.VegetationGuid = AssetDatabase.AssetPathToGUID(assetPath);
#endif

            switch (vegetationType)
            {
                case VegetationType.Grass:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(0.9f, 1.3f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.FollowTerrainScale;
                    break;
                case VegetationType.Plant:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(1.7f, 3f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.RotateY;
                    break;
                case VegetationType.Objects:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(3.3f, 4.5f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.RotateY;
                    break;
                case VegetationType.Tree:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(7f, 9f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.RotateY;
                    break;
                case VegetationType.LargeObjects:
                    vegetationItemInfoPro.SampleDistance = UnityEngine.Random.Range(8f, 13f);
                    vegetationItemInfoPro.Rotation = VegetationRotationType.RotateY;
                    break;
            }
            string shaderName = ShaderSelector.GetShaderName(go);
            Material[] vegetationItemMaterials = ShaderSelector.GetVegetationItemMaterials(go);
            IShaderController shaderController = ShaderSelector.GetShaderControler(shaderName);
            shaderController.CreateDefaultSettings(vegetationItemMaterials);

            vegetationItemInfoPro.BillboardRenderMode = shaderController.Settings.BillboardRenderMode;
            
            vegetationItemInfoPro.ShaderName = shaderName;
            vegetationItemInfoPro.ShaderControllerSettings = shaderController.Settings;
            VegetationInfoList.Add(vegetationItemInfoPro);

            if (vegetationType == VegetationType.Tree)
            {
                GenerateBillboard(vegetationItemInfoPro.VegetationItemID);
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (shaderController.Settings.SupportsInstantIndirect)
                {
                    vegetationItemInfoPro.VegetationRenderMode = VegetationRenderMode.InstancedIndirect;
                }
            }
        }

        public void DuplicateVegetationItem(VegetationItemInfoPro vegetationItemInfo)
        {
            VegetationItemInfoPro newVegetationItemInfo = new VegetationItemInfoPro(vegetationItemInfo);
            newVegetationItemInfo.Name += " Copy";
            VegetationInfoList.Add(newVegetationItemInfo);
        }

        public void GenerateBillboard(int vegetationItemIndex)
        {
#if UNITY_EDITOR

            //string.IsNullOrEmpty
            VegetationItemInfoPro vegetationItemInfoPro = VegetationInfoList[vegetationItemIndex];
            string overrideBillboardAtlasShader = "";
            string overrideBillboardAtlasNormalShader = "";

            if (vegetationItemInfoPro.ShaderControllerSettings != null)
            {
                if (!string.IsNullOrEmpty(vegetationItemInfoPro.ShaderControllerSettings.OverrideBillboardAtlasShader))
                {
                    overrideBillboardAtlasShader =
                        vegetationItemInfoPro.ShaderControllerSettings.OverrideBillboardAtlasShader;
                }

                if (!string.IsNullOrEmpty(vegetationItemInfoPro.ShaderControllerSettings.OverrideBillboardAtlasNormalShader))
                {
                    overrideBillboardAtlasNormalShader =
                        vegetationItemInfoPro.ShaderControllerSettings.OverrideBillboardAtlasNormalShader;
                }
            }


            EditorUtility.DisplayProgressBar("Generate Billboard Atlas", "Diffuse", 0);

            var assetPath = AssetDatabase.GetAssetPath(this);
            var directory = Path.GetDirectoryName(assetPath);
            var filename = Path.GetFileNameWithoutExtension(assetPath);
            var folderName = filename + "_billboards";

            if (!AssetDatabase.IsValidFolder(directory + "/" + folderName))
                AssetDatabase.CreateFolder(directory, folderName);
            var billboardID = VegetationInfoList[vegetationItemIndex].VegetationItemID;

            var billboardTexturePath = directory + "/" + folderName + "/" + "billboard_" + billboardID + ".png";
            var billboardNormalTexturePath = directory + "/" + folderName + "/" + "billboardNormal_" + billboardID + ".png";
            //var billboardAoTexturePath = directory + "/" + folderName + "/" + "billboardAO_" + billboardID + ".png";

            var billboardTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardTexturePath);
            if (billboardTexture) AssetDatabase.DeleteAsset(billboardTexturePath);

            var billboardNormalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardNormalTexturePath);
            if (billboardNormalTexture) AssetDatabase.DeleteAsset(billboardNormalTexturePath);

            //var billboardAoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardAoTexturePath);
            //if (billboardAoTexture) AssetDatabase.DeleteAsset(billboardAoTexturePath);

            Quaternion rotationOffset = Quaternion.Euler(VegetationInfoList[vegetationItemIndex].RotationOffset);

            billboardTexture = BillboardAtlasRenderer.GenerateBillboardTexture(VegetationInfoList[vegetationItemIndex].VegetationPrefab, VegetationInfoList[vegetationItemIndex].BillboardQuality, VegetationInfoList[vegetationItemIndex].BillboardSourceLODLevel, VegetationShaderType.Speedtree, rotationOffset, VegetationInfoList[vegetationItemIndex].BillboardAtlasBackgroundColor,overrideBillboardAtlasShader,VegetationInfoList[vegetationItemIndex].BillboardRecalculateNormals,VegetationInfoList[vegetationItemIndex].BillboardNormalBlendFactor);
            Texture2D paddedBillboardTexture = TextureExtention.CreatePaddedTexture(billboardTexture);
            if (paddedBillboardTexture == null)
            {
                paddedBillboardTexture = billboardTexture;
            }
            
            TextureExtention.FixBillboardArtifact(paddedBillboardTexture,
                VegetationInfoList[vegetationItemIndex].BillboardQuality);
            
            BillboardAtlasRenderer.SaveTexture(paddedBillboardTexture, directory + "/" + folderName + "/" + "billboard_" + billboardID);
            EditorUtility.DisplayProgressBar("Generate Billboard Atlas", "Normals", 0.33f);

            billboardNormalTexture = BillboardAtlasRenderer.GenerateBillboardNormalTexture(VegetationInfoList[vegetationItemIndex].VegetationPrefab, VegetationInfoList[vegetationItemIndex].BillboardQuality, VegetationInfoList[vegetationItemIndex].BillboardSourceLODLevel, rotationOffset,overrideBillboardAtlasNormalShader,VegetationInfoList[vegetationItemIndex].BillboardRecalculateNormals,VegetationInfoList[vegetationItemIndex].BillboardNormalBlendFactor,VegetationInfoList[vegetationItemIndex].BillboardFlipBackNormals);
            Texture2D paddedNormalTexture = TextureExtention.CreatePaddedTexture(billboardNormalTexture);
            if (paddedNormalTexture == null)
            {
                paddedNormalTexture = billboardNormalTexture;
            }            
            
            BillboardAtlasRenderer.SaveTexture(paddedNormalTexture, directory + "/" + folderName + "/" + "billboardNormal_" + billboardID);

            //billboardAoTexture = BillboardAtlasRenderer.GenerateBillboardAOTexture(VegetationInfoList[vegetationItemIndex].VegetationPrefab, VegetationInfoList[vegetationItemIndex].BillboardQuality, VegetationInfoList[vegetationItemIndex].BillboardSourceLODLevel, rotationOffset);
            //BillboardAtlasRenderer.SaveTexture(billboardAoTexture, directory + "/" + folderName + "/" + "billboardAO_" + billboardID);

            EditorUtility.DisplayProgressBar("Generate Billboard Atlas", "Importing assets", 0.66f);
            AssetDatabase.ImportAsset(billboardTexturePath);
            AssetDatabase.ImportAsset(billboardNormalTexturePath);
            //AssetDatabase.ImportAsset(billboardAoTexturePath);

            VegetationInfoList[vegetationItemIndex].BillboardTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardTexturePath);
            VegetationInfoList[vegetationItemIndex].BillboardNormalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardNormalTexturePath);
            //VegetationInfoList[vegetationItemIndex].BillboardAoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(billboardAoTexturePath);
            VegetationInfoList[vegetationItemIndex].BillboardColorSpace = PlayerSettings.colorSpace;

            BillboardAtlasRenderer.SetTextureImportSettings(VegetationInfoList[vegetationItemIndex].BillboardTexture, false);
            BillboardAtlasRenderer.SetTextureImportSettings(VegetationInfoList[vegetationItemIndex].BillboardNormalTexture, true);
            BillboardAtlasRenderer.SetTextureImportSettings(VegetationInfoList[vegetationItemIndex].BillboardAoTexture, false);
            EditorUtility.ClearProgressBar();
#endif
        }

        public void GenerateBillboard(string vegetationItemID)
        {
            var vegetationItemIndex = GetVegetationItemIndexFromID(vegetationItemID);
            GenerateBillboard(vegetationItemIndex);
        }

        public int GetVegetationItemIndexFromID(string id)
        {
            for (var i = 0; i <= VegetationInfoList.Count - 1; i++)
                if (VegetationInfoList[i].VegetationItemID == id) return i;
            return -1;
        }
    }
}
