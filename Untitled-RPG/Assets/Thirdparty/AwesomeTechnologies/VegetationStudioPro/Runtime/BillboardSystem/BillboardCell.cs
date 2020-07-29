using System.Collections.Generic;
using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.BillboardSystem
{
    public class BillboardCell : IHasRect
    {
        public Bounds BilllboardCellBounds;
        public List<VegetationPackageBillboardInstances> VegetationPackageBillboardInstancesList = new List<VegetationPackageBillboardInstances>(8);
        public List<VegetationCell> OverlapVegetationCells = new List<VegetationCell>();
        public int Index;

        public bool Prepared;
        public bool Loaded;


        public void ClearCache()
        {
            for (int i = 0; i <= VegetationPackageBillboardInstancesList.Count - 1; i++)
            {
                VegetationPackageBillboardInstancesList[i].ClearCache();
            }

            Loaded = false;
        }

        public void ClearCache(int vegetationPackageIndex, int vegetationItemIndex)
        {
            if (vegetationPackageIndex < VegetationPackageBillboardInstancesList.Count)
            {
                if (vegetationItemIndex < VegetationPackageBillboardInstancesList[vegetationPackageIndex]
                        .BillboardInstanceList.Count)
                {
                    VegetationPackageBillboardInstancesList[vegetationPackageIndex]
                        .BillboardInstanceList[vegetationItemIndex].ClearCache();
                    Loaded = false;
                }
            }            
        }

        public BillboardCell(Rect rectangle, float centerY, float sizeY)
        {
            BilllboardCellBounds = RectExtension.CreateBoundsFromRect(rectangle, centerY,sizeY);            
        }

        public Rect Rectangle
        {
            get { return RectExtension.CreateRectFromBounds(BilllboardCellBounds); }
            set { BilllboardCellBounds = RectExtension.CreateBoundsFromRect(value); }
        }

        public BoundingSphere GetBoundingSphere()
        {
            return new BoundingSphere(BilllboardCellBounds.center, BilllboardCellBounds.extents.magnitude);
        }

        public void Dispose()
        {
            for (int i = 0; i <= VegetationPackageBillboardInstancesList.Count - 1; i++)
            {
                VegetationPackageBillboardInstancesList[i].Dispose();
            }

            VegetationPackageBillboardInstancesList.Clear();
        }


        public void PrepareBillboardCell(List<VegetationPackagePro> vegetationPackageProList)
        {
            for (int i = 0; i <= vegetationPackageProList.Count - 1; i++)
            {
                VegetationPackageBillboardInstances vegetationPackageBillboardInstances = new VegetationPackageBillboardInstances(vegetationPackageProList[i].VegetationInfoList.Count);
                VegetationPackageBillboardInstancesList.Add(vegetationPackageBillboardInstances);
            }
            Prepared = true;
        }
    }
}
