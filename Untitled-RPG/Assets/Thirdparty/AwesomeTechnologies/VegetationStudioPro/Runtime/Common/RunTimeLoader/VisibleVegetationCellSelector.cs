using System;
using System.Collections.Generic;
using AwesomeTechnologies.Utility.Culling;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;
// ReSharper disable DelegateSubtraction

namespace AwesomeTechnologies.Common
{
	public class SelectedVegetationCell
	{
		public readonly VegetationCell VegetationCell;
		public int CameraCount;
		
		private readonly List<VegetationStudioCamera> _vegetationStudioCameraList = new List<VegetationStudioCamera>(); 	
		public SelectedVegetationCell(VegetationCell vegetationCell, VegetationStudioCamera vegetationStudioCamera)
		{
			VegetationCell = vegetationCell;
			CameraCount = 0;

			AddCameraReference(vegetationStudioCamera);
		}

		public void AddCameraReference(VegetationStudioCamera vegetationStudioCamera)		
		{			
			if (!_vegetationStudioCameraList.Contains(vegetationStudioCamera))
			{
				CameraCount++;
				_vegetationStudioCameraList.Add(vegetationStudioCamera);
				//Debug.Log("Never happens");
			}
		}

		public void RemoveCameraReference(VegetationStudioCamera vegetationStudioCamera)
		{
			if (_vegetationStudioCameraList.Contains(vegetationStudioCamera))
			{
				_vegetationStudioCameraList.Remove(vegetationStudioCamera);
				CameraCount--;
			}
			
//			CameraCount--;
		}		
	}
	
	public class VisibleVegetationCellSelector
	{
		private VegetationSystemPro _vegetationSystemPro;
		
		public delegate void MultiOnVegetationCellVisibleDelegate(VegetationCell vegetationCell);
		public MultiOnVegetationCellVisibleDelegate OnVegetationCellVisibleDelegate;

		public delegate void MultiOnVegetationCellInvisibleDelegate(VegetationCell vegetationCell);
		public MultiOnVegetationCellInvisibleDelegate OnVegetationCellInvisibleDelegate;
		
		[NonSerialized]
		public readonly List<SelectedVegetationCell> VisibleSelectorVegetationCellList = new List<SelectedVegetationCell>();
		
//		public VisibleVegetationCellSelector()
//		{
//			
//		}

		public void Init(VegetationSystemPro vegetationSystemPro)
		{
			_vegetationSystemPro = vegetationSystemPro;

			_vegetationSystemPro.OnAddCameraDelegate += OnAddCamera;
			_vegetationSystemPro.OnAddCameraDelegate += OnRemoveCamera;
			
			AddVisibleVegetationCells();
		}

		private void AddVisibleVegetationCells()
		{
			for (int i = 0; i <= _vegetationSystemPro.VegetationStudioCameraList.Count - 1; i++)
			{
				VegetationStudioCamera vegetationStudioCamera = _vegetationSystemPro.VegetationStudioCameraList[i];
				OnAddCamera(vegetationStudioCamera);
			}
		}

		private SelectedVegetationCell GetSelectorVegetationCell(VegetationCell vegetationCell)
		{
			for (int i = 0; i <= VisibleSelectorVegetationCellList.Count - 1; i++)
			{
				if (VisibleSelectorVegetationCellList[i].VegetationCell == vegetationCell)
				{
					return VisibleSelectorVegetationCellList[i];
				}
			}				
			return null;
		}

		private void AddVisisbleCellsFromCamera(VegetationStudioCamera vegetationStudioCamera)
		{						
			JobCullingGroup jobCullingGroup = vegetationStudioCamera.JobCullingGroup;		
			if (jobCullingGroup == null) return;		
			for (int i = 0; i <= jobCullingGroup.VisibleCellIndexList.Length - 1; i++)
			{												
				VegetationCell vegetationCell =
					vegetationStudioCamera.PotentialVisibleCellList[jobCullingGroup.VisibleCellIndexList[i]];

				SelectedVegetationCell selectedVegetationCell = GetSelectorVegetationCell(vegetationCell);

				if (selectedVegetationCell != null)
				{
					selectedVegetationCell.AddCameraReference(vegetationStudioCamera);
				}
				else
				{
					selectedVegetationCell = new SelectedVegetationCell(vegetationCell,vegetationStudioCamera);
					
					VisibleSelectorVegetationCellList.Add(selectedVegetationCell);
					OnVegetationCellVisibleDelegate?.Invoke(selectedVegetationCell.VegetationCell);
				}
			}			
		}

