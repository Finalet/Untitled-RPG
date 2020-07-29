using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;


[Serializable]
public class RuntimePrefabInfo
{
	public GameObject RuntimeObject;
	public int VegetationCellIndex;
	public int VegetationCellItemIndex;
}

public class RuntimePrefabStorage
{
	[NonSerialized]
	public readonly List<RuntimePrefabInfo> RuntimePrefabInfoList = new List<RuntimePrefabInfo>();
	private readonly VegetationItemPool _vegetationItemPool;
	
	public RuntimePrefabStorage(VegetationItemPool vegetationItemPool)
	{
		_vegetationItemPool = vegetationItemPool;
	}
	
	public void SetPrefabVisibility(bool value)
	{
		for (int i = 0; i <= RuntimePrefabInfoList.Count - 1; i++)
		{
			RuntimePrefabInfo runtimePrefabInfo = RuntimePrefabInfoList[i];

			if (value)
			{
				runtimePrefabInfo.RuntimeObject.hideFlags = HideFlags.DontSave;
			}
			else
			{
				runtimePrefabInfo.RuntimeObject.hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}

	public void UpdateFloatingOrigin(Vector3 deltaFloatingOriginOffset)
	{
		for (int i = RuntimePrefabInfoList.Count - 1; i >= 0; i--)
		{
			RuntimePrefabInfo runtimePrefabInfo = RuntimePrefabInfoList[i];
			{
				runtimePrefabInfo.RuntimeObject.transform.position += deltaFloatingOriginOffset;
			}
		}
	}
	
	public void RemoveRuntimePrefab(int vegetationCellIndex)
	{	
		for (int i = RuntimePrefabInfoList.Count - 1; i >= 0; i--)
		{
			RuntimePrefabInfo runtimePrefabInfo = RuntimePrefabInfoList[i];
			if (runtimePrefabInfo.VegetationCellIndex == vegetationCellIndex)
			{
				if (_vegetationItemPool != null)
				{
					_vegetationItemPool.ReturnObject(runtimePrefabInfo.RuntimeObject);
				}
				else
				{
					DestroyRuntimePrefab(runtimePrefabInfo);
				}	
				
				RuntimePrefabInfoList.RemoveAtSwapBack(i);
			}						
		}				
	}

	public void AddRuntimePrefab(GameObject runtimeObject, int vegetationCellIndex, int vegetationCellItemIndex)
	{
		RuntimePrefabInfo runtimePrefabInfo = new RuntimePrefabInfo
		{
			VegetationCellIndex = vegetationCellIndex,
			VegetationCellItemIndex = vegetationCellItemIndex,
			RuntimeObject = runtimeObject
		};

		RuntimePrefabInfoList.Add(runtimePrefabInfo);
	}	
	
	public void RemoveRuntimePrefab(int vegetationCellIndex, int vegetationCellItemIndex, VegetationItemPool vegetationItemPool)
	{
	
		for (int i = RuntimePrefabInfoList.Count - 1; i >= 0; i--)
		{
			RuntimePrefabInfo runtimePrefabInfo = RuntimePrefabInfoList[i];
			if (runtimePrefabInfo.VegetationCellIndex == vegetationCellIndex && runtimePrefabInfo.VegetationCellItemIndex == vegetationCellItemIndex)
			{
				if (vegetationItemPool != null)
				{
					vegetationItemPool.ReturnObject(runtimePrefabInfo.RuntimeObject);
				}
				else
				{
					DestroyRuntimePrefab(runtimePrefabInfo);
				}				
				RuntimePrefabInfoList.RemoveAtSwapBack(i);
			}						
		}				
	}

	public GameObject GetRuntimePrefab(int vegetationCellIndex, int vegetationCellItemIndex)
	{
		for (int i = RuntimePrefabInfoList.Count - 1; i >= 0; i--)
		{
			RuntimePrefabInfo runtimePrefabInfo = RuntimePrefabInfoList[i];
			if (runtimePrefabInfo.VegetationCellIndex == vegetationCellIndex &&
			    runtimePrefabInfo.VegetationCellItemIndex == vegetationCellItemIndex)
			{
				return runtimePrefabInfo.RuntimeObject;
			}
		}
		return null;
	}

	void DestroyRuntimePrefab(RuntimePrefabInfo runtimePrefabInfo)
	{		
		// add delegate for pooling system						
		if (Application.isPlaying)
		{
			Object.Destroy(runtimePrefabInfo.RuntimeObject);
		}
		else
		{
			Object.DestroyImmediate(runtimePrefabInfo.RuntimeObject);
		}
	}

	public void Dispose()
	{
		for (int i = RuntimePrefabInfoList.Count - 1; i >= 0; i--)
		{
			RuntimePrefabInfo runtimePrefabInfo = RuntimePrefabInfoList[i];

			if (_vegetationItemPool != null)
			{
				_vegetationItemPool.ReturnObject(runtimePrefabInfo.RuntimeObject);
			}
			else
			{
				DestroyRuntimePrefab(runtimePrefabInfo);
			}		
		}
		RuntimePrefabInfoList.Clear();
	}
}
