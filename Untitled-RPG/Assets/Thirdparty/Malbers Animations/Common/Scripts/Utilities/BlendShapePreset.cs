using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/BlendShape Preset")]
    public class BlendShapePreset : ScriptableObject
    {
        [Header("Smooth BlendShapes")]
        public FloatReference BlendTime = new FloatReference(1.5f);
        public AnimationCurve BlendCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });


        private MonoBehaviour coroutine;

        [Space, Header("Blend Shapes Weights")]
        public float[] blendShapes;

        public void Load(SkinnedMeshRenderer mesh)
        {
            int Length = Mathf.Min(mesh.sharedMesh.blendShapeCount, blendShapes.Length);

            for (int i = 0; i < Length; i++)
            {
                mesh.SetBlendShapeWeight(i, blendShapes[i]);
            }
        }

        public virtual void SmoothBlend(MonoBehaviour coroutine, SkinnedMeshRenderer mesh)
        {
            this.coroutine = coroutine;
            this.coroutine.StartCoroutine(C_SmoothBlend(mesh));
        }

        public virtual void SmoothBlend(MonoBehaviour coroutine)
        {
            this.coroutine = coroutine;
        }

        public virtual void SmoothBlend(SkinnedMeshRenderer mesh)
        {
            if (coroutine)
            {
                coroutine.StartCoroutine(C_SmoothBlend(mesh));
            }
            else
            {
                Debug.LogWarning("Call First  |SmoothBlend(MonoBehaviour coroutine)| if you want to use Smooth Blend");
            }
        }


        protected IEnumerator C_SmoothBlend(SkinnedMeshRenderer mesh)
        {
            var BSScript = coroutine.GetComponentInParent<BlendShape>();

            float elapsedTime = 0;
            int Length = Mathf.Min(mesh.sharedMesh.blendShapeCount, blendShapes.Length);

            float[] StartBlends = new float[mesh.sharedMesh.blendShapeCount];

            for (int i = 0; i < Length; i++)
            {
                StartBlends[i] = mesh.GetBlendShapeWeight(i);
            }


            while ((BlendTime > 0) && (elapsedTime <= BlendTime))
            {
                float result = BlendCurve.Evaluate(elapsedTime / BlendTime);             //Evaluation of the Pos curve

                for (int i = 0; i < Length; i++)
                {
                    var newWeight = Mathf.Lerp(StartBlends[i], blendShapes[i], result);

                    if (BSScript)
                    {
                        BSScript.blendShapes[i] = newWeight;
                        BSScript.UpdateBlendShapes();
                    }
                    else
                    {
                        mesh.SetBlendShapeWeight(i, newWeight);
                    }
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            if (BSScript)
            {
                BSScript.LoadBlendShapePreset(this);
            }
            else
            {
                Load(mesh);
            }
          
            if (BSScript) BSScript.SetShapesCount();

            yield return null;
        }
    }
}