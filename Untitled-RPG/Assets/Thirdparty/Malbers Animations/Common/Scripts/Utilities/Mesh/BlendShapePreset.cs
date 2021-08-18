using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Preset/BlendShape", order = 200)]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/utilities/blend-shapes/blend-shape-preset")]
    public class BlendShapePreset : ScriptableCoroutine
    {
        [Header("Smooth BlendShapes")]
        public FloatReference BlendTime = new FloatReference(1.5f);
        public AnimationCurve BlendCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });

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
        public virtual void SmoothBlend(SkinnedMeshRenderer mesh)
        {
            StartCoroutine(mesh, C_SmoothBlend(mesh));
        }


        protected IEnumerator C_SmoothBlend(SkinnedMeshRenderer mesh)
        {
            float elapsedTime = 0;
            int Length = Mathf.Min(mesh.sharedMesh.blendShapeCount, blendShapes.Length, blendShapes.Length);

            float[] StartBlends = new float[mesh.sharedMesh.blendShapeCount];


            int SamePreset = 0;

            for (int i = 0; i < Length; i++)
            {
                StartBlends[i] = mesh.GetBlendShapeWeight(i);

                if (StartBlends[i] == blendShapes[i]) SamePreset++;
            }

            if (SamePreset == Length)
            {
                Debug.Log("Loading same BlendShape preset. Ingore");
                yield return null;
                Stop(mesh);
            }
            else
            {

                while ((BlendTime > 0) && (elapsedTime <= BlendTime))
                {
                    float result = BlendCurve.Evaluate(elapsedTime / BlendTime);             //Evaluation of the Pos curve

                    for (int i = 0; i < Length; i++)
                    {
                        var newWeight = Mathf.Lerp(StartBlends[i], blendShapes[i], result);

                        mesh.SetBlendShapeWeight(i, newWeight);
                    }

                    elapsedTime += Time.deltaTime;

                    yield return null;
                }

                Load(mesh);

                var BSScript = mesh.transform.root.FindComponent<BlendShape>();
                if (BSScript)
                {
                    BSScript.LoadPreset(this);
                    BSScript.SetShapesCount();
                }

                yield return null;

                Stop(mesh);
            }
        }
    }
}