using System.Collections.Generic;
using AwesomeTechnologies.BillboardSystem;
using AwesomeTechnologies.TerrainSystem;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.VegetationStudio
{
    public partial class VegetationStudioManager
    {
        public static bool ShowBiomes
        {
            get { return _showBiomes; }
            set
            {
                if (value != _showBiomes)
                {
#if UNITY_EDITOR
                    SceneView.RepaintAll();
#endif

                }
                _showBiomes = value;
            }
        }

        public static void RemoveBiomeMask(PolygonBiomeMask maskArea)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.Instance_RemoveBiomeMask(maskArea);
        }

        public static void AddBiomeMask(PolygonBiomeMask maskArea)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.Instance_AddBiomeMask(maskArea);
        }

        public static List<PolygonBiomeMask> GetBiomeMasks(BiomeType biomeType)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                return Instance.Instance_GetBiomeMasks(biomeType);
            }
            else
            {
                return new List<PolygonBiomeMask>();
            }
        }

        public List<PolygonBiomeMask> Instance_GetBiomeMasks(BiomeType biomeType)
        {
            List<PolygonBiomeMask> biomeList = new List<PolygonBiomeMask>();
            for (int i = 0; i <= _biomeMaskList.Count - 1; i++)
            {
                if (_biomeMaskList[i].BiomeType == biomeType)
                {
                    biomeList.Add(_biomeMaskList[i]);
                }
            }
            return biomeList;
        }

        void DisposeBiomeMasks()
        {
            for (int i = 0; i <= _biomeMaskList.Count - 1; i++)
            {
                _biomeMaskList[i].CallDeleteEvent();
                _biomeMaskList[i].Dispose();
            }

            _biomeMaskList.Clear();
        }

        protected void Instance_AddBiomeMask(PolygonBiomeMask maskArea)
        {
            if (!_biomeMaskList.Contains(maskArea))
            {
                _biomeMaskList.Add(maskArea);
            }

            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                AddBiomeMaskToVegetationSystem(VegetationSystemList[i], maskArea);
            }
        }

        protected void Instance_RemoveBiomeMask(PolygonBiomeMask maskArea)
        {
            _biomeMaskList.Remove(maskArea);                     
            Rect maskRect = RectExtension.CreateRectFromBounds(maskArea.MaskBounds);
            List<BillboardCell> selectedBillboardCellList = new List<BillboardCell>();
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemPro vegetationSystem = VegetationSystemList[i];
                vegetationSystem.CompleteCellLoading();

                vegetationSystem.BillboardCellQuadTree.Query(maskRect, selectedBillboardCellList);
                for (int j = 0; j <= selectedBillboardCellList.Count - 1; j++)
                {
                    selectedBillboardCellList[j].ClearCache();
                }
            }

            maskArea.CallDeleteEvent();
            maskArea.Dispose();
        }

       

        private static void AddBiomeMaskToVegetationSystem(VegetationSystemPro vegetationSystem, PolygonBiomeMask maskArea)
        {
            int biomeSortOrder = vegetationSystem.GetBiomeSortOrder(maskArea.BiomeType);
            maskArea.BiomeSortOrder = biomeSortOrder;

            Rect maskRect = RectExtension.CreateRectFromBounds(maskArea.MaskBounds);
            if (vegetationSystem.VegetationCellQuadTree != null && vegetationSystem.BillboardCellQuadTree != null)
            {
                List<VegetationCell> selectedCellList = new List<VegetationCell>();
                vegetationSystem.VegetationCellQuadTree.Query(maskRect, selectedCellList);
                for (int i = 0; i <= selectedCellList.Count - 1; i++)
                {
                    selectedCellList[i].AddBiomeMask(maskArea);
                }

                List<BillboardCell> selectedBillboardCellList = new List<BillboardCell>();
                vegetationSystem.BillboardCellQuadTree.Query(maskRect, selectedBillboardCellList);
                for (int i = 0; i <= selectedBillboardCellList.Count - 1; i++)
                {
                    selectedBillboardCellList[i].ClearCache();
                }
            }
        }

        public static void GenerateSplatMap()
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                for (int i = 0; i <= Instance.VegetationSystemList.Count - 1; i++)
                {
                    TerrainSystemPro terrainSystem =
                        Instance.VegetationSystemList[i].gameObject.GetComponent<TerrainSystemPro>();
                    if (terrainSystem)
                    {
                        terrainSystem.GenerateSplatMap(false);
                        terrainSystem.ShowTerrainHeatmap(false);
                    }
                }
            }
        }


        public BiomeType Instance_GetBiomeType(Vector3 position)
        {
            int currentSortOrder = -1;
            BiomeType currentBiomeType = BiomeType.Default;

            for (int i = 0; i <= _biomeMaskList.Count - 1; i++)
            {
                if (_biomeMaskList[i].Contains(position))
                {
                    if (_biomeMaskList[i].BiomeSortOrder > currentSortOrder)
                    {
                        currentSortOrder = _biomeMaskList[i].BiomeSortOrder;
                        currentBiomeType = _biomeMaskList[i].BiomeType;
                    }
                }
            }

            return currentBiomeType;
        }

        public static BiomeType GetBiomeType(Vector3 position)
        {

            if (!Instance) FindInstance();
            if (Instance)
            {
                Instance.Instance_GetBiomeType(position);
            }
            return BiomeType.Default;
        }

        //TODO clear and add mask cell delegates again when refreshing vegetation system
        //public static void ClearBiomeCellDelegates()
        //{

        //}

        //public void Internal_ClearBiomeCellDelegates()
        //{

        //}

    }
}
