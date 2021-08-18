using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Aling/Aligner")]
    public class Aligner : MonoBehaviour, IAlign
    {
        /// <summary></summary>
        public TransformReference mainPoint = new TransformReference();
        /// <summary></summary>
        public TransformReference secondPoint = new TransformReference();
        /// <summary></summary>
        public float AlignTime = 0.25f;

        [Tooltip("Add an offset to the rotation alignment")]
        public float AngleOffset = 0;
        /// <summary></summary>
        public AnimationCurve AlignCurve = new AnimationCurve(MTools.DefaultCurve);

        /// <summary></summary>
        public bool AlignPos = true;
        /// <summary></summary>
        public bool AlignRot = true;
        /// <summary>When Rotation is Enabled then It will find the closest Rotation</summary>
        public bool DoubleSided = true;
        /// <summary>Align a gameObject Looking at the Aligner</summary>
        public bool AlignLookAt = false;
        /// <summary>The Target will move close to the Aligner equals to the Radius</summary>
        public float LookAtRadius;
        public float LookAtRadiusTime = 0.25f;
       
        ///// <summary>Minimum Distance the animal will move if the Radius is greater than zero</summary>
        //public float LookAtDistance;
        public Color DebugColor = new Color(1, 0.23f, 0, 1f);


        public bool Active { get => enabled; set => enabled = value; }

        public Transform MainPoint => mainPoint.Value;
        public Transform SecondPoint => secondPoint.Value;

        public virtual void Align(GameObject Target) => Align(Target.transform);

        public virtual void Align(Collider Target) => Align(Target.transform.root);

        public virtual void Align(Component Target) => Align(Target.transform.root);
        public virtual void StopAling() { StopAllCoroutines(); }
        public virtual void Align_Self_To(GameObject Target) => Align_Self_To(Target.transform);

        public virtual void Align_Self_To(Collider Target) => Align_Self_To(Target.transform.root);

        public virtual void Align_Self_To(Component Target) => Align_Self_To(Target.transform.root);

        public virtual void Align_Self_To(Transform reference)
        {
            if (Active && MainPoint && reference != null)
            {
                if (AlignLookAt)
                {
                    StartCoroutine(AlignLookAtTransform(mainPoint, reference, AlignTime, AlignCurve));  //Align Look At the Zone
                    if (LookAtRadius > 0) StartCoroutine(MTools.AlignTransformRadius(reference, mainPoint.position, LookAtRadiusTime, LookAtRadius, AlignCurve));  //Align Look At the Zone
                }
                else
                {
                    if (AlignPos)
                    {
                        Vector3 AlingPosition = reference.position;
                        StartCoroutine(MTools.AlignTransform_Position(MainPoint, AlingPosition, AlignTime, AlignCurve));
                    }
                    if (AlignRot)
                    {
                        Quaternion Side1 = reference.rotation;
                        Quaternion self = MainPoint.rotation;

                        if (DoubleSided)
                        {
                            Quaternion Side2 = reference.rotation * Quaternion.Euler(0, 180, 0);

                            var Side1Angle = Quaternion.Angle(self, Side1);
                            var Side2Angle = Quaternion.Angle(self, Side2);

                            StartCoroutine(MTools.AlignTransform_Rotation(MainPoint, Side1Angle < Side2Angle ? Side1 : Side2, AlignTime, AlignCurve));
                        }
                        else
                            StartCoroutine(MTools.AlignTransform_Rotation(MainPoint, Side1, AlignTime, AlignCurve));
                    }
                }
            }
        }

        public virtual void Align(Transform TargetToAlign)
        {
            if (Active && MainPoint && TargetToAlign != null)
            {
                if (AlignLookAt)
                {
                    StartCoroutine(AlignLookAtTransform(TargetToAlign, mainPoint, AlignTime, AlignCurve));  //Align Look At the Zone
                    if (LookAtRadius > 0)
                        StartCoroutine(MTools.AlignTransformRadius(TargetToAlign, mainPoint.position, LookAtRadiusTime, LookAtRadius, AlignCurve));  //Align Look At the Zone
                }
                else
                {
                    if (AlignPos)
                    {
                        Vector3 AlingPosition = MainPoint.position;

                        if (SecondPoint)                //In case there's a line ... move to the closest point between the two transforms
                        {
                            AlingPosition = MTools.ClosestPointOnLine(MainPoint.position, SecondPoint.position, TargetToAlign.transform.position);
                        }

                        if (DoubleSided)
                        {
                            Vector3 AlingPosOpposite = transform.InverseTransformPoint(AlingPosition);
                            AlingPosOpposite.z *= -1;
                            AlingPosOpposite = transform.TransformPoint(AlingPosOpposite);

                            var Distance1 = Vector3.Distance(TargetToAlign.transform.position, AlingPosition);
                            var Distance2 = Vector3.Distance(TargetToAlign.transform.position, AlingPosOpposite);

                            var AlignTransformResult = Distance2 < Distance1 ? AlingPosOpposite : AlingPosition;

                            StartCoroutine(MTools.AlignTransform_Position(TargetToAlign.transform, AlignTransformResult, AlignTime, AlignCurve));
                        }
                        else
                        {
                            StartCoroutine(MTools.AlignTransform_Position(TargetToAlign.transform, AlingPosition, AlignTime, AlignCurve));
                        }
                    }
                    if (AlignRot)
                    {
                        Quaternion Side1 = MainPoint.rotation * Quaternion.Euler(0, AngleOffset, 0);
                        Quaternion AnimalRot = TargetToAlign.transform.rotation;

                        if (DoubleSided)
                        {
                            Quaternion Side2 = MainPoint.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Euler(0, AngleOffset, 0);

                            var Side1Angle = Quaternion.Angle(AnimalRot, Side1);
                            var Side2Angle = Quaternion.Angle(AnimalRot, Side2);

                            StartCoroutine(MTools.AlignTransform_Rotation(TargetToAlign.transform, Side1Angle < Side2Angle ? Side1 : Side2, AlignTime, AlignCurve));
                        }
                        else
                            StartCoroutine(MTools.AlignTransform_Rotation(TargetToAlign.transform, Side1, AlignTime, AlignCurve));
                    }
                }
            }
        }

         
        /// <summary>
        /// Makes a transform Rotate towards another using LookAt Rotation
        /// </summary>
        /// <param name="t1">Transform that it will be rotated</param>
        /// <param name="t2">Transform reference to Look At</param>
        /// <param name="time">time to do the lookat alignment</param>
        /// <param name="curve">curve for the aligment</param>
        /// <returns></returns>
        IEnumerator AlignLookAtTransform(Transform t1, Transform t2, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            var Wait = new WaitForFixedUpdate();

            Quaternion CurrentRot = t1.rotation;
            Vector3 direction = (t2.position - t1.position).normalized;
            direction.y = t1.forward.y;
            Quaternion FinalRot = Quaternion.LookRotation(direction) * Quaternion.Euler(0, AngleOffset, 0);

            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);

                elapsedTime += Time.fixedDeltaTime;

                yield return Wait;
            }
            t1.rotation = FinalRot;
        }

     

