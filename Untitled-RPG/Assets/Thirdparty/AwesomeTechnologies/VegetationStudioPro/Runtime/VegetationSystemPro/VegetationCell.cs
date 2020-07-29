using System.Collections.Generic;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Collections;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public class VegetationCell : IHasRect
    {
        public Bounds VegetationCellBounds;
        public readonly List<VegetationPackageInstances> VegetationPackageInstancesList = new List<VegetationPackageInstances>(8);
        public readonly List<VegetationInstanceData> VegetationInstanceDataList = new List<VegetationInstanceData>();
        public int LoadedDistanceBand = 99;
        public bool LoadedBillboards;
        public bool Prepared;
        public int Index;

        public bool Important = false;
        
        public List<PolygonBiomeMask> BiomeMaskList;
        public List<BaseMaskArea> VegetationMaskList;

        public VegetationCell(Rect rectangle)
        {
            VegetationCellBounds = RectExtension.CreateBoundsFromRect(rectangle, -100000);
        }

        public Rect Rectangle
        {
            get
            {
                return RectExtension.CreateRectFromBounds(VegetationCellBounds);
            }
            set
            {
                VegetationCellBounds = RectExtension.CreateBoundsFromRect(value);
            }
        }

        public BoundingSphere GetBoundingSphere()
        {
            return new BoundingSphere(VegetationCellBounds.center, VegetationCellBounds.extents.magnitude);
        }

        public void Dispose()
        {
            if (BiomeMaskList != null)
            {
                for (int i = 0; i <= BiomeMaskList.Count - 1; i++)
                {
                    // ReSharper disable once DelegateSubtraction
                    BiomeMaskList[i].OnMaskDeleteDelegate -= OnBiomeMaskDelete;
                }

                BiomeMaskList.Clear();
            }

            for (int i = 0; i <= VegetationPackageInstancesList.Count - 1; i++)
            {
                VegetationPackageInstancesList[i].Dispose();
            }

            VegetationPackageInstancesList.Clear();
            
            for (int i = 0; i <= VegetationInstanceDataList.Count - 1; i++)
            {
                VegetationInstanceDataList[i].Dispose();
            }
            VegetationInstanceDataList.Clear();
        }
   
        public void ClearInstanceMemory()
        {
            for (int i = 0; i <= VegetationPackageInstancesList.Count - 1; i++)
            {
                VegetationPackageInstancesList[i].ClearInstanceMemory();
            }
        }

        public void ClearCache()
        {          
            for (int i = 0; i <= VegetationPackageInstancesList.Count - 1; i++)
            {
                if (VegetationPackageInstancesList[i].LoadStateList.IsCreated)
                {
                    for (int j = 0; j <= VegetationPackageInstancesList[i].LoadStateList.Length - 1; j++)
                    {
                        VegetationPackageInstancesList[i].LoadStateList[j] = 0;
                    }                                      
                }              

                for (int j = 0; j <= VegetationPackageInstancesList[i].VegetationItemComputeBufferList.Count - 1; j++)
                {
                    if (VegetationPackageInstancesList[i].VegetationItemComputeBufferList[j].Created)
                    {
                        VegetationPackageInstancesList[i].VegetationItemComputeBufferList[j].ComputeBuffer.Dispose();
                        VegetationPackageInstancesList[i].VegetationItemComputeBufferList[j].Created = false;
                    }
                }

                for (int j = 0; j <= VegetationPackageInstancesList[i].VegetationItemInstancedIndirectInstanceList.Count - 1; j++)
                {
                    if (VegetationPackageInstancesList[i].VegetationItemInstancedIndirectInstanceList[j].Created)
                    {
                        if (VegetationPackageInstancesList[i].VegetationItemInstancedIndirectInstanceList[j]
                            .InstancedIndirectInstanceList.IsCreated)
                        {
                            VegetationPackageInstancesList[i].VegetationItemInstancedIndirectInstanceList[j].InstancedIndirectInstanceList.Dispose();
                        }
                        VegetationPackageInstancesList[i].VegetationItemInstancedIndirectInstanceList[j].Created = false;
                    }
                }               
            }
            LoadedDistanceBand = 99;
            LoadedBillboards = false;

            ClearInstanceMemory();
           // CompactMemory();
        }

        public void ClearCache(int vegetationPackageIndex, int vegetationItemIndex, bool tree)
        {          
            if (tree)
            {
                LoadedDistanceBand = 99;
                LoadedBillboards = false;
            }
            else
            {
                LoadedDistanceBand += 1;
                LoadedBillboards = false;
            }
            
            if (VegetationPackageInstancesList.Count > vegetationPackageIndex && VegetationPackageInstancesList[vegetationPackageIndex].LoadStateList.Length > vegetationItemIndex)
            {
                VegetationPackageInstancesList[vegetationPackageIndex].LoadStateList[vegetationItemIndex] = 0;

                if (VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemInstancedIndirectInstanceList[vegetationItemIndex].Created)
                {
                    VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemInstancedIndirectInstanceList[vegetationItemIndex].InstancedIndirectInstanceList.Dispose();
                    VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemInstancedIndirectInstanceList[vegetationItemIndex].Created = false;
                }

                if (VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemComputeBufferList[vegetationItemIndex].Created)
                {
                    VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemComputeBufferList[vegetationItemIndex].ComputeBuffer.Dispose();
                    VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemComputeBufferList[vegetationItemIndex].Created = false;
                }
            }
        }

        public bool Enabled => VegetationCellBounds.center.y > -99999;
        public int EnabledInt => VegetationCellBounds.center.y > -99999 ? 1 : 0;

        public void AddBiomeMask(PolygonBiomeMask maskArea)
        {
            if (BiomeMaskList == null) BiomeMaskList = new List<PolygonBiomeMask>();
            BiomeMaskList.Add(maskArea);

            if (BiomeMaskList.Count > 1)
            {
                SortBiomeList();
            }

            maskArea.OnMaskDeleteDelegate += OnBiomeMaskDelete;
            ClearCache();
        }

        public void AddVegetationMask(BaseMaskArea maskArea)
        {
            if (VegetationMaskList == null) VegetationMaskList = new List<BaseMaskArea>();
            VegetationMaskList.Add(maskArea);

            maskArea.OnMaskDeleteDelegate += OnVegetationMaskDelete;
            ClearCache();
        }
        
        public void AddVegetationMask(BaseMaskArea maskArea, int vegetationPackageIndex, int vegetationItemIndex)
        {
            if (VegetationMaskList == null) VegetationMaskList = new List<BaseMaskArea>();
            VegetationMaskList.Add(maskArea);

            maskArea.OnMaskDeleteDelegate += OnVegetationMaskDelete;
            ClearCache(vegetationPackageIndex,vegetationItemIndex,true);
        }

        private void OnVegetationMaskDelete(BaseMaskArea maskArea)
        {
            // ReSharper disable once DelegateSubtraction
            maskArea.OnMaskDeleteDelegate -= OnVegetationMaskDelete;
            if (VegetationMaskList != null)
            {
                VegetationMaskList.Remove(maskArea);
                ClearCache();
            }
        }

        void SortBiomeList()
        {
            BiomeMaskSortOrderComparer biomeMaskSortOrderComparer = new BiomeMaskSortOrderComparer();
            BiomeMaskList.Sort(biomeMaskSortOrderComparer);
        }

        private void OnBiomeMaskDelete(PolygonBiomeMask maskArea)
        {
            // ReSharper disable once DelegateSubtraction
            maskArea.OnMaskDeleteDelegate -= OnBiomeMaskDelete;
            if (BiomeMaskList != null)
            {
                BiomeMaskList.Remove(maskArea);
                ClearCache();
            }
        }

        public bool HasBiome(BiomeType biomeType)
        {
            if (BiomeMaskList == null) return false;
            for (int i = 0; i <= BiomeMaskList.Count - 1; i++)
            {
                if (BiomeMaskList[i].BiomeType == biomeType) return true;
            }
            return false;
        }

//        public NativeList<MatrixInstance> GetVegetationPackageInstancesList(int vegetationPackageIndex, int vegetatonItemIndex)
//        {
//            if (VegetationPackageInstancesList.Count > vegetationPackageIndex)
//            {
//                if (VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemMatrixList.Count >
//                    vegetatonItemIndex)
//                {
//                    return VegetationPackageInstancesList[vegetationPackageIndex].VegetationItemMatrixList[vegetatonItemIndex];
//                }
//            }
//            return new NativeList<MatrixInstance>();
//        }
        
    }
}

