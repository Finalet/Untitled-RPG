using System;
using System.Collections.Generic;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
// ReSharper disable DelegateSubtraction

namespace AwesomeTechnologies.Utility
{
	public struct ItemSelectorInstanceInfo
	{
		public int VegetationCellIndex;
		public int VegetationCellItemIndex;
		public Vector3 Position;
		public Quaternion Rotation;
		public Vector3 Scale;
		public int Visible;
		public int LastVisible;
		public int Remove;
	}

	public class VegetationItemSelector
	{
		private readonly VisibleVegetationCellSelector _visibleVegetationCellSelector;
		[NonSerialized]
		public readonly List<VegetationCell> ReadyToLoadVegetationCellList = new List<VegetationCell>();		
		[NonSerialized]
		public readonly List<VegetationCell> ReadyToUnloadVegetationCellList = new List<VegetationCell>();						
		[NonSerialized]
		public readonly List<VegetationCell> LoadedVegetationCellList = new List<VegetationCell>();
		public NativeList<ItemSelectorInstanceInfo> InstanceList;

		public delegate void MultiOnVegetationItemVisibilityChangeDelegate(ItemSelectorInstanceInfo itemSelectorInstanceInfo,VegetationItemIndexes vegetationItemIndexes,string vegetationItemID);
		public MultiOnVegetationItemVisibilityChangeDelegate OnVegetationItemVisibleDelegate;
		public MultiOnVegetationItemVisibilityChangeDelegate OnVegetationItemInvisibleDelegate;
		
		public delegate void MultiOnVegetationCellVisibilityChangeDelegate(int vegetationCellIndex);
		public MultiOnVegetationCellVisibilityChangeDelegate OnVegetationCellInvisibleDelegate;
        				
		private NativeList<int> _removeVegetationCellIndexList; 
		private NativeList<int> _visibilityChangedIndexList;
		private readonly VegetationSystemPro _vegetationSystemPro;
		public readonly string VegetationItemID;
		private readonly VegetationItemIndexes _vegetationItemIndexes;
		public float CullingDistance = 50f;

		private readonly bool _useSpawnChance;
		private readonly float _spawnChance;
		private readonly int _spawnSeed;
			
		public VegetationItemSelector(VisibleVegetationCellSelector visibleVegetationCellSelector, VegetationSystemPro vegetationSystemPro, VegetationItemInfoPro vegetationItemInfoPro, bool useSpawnChance, float spawnChance, int spawnSeed)
		{
			_useSpawnChance = useSpawnChance;
			_spawnChance = spawnChance;
			_spawnSeed = spawnSeed;
			
			_visibleVegetationCellSelector = visibleVegetationCellSelector;
			_vegetationSystemPro = vegetationSystemPro;

			VegetationItemID = vegetationItemInfoPro.VegetationItemID;
			_vegetationItemIndexes = _vegetationSystemPro.GetVegetationItemIndexes(VegetationItemID);
			
			_visibleVegetationCellSelector.OnVegetationCellVisibleDelegate += OnVegetationCellVisible;
			_visibleVegetationCellSelector.OnVegetationCellInvisibleDelegate += OnVegetationCellInvisible;

			_vegetationSystemPro.OnVegetationCellLoaded += OnVegetationCellLoaded;
			
			InstanceList = new NativeList<ItemSelectorInstanceInfo>(512,Allocator.Persistent);	
			_removeVegetationCellIndexList = new NativeList<int>(64,Allocator.Persistent);
			_visibilityChangedIndexList = new NativeList<int>(512,Allocator.Persistent);
		}

		public void Dispose()
		{
			_visibleVegetationCellSelector.OnVegetationCellVisibleDelegate -= OnVegetationCellVisible;
			_visibleVegetationCellSelector.OnVegetationCellInvisibleDelegate -= OnVegetationCellInvisible;
			_vegetationSystemPro.OnVegetationCellLoaded -= OnVegetationCellLoaded;
			
			if (InstanceList.IsCreated) InstanceList.Dispose();
			if (_removeVegetationCellIndexList.IsCreated) _removeVegetationCellIndexList.Dispose();
			if (_visibilityChangedIndexList.IsCreated) _visibilityChangedIndexList.Dispose();
		}

