using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AwesomeTechnologies.Extensions;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Shaders
{
    public class ShaderSelector
    {
        public static IShaderController GetShaderControler(string shaderName)
        {
            var interfaceType = typeof(IShaderController);
            var shaderControlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetLoadableTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(Activator.CreateInstance);

            foreach (var shaderControler in shaderControlerTypes)
            {
                IShaderController shaderControllerInterface = (IShaderController)shaderControler;
                if (shaderControllerInterface == null) continue;

                if (shaderControllerInterface.MatchShader(shaderName))
                {
                    return shaderControllerInterface;
                }
            }
            return new DefaultShaderController();
        }

        public static string GetShaderName(GameObject prefab)
        {
            GameObject selectedVegetationModel = MeshUtils.SelectMeshObject(prefab, LODLevel.LOD0);
            MeshRenderer meshrenderer = selectedVegetationModel.GetComponentInChildren<MeshRenderer>();
            if (!meshrenderer || !meshrenderer.sharedMaterial) return "";

            Shader shader = meshrenderer.sharedMaterial.shader;
            return shader.name;
        }

        public static Material GetVegetationItemMaterial(GameObject prefab)
        {
            GameObject selectedVegetationModel = MeshUtils.SelectMeshObject(prefab, LODLevel.LOD0);
            MeshRenderer meshrenderer = selectedVegetationModel.GetComponentInChildren<MeshRenderer>();
            if (!meshrenderer || !meshrenderer.sharedMaterial) return null;
            return meshrenderer.sharedMaterial;
        }

        public static Material[] GetVegetationItemMaterials(GameObject prefab)
        {
            GameObject selectedVegetationModel = MeshUtils.SelectMeshObject(prefab, LODLevel.LOD0);
            MeshRenderer meshrenderer = selectedVegetationModel.GetComponentInChildren<MeshRenderer>();
            if (!meshrenderer || !meshrenderer.sharedMaterial) return null;
            return meshrenderer.sharedMaterials;
        }
    }
}
