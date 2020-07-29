using System;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    [Serializable]
    public enum PrecisionPaintingMode
    {
        Terrain,
        TerrainAndColliders,
        TerrainAndMeshes
    }
    
    [HelpURL("http://www.awesometech.no/index.php/persistent-vegetation-storage")]   
    public class PersistentVegetationStorage : MonoBehaviour
    {
        public PersistentVegetationStoragePackage PersistentVegetationStoragePackage;
        public VegetationSystemPro VegetationSystemPro;

        [NonSerialized]
        public int CurrentTabIndex;
        public int SelectedBrushIndex;
        public float BrushSize = 5;
        public float SampleDistance = 1f;
        public bool RandomizePosition = true;
        public bool PaintOnColliders;
        public bool UseSteepnessRules;

        public bool IgnoreHeight = true;
        //public bool UseScaleRules = true;
        public bool DisablePersistentStorage;
        
        public LayerMask GroundLayerMask;
        public int SelectedVegetationPackageIndex;
        
        public string SelectedEditVegetationID;
        public string SelectedPaintVegetationID;
        public string SelectedBakeVegetationID;
        public string SelectedStorageVegetationID;
        public string SelectedPrecisionPaintingVegetationID;
        public PrecisionPaintingMode PrecisionPaintingMode = PrecisionPaintingMode.TerrainAndMeshes;

        //public bool AutoInitPersistentVegetationStoragePackage;

        public List<IVegetationImporter> VegetationImporterList = new List<IVegetationImporter>();
        public int SelectedImporterIndex;

        /// <summary>
        /// Tests if the persistent storage is initialized for the current terrain. 
        /// </summary>
        /// <param name="cellCount"></param>
        /// <returns></returns>
        public bool HasValidPersistentStorage(int cellCount)
        {
            if (PersistentVegetationStoragePackage == null) return false;
            if (PersistentVegetationStoragePackage.PersistentVegetationCellList.Count != cellCount) return false;

            return true;
        }

        /// <summary>
        /// Sets a new persistentVegetationStoragePackage. Will refresh the VegetationSystem component.
        /// </summary>
        /// <param name="persistentVegetationStoragePackage"></param>
        public void SetPersistentVegetationStoragePackage(
            PersistentVegetationStoragePackage persistentVegetationStoragePackage)
        {
            PersistentVegetationStoragePackage = persistentVegetationStoragePackage;
            if (VegetationSystemPro)
            {
                VegetationSystemPro.ClearCache();
            }
        }

        /// <summary>
        /// InitializePersistentStorage will clean the storage and set it up for the current VegetationSystem.
        /// </summary>
        public void InitializePersistentStorage()
        {            
            if (PersistentVegetationStoragePackage != null)
            {               
                PersistentVegetationStoragePackage.ClearPersistentVegetationCells();
                for (int i = 0; i <= VegetationSystemPro.VegetationCellList.Count - 1; i++)
                {
                    PersistentVegetationStoragePackage.AddVegetationCell();
                }
            }
        }

        public void InitializePersistentStorage(int cellCount)
        {
            if (PersistentVegetationStoragePackage != null)
            {
                PersistentVegetationStoragePackage.ClearPersistentVegetationCells();
                for (int i = 0; i <= cellCount - 1; i++)
                {
                    PersistentVegetationStoragePackage.AddVegetationCell();
                }
            }
        }

        /// <summary>
        /// AddVegetationItem will add a new instance of a Vegetation Item to the persistent storage. Position, scale and rotation is in worldspace. The Optional clearCellCache will refresh the area where the item is added. 
        /// </summary>
        /// <param name="vegetationItemID"></param>
        /// <param name="worldPosition"></param>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="applyMeshRotation"></param>
        /// <param name="vegetationSourceID"></param>
        /// <param name="distanceFalloff"></param>
        /// <param name="clearCellCache"></param>
        public void AddVegetationItemInstance(string vegetationItemID, Vector3 worldPosition, Vector3 scale, Quaternion rotation, bool applyMeshRotation, byte vegetationSourceID,float distanceFalloff, bool clearCellCache = false)
        {
            if (!VegetationSystemPro || !PersistentVegetationStoragePackage) return;
            Rect positionRect = new Rect(new Vector2(worldPosition.x, worldPosition.z), Vector2.zero);

            VegetationItemInfoPro vegetationItemInfo = VegetationSystemPro.GetVegetationItemInfo(vegetationItemID);

            if (applyMeshRotation)
            {
                rotation *= Quaternion.Euler(vegetationItemInfo.RotationOffset);
            }

            List<VegetationCell> overlapCellList = new List<VegetationCell>(); 
            VegetationSystemPro.VegetationCellQuadTree.Query(positionRect,overlapCellList);
          
            for (int i = 0; i <= overlapCellList.Count - 1; i++)
            {
                int cellIndex = overlapCellList[i].Index;
                if (clearCellCache)
                {
                    VegetationItemIndexes indexes = VegetationSystemPro.GetVegetationItemIndexes(vegetationItemID);                    
                    VegetationSystemPro.ClearCache(overlapCellList[i],indexes.VegetationPackageIndex,indexes.VegetationItemIndex);
                }
                PersistentVegetationStoragePackage.AddVegetationItemInstance(cellIndex, vegetationItemID, worldPosition - VegetationSystemPro.VegetationSystemPosition, scale, rotation,vegetationSourceID,distanceFalloff);
            }           
        }

        public void AddVegetationItemInstanceEx(string vegetationItemID, Vector3 worldPosition, Vector3 scale, Quaternion rotation, byte vegetationSourceID,float minimumDistance, float distanceFalloff, bool clearCellCache = false)
        {
            if (!VegetationSystemPro || !PersistentVegetationStoragePackage || VegetationSystemPro.VegetationCellQuadTree == null) return;

            Rect positionRect = new Rect(new Vector2(worldPosition.x, worldPosition.z), Vector2.zero);

            List<VegetationCell> overlapCellList = new List<VegetationCell>();                 
            VegetationSystemPro.VegetationCellQuadTree.Query(positionRect,overlapCellList);

            for (int i = 0; i <= overlapCellList.Count - 1; i++)
            {
                int cellIndex = overlapCellList[i].Index;
                if (clearCellCache)
                {
                    VegetationItemIndexes indexes = VegetationSystemPro.GetVegetationItemIndexes(vegetationItemID);                    
                    VegetationSystemPro.ClearCache(overlapCellList[i],indexes.VegetationPackageIndex,indexes.VegetationItemIndex);
                }
                PersistentVegetationStoragePackage.AddVegetationItemInstanceEx(cellIndex, vegetationItemID, worldPosition - VegetationSystemPro.VegetationSystemPosition, scale, rotation, vegetationSourceID, minimumDistance,distanceFalloff);
            }
        }

        public void RemoveVegetationItemInstance(string vegetationItemID, Vector3 worldPosition, float minimumDistance, bool clearCellCache = false)
        {
            if (!VegetationSystemPro || !PersistentVegetationStoragePackage) return;
            Rect positionRect = new Rect(new Vector2(worldPosition.x, worldPosition.z), Vector2.zero);

            List<VegetationCell> overlapCellList = new List<VegetationCell>();                 
            VegetationSystemPro.VegetationCellQuadTree.Query(positionRect,overlapCellList);

            for (int i = 0; i <= overlapCellList.Count - 1; i++)
            {
                int cellIndex = overlapCellList[i].Index;
                if (clearCellCache)
                {
                    VegetationItemIndexes indexes = VegetationSystemPro.GetVegetationItemIndexes(vegetationItemID);                    
                    VegetationSystemPro.ClearCache(overlapCellList[i],indexes.VegetationPackageIndex,indexes.VegetationItemIndex);
                }

                PersistentVegetationStoragePackage.RemoveVegetationItemInstance(cellIndex, vegetationItemID, worldPosition - VegetationSystemPro.VegetationSystemPosition,  minimumDistance);
            }
        }

        public void RemoveVegetationItemInstance2D(string vegetationItemID, Vector3 worldPosition, float minimumDistance, bool clearCellCache = false)
        {
            if (!VegetationSystemPro || !PersistentVegetationStoragePackage) return;
            Rect positionRect = new Rect(new Vector2(worldPosition.x, worldPosition.z), Vector2.zero);

            List<VegetationCell> overlapCellList = new List<VegetationCell>();
            VegetationSystemPro.VegetationCellQuadTree.Query(positionRect, overlapCellList);

            for (int i = 0; i <= overlapCellList.Count - 1; i++)
            {
                int cellIndex = overlapCellList[i].Index;
                if (clearCellCache)
                {
                    VegetationItemIndexes indexes = VegetationSystemPro.GetVegetationItemIndexes(vegetationItemID);
                    VegetationSystemPro.ClearCache(overlapCellList[i], indexes.VegetationPackageIndex, indexes.VegetationItemIndex);
                }

                PersistentVegetationStoragePackage.RemoveVegetationItemInstance2D(cellIndex, vegetationItemID, worldPosition - VegetationSystemPro.VegetationSystemPosition, minimumDistance);
            }
        }

        /// <summary>
        /// RepositionCellItems is used to check all instances of a VegetationItem in a cell and confirm that they are located in the correct cell. 
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <param name="vegetationItemID"></param>
        public void RepositionCellItems(int cellIndex, string vegetationItemID)
        {
            PersistentVegetationInfo persistentVegetationInfo = PersistentVegetationStoragePackage.PersistentVegetationCellList[cellIndex].GetPersistentVegetationInfo(vegetationItemID);
            if (persistentVegetationInfo == null) return;

            List<PersistentVegetationItem> origialItemList = new List<PersistentVegetationItem>();
            origialItemList.AddRange(persistentVegetationInfo.VegetationItemList);
            persistentVegetationInfo.ClearCell();

            for (int i = 0; i <= origialItemList.Count - 1; i++)
            {
                AddVegetationItemInstance(vegetationItemID, origialItemList[i].Position + VegetationSystemPro.VegetationSystemPosition, origialItemList[i].Scale,
                    origialItemList[i].Rotation, false, origialItemList[i].VegetationSourceID,origialItemList[i].DistanceFalloff, true);
            }

            VegetationItemIndexes indexes = VegetationSystemPro.GetVegetationItemIndexes(vegetationItemID);                    
            VegetationSystemPro.ClearCache(VegetationSystemPro.VegetationCellList[cellIndex],indexes.VegetationPackageIndex,indexes.VegetationItemIndex);
        }

        /// <summary>
        /// Returns the numbers of cells in the persistent vegetation storage.
        /// </summary>
        /// <returns></returns>
        public int GetPersistentVegetationCellCount()
        {
            if (PersistentVegetationStoragePackage && PersistentVegetationStoragePackage.PersistentVegetationCellList != null)
            {
                return PersistentVegetationStoragePackage.PersistentVegetationCellList.Count;
            }

            return 0;
        }
        
        public PersistentVegetationCell GetPersistentVegetationCell(int index)
        {
            if (PersistentVegetationStoragePackage && PersistentVegetationStoragePackage.PersistentVegetationCellList != null)
            {
                if (index < PersistentVegetationStoragePackage.PersistentVegetationCellList.Count)
                {
                    return PersistentVegetationStoragePackage.PersistentVegetationCellList[index]; 
                }                
            }

            return null;
        }
       
        // ReSharper disable once UnusedMember.Local
        void Reset()
        {
            VegetationSystemPro = GetComponent<VegetationSystemPro>();
            if (VegetationSystemPro) VegetationSystemPro.DetectPersistentVegetationStorage();
        }

//        private void OnEnable()
//        {
//            if (!VegetationSystemPro)
//            {
//                VegetationSystemPro = gameObject.GetComponent<VegetationSystemPro>();
//                if (VegetationSystemPro) VegetationSystemPro.DetectPersistentVegetationStorage();
//            }
//        }

        public void Dispose()
        {
            if (PersistentVegetationStoragePackage)
            {
                PersistentVegetationStoragePackage.Dispose();
            }
        }

        /// <summary>
        /// ClearVegetationItem will remove any instanced of vegetation in the storage with the provided VegetationItemID and VegetationSourceID
        /// </summary>
        /// <param name="vegetationItemID"></param>
        /// <param name="vegetationSourceID"></param>
        public void RemoveVegetationItemInstances(string vegetationItemID, byte vegetationSourceID)
        {
            if (PersistentVegetationStoragePackage == null) return;
            PersistentVegetationStoragePackage.RemoveVegetationItemInstances(vegetationItemID, vegetationSourceID);
        }

        /// <summary>
        /// ClearVegetationItem will remove any instances of a VegetationItem from the storage. Items from all sourceIDs will be removed.
        /// </summary>
        /// <param name="vegetationItemID"></param>
        public void RemoveVegetationItemInstances(string vegetationItemID)
        {
            if (PersistentVegetationStoragePackage == null) return;
            PersistentVegetationStoragePackage.RemoveVegetationItemInstances(vegetationItemID);
        }

        /// <summary>
        /// BakeVegetationItem will bake all instances of a VegetationItem from the rules to the Persisitent Vegetation Storage. The original rule will set "Include in Terrain" to false.
        /// </summary>
        /// <param name="vegetationItemID"></param>
        public void BakeVegetationItem(string vegetationItemID)
        {
            if (!VegetationSystemPro) return;

            if (vegetationItemID == "")
            {
                Debug.Log("vegetationItemID empty");
                return;
            }
            
            GC.Collect();

            VegetationItemInfoPro vegetationItemInfo = VegetationSystemPro.GetVegetationItemInfo(vegetationItemID);
            vegetationItemInfo.EnableRuntimeSpawn = true;

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.DisplayProgressBar("Bake vegetation item: " + vegetationItemInfo.Name, "Spawn all cells", 0);
#endif
            
            for (int i = 0; i <= VegetationSystemPro.VegetationCellList.Count - 1; i++)
            {
                VegetationCell vegetationCell = VegetationSystemPro.VegetationCellList[i];
                
#if UNITY_EDITOR
                if (i % 10 == 0)
                {

                    if (!Application.isPlaying) EditorUtility.DisplayProgressBar("Bake vegetation item: " + vegetationItemInfo.Name, "Spawn cell " + i + "/" + (VegetationSystemPro.VegetationCellList.Count - 1), i/((float)VegetationSystemPro.VegetationCellList.Count - 1));
                }
#endif
                VegetationSystemPro.SpawnVegetationCell(vegetationCell,vegetationItemID);
                NativeList<MatrixInstance> vegetationInstanceList =
                    VegetationSystemPro.GetVegetationItemInstances(vegetationCell, vegetationItemID);
                               
                for (int j = 0; j <= vegetationInstanceList.Length - 1; j++)
                {
                    Matrix4x4 vegetationItemMatrix = vegetationInstanceList[j].Matrix;
                    //AddVegetationItemInstance(vegetationItemID, MatrixTools.ExtractTranslationFromMatrix(vegetationItemMatrix),
                    //    MatrixTools.ExtractScaleFromMatrix(vegetationItemMatrix),
                    //    MatrixTools.ExtractRotationFromMatrix(vegetationItemMatrix), false,0);                    
                    PersistentVegetationStoragePackage.AddVegetationItemInstance(vegetationCell.Index, vegetationItemID, MatrixTools.ExtractTranslationFromMatrix(vegetationItemMatrix) - VegetationSystemPro.VegetationSystemPosition, MatrixTools.ExtractScaleFromMatrix(vegetationItemMatrix), MatrixTools.ExtractRotationFromMatrix(vegetationItemMatrix),0,vegetationInstanceList[j].DistanceFalloff);                    
                }
                
                vegetationCell.ClearCache();
            }            
            
            VegetationSystemPro.ClearCache(vegetationItemID);            
            vegetationItemInfo.EnableRuntimeSpawn = false;
#if UNITY_EDITOR
            if (!Application.isPlaying)  EditorUtility.ClearProgressBar();
#endif
        }
    }
}