		private void RemoveVisisbleCellsFromCamera(VegetationStudioCamera vegetationStudioCamera)
		{
			JobCullingGroup jobCullingGroup = vegetationStudioCamera.JobCullingGroup;
			if (jobCullingGroup == null) return;

			for (int j = 0; j <= jobCullingGroup.VisibleCellIndexList.Length - 1; j++)
			{							
				VegetationCell vegetationCell =
					vegetationStudioCamera.PotentialVisibleCellList[jobCullingGroup.VisibleCellIndexList[j]];

				SelectedVegetationCell selectedVegetationCell = GetSelectorVegetationCell(vegetationCell);

				if (selectedVegetationCell == null) continue;
				
				selectedVegetationCell.RemoveCameraReference(vegetationStudioCamera);
				if (selectedVegetationCell.CameraCount == 0)
				{
					VisibleSelectorVegetationCellList.Remove(selectedVegetationCell);
					OnVegetationCellInvisibleDelegate?.Invoke(selectedVegetationCell.VegetationCell);
				}
			}			
		}

		private void OnAddCamera(VegetationStudioCamera vegetationStudioCamera)
		{	
			vegetationStudioCamera.OnPotentialCellInvisibleDelegate += OnVegetationCellInvisible;			
			vegetationStudioCamera.OnVegetationCellDistanceBandChangeDelegate += OnVegetationCellDistanceBandChanged;
			AddVisisbleCellsFromCamera(vegetationStudioCamera);
		}

		private void OnRemoveCamera(VegetationStudioCamera vegetationStudioCamera)
		{	
			vegetationStudioCamera.OnPotentialCellInvisibleDelegate -= OnVegetationCellInvisible;
			vegetationStudioCamera.OnVegetationCellDistanceBandChangeDelegate -= OnVegetationCellDistanceBandChanged;
			RemoveVisisbleCellsFromCamera(vegetationStudioCamera);
		}

		public void DrawDebugGizmos()
		{		
			for (int i = 0; i <= VisibleSelectorVegetationCellList.Count - 1; i++)
			{
				VegetationCell vegetationCell = VisibleSelectorVegetationCellList[i].VegetationCell;
				Gizmos.color = SelectVegetationCellGizmoColor(VisibleSelectorVegetationCellList[i].CameraCount);
				Gizmos.DrawWireCube(vegetationCell.VegetationCellBounds.center,vegetationCell.VegetationCellBounds.size);							
			}
		}

		private static Color SelectVegetationCellGizmoColor(int count)
		{
			switch (count)
			{
					case 0:
						return Color.black;						
					case 1:
						return Color.white;
					case 2:
						return Color.yellow;
					case 3:
						return Color.red;
					default:
						return Color.green;
			}
		}

		public void Dispose()
		{			
			_vegetationSystemPro.OnAddCameraDelegate -= OnAddCamera;
			_vegetationSystemPro.OnAddCameraDelegate -= OnRemoveCamera;

			for (int i = 0; i <= _vegetationSystemPro.VegetationStudioCameraList.Count - 1; i++)
			{
				OnRemoveCamera(_vegetationSystemPro.VegetationStudioCameraList[i]);
			}			
		}

		private void OnVegetationCellDistanceBandChanged(VegetationStudioCamera vegetationStudioCamera,
			VegetationCell vegetationCell, int currentDistanceBand, int previousDistanceBand)
		{
			if (currentDistanceBand == 0)
			{
				OnVegetationCellVisible(vegetationStudioCamera, vegetationCell);
			}
			else if (previousDistanceBand == 0)
			{
				OnVegetationCellInvisible(vegetationStudioCamera, vegetationCell);
			}			
		}

		private void OnVegetationCellVisible(VegetationStudioCamera vegetationStudioCamera,
			VegetationCell vegetationCell)
		{		
			SelectedVegetationCell selectedVegetationCell = GetSelectorVegetationCell(vegetationCell);
			if (selectedVegetationCell != null)
			{
				selectedVegetationCell.AddCameraReference(vegetationStudioCamera);
			}
			else
			{
				selectedVegetationCell = new SelectedVegetationCell(vegetationCell,vegetationStudioCamera);				
				VisibleSelectorVegetationCellList.Add(selectedVegetationCell);
				OnVegetationCellVisibleDelegate?.Invoke(selectedVegetationCell.VegetationCell);
			}
		}
		
		private void OnVegetationCellInvisible(VegetationStudioCamera vegetationStudioCamera,
			VegetationCell vegetationCell)
		{		
			SelectedVegetationCell selectedVegetationCell = GetSelectorVegetationCell(vegetationCell);
			if (selectedVegetationCell == null) return;
				
			selectedVegetationCell.RemoveCameraReference(vegetationStudioCamera);
			if (selectedVegetationCell.CameraCount == 0)
			{
				VisibleSelectorVegetationCellList.Remove(selectedVegetationCell);
				OnVegetationCellInvisibleDelegate?.Invoke(selectedVegetationCell.VegetationCell);
			}			
		}					
	}	
}