		public void OnVegetationCellLoaded(VegetationCell vegetationCell)
		{
			if (LoadedVegetationCellList.Contains(vegetationCell))
			{
				if (!ReadyToUnloadVegetationCellList.Contains(vegetationCell))
				{
					ReadyToUnloadVegetationCellList.Add(vegetationCell);
				}
				
				if (!ReadyToLoadVegetationCellList.Contains(vegetationCell))
				{
					ReadyToLoadVegetationCellList.Add(vegetationCell);
				}
			}
		}
		
		public void OnVegetationCellVisible(VegetationCell vegetationCell)
		{			
			ReadyToLoadVegetationCellList.Add(vegetationCell);
		}		
		
		public void OnVegetationCellInvisible(VegetationCell vegetationCell)
		{		
			ReadyToUnloadVegetationCellList.Add(vegetationCell);
		}

		public void RefreshVegetationCell(VegetationCell vegetationCell)
		{
			if (!LoadedVegetationCellList.Contains(vegetationCell)) return;
			
			if (!ReadyToUnloadVegetationCellList.Contains(vegetationCell))
			{
				ReadyToUnloadVegetationCellList.Add(vegetationCell);
			}

			if (!ReadyToLoadVegetationCellList.Contains(vegetationCell))
			{
				ReadyToLoadVegetationCellList.Add(vegetationCell);
			}
		}

		public void RefreshAllVegetationCells()
		{
			for (int i = 0; i <= LoadedVegetationCellList.Count - 1; i++)
			{
				VegetationCell vegetationCell = LoadedVegetationCellList[i];
				
				if (!ReadyToUnloadVegetationCellList.Contains(vegetationCell))
				{
					ReadyToUnloadVegetationCellList.Add(vegetationCell);
				}

				if (!ReadyToLoadVegetationCellList.Contains(vegetationCell))
				{
					ReadyToLoadVegetationCellList.Add(vegetationCell);
				}
			}
		}
		
		public JobHandle ProcessVisibleCells(JobHandle processCullingHandle)
		{
			processCullingHandle = LoadVisibleCells(processCullingHandle);
			return processCullingHandle;
		}
		
		public JobHandle ProcessInvisibleCells(JobHandle processCullingHandle)
		{
			processCullingHandle = RemoveInvisibleCells(processCullingHandle);
			return processCullingHandle;
		}
					
		public JobHandle ProcessCulling(JobHandle processCullingHandle)
		{									
			ResetVisibilityJob resetVisibilityJob = new ResetVisibilityJob
			{
				InstanceList = InstanceList
			};
			processCullingHandle = resetVisibilityJob.Schedule(processCullingHandle);
			
			for (int i = 0; i <= _vegetationSystemPro.VegetationStudioCameraList.Count - 1; i++)
			{
				VegetationStudioCamera vegetationStudioCamera = _vegetationSystemPro.VegetationStudioCameraList[i];
				
				
				if (!vegetationStudioCamera.Enabled) continue;
				if (vegetationStudioCamera.SelectedCamera == null) continue;
				
				DistanceCullingJob distanceCullingJob = new DistanceCullingJob
				{
					InstanceList = InstanceList,
					CameraPosition = vegetationStudioCamera.SelectedCamera.transform.position - _vegetationSystemPro.FloatingOriginOffset,
					CullingDistance = CullingDistance,
				};			
				processCullingHandle = distanceCullingJob.Schedule(processCullingHandle);
			}

			_visibilityChangedIndexList.Clear();

			VisibilityChangedFilterManualJob visibilityChangedFilterManualJob =
				new VisibilityChangedFilterManualJob
				{
					InstanceList = InstanceList, VisibilityChangedIndexList = _visibilityChangedIndexList
				};
			processCullingHandle = visibilityChangedFilterManualJob.Schedule(processCullingHandle);
			return processCullingHandle;
		}

