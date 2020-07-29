using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using AwesomeTechnologies.Utility;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    [Serializable]
    public class SourceCount
    {
        public byte VegetationSourceID;
        public int Count;
    }

    [Serializable]
    public class PersistentVegetationInstanceInfo
    {
        public string VegetationItemID;
        public int Count;
        public List<SourceCount> SourceCountList = new List<SourceCount>();

        public void AddSourceCountList(List<SourceCount> sourceCountList)
        {
            for (int i = 0; i <= sourceCountList.Count - 1; i++)
            {
                AddSourceCount(sourceCountList[i]);
            }
        }

        public void AddSourceCount(SourceCount sourceCount)
        {
            SourceCount tempSourceCount = GetSourceCount(sourceCount.VegetationSourceID);
            if (tempSourceCount == null)
            {
                tempSourceCount = new SourceCount { VegetationSourceID = sourceCount.VegetationSourceID };
                SourceCountList.Add(tempSourceCount);
            }
            tempSourceCount.Count += sourceCount.Count;
        }

        SourceCount GetSourceCount(byte vegetationSourceID)
        {
            for (int i = 0; i <= SourceCountList.Count - 1; i++)
            {
                if (SourceCountList[i].VegetationSourceID == vegetationSourceID) return SourceCountList[i];
            }
            return null;
        }

    }

    [Serializable]
    public struct PersistentVegetationItem
    {
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;
        public byte VegetationSourceID;
        public float DistanceFalloff;
    }

    [Serializable]
    public class PersistentVegetationInfo
    {
        public string VegetationItemID;
        [SerializeField]
        public List<PersistentVegetationItem> VegetationItemList = new List<PersistentVegetationItem>();
        [NonSerialized] public NativeArray<PersistentVegetationItem> NativeVegetationItemArray;
        
        public List<SourceCount> SourceCountList = new List<SourceCount>();

        public void CopyToNativeArray()
        {           
            NativeVegetationItemArray = new NativeArray<PersistentVegetationItem>(VegetationItemList.Count,Allocator.Persistent);
            NativeVegetationItemArray.CopyFromFast(VegetationItemList);
        }
        
        public void ClearCell()
        {
            VegetationItemList.Clear();
            SourceCountList.Clear();
        }

        public void AddPersistentVegetationItemInstance(ref PersistentVegetationItem persistentVegetationItem)
        {
            IncreaseSourceCount(persistentVegetationItem.VegetationSourceID);
            VegetationItemList.Add(persistentVegetationItem);
        }

        public void RemovePersistentVegetationItemInstance(ref PersistentVegetationItem persistentVegetationItem)
        {
            DecreaseSourceCount(persistentVegetationItem.VegetationSourceID);
            VegetationItemList.Remove(persistentVegetationItem);
        }

        public void RemovePersistentVegetationInstanceAtIndex(int index)
        {
            if (index >= VegetationItemList.Count) return;
            DecreaseSourceCount(VegetationItemList[index].VegetationSourceID);
            VegetationItemList.RemoveAt(index);
        }

        public void UpdatePersistentVegetationItemInstanceSourceId(ref PersistentVegetationItem persistentVegetationItem, byte newSourceID)
        {
            if (persistentVegetationItem.VegetationSourceID != newSourceID)
            {
                DecreaseSourceCount(persistentVegetationItem.VegetationSourceID);
                persistentVegetationItem.VegetationSourceID = newSourceID;
                IncreaseSourceCount(persistentVegetationItem.VegetationSourceID);
            }
        }

        void IncreaseSourceCount(byte vegetationSourceID)
        {
            SourceCount sourceCount = GetSourceCount(vegetationSourceID);
            if (sourceCount == null)
            {
                sourceCount = new SourceCount { VegetationSourceID = vegetationSourceID };
                SourceCountList.Add(sourceCount);
            }
            sourceCount.Count++;
        }

        SourceCount GetSourceCount(byte vegetationSourceID)
        {
            for (int i = 0; i <= SourceCountList.Count - 1; i++)
            {
                if (SourceCountList[i].VegetationSourceID == vegetationSourceID) return SourceCountList[i];
            }
            return null;
        }

        void DecreaseSourceCount(byte vegetationSourceID)
        {
            SourceCount sourceCount = GetSourceCount(vegetationSourceID);
            if (sourceCount == null) return;
            sourceCount.Count--;

            if (sourceCount.Count == 0) SourceCountList.Remove(sourceCount);
        }

        public void Dispose()
        {
            if (NativeVegetationItemArray.IsCreated)
            {
                NativeVegetationItemArray.Dispose();
            }
        }
    }

    [Serializable]
    public class PersistentVegetationCell
    {
        public List<PersistentVegetationInfo> PersistentVegetationInfoList = new List<PersistentVegetationInfo>();

        public void Dispose()
        {           
            for (int i = 0; i <= PersistentVegetationInfoList.Count -1; i++)
            {
                PersistentVegetationInfoList[i].Dispose();
              
            }
        }
        
        public void AddVegetationItemInstance(string vegetationItemID, Vector3 position, Vector3 scale, Quaternion rotation, byte vegetationSourceID, float distanceFalloff)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(vegetationItemID);
            if (persistentVegetationInfo == null)
            {
                persistentVegetationInfo = new PersistentVegetationInfo { VegetationItemID = vegetationItemID };
                PersistentVegetationInfoList.Add(persistentVegetationInfo);
            }

            PersistentVegetationItem persistentVegetationItem = new PersistentVegetationItem
            {
                Position = position,
                Rotation = rotation,
                Scale = scale,
                VegetationSourceID = vegetationSourceID,
                DistanceFalloff = distanceFalloff
            };

            persistentVegetationInfo.AddPersistentVegetationItemInstance(ref persistentVegetationItem);
        }


        public void RemoveVegetationItemInstance(string vegetationItemID, Vector3 position, float minimumDistance)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(vegetationItemID);
            if (persistentVegetationInfo == null) return;

            for (int i = persistentVegetationInfo.VegetationItemList.Count - 1; i >= 0; i--)
            {
                if (Vector3.Distance(persistentVegetationInfo.VegetationItemList[i].Position, position) <
                    minimumDistance)
                {
                    persistentVegetationInfo.VegetationItemList.RemoveAt(i);
                }
            }
        }

        public void RemoveVegetationItemInstance2D(string vegetationItemID, Vector3 position, float minimumDistance)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(vegetationItemID);
            if (persistentVegetationInfo == null) return;

            for (int i = persistentVegetationInfo.VegetationItemList.Count - 1; i >= 0; i--)
            {
                if (Vector2.Distance(
                        new Vector2(persistentVegetationInfo.VegetationItemList[i].Position.x,
                            persistentVegetationInfo.VegetationItemList[i].Position.z), new Vector2(position.x, position.z)) <
                    minimumDistance)
                {
                    persistentVegetationInfo.VegetationItemList.RemoveAt(i);
                }
            }
        }

        public void AddVegetationItemInstanceEx(string vegetationItemID, Vector3 position, Vector3 scale, Quaternion rotation, byte vegetationSourceID, float minimumDistance, float distanceFalloff)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(vegetationItemID);
            if (persistentVegetationInfo == null)
            {
                persistentVegetationInfo = new PersistentVegetationInfo { VegetationItemID = vegetationItemID };
                PersistentVegetationInfoList.Add(persistentVegetationInfo);
            }

            float closestDistance = CalculateClosestItemDistance(position, persistentVegetationInfo.VegetationItemList);
            if (closestDistance < minimumDistance) return;

            PersistentVegetationItem persistentVegetationItem = new PersistentVegetationItem
            {
                Position = position,
                Rotation = rotation,
                Scale = scale,
                VegetationSourceID = vegetationSourceID,
                DistanceFalloff = distanceFalloff
            };

            persistentVegetationInfo.AddPersistentVegetationItemInstance(ref persistentVegetationItem);
        }

        float CalculateClosestItemDistance(Vector3 position, List<PersistentVegetationItem> instanceList)
        {
            float nearestSqrMag = float.PositiveInfinity;
            Vector3 nearestVector3 = Vector3.zero;

            for (int i = 0; i < instanceList.Count; i++)
            {
                float sqrMag = (instanceList[i].Position - position).sqrMagnitude;
                if (sqrMag < nearestSqrMag)
                {
                    nearestSqrMag = sqrMag;
                    nearestVector3 = instanceList[i].Position;
                }
            }

            return Vector3.Distance(nearestVector3, position);
        }

        public void ClearCell()
        {
            PersistentVegetationInfoList.Clear();
        }

        public PersistentVegetationInfo GetPersistentVegetationInfo(string vegetationItemID)
        {
            for (int i = 0; i <= PersistentVegetationInfoList.Count - 1; i++)
            {
                if (PersistentVegetationInfoList[i].VegetationItemID == vegetationItemID) return PersistentVegetationInfoList[i];
            }

            return null;
        }

        public void RemoveVegetationItemInstances(string vegetationItemID)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(vegetationItemID);
            if (persistentVegetationInfo != null)
            {
                PersistentVegetationInfoList.Remove(persistentVegetationInfo);
            }
        }

        public void RemoveVegetationItemInstances(string vegetationItemID, byte vegetationSourceID)
        {
            PersistentVegetationInfo persistentVegetationInfo = GetPersistentVegetationInfo(vegetationItemID);
            if (persistentVegetationInfo != null)
            {
                for (int i = persistentVegetationInfo.VegetationItemList.Count - 1; i >= 0; i--)
                {
                    if (persistentVegetationInfo.VegetationItemList[i].VegetationSourceID == vegetationSourceID)
                    {
                        persistentVegetationInfo.RemovePersistentVegetationInstanceAtIndex(i);
                    }
                }

                if (persistentVegetationInfo.VegetationItemList.Count == 0)
                {
                    PersistentVegetationInfoList.Remove(persistentVegetationInfo);
                }
            }
        }
    }

    [Serializable]
    public class ExportData
    {
        public List<PersistentVegetationCell> PersistentVegetationCellList;
        public List<PersistentVegetationInstanceInfo> PersistentVegetationInstanceInfoList;
        public List<byte> PersistentVegetationInstanceSourceList;
    }

    [PreferBinarySerialization]
    [Serializable]
    public class PersistentVegetationStoragePackage : ScriptableObject
    {
        public List<PersistentVegetationCell> PersistentVegetationCellList = new List<PersistentVegetationCell>();
        public List<PersistentVegetationInstanceInfo> PersistentVegetationInstanceInfoList = new List<PersistentVegetationInstanceInfo>();
        public List<byte> PersistentVegetationInstanceSourceList = new List<byte>();

        public void Dispose()
        {           
            for (int i = 0; i <= PersistentVegetationCellList.Count -1; i++)
            {               
                PersistentVegetationCellList[i].Dispose();
            }
        }
        
        public void ExportToFile(string filename)
        {
            var exportData = new ExportData
            {
                PersistentVegetationCellList = PersistentVegetationCellList,
                PersistentVegetationInstanceInfoList = PersistentVegetationInstanceInfoList,
                PersistentVegetationInstanceSourceList = PersistentVegetationInstanceSourceList
            };

            BinaryFormatter bf = SerializationSurrogateUtil.GetBinaryFormatter();
            FileStream file = File.Create(filename);
            bf.Serialize(file, exportData);
            file.Close();
        }

        public void ExportToStream(Stream outputStream)
        {
            var exportData = new ExportData
            {
                PersistentVegetationCellList = PersistentVegetationCellList,
                PersistentVegetationInstanceInfoList = PersistentVegetationInstanceInfoList,
                PersistentVegetationInstanceSourceList = PersistentVegetationInstanceSourceList
            };

            BinaryFormatter bf = SerializationSurrogateUtil.GetBinaryFormatter();
            bf.Serialize(outputStream, exportData);
            outputStream.Position = 0;
        }

        public void ImportFromStream(Stream inputStream)
        {
            BinaryFormatter bf = SerializationSurrogateUtil.GetBinaryFormatter();         
            var exportData = (ExportData)bf.Deserialize(inputStream);
            PersistentVegetationCellList = exportData.PersistentVegetationCellList;
            PersistentVegetationInstanceInfoList = exportData.PersistentVegetationInstanceInfoList;
            PersistentVegetationInstanceSourceList = exportData.PersistentVegetationInstanceSourceList;
            inputStream.Position = 0;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void ImportFromFile(string filename)
        {
            BinaryFormatter bf = SerializationSurrogateUtil.GetBinaryFormatter();
            FileStream file = File.Open(filename,FileMode.Open);
            var exportData = (ExportData)bf.Deserialize(file);

            PersistentVegetationCellList = exportData.PersistentVegetationCellList;
            PersistentVegetationInstanceInfoList = exportData.PersistentVegetationInstanceInfoList;
            PersistentVegetationInstanceSourceList = exportData.PersistentVegetationInstanceSourceList;

            file.Close();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }


        public bool Initialized
        {
            get { return PersistentVegetationCellList.Count > 0; }
        }

        [SerializeField]
        private bool _instanceInfoDirty;
        public void ClearPersistentVegetationCells()
        {
            PersistentVegetationCellList.Clear();
        }

        public void SetInstanceInfoDirty()
        {
            _instanceInfoDirty = true;
        }

        public void RemoveVegetationItemInstances(string vegetationItemID)
        {
            for (int i = 0; i <= PersistentVegetationCellList.Count - 1; i++)
            {
                PersistentVegetationCellList[i].RemoveVegetationItemInstances(vegetationItemID);
            }
            _instanceInfoDirty = true;
        }

        public void RemoveVegetationItemInstances(string vegetationItemID, byte vegetationSourceID)
        {
            for (int i = 0; i <= PersistentVegetationCellList.Count - 1; i++)
            {
                PersistentVegetationCellList[i].RemoveVegetationItemInstances(vegetationItemID, vegetationSourceID);
            }
            _instanceInfoDirty = true;
        }

        public void AddVegetationCell()
        {
            PersistentVegetationCell persistentVegetationCell = new PersistentVegetationCell();
            PersistentVegetationCellList.Add(persistentVegetationCell);

            _instanceInfoDirty = true;
        }

        public void AddVegetationItemInstance(int cellIndex, string vegetationItemID, Vector3 position, Vector3 scale, Quaternion rotation, byte vegetationSourceID,float distanceFalloff)
        {
            if (PersistentVegetationCellList.Count > cellIndex)
            {
                PersistentVegetationCellList[cellIndex].AddVegetationItemInstance(vegetationItemID, position, scale, rotation, vegetationSourceID,distanceFalloff);
            }

            _instanceInfoDirty = true;

#if UNITY_EDITOR
            if (ThreadUtility.MainThread == Thread.CurrentThread)
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }

        public void AddVegetationItemInstanceEx(int cellIndex, string vegetationItemID, Vector3 position, Vector3 scale, Quaternion rotation, byte vegetationSourceID, float minimumDistance, float distanceFalloff)
        {
            if (PersistentVegetationCellList.Count > cellIndex)
            {
                PersistentVegetationCellList[cellIndex].AddVegetationItemInstanceEx(vegetationItemID, position, scale, rotation, vegetationSourceID, minimumDistance,distanceFalloff);
            }

            _instanceInfoDirty = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveVegetationItemInstance(int cellIndex, string vegetationItemID, Vector3 position, float minimumDistance)
        {
            if (PersistentVegetationCellList.Count > cellIndex)
            {
                PersistentVegetationCellList[cellIndex].RemoveVegetationItemInstance(vegetationItemID, position, minimumDistance);
            }

            _instanceInfoDirty = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveVegetationItemInstance2D(int cellIndex, string vegetationItemID, Vector3 position, float minimumDistance)
        {
            if (PersistentVegetationCellList.Count > cellIndex)
            {
                PersistentVegetationCellList[cellIndex].RemoveVegetationItemInstance2D(vegetationItemID, position, minimumDistance);
            }

            _instanceInfoDirty = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public List<PersistentVegetationInstanceInfo> GetPersistentVegetationInstanceInfoList()
        {
            if (_instanceInfoDirty)
            {
                UpdatePersistentVegetationInstanceInfo();
                _instanceInfoDirty = false;
            }

            return PersistentVegetationInstanceInfoList;
        }

        void UpdatePersistentVegetationInstanceInfo()
        {
            PersistentVegetationInstanceInfoList.Clear();

            for (int i = 0; i <= PersistentVegetationCellList.Count - 1; i++)
            {
                PersistentVegetationCell cell = PersistentVegetationCellList[i];
                for (int j = 0; j <= cell.PersistentVegetationInfoList.Count - 1; j++)
                {
                    PersistentVegetationInstanceInfo instanceInfo =
                        GetPersistentVegetationInstanceInfo(cell.PersistentVegetationInfoList[j].VegetationItemID);
                    if (instanceInfo == null)
                    {
                        instanceInfo =
                            new PersistentVegetationInstanceInfo { VegetationItemID = cell.PersistentVegetationInfoList[j].VegetationItemID };
                        PersistentVegetationInstanceInfoList.Add(instanceInfo);
                    }
                    instanceInfo.Count += cell.PersistentVegetationInfoList[j].VegetationItemList.Count;
                    instanceInfo.AddSourceCountList(cell.PersistentVegetationInfoList[j].SourceCountList);
                }
            }
        }

        PersistentVegetationInstanceInfo GetPersistentVegetationInstanceInfo(string vegetationItemID)
        {
            for (int i = 0; i <= PersistentVegetationInstanceInfoList.Count - 1; i++)
            {
                if (PersistentVegetationInstanceInfoList[i].VegetationItemID == vegetationItemID) return PersistentVegetationInstanceInfoList[i];
            }
            return null;
        }
    }
}
