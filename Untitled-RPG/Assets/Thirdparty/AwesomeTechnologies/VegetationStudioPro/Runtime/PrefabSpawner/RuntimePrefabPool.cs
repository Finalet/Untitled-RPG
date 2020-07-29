using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RuntimePrefabPool : VegetationItemPool
{
	private readonly List<GameObject> _prefabPoolList = new List<GameObject>();
	private readonly RuntimePrefabRule _runtimePrefabRule;

	private int _prefabCounter;
	private readonly Transform _prefabParent;

	private bool _showPrefabsInHierarchy;
	private VegetationItemInfoPro _vegetationItemInfoPro;
	private VegetationSystemPro _vegetationSystemPro;

	public RuntimePrefabPool(RuntimePrefabRule runtimePrefabRule, VegetationItemInfoPro vegetationItemInfoPro,
		Transform prefabParent, bool showPrefabsInHierarchy, VegetationSystemPro vegetationSystemPro)
	{
		_vegetationSystemPro = vegetationSystemPro;
		_vegetationItemInfoPro = vegetationItemInfoPro;
		_prefabParent = prefabParent;
		_showPrefabsInHierarchy = showPrefabsInHierarchy;
		_runtimePrefabRule = runtimePrefabRule;
	}

	public void SetPrefabVisibility(bool value)
	{
		_showPrefabsInHierarchy = value;
		for (int i = 0; i <= _prefabPoolList.Count - 1; i++)
		{
			GameObject go = _prefabPoolList[i];

			if (value)
			{
				go.hideFlags = HideFlags.DontSave;
			}
			else
			{
				go.hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}
	
	void AddVegetationItemInstanceInfo(GameObject colliderObject)
	{
		var vegetationItemInstanceInfo = colliderObject.AddComponent<VegetationItemInstanceInfo>();
		vegetationItemInstanceInfo.VegetationType = _vegetationItemInfoPro.VegetationType;
		vegetationItemInstanceInfo.VegetationItemID = _vegetationItemInfoPro.VegetationItemID;
		
		RuntimeObjectInfo runtimeObjectInfo = colliderObject.AddComponent<RuntimeObjectInfo>();
		runtimeObjectInfo.VegetationItemInfo = _vegetationItemInfoPro;		
	}

	void UpdateVegetationItemInstanceInfo(GameObject colliderObject, ItemSelectorInstanceInfo info)
	{
		VegetationItemInstanceInfo vegetationItemInstanceInfo = colliderObject.GetComponent<VegetationItemInstanceInfo>();
	
		if (vegetationItemInstanceInfo)
		{				
			vegetationItemInstanceInfo.Position = info.Position;
			vegetationItemInstanceInfo.VegetationItemInstanceID = Mathf.RoundToInt(vegetationItemInstanceInfo.Position.x *100f).ToString() + "_" +
			                                                      Mathf.RoundToInt(vegetationItemInstanceInfo.Position.y * 100f).ToString() + "_" +
			                                                      Mathf.RoundToInt(vegetationItemInstanceInfo.Position.z * 100f).ToString();
			vegetationItemInstanceInfo.Rotation = info.Rotation;
			vegetationItemInstanceInfo.Scale = info.Scale;
		}	
	}

	public override GameObject GetObject(ItemSelectorInstanceInfo info)
	{
		if (_prefabPoolList.Count <= 0) return CreateRuntimePrefabObject(info);

		GameObject prefabObject = _prefabPoolList[_prefabPoolList.Count - 1];
		_prefabPoolList.RemoveAtSwapBack(_prefabPoolList.Count - 1);
		prefabObject.SetActive(true);
		PositionPrefabObject(prefabObject, info);
		return prefabObject;
	}

	public GameObject CreateRuntimePrefabObject(ItemSelectorInstanceInfo info)
	{
		_prefabCounter++;
		GameObject newRuntimePrefab;
		if (_runtimePrefabRule.RuntimePrefab)
		{
			newRuntimePrefab = Object.Instantiate(_runtimePrefabRule.RuntimePrefab);
			newRuntimePrefab.name = _runtimePrefabRule.RuntimePrefab.name;
		}
		else
		{
			newRuntimePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
			newRuntimePrefab.name = "Run-time prefab" + _prefabCounter.ToString() + "_" + newRuntimePrefab.name;
		}	
		
		newRuntimePrefab.hideFlags = GetVisibilityHideFlags();
		newRuntimePrefab.transform.SetParent(_prefabParent);
		newRuntimePrefab.SetActive(true);
		newRuntimePrefab.layer = _runtimePrefabRule.PrefabLayer;
		AddVegetationItemInstanceInfo(newRuntimePrefab);
		
		PositionPrefabObject(newRuntimePrefab, info);
		
		return newRuntimePrefab;
	}

private HideFlags GetVisibilityHideFlags()
	{
		return _showPrefabsInHierarchy ? HideFlags.DontSave : HideFlags.HideAndDontSave;
	}

	void PositionPrefabObject(GameObject prefabObject, ItemSelectorInstanceInfo info)
	{
		Vector3 scale = Vector3.one;
		if (_runtimePrefabRule.UseVegetationItemScale)
		{
			scale = new Vector3(scale.x * info.Scale.x ,scale.y * info.Scale.y,scale.z* info.Scale.z);
		}				
		Vector3 position = info.Position + _vegetationSystemPro.FloatingOriginOffset;

		Vector3 offset;
		if (_runtimePrefabRule.UseVegetationItemScale)
		{
			offset =new Vector3(info.Scale.x * _runtimePrefabRule.PrefabOffset.x, info.Scale.y * _runtimePrefabRule.PrefabOffset.y, info.Scale.z * _runtimePrefabRule.PrefabOffset.z);
		}
		else
		{
			offset = _runtimePrefabRule.PrefabOffset;
		}		
		
		offset +=info.Rotation * offset;
		
		prefabObject.transform.position = position + offset;
		prefabObject.transform.localScale = new Vector3(scale.x * _runtimePrefabRule.PrefabScale.x,scale.y * _runtimePrefabRule.PrefabScale.y,scale.z * _runtimePrefabRule.PrefabScale.z);
		prefabObject.transform.rotation = info.Rotation * Quaternion.Euler(_runtimePrefabRule.PrefabRotation);

		UpdateVegetationItemInstanceInfo(prefabObject, info);
	}

	public override void ReturnObject(GameObject prefabObject)
	{
		if (prefabObject == null) return;
		
		if (_runtimePrefabRule.UsePool)
		{
			prefabObject.SetActive(false);
			_prefabPoolList.Add(prefabObject);
		}
		else
		{
			DestroyObject(prefabObject);
		}
		

	}

	private static void DestroyObject(GameObject go)
	{
		if (Application.isPlaying)
		{		
			Object.DestroyImmediate(go);
		}
		else
		{
			Object.DestroyImmediate(go);
		}				
	}

	public void Dispose()
	{	
		for (int i = 0; i <= _prefabPoolList.Count - 1; i++)
		{
			DestroyObject(_prefabPoolList[i]);
		}
		_prefabPoolList.Clear();		
	}	
}
