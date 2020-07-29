using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;

namespace AwesomeTechnologies
{
    [HelpURL("http://www.awesometech.no/index.php/home/vegetation-studio/components/vegetation-masks/vegetation-mask-line")]
    [AwesomeTechnologiesScriptOrder(99)]
    public class VegetationMaskLine : VegetationMask
    {
        public float LineWidth = 2f;
        private readonly List<LineMaskArea> _lineMaskList = new List<LineMaskArea>();

        public override void Reset()
        {
            ClosedArea = false;
            LineWidth = 2f;
            base.Reset();
        }

        public override void UpdateVegetationMask()
        {
            //float StartTime = Time.realtimeSinceStartup;

            if (!enabled || !gameObject.activeSelf) return;

            List<Vector3> worldSpaceNodeList = GetWorldSpaceNodePositions();           
            if (_lineMaskList.Count > 0)
            {
                for (int i = 0; i <= _lineMaskList.Count - 1; i++)
                {
                    VegetationStudioManager.RemoveVegetationMask(_lineMaskList[i]);
                }

                _lineMaskList.Clear();
            }
         
            if (worldSpaceNodeList.Count > 1)
            {
                for (int i = 0; i <= worldSpaceNodeList.Count - 2; i++)
                {
                    if (!Nodes[i].Active) continue;
                    float width = LineWidth;
                    if (Nodes[i].OverrideWidth) width = Nodes[i].CustomWidth;

                    LineMaskArea maskArea = new LineMaskArea
                    {
                        RemoveGrass = RemoveGrass,
                        RemovePlants = RemovePlants,
                        RemoveTrees = RemoveTrees,
                        RemoveObjects = RemoveObjects,
                        RemoveLargeObjects = RemoveLargeObjects,
                        AdditionalGrassWidth = AdditionalGrassPerimiter,
                        AdditionalPlantWidth = AdditionalPlantPerimiter,
                        AdditionalTreeWidth = AdditionalTreePerimiter,
                        AdditionalObjectWidth = AdditionalObjectPerimiter,
                        AdditionalLargeObjectWidth = AdditionalLargeObjectPerimiter,

                        AdditionalGrassWidthMax = AdditionalGrassPerimiterMax,
                        AdditionalPlantWidthMax = AdditionalPlantPerimiterMax,
                        AdditionalTreeWidthMax = AdditionalTreePerimiterMax,
                        AdditionalObjectWidthMax = AdditionalObjectPerimiterMax,
                        AdditionalLargeObjectWidthMax = AdditionalLargeObjectPerimiterMax,

                        NoiseScaleGrass = NoiseScaleGrass,
                        NoiseScalePlant = NoiseScalePlant,
                        NoiseScaleTree = NoiseScaleTree,
                        NoiseScaleObject = NoiseScaleObject,
                        NoiseScaleLargeObject = NoiseScaleLargeObject                       
                    };

                    if (maskArea.AdditionalGrassWidthMax < maskArea.AdditionalGrassWidth)
                        maskArea.AdditionalGrassWidthMax = maskArea.AdditionalGrassWidth;

                    if (maskArea.AdditionalPlantWidthMax < maskArea.AdditionalPlantWidth)
                        maskArea.AdditionalPlantWidthMax = maskArea.AdditionalPlantWidth;

                    if (maskArea.AdditionalTreeWidthMax < maskArea.AdditionalTreeWidth)
                        maskArea.AdditionalTreeWidthMax = maskArea.AdditionalTreeWidth;

                    if (maskArea.AdditionalObjectWidthMax < maskArea.AdditionalObjectWidth)
                        maskArea.AdditionalObjectWidthMax = maskArea.AdditionalObjectWidth;

                    if (maskArea.AdditionalLargeObjectWidthMax < maskArea.AdditionalLargeObjectWidth)
                        maskArea.AdditionalLargeObjectWidthMax = maskArea.AdditionalLargeObjectWidth;

                    if (IncludeVegetationType) AddVegetationTypes(maskArea);

                    maskArea.SetLineData(worldSpaceNodeList[i], worldSpaceNodeList[i + 1], width);
                    _lineMaskList.Add(maskArea);
                    VegetationStudioManager.AddVegetationMask(maskArea);
                }
            }
            //Debug.Log(Time.realtimeSinceStartup - StartTime);         
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
            if (_lineMaskList.Count > 0)
            {
                for (int i = 0; i <= _lineMaskList.Count - 1; i++)
                {
                    VegetationStudioManager.RemoveVegetationMask(_lineMaskList[i]);
                    _lineMaskList[i].Dispose();
                }

                _lineMaskList.Clear();
            }
        }
    }
}