		public void ProcessEvents()
		{
			for (int i = 0; i <= _visibilityChangedIndexList.Length - 1; i++)
			{
				ItemSelectorInstanceInfo itemSelectorInstanceInfo = InstanceList[_visibilityChangedIndexList[i]];
				if (itemSelectorInstanceInfo.Visible == 1)
				{
					OnVegetationItemVisibleDelegate?.Invoke(itemSelectorInstanceInfo,_vegetationItemIndexes,VegetationItemID);
				}
				else
				{
					OnVegetationItemInvisibleDelegate?.Invoke(itemSelectorInstanceInfo,_vegetationItemIndexes,VegetationItemID);
				}				
			}
		}
			
		JobHandle LoadVisibleCells(JobHandle processCullingHandle)
		{
			for (int i = 0; i <= ReadyToLoadVegetationCellList.Count - 1; i++)
			{				
				VegetationCell vegetationCell = ReadyToLoadVegetationCellList[i];
				if (!vegetationCell.Prepared)
				{
					Debug.Log("Unprepared cell" +  vegetationCell.Index);
					continue;
				}
				//NativeList<MatrixInstance> matrixInstanceArray = vegetationCell.GetVegetationPackageInstancesList(_vegetationItemIndexes.VegetationPackageIndex,_vegetationItemIndexes.VegetationItemIndex);
					NativeList<MatrixInstance> matrixInstanceArray = vegetationCell
					.VegetationPackageInstancesList[_vegetationItemIndexes.VegetationPackageIndex]
					.VegetationItemMatrixList[_vegetationItemIndexes.VegetationItemIndex];

//				if (!matrixInstanceArray.IsCreated)
//				{
//					Debug.Log("not created");
//					continue;
//				}
				
				if (_useSpawnChance)
				{
					AddInstancesSpawnChanceJob addInstancesSpawnChanceJob = new AddInstancesSpawnChanceJob
					{
						InstanceList = InstanceList,
						RandomNumbers = _vegetationSystemPro.VegetationCellSpawner.RandomNumbers,
						SpawnChance = _spawnChance,
						MatrixInstanceList = matrixInstanceArray,
						RandomNumberIndex = vegetationCell.Index + _spawnSeed,
						VegetationCellIndex = vegetationCell.Index					
					};
					processCullingHandle = addInstancesSpawnChanceJob.Schedule(processCullingHandle);	
				}
				else
				{
					AddInstancesJob addInstancesJob = new AddInstancesJob
					{
						InstanceList = InstanceList,
						MatrixInstanceList = matrixInstanceArray,
						VegetationCellIndex = vegetationCell.Index
					};												

					processCullingHandle = addInstancesJob.Schedule(processCullingHandle);			
				}				
				LoadedVegetationCellList.Add(vegetationCell);
			}

			ReadyToLoadVegetationCellList.Clear();

			return processCullingHandle;
		}

		JobHandle RemoveInvisibleCells(JobHandle processCullingHandle)
		{			
			_removeVegetationCellIndexList.Clear();
			bool needsRemoval = false;
			
			for (int i = 0; i <= ReadyToUnloadVegetationCellList.Count - 1; i++)
			{				
				_removeVegetationCellIndexList.Add(ReadyToUnloadVegetationCellList[i].Index);
				
				VegetationCell vegetationCell = ReadyToUnloadVegetationCellList[i];
				
				// ReSharper disable once InlineOutVariableDeclaration
				int index = LoadedVegetationCellList.IndexOf(ReadyToUnloadVegetationCellList[i]);
				if (index > -1)
				{
					LoadedVegetationCellList.RemoveAtSwapBack(index);	
				}

				OnVegetationCellInvisibleDelegate?.Invoke(vegetationCell.Index);
				needsRemoval = true;
			}
			ReadyToUnloadVegetationCellList.Clear();
			
			if (needsRemoval)
			{
				FlagInstancesForRemovalJob flagInstancesForRemovalJob =
					new FlagInstancesForRemovalJob
					{
						InstanceList = InstanceList,
						RemoveCellIndexList = _removeVegetationCellIndexList
					};
						
				processCullingHandle = flagInstancesForRemovalJob.Schedule(processCullingHandle);

				RemoveInstancesJob removeInstancesJob = new RemoveInstancesJob
				{
					InstanceList = InstanceList
				};
		
				processCullingHandle = removeInstancesJob.Schedule(processCullingHandle);
			}
			return processCullingHandle;
		}				
	}

