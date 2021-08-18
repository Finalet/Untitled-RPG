using UnityEngine;
using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Mesh/Material Lerp")]

    public class MaterialLerp : MonoBehaviour
    {
        public List<InternalMaterialLerp> materials;


        private void Start()
        {
            
        }

        public virtual void Lerp() => StartCoroutine(Lerper());


        
        IEnumerator Lerper()
        {
            //float elapsedTime = 0;

            //var rendererMaterials = new List<Material>();

            //foreach (var item in materials)
            //{
            //    rendererMaterials.Add(item.sharedMaterials[materialIndex]);   //get the Material from the renderer)
            //}
            
         

            //while (elapsedTime <= time.Value)
            //{
            //    float value = curve.Evaluate(elapsedTime / time);

            //    mesh.material.Lerp(rendererMaterial, ToMaterial, value);
            //    elapsedTime += Time.deltaTime;

            //    // Debug.Log("value = " + value.ToString("F2"));
            //    yield return null;
            //}

            //mesh.material.Lerp(rendererMaterial, ToMaterial, curve.Evaluate(1f));
            yield return null;
        }
    }

    [System.Serializable]
    public class InternalMaterialLerp
    {
        public Renderer renderer;
        [CreateScriptableAsset(false)] public MaterialLerpSO materials;
    }
}

