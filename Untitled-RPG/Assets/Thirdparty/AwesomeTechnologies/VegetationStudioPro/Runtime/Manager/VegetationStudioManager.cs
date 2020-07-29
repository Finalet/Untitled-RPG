using System;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using UnityEngine;

namespace AwesomeTechnologies.VegetationStudio
{
    [ExecuteInEditMode]
    public partial class VegetationStudioManager : MonoBehaviour
    {

        public int CurrentTabIndex = 0;

        public static VegetationStudioManager Instance;
        public List<VegetationSystemPro> VegetationSystemList = new List<VegetationSystemPro>();

        public delegate void MultiAddVegetationSystemDelegate(VegetationSystemPro vegetationSystem);

        public MultiAddVegetationSystemDelegate OnAddVegetationSystemDelegate;

        public delegate void MultiRemoveVegetationSystemDelegate(VegetationSystemPro vegetationSystem);
        public MultiRemoveVegetationSystemDelegate OnRemoveVegetationSystemDelegate;

        [NonSerialized]
        private VegetationItemInfoPro _clippboardvegetationItemInfo;


        [NonSerialized] private AnimationCurve _clippboardAnimationCurve;

#if UNITY_POST_PROCESSING_STACK_V2
        public List<PostProcessProfileInfo> PostProcessProfileInfoList = new List<PostProcessProfileInfo>();
#endif
        public LayerMask PostProcessingLayer = 0;

        private readonly List<PolygonBiomeMask> _biomeMaskList = new List<PolygonBiomeMask>();
        private static bool _showBiomes;

        private readonly List<BaseMaskArea> _vegetationMaskList = new List<BaseMaskArea>();

        /// <summary>
        ///  Internal function used by VegetationSystem components to register with the manager
        /// </summary>
        /// <param name="vegetationSystem"></param>
        public static void RegisterVegetationSystem(VegetationSystemPro vegetationSystem)
        {
            if (!Instance) FindInstance();

            if (Instance)
            {
                Instance.Instance_RegisterVegetationSystem(vegetationSystem);
            }
        }

        /// <summary>
        /// Internal function used by VegetationSystem components to register with the manager
        /// </summary>
        /// <param name="vegetationSystem"></param>
        protected void Instance_RegisterVegetationSystem(VegetationSystemPro vegetationSystem)
        {
            if (!VegetationSystemList.Contains(vegetationSystem))
            {
                VegetationSystemList.Add(vegetationSystem);
                OnAddVegetationSystem(vegetationSystem);
                if (OnAddVegetationSystemDelegate != null) OnAddVegetationSystemDelegate(vegetationSystem);
            }
        }

        /// <summary>
        /// Static function to find the singelton instance
        /// </summary>
        protected static void FindInstance()
        {
            Instance = (VegetationStudioManager) FindObjectOfType(typeof(VegetationStudioManager));
        }

        /// <summary>
        /// Internal function on the instance to unregister vegetation system components. 
        /// </summary>
        /// <param name="vegetationSystem"></param>
        protected void Instance_UnregisterVegetationSystem(VegetationSystemPro vegetationSystem)
        {
            VegetationSystemList.Remove(vegetationSystem);
            OnRemoveVegetationSystem(vegetationSystem);
            OnRemoveVegetationSystemDelegate?.Invoke(vegetationSystem);
        }

        /// <summary>
        ///  Internal function used by VegetationSystem components to unregister with the manager
        /// </summary>
        /// <param name="vegetationSystem"></param>
        public static void UnregisterVegetationSystem(VegetationSystemPro vegetationSystem)
        {
            if (!Instance) FindInstance();

            if (Instance)
            {
                Instance.Instance_UnregisterVegetationSystem(vegetationSystem);
            }
        }

        public void OnAddVegetationSystem(VegetationSystemPro vegetationSystem)
        {

        }

        public void OnRemoveVegetationSystem(VegetationSystemPro vegetationSystem)
        {

        }


