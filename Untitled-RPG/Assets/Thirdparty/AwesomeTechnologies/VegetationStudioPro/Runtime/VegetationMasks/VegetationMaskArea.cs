using System.Collections.Generic;
using System.Net.Security;
#if UNITY_EDITOR
#endif
using UnityEngine;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;

namespace AwesomeTechnologies
{

    //public class Vector2
    //{
    //    public float x;
    //    public float y;
    //}

    [HelpURL("http://www.awesometech.no/index.php/home/vegetation-studio/components/vegetation-masks/vegetation-mask-area")]
    [ExecuteInEditMode]
    [AwesomeTechnologiesScriptOrder(99)]
    public class VegetationMaskArea : VegetationMask
    {
        public float ReductionTolerance = 0.2f;
        private PolygonMaskArea _currentMaskArea;

        public override void UpdateVegetationMask()
        {
            if (!enabled || !gameObject.activeSelf) return;

            List<Vector3> worldSpaceNodeList = GetWorldSpaceNodePositions();
            PolygonMaskArea maskArea = new PolygonMaskArea
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

            maskArea.AddPolygon(worldSpaceNodeList);

            if (_currentMaskArea != null)
            {
                VegetationStudioManager.RemoveVegetationMask(_currentMaskArea);
                _currentMaskArea = null;
            }

            _currentMaskArea = maskArea;
            VegetationStudioManager.AddVegetationMask(maskArea);
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable()
        {
            if (_currentMaskArea != null)
            {
                VegetationStudioManager.RemoveVegetationMask(_currentMaskArea);
                _currentMaskArea.Dispose();
                _currentMaskArea = null;              
            }
        }

        public void GenerateHullNodes(float tolerance)
        {
            List<Vector2> worldSpacePointList = new List<Vector2>();

            MeshFilter[] mersFilters = GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i <= mersFilters.Length - 1; i++)
            {
                Mesh mesh = mersFilters[i].sharedMesh;
                if (mesh)
                {
                    List<Vector3> verticeList = new List<Vector3>();
                    mesh.GetVertices(verticeList);
                    for (int j = 0; j <= verticeList.Count - 1; j++)
                    {
                        Vector3 worldSpacePosition = mersFilters[i].transform.TransformPoint(verticeList[j]);
                        Vector2 worldSpacePoint = new Vector2
                        {
                            x = worldSpacePosition.x,
                            y = worldSpacePosition.z
                        };
                        worldSpacePointList.Add(worldSpacePoint);
                    }                   
                }
            }

            List<Vector2> hullPointList= PolygonUtility.GetConvexHull(worldSpacePointList);
            List<Vector2> reducedPointList = PolygonUtility.DouglasPeuckerReduction(hullPointList, tolerance);


            if (reducedPointList.Count >= 3)
            {
                ClearNodes();
                for (int i = 0; i <= reducedPointList.Count - 1; i++)
                {
                    Vector3 worldSpacePosition = new Vector3(reducedPointList[i].x, 0, reducedPointList[i].y);
                    AddNode(worldSpacePosition);
                }
                PositionNodes();
            }                                           
        }      
    }
}