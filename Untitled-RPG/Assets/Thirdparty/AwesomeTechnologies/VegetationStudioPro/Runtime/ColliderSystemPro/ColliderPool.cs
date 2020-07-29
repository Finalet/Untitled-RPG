using System.Collections.Generic;
using System.Xml;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ColliderPool : VegetationItemPool
{
	private readonly List<GameObject> _colliderPoolList = new List<GameObject>();
	private readonly VegetationItemInfoPro _vegetationItemInfoPro;
	private readonly VegetationItemModelInfo _vegetationItemModelInfo;
	private readonly VegetationSystemPro _vegetationSystemPro;

	private int _colliderCounter;
	private readonly Transform _colliderParent;

	private readonly GameObject _sourceColliderObject;
	private bool _showColliders;
	private LayerMask _colliderLayer;
	private string _colliderTag;
	
	public ColliderPool(VegetationItemInfoPro vegetationItemInfoPro, VegetationItemModelInfo vegetationItemModelInfo, VegetationSystemPro vegetationSystemPro, Transform colliderParent, bool showColliders)
	{
		_vegetationItemInfoPro = vegetationItemInfoPro;
		_vegetationItemModelInfo = vegetationItemModelInfo;
		_vegetationSystemPro = vegetationSystemPro;
		_colliderParent = colliderParent;
		_showColliders = showColliders;

		_colliderLayer = vegetationSystemPro.VegetationSettings.GetLayer(vegetationItemInfoPro.VegetationType);
		_colliderTag = vegetationItemInfoPro.ColliderTag;
		if (_colliderTag == "")
		{
			_colliderTag = "Untagged";
		}
		
		if (_vegetationItemInfoPro.ColliderType == ColliderType.FromPrefab)
		{
			GameObject tmpColliderObject = Object.Instantiate(vegetationItemInfoPro.VegetationPrefab);
			if (_vegetationItemInfoPro.ColliderTag != "")
			{
				tmpColliderObject.tag = vegetationItemInfoPro.ColliderTag;
			}

			tmpColliderObject.hideFlags = HideFlags.DontSave;
			tmpColliderObject.name = "ColliderSource_" + _vegetationItemInfoPro.VegetationItemID;
			tmpColliderObject.transform.SetParent(_colliderParent);
			_sourceColliderObject = CreateColliderObject(tmpColliderObject);
			DestroyObject(tmpColliderObject);		
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
	
	public void SetColliderVisibility(bool value)
	{
		_showColliders = value;
		for (int i = 0; i <= _colliderPoolList.Count - 1; i++)
		{
			GameObject go = _colliderPoolList[i];

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

	public override GameObject GetObject(ItemSelectorInstanceInfo info)
	{
		if (_colliderPoolList.Count <= 0) return CreateColliderObject(info);
		
		GameObject colliderObject = _colliderPoolList[_colliderPoolList.Count - 1];
		_colliderPoolList.RemoveAtSwapBack(_colliderPoolList.Count -1);
		colliderObject.SetActive(true);
		PositionColliderObject(colliderObject, info);
		return colliderObject;
	}

	private HideFlags GetVisibilityHideFlags()
	{
		return _showColliders ? HideFlags.DontSave : HideFlags.HideAndDontSave;
	}

	void PositionColliderObject(GameObject colliderObject, ItemSelectorInstanceInfo info)
	{
		colliderObject.transform.position = info.Position + _vegetationSystemPro.FloatingOriginOffset;
		colliderObject.transform.localScale = info.Scale;
		colliderObject.transform.rotation = info.Rotation;

		UpdateVegetationItemInstanceInfo(colliderObject, info);
	}

	public GameObject CreateColliderObject(ItemSelectorInstanceInfo info)
	{
		_colliderCounter++;		
		GameObject newColliderObject;
		if (_vegetationItemInfoPro.ColliderType == ColliderType.FromPrefab)
		{
			newColliderObject = Object.Instantiate(_sourceColliderObject);
			newColliderObject.name = "Collider_" + _colliderCounter.ToString();
			newColliderObject.hideFlags = GetVisibilityHideFlags();
			newColliderObject.transform.SetParent(_colliderParent);			
		}
		else
		{	
			newColliderObject = CreatePrimitiveCollider(info);
		}

		if (_vegetationItemInfoPro.ColliderTag != "")
		{
			newColliderObject.tag = _vegetationItemInfoPro.ColliderTag;
		}		
		
		AddNavMesObstacle(newColliderObject);
		newColliderObject.SetActive(true);
		AddVegetationItemInstanceInfo(newColliderObject);
		
		PositionColliderObject(newColliderObject,info);
		newColliderObject.layer =
			_vegetationSystemPro.VegetationSettings.GetLayer(_vegetationItemInfoPro.VegetationType);

		
		return newColliderObject;
	}

	private GameObject CreatePrimitiveCollider(ItemSelectorInstanceInfo info)
	{		 		
		switch (_vegetationItemInfoPro.ColliderType)
		{
			case ColliderType.Capsule:
				GameObject newCapsuleColliderObject = new GameObject("CapsuleCollider_" + _colliderCounter);
				newCapsuleColliderObject.layer = _colliderLayer;
				newCapsuleColliderObject.tag = _colliderTag;
				newCapsuleColliderObject.transform.SetParent(_colliderParent);
				newCapsuleColliderObject.hideFlags = GetVisibilityHideFlags();

				CapsuleCollider capsuleCollider = newCapsuleColliderObject.AddComponent<CapsuleCollider>();
				capsuleCollider.height = _vegetationItemInfoPro.ColliderHeight;
				capsuleCollider.radius = _vegetationItemInfoPro.ColliderRadius;
				capsuleCollider.isTrigger =_vegetationItemInfoPro.ColliderTrigger;
				Vector3 capsuleColliderOffset = new Vector3(info.Scale.x * _vegetationItemInfoPro.ColliderOffset.x, info.Scale.y * _vegetationItemInfoPro.ColliderOffset.y, info.Scale.z * _vegetationItemInfoPro.ColliderOffset.z);			
				capsuleColliderOffset +=info.Rotation * capsuleColliderOffset;
				capsuleCollider.center = capsuleColliderOffset;
				return newCapsuleColliderObject;
			
			case ColliderType.Sphere:
				GameObject newSphereColliderObject = new GameObject("SphereCollider_" + _colliderCounter);
				newSphereColliderObject.layer = _colliderLayer;
				newSphereColliderObject.tag = _colliderTag;
				newSphereColliderObject.transform.SetParent(_colliderParent);
				newSphereColliderObject.hideFlags =GetVisibilityHideFlags();
				SphereCollider sphereCollider = newSphereColliderObject.AddComponent<SphereCollider>();
				sphereCollider.radius = _vegetationItemInfoPro.ColliderRadius;
				sphereCollider.isTrigger =_vegetationItemInfoPro.ColliderTrigger;
				Vector3 sphereColliderOffset = new Vector3(info.Scale.x * _vegetationItemInfoPro.ColliderOffset.x, info.Scale.y * _vegetationItemInfoPro.ColliderOffset.y, info.Scale.z * _vegetationItemInfoPro.ColliderOffset.z);			
				sphereColliderOffset +=info.Rotation * sphereColliderOffset;
				sphereCollider.center = sphereColliderOffset;
				return newSphereColliderObject;
			
			case ColliderType.Box :
				GameObject newColliderObject = new GameObject("BoxCollider_" + _colliderCounter);
				newColliderObject.layer = _colliderLayer;
				newColliderObject.tag = _colliderTag;
				newColliderObject.transform.SetParent(_colliderParent);
				newColliderObject.hideFlags =GetVisibilityHideFlags();
				BoxCollider boxCollider = newColliderObject.AddComponent<BoxCollider>();
				Vector3 boxColliderSize = new Vector3(info.Scale.x * _vegetationItemInfoPro.ColliderSize.x, info.Scale.y * _vegetationItemInfoPro.ColliderSize.y, info.Scale.z * _vegetationItemInfoPro.ColliderSize.z);			
				
				boxCollider.size = boxColliderSize;
				boxCollider.isTrigger =_vegetationItemInfoPro.ColliderTrigger;
				Vector3 boxColliderOffset = new Vector3(info.Scale.x * _vegetationItemInfoPro.ColliderOffset.x, info.Scale.y * _vegetationItemInfoPro.ColliderOffset.y, info.Scale.z * _vegetationItemInfoPro.ColliderOffset.z);			
				boxColliderOffset +=info.Rotation * boxColliderOffset;
				boxCollider.center = boxColliderOffset;
				return newColliderObject;
			
			case ColliderType.CustomMesh :
				GameObject newCustomMeshColliderObject = new GameObject("MeshCollider_" + _colliderCounter);
				newCustomMeshColliderObject.layer = _colliderLayer;
				newCustomMeshColliderObject.tag = _colliderTag;
				newCustomMeshColliderObject.transform.SetParent(_colliderParent);
				newCustomMeshColliderObject.hideFlags = GetVisibilityHideFlags();
				MeshCollider customMeshCollider = newCustomMeshColliderObject.AddComponent<MeshCollider>();
				customMeshCollider.isTrigger =_vegetationItemInfoPro.ColliderTrigger;
				customMeshCollider.sharedMesh = _vegetationItemInfoPro.ColliderMesh;
				customMeshCollider.convex = _vegetationItemInfoPro.ColliderConvex;
				return newCustomMeshColliderObject;
			
			case ColliderType.Mesh :
				GameObject newMeshColliderObject = new GameObject("MeshCollider_" + _colliderCounter);
				newMeshColliderObject.layer = _colliderLayer;
				newMeshColliderObject.tag = _colliderTag;
				newMeshColliderObject.transform.SetParent(_colliderParent);
				newMeshColliderObject.hideFlags = GetVisibilityHideFlags();
				MeshCollider meshCollider = newMeshColliderObject.AddComponent<MeshCollider>();
				meshCollider.isTrigger =_vegetationItemInfoPro.ColliderTrigger;
				meshCollider.sharedMesh = _vegetationItemModelInfo.VegetationMeshLod0;
				meshCollider.convex = _vegetationItemInfoPro.ColliderConvex;
				return newMeshColliderObject;			
		}		
		return new GameObject("Empty collider object");
	}
	
	void AddNavMesObstacle(GameObject go)
	{
		NavMeshObstacle navMeshObstacle;

		switch (_vegetationItemInfoPro.NavMeshObstacleType)
		{
			case NavMeshObstacleType.Box:
				navMeshObstacle = go.AddComponent<NavMeshObstacle>();
				navMeshObstacle.shape = NavMeshObstacleShape.Box;
				navMeshObstacle.center = _vegetationItemInfoPro.NavMeshObstacleCenter;
				navMeshObstacle.size = _vegetationItemInfoPro.NavMeshObstacleSize;
				navMeshObstacle.carving = _vegetationItemInfoPro.NavMeshObstacleCarve;
				break;
			case NavMeshObstacleType.Capsule:
				navMeshObstacle = go.AddComponent<NavMeshObstacle>();
				navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
				navMeshObstacle.center = _vegetationItemInfoPro.NavMeshObstacleCenter;
				navMeshObstacle.radius = _vegetationItemInfoPro.NavMeshObstacleRadius;
				navMeshObstacle.height = _vegetationItemInfoPro.NavMeshObstacleHeight;
				navMeshObstacle.carving = _vegetationItemInfoPro.NavMeshObstacleCarve;
				break;
		}
	}
	
	public override void ReturnObject(GameObject colliderObject)
	{
		colliderObject.SetActive(false);
		//CleanColliderObject(colliderObject);
		_colliderPoolList.Add(colliderObject);
	}

//	private void CleanColliderObject(GameObject colliderObject)
//	{
//		return;
				
//		Component[] components = colliderObject.GetComponents<Component>();
//		for (int i = 0; i <= components.Length - 1; i++)
//		{					
//			if (!IsReservedComponent(components[i]))
//			{
//				DestroyComponent(components[i]);
//			}			
//		}				
//	}

	private GameObject CreateColliderObject(GameObject sourceObject)
	{
		sourceObject.transform.position = Vector3.zero;
		sourceObject.transform.localScale = Vector3.one;
		sourceObject.transform.rotation = Quaternion.identity;

		GameObject colliderObject = new GameObject("SourceColliderObject") {hideFlags = HideFlags.DontSave};
		colliderObject.transform.SetParent(_colliderParent);
		colliderObject.transform.position = Vector3.zero;
		colliderObject.transform.localScale = Vector3.one;
		colliderObject.transform.rotation = Quaternion.identity;
		colliderObject.layer = _colliderLayer;
		colliderObject.tag = _colliderTag;
		colliderObject.SetActive(false);

		MeshCollider[] meshColliders = sourceObject.GetComponentsInChildren<MeshCollider>();
		SphereCollider[] sphereColliders = sourceObject.GetComponentsInChildren<SphereCollider>();
		BoxCollider[] boxColliders = sourceObject.GetComponentsInChildren<BoxCollider>();
		CapsuleCollider[] capsuleColliders = sourceObject.GetComponentsInChildren<CapsuleCollider>();

		for (int i = 0; i <= capsuleColliders.Length - 1; i++)
		{
			GameObject capsuleObject = new GameObject("CapsuleCollider") {hideFlags = HideFlags.DontSave};			
			capsuleObject.transform.SetParent(colliderObject.transform);
			capsuleObject.transform.position = capsuleColliders[i].transform.position;
			capsuleObject.transform.localScale = capsuleColliders[i].transform.localScale;
			capsuleObject.transform.rotation = capsuleColliders[i].transform.rotation;
			capsuleObject.layer = _colliderLayer;
			capsuleObject.tag = _colliderTag;

			CapsuleCollider newCollider = capsuleObject.AddComponent<CapsuleCollider>();
			newCollider.radius = capsuleColliders[i].radius;
			newCollider.height = capsuleColliders[i].height;
			newCollider.center = capsuleColliders[i].center;
			newCollider.direction = capsuleColliders[i].direction;
			newCollider.sharedMaterial = capsuleColliders[i].sharedMaterial;
			newCollider.isTrigger = capsuleColliders[i].isTrigger;
		}
		
		for (int i = 0; i <= boxColliders.Length - 1; i++)
		{
			GameObject boxobject = new GameObject("BoxCollider") {hideFlags = HideFlags.DontSave};			
			boxobject.transform.SetParent(colliderObject.transform);
			boxobject.transform.position = boxColliders[i].transform.position;
			boxobject.transform.localScale = boxColliders[i].transform.localScale;
			boxobject.transform.rotation = boxColliders[i].transform.rotation;
			boxobject.layer = _colliderLayer;
			boxobject.tag = _colliderTag;

			BoxCollider newCollider = boxobject.AddComponent<BoxCollider>();
			newCollider.center = boxColliders[i].center;
			newCollider.size = boxColliders[i].size;
			newCollider.sharedMaterial = boxColliders[i].sharedMaterial;
			newCollider.isTrigger = boxColliders[i].isTrigger;
		}
		
		for (int i = 0; i <= sphereColliders.Length - 1; i++)
		{
			GameObject sphereObject = new GameObject("SphereCollider") {hideFlags = HideFlags.DontSave};			
			sphereObject.transform.SetParent(colliderObject.transform);
			sphereObject.transform.position = sphereColliders[i].transform.position;
			sphereObject.transform.localScale = sphereColliders[i].transform.localScale;
			sphereObject.transform.rotation = sphereColliders[i].transform.rotation;
			sphereObject.layer = _colliderLayer;
			sphereObject.tag = _colliderTag;

			SphereCollider newCollider = sphereObject.AddComponent<SphereCollider>();
			newCollider.center = sphereColliders[i].center;
			newCollider.radius = sphereColliders[i].radius;
			newCollider.sharedMaterial = sphereColliders[i].sharedMaterial;
			newCollider.isTrigger = sphereColliders[i].isTrigger;
		}
		
		for (int i = 0; i <= meshColliders.Length - 1; i++)
		{
			GameObject meshObject = new GameObject("MeshCollider") {hideFlags = HideFlags.DontSave};		
			meshObject.transform.SetParent(colliderObject.transform);
			meshObject.transform.position = meshColliders[i].transform.position;
			meshObject.transform.localScale = meshColliders[i].transform.localScale;
			meshObject.transform.rotation = meshColliders[i].transform.rotation;
			meshObject.layer = _colliderLayer;
			meshObject.tag = _colliderTag;

			MeshCollider newCollider = meshObject.AddComponent<MeshCollider>();
			newCollider.cookingOptions = meshColliders[i].cookingOptions;
			newCollider.convex = meshColliders[i].convex;
			newCollider.sharedMesh = meshColliders[i].sharedMesh;
			newCollider.sharedMaterial = meshColliders[i].sharedMaterial;
			newCollider.isTrigger = meshColliders[i].isTrigger;
		}
		return colliderObject;
	}
	
//	private bool IsReservedComponent(Component component)
//	{
//		if (component is Collider) return true;
//		if (component is Transform) return true;
//		if (component is Rigidbody) return true;
//		return false;
//	}

//	private static void DestroyComponent(Object component)
//	{	
//		if (Application.isPlaying)
//		{
//			Object.Destroy(component);
//		}
//		else
//		{
//			Object.DestroyImmediate(component);
//		}				
//	}
	
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
		for (int i = 0; i <= _colliderPoolList.Count - 1; i++)
		{
			DestroyObject(_colliderPoolList[i]);
		}
		_colliderPoolList.Clear();
		if (_vegetationItemInfoPro.ColliderType == ColliderType.FromPrefab)
		{
			DestroyObject(_sourceColliderObject);
		}
	}	
}
