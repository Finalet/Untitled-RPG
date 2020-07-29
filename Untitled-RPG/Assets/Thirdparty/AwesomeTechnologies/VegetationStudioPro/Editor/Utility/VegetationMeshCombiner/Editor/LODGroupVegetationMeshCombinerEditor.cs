using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Utility.MeshTools
{
	[CustomEditor(typeof(LODGroupVegetationMeshCombiner))]
	public class LODGroupVegetationMeshCombinerEditor : VegetationStudioProBaseEditor
	{
		private LODGroupVegetationMeshCombiner _lodGroupVegetationMeshCombiner;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			OverrideLogoTextureName = "Banner_VegetationMeshCombiner";
			LargeLogo = false;
			
			_lodGroupVegetationMeshCombiner = (LODGroupVegetationMeshCombiner) target;
			_lodGroupVegetationMeshCombiner.TargetGameObject = EditorGUILayout.ObjectField("Root GameObject", _lodGroupVegetationMeshCombiner.TargetGameObject, typeof(GameObject),true) as GameObject;
			EditorGUILayout.HelpBox("This tool will merge a LODGroup with multiple meshes with a single mesh and materials into a single mesh with submeshes.", MessageType.Info);
			EditorGUILayout.HelpBox("Add all meshes as children to a GameObject and assign it as root gameobject. A dialog will ask you for a location to save the merged mesh. A new Gameobject will be created in the scene with the merged mesh and materials set up.", MessageType.Info);

            
			if (!_lodGroupVegetationMeshCombiner.TargetGameObject)
			{
				EditorGUILayout.HelpBox("You need to assign a root GameObject", MessageType.Warning);
			}
			
			if (_lodGroupVegetationMeshCombiner.TargetGameObject!= null)
			{
					LODGroup lodGroup = _lodGroupVegetationMeshCombiner.TargetGameObject
						.GetComponentInChildren<LODGroup>();
				if (lodGroup == null)
				{
					EditorGUILayout.HelpBox("The GameObject does not have a LODGroup", MessageType.Warning);
				}			
			}
			
			_lodGroupVegetationMeshCombiner.MergeSubmeshesWitEquialMaterial = EditorGUILayout.Toggle("Merge submeshes",
				_lodGroupVegetationMeshCombiner.MergeSubmeshesWitEquialMaterial);
			EditorGUILayout.HelpBox("When enabled this will merge submeshes using the same material.", MessageType.Info);
    
			if (GUILayout.Button("Generate merged LODGroup from single material meshes"))
			{
				if (_lodGroupVegetationMeshCombiner.TargetGameObject)
				{
					CreateMergedLODGroupFromSingleMaterialMeshes();
				}
			}
			
			if (GUILayout.Button("Combine submeshes with identical material"))
			{
				if (_lodGroupVegetationMeshCombiner.TargetGameObject)
				{
					CreateMergedLODGroup();
				}
			}
		}

		
		void CreateMergedLODGroupFromSingleMaterialMeshes()
		{
			string path = EditorUtility.SaveFilePanelInProject("Save merged mesh", "", "",
				"Please enter a file name to save the merged mesh to");
			if (path.Length != 0)
			{
				if (_lodGroupVegetationMeshCombiner.TargetGameObject == null) return;

				LODGroup lodGroup = _lodGroupVegetationMeshCombiner.TargetGameObject.GetComponentInChildren<LODGroup>();
				GameObject sourceObject = _lodGroupVegetationMeshCombiner.TargetGameObject;
				
				Vector3 targetPosition = sourceObject.transform.position;
				Quaternion targetRotation = sourceObject.transform.rotation;
				Vector3 targetScale = sourceObject.transform.localScale;

				sourceObject.transform.position = new Vector3(0, 0, 0);
				sourceObject.transform.rotation = Quaternion.identity;
				sourceObject.transform.localScale = Vector3.one;				
				
				if (lodGroup == null) return;

				GameObject mergedLODGroupObject = new GameObject("Merged_" + sourceObject.name);
				LODGroup newLODGroup = mergedLODGroupObject.AddComponent<LODGroup>();
				List<CombineInstance> combineInstanceList = new List<CombineInstance>();

				LOD[] lods = lodGroup.GetLODs();

				// ReSharper disable once InconsistentNaming
				LOD[] newLODs = lodGroup.GetLODs();
				for (int i = 0; i <= lods.Length - 1; i++)
				{
					combineInstanceList.Clear();
					LOD lod = lods[i];

					if (lod.renderers.Length > 0)
					{
						GameObject lodObject = new GameObject(sourceObject.name + "_LOD" + i.ToString());
						lodObject.transform.SetParent(mergedLODGroupObject.transform);
						MeshRenderer meshRenderer = lodObject.AddComponent<MeshRenderer>();
						MeshFilter meshFilter = lodObject.AddComponent<MeshFilter>();
						Renderer[] newRenderers = new Renderer[1];
						newRenderers[0] = meshRenderer;
						newLODs[i].renderers = newRenderers;


						List<Material> materialList = new List<Material>();
						for (int j = 0; j <= lod.renderers.Length - 1; j++)
						{
							MeshFilter tempMeshFilter = lod.renderers[j].gameObject.GetComponent<MeshFilter>();
							if (!tempMeshFilter) continue;

							CombineInstance combineInstance = new CombineInstance
							{
								mesh = tempMeshFilter.sharedMesh, transform = meshFilter.transform.localToWorldMatrix
							};
							combineInstanceList.Add(combineInstance);
							materialList.Add(lod.renderers[j].sharedMaterial);
						}

						Mesh mesh = new Mesh();
						Material[] materials = materialList.ToArray();
						mesh.CombineMeshes(combineInstanceList.ToArray(), false, true);

						if (_lodGroupVegetationMeshCombiner.MergeSubmeshesWitEquialMaterial)
						{
							SubmeshCombiner submeshCombiner = new SubmeshCombiner();
							for (int j = 0; j <= mesh.subMeshCount - 1; j++)
							{
								submeshCombiner.AddSubmesh(mesh.GetIndices(j), materials[j]);
							}

							submeshCombiner.UpdateMesh(mesh);
							materials = submeshCombiner.GetMaterials();
						}

						meshFilter.sharedMesh = mesh;
						AssetDatabase.CreateAsset(meshFilter.sharedMesh, path + "_LOD" + i.ToString() + ".asset");					
						meshRenderer.materials = materials;
					}
				}

				newLODGroup.SetLODs(newLODs);
				
				sourceObject.transform.position = targetPosition;
				sourceObject.transform.rotation = targetRotation;
				sourceObject.transform.localScale = targetScale;
			}
		}
		
		void CreateMergedLODGroup()
		{
			string path = EditorUtility.SaveFilePanelInProject("Save merged mesh", "", "",
				"Please enter a file name to save the merged mesh to");
			if (path.Length != 0)
			{
				if (_lodGroupVegetationMeshCombiner.TargetGameObject == null) return;

				LODGroup lodGroup = _lodGroupVegetationMeshCombiner.TargetGameObject.GetComponentInChildren<LODGroup>();
				GameObject sourceObject = _lodGroupVegetationMeshCombiner.TargetGameObject;
				
				Vector3 targetPosition = sourceObject.transform.position;
				Quaternion targetRotation = sourceObject.transform.rotation;
				Vector3 targetScale = sourceObject.transform.localScale;

				sourceObject.transform.position = new Vector3(0, 0, 0);
				sourceObject.transform.rotation = Quaternion.identity;
				sourceObject.transform.localScale = Vector3.one;				
				
				if (lodGroup == null) return;

				GameObject mergedLODGroupObject = new GameObject("Merged_" + sourceObject.name);
				LODGroup newLODGroup = mergedLODGroupObject.AddComponent<LODGroup>();
				LOD[] lods = lodGroup.GetLODs();

				// ReSharper disable once InconsistentNaming
				LOD[] newLODs = lodGroup.GetLODs();
				
				for (int i = 0; i <= lods.Length - 1; i++)
				{
					LOD lod = lods[i];
					
					if (lod.renderers.Length > 0)
					{
						GameObject lodObject = new GameObject(sourceObject.name + "_LOD" + i.ToString());
						lodObject.transform.SetParent(mergedLODGroupObject.transform);
						MeshRenderer meshRenderer = lodObject.AddComponent<MeshRenderer>();
						MeshFilter meshFilter = lodObject.AddComponent<MeshFilter>();

						Renderer renderer = lod.renderers[0];
						MeshFilter tempMeshFilter = lod.renderers[0].gameObject.GetComponent<MeshFilter>();
						
						Renderer[] newRenderers = new Renderer[1];
						newRenderers[0] = meshRenderer;
						newLODs[i].renderers = newRenderers;
						newLODs[i].screenRelativeTransitionHeight = (float)1 / (i + 1);
						
						Mesh mesh = Instantiate(tempMeshFilter.sharedMesh);
						Material[] materials = renderer.sharedMaterials;
						
						SubmeshCombiner submeshCombiner = new SubmeshCombiner();
						for (int j = 0; j <= mesh.subMeshCount - 1; j++)
						{
							submeshCombiner.AddSubmesh(mesh.GetIndices(j), materials[j]);
						}

						submeshCombiner.UpdateMesh(mesh);
						materials = submeshCombiner.GetMaterials();

						meshFilter.sharedMesh = mesh;
						AssetDatabase.CreateAsset(meshFilter.sharedMesh, path + "_LOD" + i.ToString() + ".asset");					
						meshRenderer.materials = materials;
					}
				}

				newLODGroup.SetLODs(newLODs);
				
				sourceObject.transform.position = targetPosition;
				sourceObject.transform.rotation = targetRotation;
				sourceObject.transform.localScale = targetScale;
			}
		}
	}
}
