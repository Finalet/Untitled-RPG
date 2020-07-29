using System;
using UnityEngine;


namespace AwesomeTechnologies.Utility
{
    public class MaterialUtility
    {
        public static void EnableMaterialInstancing(GameObject go)
        {
            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                Material[] materials = meshRenderer.sharedMaterials;
                foreach (Material material in materials)
                {
                    if (!material.enableInstancing)
                    {
                        try
                        {
                            material.enableInstancing = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }                      
                    }
                }
            }
        }
    }
}

