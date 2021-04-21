using UnityEngine;

namespace FIMSpace.FTools
{
    /// <summary>
    /// FC: Base class for processing IK logics
    /// </summary>
    [System.Serializable]
    public abstract class FIK_ProcessorBase
    {
        [Range(0f, 1f)] public float IKWeight = 1f;
        public Vector3 IKTargetPosition;
        public Quaternion IKTargetRotation;
        public Vector3 LastLocalDirection;
        public Vector3 LocalDirection;

        /// <summary> Length of whole bones chain (squared) </summary>
        protected float fullLength;

        public bool Initialized { get; protected set; }

        public FIK_IKBoneBase[] Bones { get; protected set; }
        public FIK_IKBoneBase StartBone { get { return Bones[0]; } }
        public FIK_IKBoneBase EndBone { get { return Bones[Bones.Length - 1]; } }

        public virtual void Init(Transform root) { }
        public virtual void Update()
        {
        }

        public static float EaseInOutQuint(float start, float end, float value)
        {
            value /= .5f; end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value * value + start; value -= 2;
            return end * 0.5f * (value * value * value * value * value + 2) + start;
        }
    }


    /// <summary>
    /// FC: Base class for IK bones computations
    /// </summary>
    [System.Serializable]
    public abstract class FIK_IKBoneBase
    {
        public FIK_IKBoneBase Child { get; private set; }

        public Transform transform { get; protected set; }
        public float BoneLength = 0.1f;
        public float MotionWeight = 1f;

        public Vector3 InitialLocalPosition;
        public Quaternion InitialLocalRotation;
        public Quaternion LastKeyLocalRotation;

        public FIK_IKBoneBase(Transform t)
        {
            transform = t;
            InitialLocalPosition = transform.localPosition;
            InitialLocalRotation = transform.localRotation;
            LastKeyLocalRotation = t.localRotation;
        }

        public virtual void SetChild(FIK_IKBoneBase child)
        {
            Child = child;
            BoneLength = (child.transform.position - transform.position).sqrMagnitude;
        }

    }

}
