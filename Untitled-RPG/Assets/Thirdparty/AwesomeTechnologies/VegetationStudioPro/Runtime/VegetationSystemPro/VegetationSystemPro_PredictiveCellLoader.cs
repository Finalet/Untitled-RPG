using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        private void SetupPredictiveCellLoader()
        {
            PredictiveCellLoader = new PredictiveCellLoader(this);
        }
        
        public void PreloadArea(Rect rect, bool important)
        {
            PredictiveCellLoader?.PreloadArea(rect, important);
        }

        public void PreloadArea(Rect rect, List<VegetationCell> overlapVegetationCellList, bool important)
        {
            PredictiveCellLoader?.PreloadArea(rect,overlapVegetationCellList,important);
        }

        public void PreloadAllVegetationCells()
        {
            PredictiveCellLoader?.PreloadAllVegetationCells();
        }
    }
}
