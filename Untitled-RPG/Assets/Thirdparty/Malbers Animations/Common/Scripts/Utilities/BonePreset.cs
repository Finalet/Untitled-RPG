using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Bone Preset")]
    public class BonePreset : ScriptableObject
    {
        [Header("Smooth BlendShapes")]
        public FloatReference BlendTime = new FloatReference(1.5f);
        public AnimationCurve BlendCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });

        [Space, Header("Attributes to modify")]
        public bool positions = false;
        public bool scales = true;

        [Space, Header("Bones Properties")]
        public List<MiniTransform> Bones;
        private MonoBehaviour coroutine;


        public void SmoothBlendBones(MonoBehaviour coroutine) 
        { this.coroutine = coroutine; }
       
        public virtual void SmoothBlendBones(MonoBehaviour coroutine, Transform root)
        {
            this.coroutine = coroutine;
            this.coroutine.StartCoroutine(C_SmoothBlendBones(root));
        }

        public virtual void SmoothBlendBones(Transform root)
        {
            if (coroutine)
                coroutine.StartCoroutine(C_SmoothBlendBones(root));
            else
                Debug.LogWarning("Call First  |SmoothBlendBones(MonoBehaviour coroutine)| if you want to use Smooth Blend Bones");
        }


        private IEnumerator C_SmoothBlendBones(Transform root)
        {
            var AnimalTransforms = root.GetComponentsInChildren<Transform>().ToList(); ;

            List<MiniTransform> AnimalStartBones = new List<MiniTransform>();
            List<Transform> AnimalBonesTransforms = new List<Transform>();

            AnimalStartBones.Add(new MiniTransform("Root", Vector3.zero, root.localScale));
            AnimalBonesTransforms.Add(root);

            foreach (var bone in Bones)
            {
                var Bone_Found = AnimalTransforms.Find(item => item.name.ToLower() == bone.name.ToLower());

                if (Bone_Found)
                {
                    AnimalStartBones.Add(new MiniTransform(Bone_Found.name, Bone_Found.localPosition, Bone_Found.localScale));
                    AnimalBonesTransforms.Add(Bone_Found);
                }
            }

            //Debug.Log("AnimalBonesTransforms " + AnimalBonesTransforms.Count);

            float elapsedTime = 0;
            //Debug.Log("AnimalStartBones" + AnimalStartBones.Count);
            //Debug.Log("Bones" + Bones.Count);
            int max = Mathf.Min(Bones.Count, AnimalStartBones.Count);

            while ((BlendTime > 0) && (elapsedTime <= BlendTime))
            {
                float result = BlendCurve.Evaluate(elapsedTime / BlendTime);             //Evaluation of the curve


                //ROOT BONE
                if (scales) root.localScale = Vector3.LerpUnclamped(AnimalStartBones[0].Scale, Bones[0].Scale, result);

                for (int i = 1; i < max; i++)
                {
                    var NewPos = Vector3.LerpUnclamped(AnimalStartBones[i].Position, Bones[i].Position, result);
                    var NewScale = Vector3.LerpUnclamped(AnimalStartBones[i].Scale, Bones[i].Scale, result);

                    var Bone_Found = AnimalBonesTransforms.Find(item => item.name == Bones[i].name);

                    if (Bone_Found)
                    {
                        if (scales) Bone_Found.localScale = NewScale;
                        if (positions) Bone_Found.localPosition = NewPos;
                    }
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            Load(root);

            yield return null;
        }


        public void Load(Transform root)
        {
           var TransfBones = root.GetComponentsInChildren<Transform>().ToList(); ;

            if (Bones[0].name == "Root")
            {
                if (scales) root.localScale = Bones[0].Scale;
            }

            foreach (var bone in Bones)
            {
                var Bone_Found = TransfBones.Find(item => item.name == bone.name);

                if (Bone_Found)
                {
                    if (positions) Bone_Found.localPosition = bone.Position;
                    //if (rotations) Bone_Found.rotation = bone.rotation;
                    if (scales) Bone_Found.localScale = bone.Scale;
                }
            }
        }
    }

    [System.Serializable]
    public class MiniTransform
    {
        public string name = "bone";
        public Vector3 Position;
        //public Vector3 Rotation;
        public Vector3 Scale;

        public MiniTransform(string n, Vector3 p,/* Vector3 r,*/ Vector3 s)
        {
            name = n;
            Position = p;
          //  Rotation = r;
            Scale = s;
        }
    }
}