        public static void OnVegetationCellRefresh(VegetationSystemPro vegetationSystem)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Internal_OnVegetationCellRefresh(vegetationSystem);
            }
        }

        public void Internal_OnVegetationCellRefresh(VegetationSystemPro vegetationSystem)
        {
            for (int i = 0; i <= _biomeMaskList.Count - 1; i++)
            {
                AddBiomeMaskToVegetationSystem(vegetationSystem, _biomeMaskList[i]);
            }

            for (int i = 0; i <= _vegetationMaskList.Count - 1; i++)
            {
                AddVegetationMaskToVegetationSystem(vegetationSystem, _vegetationMaskList[i]);
            }
        }

        public static void AddAnimationCurveToClipboard(AnimationCurve animationCurve)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Internal_AddAnimationCurveToClipboard(animationCurve);
            }
        }

        private void Internal_AddAnimationCurveToClipboard(AnimationCurve animationCurve)
        {
            _clippboardAnimationCurve = animationCurve;
        }

        public static AnimationCurve GetAnimationCurveFromClippboard()
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                return Instance.Internal_GetAnimationCurveFromClippboard();
            }

            return null;
        }

        public AnimationCurve Internal_GetAnimationCurveFromClippboard()
        {
            return _clippboardAnimationCurve;
        }

        public void Internal_ClearCache()
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].ClearCache();
            }
        }

        public void Internal_ClearCache(Bounds bounds)
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].ClearCache();
            }
        }

        public static void ClearCache()
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Internal_ClearCache();
            }
        }

        public static void ClearCache(Bounds bounds)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Internal_ClearCache(bounds);
            }
        }


        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
            DisposeBiomeMasks();
            DisposeVegetationMasksMasks();
        }


        protected void Internal_AddVegetationItemToClipboard(VegetationItemInfoPro vegetationItemInfo)
        {

            _clippboardvegetationItemInfo = new VegetationItemInfoPro(vegetationItemInfo);
        }

        private VegetationItemInfoPro Internal_GetVegetationItemFromClipboard()
        {
            return _clippboardvegetationItemInfo;
        }

        /// <summary>
        /// Adds a new VegetationItemInfo to the Clippboard. Used for copy paste in the VegetationSystem Inspector
        /// </summary>
        /// <param name="vegetationItemInfo"></param>
        public static void AddVegetationItemToClipboard(VegetationItemInfoPro vegetationItemInfo)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Internal_AddVegetationItemToClipboard(vegetationItemInfo);
            }
        }

        public static VegetationItemInfoPro GetVegetationItemFromClipboard()
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                return Instance.Internal_GetVegetationItemFromClipboard();
            }
            return null;
        }

        public static void RefreshTerrainHeightMap()
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_RefreshTerrainHeightmap();
            }
            
            RefreshTerrainArea();
        }

        public static void RefreshTerrainHeightMap(Bounds bounds)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_RefreshTerrainHeightmap(bounds);
            }
            
            RefreshTerrainArea(bounds);
        }       
        
        public void Instance_RefreshTerrainHeightmap()
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].RefreshTerrainHeightmap();
            }
        }

        public void Instance_RefreshTerrainHeightmap(Bounds bounds)
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].RefreshTerrainHeightmap();
            }
        }

        public void Instance_AddTerrain(GameObject go, bool forceAdd)
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                if (VegetationSystemList[i].AutomaticBoundsCalculation && !forceAdd) continue;
                VegetationSystemList[i].AddTerrain(go);
            }
        }

        public void Instance_RemoveTerrain(GameObject go)
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                if (VegetationSystemList[i].AutomaticBoundsCalculation) continue;
                VegetationSystemList[i].RemoveTerrain(go);
            }
        }

        public static void AddTerrain(GameObject go, bool forceAdd)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_AddTerrain(go,forceAdd);
            }
        }
        
        public static void AddCamera(Camera camera, bool noFrustumCulling = false, bool renderDirectToCamera = false, bool renderBillboardsOnly = false)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_AddCamera(camera,noFrustumCulling,renderDirectToCamera,renderBillboardsOnly);
            }
        }

        public void Instance_AddCamera(Camera aCamera, bool noFrustumCulling = false, bool renderDirectToCamera = false, bool renderBillboardsOnly = false)
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].AddCamera(aCamera,noFrustumCulling,renderDirectToCamera,renderBillboardsOnly);
            }
        }
        
        public static void RemoveCamera(Camera camera)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_RemoveCamera(camera);
            }
        }

        public void Instance_RemoveCamera(Camera aCamera)
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].RemoveCamera(aCamera);
            }
        }

        public static void RemoveTerrain(GameObject go)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_RemoveTerrain(go);
            }
        }

        public void Instance_RefreshTerrainArea(Bounds bounds)
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                if (!VegetationSystemList[i].InitDone) continue;
                VegetationSystemList[i].RefreshTerrainArea(bounds);
            }
        }
        
        public void Instance_RefreshTerrainArea()
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                if (!VegetationSystemList[i].InitDone) continue;
                VegetationSystemList[i].RefreshTerrainArea();
            }
        }

        public static void RefreshTerrainArea(Bounds bounds)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_RefreshTerrainArea(bounds);
            }
        }
        
        public static void RefreshTerrainArea()
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_RefreshTerrainArea();
            }
        }

        public Vector3 Instance_GetFloatingOriginOffset()
        {
            if (VegetationSystemList.Count > 0) return VegetationSystemList[0].FloatingOriginOffset;           
            return Vector3.zero;
        }

        public static Vector3 GetFloatingOriginOffset()
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                return Instance.Instance_GetFloatingOriginOffset();
            }

            return Vector3.zero;
        }

        public static void SetSunDirectionalLight(Light light)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_SetSunDirectionalLight(light);
            }
        }      
        
        public void Instance_SetSunDirectionalLight(Light alight)
        {
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemList[i].SunDirectionalLight = alight;
            }
        }   
    }
}