	[BurstCompile]
	public struct DistanceCullingJob : IJob
	{
		public NativeList<ItemSelectorInstanceInfo> InstanceList;
		public float3 CameraPosition;
		public float CullingDistance;
		
		public void Execute()
		{
			for (int i = 0; i <= InstanceList.Length - 1; i++)
			{
				ItemSelectorInstanceInfo itemSelectorInstanceInfo = InstanceList[i];			
				float distance = math.distance(itemSelectorInstanceInfo.Position, CameraPosition);

				if (distance <= CullingDistance)
				{
					itemSelectorInstanceInfo.Visible = 1;
				}
				else
				{
					itemSelectorInstanceInfo.Visible = -1;
				}
			
				InstanceList[i] = itemSelectorInstanceInfo;
			}
		}
	} 	
	
	[BurstCompile]
	public struct ResetVisibilityJob : IJob
	{
		public NativeList<ItemSelectorInstanceInfo> InstanceList;
		public void Execute()
		{
			for (int i = 0; i <= InstanceList.Length - 1; i++)
			{
				ItemSelectorInstanceInfo itemSelectorInstanceInfo = InstanceList[i];
				itemSelectorInstanceInfo.LastVisible = itemSelectorInstanceInfo.Visible;
				itemSelectorInstanceInfo.Visible = 0;			
				InstanceList[i] = itemSelectorInstanceInfo;
			}
		}
	} 		
	
	[BurstCompile]
	public struct VisibilityChangedFilterManualJob : IJob
	{
		[ReadOnly]
		public NativeList<ItemSelectorInstanceInfo> InstanceList;
		public NativeList<int> VisibilityChangedIndexList;
		public void Execute()
		{
			for (int i = 0; i <= InstanceList.Length - 1; i++)
			{
				ItemSelectorInstanceInfo itemSelectorInstanceInfo = InstanceList[i];
				if (itemSelectorInstanceInfo.Visible != itemSelectorInstanceInfo.LastVisible)
				{
					VisibilityChangedIndexList.Add(i);
				}
			}
		}
	} 	
	
	[BurstCompile]
	public struct RemoveInstancesJob : IJob
	{
		public NativeList<ItemSelectorInstanceInfo> InstanceList;
		
		public void Execute()
		{
			for (int i = InstanceList.Length - 1; i >= 0; i--)
			{
				if (InstanceList[i].Remove == 1)
				{
					InstanceList.RemoveAtSwapBack(i);
				}
			}
		}
	} 
	
	[BurstCompile]
	public struct AddInstancesSpawnChanceJob : IJob
	{
		public NativeList<ItemSelectorInstanceInfo> InstanceList;
		public NativeList<MatrixInstance> MatrixInstanceList;
		[ReadOnly]
		public NativeArray<float> RandomNumbers;

		public int RandomNumberIndex;
		public float SpawnChance;		
		public int VegetationCellIndex;
		public void Execute()
		{
			for (int i = MatrixInstanceList.Length - 1; i >= 0; i--)
			{
				MatrixInstance matrixInstance = MatrixInstanceList[i];

				if (!RandomCutoff(SpawnChance, RandomNumberIndex))
				{
					ItemSelectorInstanceInfo itemSelectorInstanceInfo = new ItemSelectorInstanceInfo
					{
						VegetationCellIndex = VegetationCellIndex,
						VegetationCellItemIndex = i,
						Position = ExtractTranslationFromMatrix(matrixInstance.Matrix),
						Scale = ExtractScaleFromMatrix(matrixInstance.Matrix),
						Rotation = ExtractRotationFromMatrix(matrixInstance.Matrix),
						LastVisible = -1,
						Visible = -1
					
					};

					InstanceList.Add(itemSelectorInstanceInfo);
				}
				RandomNumberIndex++;
			}
		}

