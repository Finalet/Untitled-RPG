using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AwesomeTechnologies.Utility.MeshTools
{
	public class LODGroupVegetationMeshCombiner : MonoBehaviour
	{
		public GameObject TargetGameObject;
		public bool MergeSubmeshesWitEquialMaterial = true;
		
		void Reset()
		{
			TargetGameObject = gameObject;
		}
	}
}
