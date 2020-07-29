using System.Collections.Generic;
using AwesomeTechnologies.MeshTerrains;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {
        public void AddTerrain(GameObject go)
        {
            //TODO only add terrains that overlap area if automatic calculation is disabled
            IVegetationStudioTerrain vegetationStudioTerrain = VegetationStudioTerrain.GetIVegetationStudioTerrain(go);
            if (vegetationStudioTerrain != null)
            {
                if (!VegetationStudioTerrainObjectList.Contains(go)) VegetationStudioTerrainObjectList.Add(go);

                RefreshVegetationStudioTerrains();

                if (AutomaticBoundsCalculation)
                {
                    CalculateVegetationSystemBounds();
                }
                else
                {
                    RefreshTerrainArea(vegetationStudioTerrain.TerrainBounds);
                }
            }

            VerifyVegetationStudioTerrains();
        }

        public void AddTerrains(List<GameObject> terrainList)
        {
            Bounds combinedBounds = new Bounds();
            
            for (int i = 0; i <= terrainList.Count -1; i++)
            {
                IVegetationStudioTerrain vegetationStudioTerrain = VegetationStudioTerrain.GetIVegetationStudioTerrain(terrainList[i]);
                if (vegetationStudioTerrain != null)
                {
                    if (!VegetationStudioTerrainObjectList.Contains(terrainList[i])) VegetationStudioTerrainObjectList.Add(terrainList[i]);                   
                }

                if (i == 0)
                {
                    if (vegetationStudioTerrain != null) combinedBounds = vegetationStudioTerrain.TerrainBounds;
                }
                else
                {
                    if (vegetationStudioTerrain != null)
                        combinedBounds.Encapsulate(vegetationStudioTerrain.TerrainBounds);
                }
            }
            
            RefreshVegetationStudioTerrains();

            if (AutomaticBoundsCalculation)
            {
                CalculateVegetationSystemBounds();
            }
            else
            {
                RefreshTerrainArea(combinedBounds);
            }

            VerifyVegetationStudioTerrains();
        }

        public void VerifySplatmapAccess()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i <= VegetationStudioTerrainList.Count - 1; i++)
                {
                    VegetationStudioTerrainList[i].VerifySplatmapAccess();
                } 
            }               
#endif
        }

        public void RefreshTerrainHeightmap()
        {
            for (int i = 0; i <= VegetationStudioTerrainList.Count - 1; i++)
            {
                VegetationStudioTerrainList[i].RefreshTerrainData();
            }
        }

        public void AddAllUnityTerrains()
        {
            Terrain[] terrains = FindObjectsOfType<Terrain>();
            
            List<GameObject> terrainList = new List<GameObject>();
            
            for (int i = 0; i <= terrains.Length - 1; i++)
            {
                UnityTerrain unityTerrain = terrains[i].gameObject.GetComponent<UnityTerrain>();
                if (!unityTerrain)
                {
                    terrains[i].gameObject.AddComponent<UnityTerrain>();
                }

                terrainList.Add(terrains[i].gameObject);
            }
            
            AddTerrains(terrainList);
        }

        public void AddAllMeshTerrains()
        {
            MeshTerrain[] terrains = FindObjectsOfType<MeshTerrain>();
            for (int i = 0; i <= terrains.Length - 1; i++)
            {
                AddTerrain(terrains[i].gameObject);
            }
        }

        public void RemoveAllTerrains()
        {
            List<GameObject> tempTerrainObjectList = new List<GameObject>(); 
            tempTerrainObjectList.AddRange(VegetationStudioTerrainObjectList);

            for (int i = 0; i <= tempTerrainObjectList.Count  - 1; i++)
            {
                RemoveTerrain(tempTerrainObjectList[i]);
            }            
        }
        
        public void AddAllRaycastTerrains()
        {
            RaycastTerrain[] terrains = FindObjectsOfType<RaycastTerrain>();
            for (int i = 0; i <= terrains.Length - 1; i++)
            {
                AddTerrain(terrains[i].gameObject);
            }
        }

        public void RemoveTerrain(GameObject go)
        {
            if (VegetationStudioTerrainObjectList.Contains(go)) VegetationStudioTerrainObjectList.Remove(go);
            RefreshVegetationStudioTerrains();

            IVegetationStudioTerrain vegetationStudioTerrain = VegetationStudioTerrain.GetIVegetationStudioTerrain(go);          
            if (AutomaticBoundsCalculation)
            {
                CalculateVegetationSystemBounds();
            }
            else
            {
                if (vegetationStudioTerrain != null)
                {
                    RefreshTerrainArea(vegetationStudioTerrain.TerrainBounds);
                }
            }

            VerifyVegetationStudioTerrains();
        }

        void RefreshVegetationStudioTerrains()
        {
            VerifyVegetationStudioTerrains();
            VegetationStudioTerrainList.Clear();
            for (int i = 0; i <= VegetationStudioTerrainObjectList.Count - 1; i++)
            {
                IVegetationStudioTerrain vegetationStudioTerrain = VegetationStudioTerrain.GetIVegetationStudioTerrain(VegetationStudioTerrainObjectList[i]);
                if (vegetationStudioTerrain != null) VegetationStudioTerrainList.Add(vegetationStudioTerrain);
            }
        }

        public void VerifyVegetationStudioTerrains()
        {
            while (VegetationStudioTerrainObjectList.Contains(null))
            {
                VegetationStudioTerrainObjectList.Remove(null);
            }

        }

        public void CalculateVegetationSystemBounds()
        {
        
            for (int i = 0; i <= VegetationStudioTerrainObjectList.Count - 1; i++)
            {
                IVegetationStudioTerrain vegetationStudioTerrain =
                    VegetationStudioTerrain.GetIVegetationStudioTerrain(VegetationStudioTerrainObjectList[i]);
                vegetationStudioTerrain?.RefreshTerrainData();
            }

            Bounds newBounds = new Bounds(Vector3.zero, Vector3.zero);
            if (AutomaticBoundsCalculation)
            {
                for (int i = 0; i <= VegetationStudioTerrainObjectList.Count - 1; i++)
                {
                    IVegetationStudioTerrain vegetationStudioTerrain = VegetationStudioTerrain.GetIVegetationStudioTerrain(VegetationStudioTerrainObjectList[i]);
                    if (vegetationStudioTerrain != null)
                    {
                        if (i == 0)
                        {
                            newBounds = vegetationStudioTerrain.TerrainBounds;
                        }
                        else
                        {
                            newBounds.Encapsulate(vegetationStudioTerrain.TerrainBounds);
                        }
                    }
                }
            }
            VegetationSystemBounds = newBounds;
            SetupVegetationSystem();
        }
    }
}
