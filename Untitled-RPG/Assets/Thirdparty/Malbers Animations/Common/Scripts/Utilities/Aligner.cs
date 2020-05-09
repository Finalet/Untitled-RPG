using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    public class Aligner : MonoBehaviour, IAlign
    {
        /// <summary></summary>
        public Transform mainPoint;
        /// <summary></summary>
        public Transform SecondPoint;
        /// <summary></summary>
        public float AlignTime = 0.25f;
        /// <summary></summary>
        public AnimationCurve AlignCurve = new AnimationCurve(MalbersTools.DefaultCurve);

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
       
        ///// <summary>Minimum Distance the animal will move if the Radius is greater than zero</summary>
        //public float LookAtDistance;
        public Color DebugColor = new Color(1, 0.23f, 0, 1f);


        public bool Active { get => enabled; set => enabled = value; }

        public Transform MainPoint => mainPoint;
       
        public virtual void Align(GameObject Target)
        {
            Align(Target.transform);
        }


        public virtual void Align(Collider Target)
        {
            Align(Target.transform.root); 
        }

        public virtual void Align(MonoBehaviour Target)
        {
            Align(Target.transform.root);
        }

        public virtual void StopAling() { StopAllCoroutines(); }
       

        public virtual void Align(Transform TargetToAlign)
        {
            //Debug.Log("ALIGNER WHo",TargetToAlign);
            //Debug.Log("ALIGNER Main Point", mainPoint);

            if (enabled && mainPoint && TargetToAlign != null)
            {
                if (AlignLookAt)
                {
                    StartCoroutine(AlignLookAtTransform(TargetToAlign, mainPoint, AlignTime, AlignCurve));  //Align Look At the Zone
                     if (LookAtRadius>0) StartCoroutine(AlignTransformRadius(TargetToAlign, mainPoint, AlignTime, LookAtRadius, AlignCurve));  //Align Look At the Zone
                }
                else
                {
                    if (AlignPos)
                    {
                        Vector3 AlingPosition = mainPoint.position;

                        if (SecondPoint)                //In case there's a line ... move to the closest point between the two transforms
                        {
                            AlingPosition = MalbersTools.ClosestPointOnLine(mainPoint.position, SecondPoint.position, TargetToAlign.transform.position);
                        }

                        if (DoubleSided)
                        {
                            Vector3 AlingPosOpposite = transform.InverseTransformPoint(AlingPosition);
                            AlingPosOpposite.z *= -1;
                            AlingPosOpposite = transform.TransformPoint(AlingPosOpposite);

                            var Distance1 = Vector3.Distance(TargetToAlign.transform.position, AlingPosition);
                            var Distance2 = Vector3.Distance(TargetToAlign.transform.position, AlingPosOpposite);

                            var AlignTransformResult = Distance2 < Distance1 ? AlingPosOpposite : AlingPosition;

                            StartCoroutine(MalbersTools.AlignTransform_Position(TargetToAlign.transform, AlignTransformResult, AlignTime, AlignCurve));
                        }
                        else
                        {
                            StartCoroutine(MalbersTools.AlignTransform_Position(TargetToAlign.transform, AlingPosition, AlignTime, AlignCurve));
                        }
                    }
                    if (AlignRot)
                    {
                        Quaternion Side1 = mainPoint.rotation;
                        Quaternion AnimalRot = TargetToAlign.transform.rotation;

                        if (DoubleSided)
                        {
                            Quaternion Side2 = mainPoint.rotation * Quaternion.Euler(0, 180, 0);

                            var Side1Angle = Quaternion.Angle(AnimalRot, Side1);
                            var Side2Angle = Quaternion.Angle(AnimalRot, Side2);

                            StartCoroutine(MalbersTools.AlignTransform_Rotation(TargetToAlign.transform, Side1Angle < Side2Angle ? Side1 : Side2, AlignTime, AlignCurve));
                        }
                        else
                            StartCoroutine(MalbersTools.AlignTransform_Rotation(TargetToAlign.transform, Side1, AlignTime, AlignCurve));
                    }
                }
            }
        }

        IEnumerator AlignLookAtTransform(Transform t1, Transform t2, float time, AnimationCurve curve = null)
        {
            float elapsedTime = 0;

            Quaternion CurrentRot = t1.rotation;
            Vector3 direction = (t2.position - t1.position).normalized;
            direction.y = t1.forward.y;
            Quaternion FinalRot = Quaternion.LookRotation(direction);

            while ((time > 0) && (elapsedTime <= time))
            {
                float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
            t1.rotation = FinalRot;
        }

        IEnumerator AlignTransformRadius(Transform TargetToAlign, Transform AlignOrigin, float time, float radius, AnimationCurve curve = null)
        {
            if (radius > 0)
            {
                float elapsedTime = 0;

                Vector3 CurrentPos = TargetToAlign.position;

                Ray TargetRay = new Ray(AlignOrigin.position, (TargetToAlign.position - AlignOrigin.position).normalized);
                Vector3 TargetPos = TargetRay.GetPoint(radius);

                //IAlign TargetAligner = TargetToAlign.GetComponent<IAlign>() ?? TargetToAlign.GetComponentInChildren<IAlign>() ?? TargetToAlign.GetComponentInParent<IAlign>();


                while ((time > 0) && (elapsedTime <= time))
                {
                    float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;               //Evaluation of the Pos curve

                    TargetToAlign.position = Vector3.LerpUnclamped(CurrentPos, TargetPos, result);

                    elapsedTime += Time.deltaTime;

                    yield return null;
                }
                TargetToAlign.position = TargetPos;
            }
            yield return null;
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
        }


        void OnDrawGizmos()
        {
            var WireColor = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1);
            if (mainPoint)
            {
                if (AlignLookAt && LookAtRadius > 0)
                {
                    UnityEditor.Handles.color = DebugColor;
                    UnityEditor.Handles.DrawWireDisc(mainPoint.position, transform.up, LookAtRadius);
                }

                if (SecondPoint)
                {
                    Gizmos.color = WireColor;
                    Gizmos.DrawLine(mainPoint.position, SecondPoint.position);
                    Gizmos.DrawCube(mainPoint.position, Vector3.one * 0.05f);
                    Gizmos.DrawCube(SecondPoint.position, Vector3.one * 0.05f);

                    if (DoubleSided)
                    {
                        var AlingPoint1Opp = transform.InverseTransformPoint(mainPoint.position);
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
            if (AlignLookAt && LookAtRadius > 0 && mainPoint)
            {
                UnityEditor.Handles.color = new Color(1, 1, 0, 1);
                UnityEditor.Handles.DrawWireDisc(mainPoint.position, transform.up, LookAtRadius);
            }
        }
#endif
    }
}