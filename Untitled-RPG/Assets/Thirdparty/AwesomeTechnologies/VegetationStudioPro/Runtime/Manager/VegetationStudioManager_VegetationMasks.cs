using System.Collections.Generic;
using AwesomeTechnologies.BillboardSystem;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.VegetationStudio
{
    public partial class VegetationStudioManager
    {
        public static void AddVegetationMask(BaseMaskArea maskArea)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.Instance_AddVegetationMask(maskArea);
        }

        public static void RemoveVegetationMask(BaseMaskArea maskArea)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.Instance_RemoveVegetationMask(maskArea);
        }

        public void Instance_AddVegetationMask(BaseMaskArea maskArea)
        {
            if (!_vegetationMaskList.Contains(maskArea))
            {
                _vegetationMaskList.Add(maskArea);
            }

            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                AddVegetationMaskToVegetationSystem(VegetationSystemList[i], maskArea);
            }
        }

        public void Instance_RemoveVegetationMask(BaseMaskArea maskArea)
        {
            _vegetationMaskList.Remove(maskArea);         
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

        void DisposeVegetationMasksMasks()
        {
            for (int i = 0; i <= _vegetationMaskList.Count - 1; i++)
            {
                _vegetationMaskList[i].CallDeleteEvent();
                _vegetationMaskList[i].Dispose();
            }

            _vegetationMaskList.Clear();
        }

        private static void AddVegetationMaskToVegetationSystem(VegetationSystemPro vegetationSystem, BaseMaskArea maskArea)
        {
            vegetationSystem.CompleteCellLoading();
            
            VegetationItemIndexes vegetationItemIndexes = new VegetationItemIndexes();
            if (maskArea.VegetationItemID != "")
            {
                vegetationItemIndexes = vegetationSystem.GetVegetationItemIndexes(maskArea.VegetationItemID);
            }
            else
            {
                vegetationItemIndexes.VegetationPackageIndex = -1;
                vegetationItemIndexes.VegetationItemIndex = -1;
            }
            
            Rect maskRect = RectExtension.CreateRectFromBounds(maskArea.MaskBounds);
            if (vegetationSystem.VegetationCellQuadTree == null ||
                vegetationSystem.BillboardCellQuadTree == null) return;
            
            List<VegetationCell> selectedCellList = new List<VegetationCell>();
            vegetationSystem.VegetationCellQuadTree.Query(maskRect, selectedCellList);

            if (vegetationItemIndexes.VegetationPackageIndex > -1)
            {
                for (int i = 0; i <= selectedCellList.Count - 1; i++)
                {
                    selectedCellList[i]
                        .AddVegetationMask(
                            maskArea ,vegetationItemIndexes.VegetationPackageIndex,vegetationItemIndexes.VegetationItemIndex);
                }
                
                List<BillboardCell> selectedBillboardCellList = new List<BillboardCell>();
                vegetationSystem.BillboardCellQuadTree.Query(maskRect, selectedBillboardCellList);
                for (int i = 0; i <= selectedBillboardCellList.Count - 1; i++)
                {
                    selectedBillboardCellList[i].ClearCache(vegetationItemIndexes.VegetationPackageIndex,vegetationItemIndexes.VegetationItemIndex);
                }
            }
            else
            {
                for (int i = 0; i <= selectedCellList.Count - 1; i++)
                {                
                    selectedCellList[i].AddVegetationMask(maskArea);
                }
                
                List<BillboardCell> selectedBillboardCellList = new List<BillboardCell>();
                vegetationSystem.BillboardCellQuadTree.Query(maskRect, selectedBillboardCellList);
                for (int i = 0; i <= selectedBillboardCellList.Count - 1; i++)
                {
                    selectedBillboardCellList[i].ClearCache();
                }
            }                                
        }
    }
}