		private bool RandomCutoff(float value, int randomNumberIndex)
		{
			float randomNumber = RandomRange(randomNumberIndex, 0, 1);
			return !(value > randomNumber);
		}

		public float RandomRange(int randomNumberIndex, float min, float max)
		{
			while (randomNumberIndex > 9999)
				randomNumberIndex = randomNumberIndex - 10000;

			return Mathf.Lerp(min, max, RandomNumbers[randomNumberIndex]);
		}
		
		private static float3 ExtractTranslationFromMatrix(Matrix4x4 matrix)
		{
			float3 translate;
			translate.x = matrix.m03;
			translate.y = matrix.m13;
			translate.z = matrix.m23;
			return translate;
		}

		private static Quaternion ExtractRotationFromMatrix(Matrix4x4 matrix)
		{
			Vector3 forward;
			forward.x = matrix.m02;
			forward.y = matrix.m12;
			forward.z = matrix.m22;

			if (forward == Vector3.zero) return Quaternion.identity;

			Vector3 upwards;
			upwards.x = matrix.m01;
			upwards.y = matrix.m11;
			upwards.z = matrix.m21;

			return Quaternion.LookRotation(forward, upwards);
		}

		private static float3 ExtractScaleFromMatrix(Matrix4x4 matrix)
		{
			return new float3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
		}
	} 	
	
	[BurstCompile]
	public struct AddInstancesJob : IJob
	{
		public NativeList<ItemSelectorInstanceInfo> InstanceList;
		public NativeList<MatrixInstance> MatrixInstanceList;
		public int VegetationCellIndex;
		public void Execute()
		{
			for (int i = MatrixInstanceList.Length - 1; i >= 0; i--)
			{
				MatrixInstance matrixInstance = MatrixInstanceList[i];

				ItemSelectorInstanceInfo itemSelectorInstanceInfo = new ItemSelectorInstanceInfo
				{
					VegetationCellIndex = VegetationCellIndex,
					VegetationCellItemIndex = i,
					Position = ExtractTranslationFromMatrix(matrixInstance.Matrix),
					Scale = ExtractScaleFromMatrix(matrixInstance.Matrix),
					Rotation = ExtractRotationFromMatrix(matrixInstance.Matrix),
					LastVisible = -1,
					Visible = -1
					
				};

				InstanceList.Add(itemSelectorInstanceInfo);
			}
		}

		private static float3 ExtractTranslationFromMatrix(Matrix4x4 matrix)
		{
			float3 translate;
			translate.x = matrix.m03;
			translate.y = matrix.m13;
			translate.z = matrix.m23;
			return translate;
		}

		private static Quaternion ExtractRotationFromMatrix(Matrix4x4 matrix)
		{
			Vector3 forward;
			forward.x = matrix.m02;
			forward.y = matrix.m12;
			forward.z = matrix.m22;

			if (forward == Vector3.zero) return Quaternion.identity;

			Vector3 upwards;
			upwards.x = matrix.m01;
			upwards.y = matrix.m11;
			upwards.z = matrix.m21;

			return Quaternion.LookRotation(forward, upwards);
		}

		private static float3 ExtractScaleFromMatrix(Matrix4x4 matrix)
		{
			return new float3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
		}
	} 	
	
	[BurstCompile]
	public struct FlagInstancesForRemovalJob : IJob
	{		
		public NativeList<ItemSelectorInstanceInfo> InstanceList;
		[ReadOnly]
		public NativeList<int> RemoveCellIndexList;

		public void Execute()
		{
			for (int j = 0; j <= InstanceList.Length - 1; j++)
			{
				ItemSelectorInstanceInfo instanceInfo = InstanceList[j];
				for (int i = 0; i <= RemoveCellIndexList.Length - 1; i++)
				{
					if (instanceInfo.VegetationCellIndex == RemoveCellIndexList[i])
					{
						instanceInfo.Remove = 1;
						InstanceList[j] = instanceInfo;
						break;
					}
				}
			}
		}
	}	
}

