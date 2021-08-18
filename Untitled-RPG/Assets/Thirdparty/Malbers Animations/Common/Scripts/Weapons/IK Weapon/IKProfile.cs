using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Weapons
{
    [CreateAssetMenu(menuName = "Malbers Animations/Weapons/IK Profile")]
    public class IKProfile : ScriptableObject
    {
        public bool LookAtIK = false;
        [/*Range(0, 1),*/ Hide("LookAtIK", true, false)]
        public float Weight = 1;
        [/*Range(0, 1),*/ Hide("LookAtIK", true, false)]
        public float BodyWeight = 1;
        [/*Range(0, 1), */Hide("LookAtIK", true, false)]
        public float HeadWeight = 1;
        [/*Range(0, 1), */Hide("LookAtIK", true, false)]
        public float EyesWeight = 1;
        [/*Range(0, 1), */Hide("LookAtIK", true, false)]
        public float ClampWeight = 0.5f;
        [Hide("LookAtIK", true, false)]
        public float Distance = 100f;
        [Hide("LookAtIK", true, false)]
        public float HorizontalOffset = 0;
        [Hide("LookAtIK", true, false)]
        public float VerticalOffset = 0;
        [Hide("LookAtIK", true, false)]
        public HumanBodyBones AimOrigin = HumanBodyBones.Head;

        [Space,Header("OFFSETS")]
        public List<BoneOfsset> offsets;
        public virtual void ApplyLookAt(Animator Anim,  Vector3 Dir, float weight)
        {
            var origin = Anim.GetBoneTransform(AimOrigin);
            Dir = Quaternion.AngleAxis(HorizontalOffset, Vector3.up) * Dir;

            var RightV = Vector3.Cross(Dir, Vector3.up);
            Dir = Quaternion.AngleAxis(VerticalOffset, RightV) * Dir;


            var ray = new Ray(origin.position, Dir);
            var Point = ray.GetPoint(Distance);
            Debug.DrawLine(origin.position, Point, Color.cyan);
            Anim.SetLookAtWeight(Weight * weight, BodyWeight, HeadWeight, EyesWeight, ClampWeight);
            Anim.SetLookAtPosition(Point);
        }

        public virtual void ApplyOffsets(Animator Anim, Vector3 Direction, float Weight)
        { 
           if (LookAtIK) ApplyLookAt(Anim, Direction, Weight);

            for (int i = 0; i < offsets.Count; i++)
            {
                if (Direction == Vector3.zero) continue;

                var offset = offsets[i];
                var bn = Anim.GetBoneTransform(offset.bone);
                if (bn == null) return;

                var OffsetRot = Quaternion.Euler(offset.RotationOffset);
                var InverseRot = Quaternion.Inverse(bn.parent.rotation);


                var WorldRotation = InverseRot * OffsetRot;
            //    var InitialRotation = bn.localRotation * Quaternion.Inverse(bn.parent.rotation);
               // var ToLocal = InitialRotation * WorldRotation;


                Quaternion finalRotation = Quaternion.identity;
                
                switch (offset.rotationType)
                {
                    case BoneOfsset.IKType.AdditiveOffset:
                        finalRotation =  bn.localRotation  * OffsetRot;
                        break;
                    case BoneOfsset.IKType.OffsetOnly:
                        finalRotation = OffsetRot;
                        break;
                    case BoneOfsset.IKType.WorldRotation:
                        finalRotation = WorldRotation;
                        break;
                    default:
                        break;
                }

                // Anim.SetBoneLocalRotation(offset.bone, bn.localRotation * WorldRotation);
                Anim.SetBoneLocalRotation(offset.bone, finalRotation);
               // Anim.SetBoneLocalRotation(offset.bone, bn.localRotation * ToLocal);
            }
        }

        private void OnValidate()
        {
            Weight = Mathf.Clamp01(Weight);
            BodyWeight = Mathf.Clamp01(BodyWeight);
            HeadWeight = Mathf.Clamp01(HeadWeight);
            EyesWeight = Mathf.Clamp01(EyesWeight);
            ClampWeight = Mathf.Clamp01(ClampWeight);
        }
    }

    [System.Serializable]
    public struct BoneOfsset
    {
        public enum IKType { AdditiveOffset, OffsetOnly, WorldRotation, LookAtDir }
        public HumanBodyBones bone;
        public IKType rotationType;
        public Vector3 RotationOffset;
    }

    public struct IKGoalOffsets
    {
        public AvatarIKGoal ikGoal;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
    }
}