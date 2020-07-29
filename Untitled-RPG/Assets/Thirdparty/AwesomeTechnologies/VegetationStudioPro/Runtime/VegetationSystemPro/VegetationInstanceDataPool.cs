using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.Utility;
using Unity.Collections;
using UnityEngine;


namespace AwesomeTechnologies.Vegetation
{
	public class VegetationInstanceDataPool
	{
		private readonly List<VegetationInstanceData> _vegetationInstanceDataList = new List<VegetationInstanceData>();
		private int _createCounter;
		public VegetationInstanceData GetObject()
		{
			if (_vegetationInstanceDataList.Count <= 0)
			{
				_createCounter++;
				return new VegetationInstanceData();
			}
		
			VegetationInstanceData vegetationInstanceData = _vegetationInstanceDataList[_vegetationInstanceDataList.Count - 1];
			_vegetationInstanceDataList.RemoveAtSwapBack(_vegetationInstanceDataList.Count -1);
			return vegetationInstanceData;
		}
				
		public void ReturnObject(VegetationInstanceData vegetationInstanceData)
		{
			vegetationInstanceData.CompactMemory();
			_vegetationInstanceDataList.Add(vegetationInstanceData);
		}

		public void Dispose()
		{
			for (int i = 0; i <= _vegetationInstanceDataList.Count - 1; i++)
			{
				_vegetationInstanceDataList[i].Dispose();
			}
			_vegetationInstanceDataList.Clear();
			_createCounter = 0;
		}

		public int GetItemsInPoolCount()
		{
			return _vegetationInstanceDataList.Count;
		}
		
		public int GetItemsCreatedCount()
		{
			return _createCounter;
		}
	}
}