#if UNITY_EDITOR

        void Reset()
        {
            mainPoint =  transform;
        }


        void OnValidate()
        {
            LookAtRadius = LookAtRadius < 0 ? 0 : LookAtRadius;
            AlignTime = AlignTime < 0 ? 0 : AlignTime;
            LookAtRadiusTime = LookAtRadiusTime < 0 ? 0 : LookAtRadiusTime;
        }


        void OnDrawGizmos()
        {
            var WireColor = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1);
            if (MainPoint)
            {
                if (AlignLookAt && LookAtRadius > 0)
                {
                    UnityEditor.Handles.color = DebugColor;
                    UnityEditor.Handles.DrawWireDisc(MainPoint.position, transform.up, LookAtRadius);
                }

                if (SecondPoint)
                {
                    Gizmos.color = WireColor;
                    Gizmos.DrawLine(MainPoint.position, SecondPoint.position);
                    Gizmos.DrawCube(MainPoint.position, Vector3.one * 0.05f);
                    Gizmos.DrawCube(SecondPoint.position, Vector3.one * 0.05f);

                    if (DoubleSided)
                    {
                        var AlingPoint1Opp = transform.InverseTransformPoint(MainPoint.position);
                        var AlingPoint2Opp = transform.InverseTransformPoint(SecondPoint.position);

                        AlingPoint1Opp.z *= -1;
                        AlingPoint2Opp.z *= -1;
                        AlingPoint1Opp = transform.TransformPoint(AlingPoint1Opp);
                        AlingPoint2Opp = transform.TransformPoint(AlingPoint2Opp);

                        Gizmos.DrawLine(AlingPoint1Opp, AlingPoint2Opp);

                        Gizmos.DrawCube(AlingPoint1Opp, Vector3.one * 0.05f);
                        Gizmos.DrawCube(AlingPoint2Opp, Vector3.one * 0.05f);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (AlignLookAt && LookAtRadius > 0 && MainPoint)
            {
                UnityEditor.Handles.color = new Color(1, 1, 0, 1);
                UnityEditor.Handles.DrawWireDisc(MainPoint.position, transform.up, LookAtRadius);
            }
        }
#endif
    }
}