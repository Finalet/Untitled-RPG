using UnityEngine;
using MalbersAnimations.Scriptables;
using System.Collections;

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Extras/Material Lerp", order = 2100)]
    public class MaterialLerpSO : ScriptableCoroutine
    {
        [Tooltip("Next material to lerp to")]
        public Material ToMaterial;
        [Tooltip("Index of the Material")]
        public int materialIndex = 0;
        [Tooltip("Time to lerp the materials")]
        public FloatReference time = new FloatReference(1f);
        [Tooltip("Curve to apply to the lerping")]
        public AnimationCurve curve = new AnimationCurve(MTools.DefaultCurve);

        public virtual void Lerp(Renderer mesh)
        {
            StartCoroutine(mesh, Lerper(mesh));
        }


        public virtual void LerpForever(Renderer mesh)
        { 
            StartCoroutine(mesh, LerperForever(mesh));
        }


        IEnumerator Lerper(Renderer mesh)
        {
            float elapsedTime = 0;

            var rendererMaterial = mesh.sharedMaterials[materialIndex];   //get the Material from the renderer

            while (elapsedTime <= time.Value)
            {
                float value = curve.Evaluate(elapsedTime / time);

                mesh.material.Lerp(rendererMaterial, ToMaterial, value);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            mesh.material.Lerp(rendererMaterial, ToMaterial, curve.Evaluate(curve.keys[curve.keys.Length - 1].time));

            yield return null;

            Stop(mesh);
        }


        IEnumerator LerperForever(Renderer mesh)
        {
            float elapsedTime = 0;

            var rendererMaterial = mesh.sharedMaterials[materialIndex];   //get the Material from the renderer

            while (true)
            {
                float value = curve.Evaluate((elapsedTime / time) % 1);
                mesh.material.Lerp(rendererMaterial, ToMaterial, value);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}

