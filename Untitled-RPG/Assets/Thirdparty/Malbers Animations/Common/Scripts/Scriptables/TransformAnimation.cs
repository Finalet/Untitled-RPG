using System.Collections;
using UnityEngine;

namespace MalbersAnimations
{
  //  public enum AnimCycle { None, Loop, Repeat, PingPong }

    [CreateAssetMenu(menuName = "Malbers Animations/Extras/Anim Transform", order = 2100)]
    public class TransformAnimation : ScriptableCoroutine
    {
        public enum AnimTransType { TransformAnimation, MountTriggerAdjustment }

        public AnimTransType animTrans = AnimTransType.TransformAnimation;

        static Keyframe[] K = { new Keyframe(0, 0), new Keyframe(1, 1) };

        public float time = 1f;
        public float delay = 1f;
        //public AnimCycle cycle;

        public bool UsePosition = false;
        public Vector3 Position;
        public AnimationCurve PosCurve = new AnimationCurve(K);

        public bool SeparateAxisPos = false;
        public AnimationCurve PosXCurve = new AnimationCurve(K);
        public AnimationCurve PosYCurve = new AnimationCurve(K);
        public AnimationCurve PosZCurve = new AnimationCurve(K);

        public bool UseRotation = false;
        public Vector3 Rotation;
        public AnimationCurve RotCurve = new AnimationCurve(K);

        public bool SeparateAxisRot = false;
        public AnimationCurve RotXCurve = new AnimationCurve(K);
        public AnimationCurve RotYCurve = new AnimationCurve(K);
        public AnimationCurve RotZCurve = new AnimationCurve(K);

        public bool UseScale = false;
        public Vector3 Scale = Vector3.one;
        public AnimationCurve ScaleCurve = new AnimationCurve(K);

        public Vector3 TargetPos { get; private set; }
        public Vector3 TargetRot { get; private set; }
        public Vector3 TargetScale { get; private set; }

        public Vector3 StartPos { get; private set; }
        public Vector3 StartRot { get; private set; }
        public Vector3 StartScale { get; private set; }

        public void Play(Transform item)
        {
            StartCoroutine(item, PlayTransformAnimation(item));
        }

        public void PlayForever(Transform item)
        {
            StartCoroutine(item, PlayTransformAnimationForever(item));
        }


        /// <summary> Plays the Transform Animations for the Selected item   </summary>
        internal IEnumerator PlayTransformAnimation(Transform item)
        {
            if (item != null)
            {
                if (delay != 0) yield return new WaitForSeconds(delay);         //Wait for the Delay     

                float elapsedTime = 0;

                StartPos = item.localPosition;                                          //Store the Current Position Rotation and Scale
                StartRot = item.localEulerAngles;
                StartScale = item.localScale;

                TargetPos = StartPos + Position;
                TargetRot = StartRot + (Rotation);
                TargetScale = Vector3.Scale(StartScale, Scale);

                

                while ((time > 0) && (elapsedTime <= time) && item != null)
                {

                    float resultPos = PosCurve.Evaluate(elapsedTime / time);               //Evaluation of the Pos curve
                    float resultRot = RotCurve.Evaluate(elapsedTime / time);               //Evaluation of the Rot curve
                    float resultSca = ScaleCurve.Evaluate(elapsedTime / time);               //Evaluation of the Scale curve

                    if (UsePosition) item.localPosition = Vector3.LerpUnclamped(StartPos, TargetPos, resultPos);

                    if (UseRotation) item.transform.localEulerAngles = Vector3.LerpUnclamped(StartRot, TargetRot, resultRot);

                    if (UseScale) item.transform.localScale = Vector3.LerpUnclamped(StartScale, TargetScale, resultSca);

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                ExitValue(item);
            }

            yield return null;
            Stop(item);
        }

        internal override void ExitValue(Component component)
        {
            var item = (Transform)component;
            if (item == null) return;

            if (UsePosition)
            {
                float FresultPos = PosCurve.Evaluate(1 / time);               //Evaluation of the Pos curve
                item.localPosition = Vector3.LerpUnclamped(StartPos, TargetPos, FresultPos);
            }
            if (UseRotation)
            {
                float FresultRot = RotCurve.Evaluate(1 / time);               //Evaluation of the Rot curve
                item.transform.localEulerAngles = Vector3.LerpUnclamped(StartRot, TargetRot, FresultRot);
            }
            if (UseScale)
            {
                float FresultSca = ScaleCurve.Evaluate(1 / time);               //Evaluation of the Scale curve
                item.transform.localScale = Vector3.LerpUnclamped(StartScale, TargetScale, FresultSca);
            }
        } 
        /// <summary> Plays the Transform Animations for the Selected item   </summary>
        internal IEnumerator PlayTransformAnimationForever(Transform item)
        {
            if (item != null)
            {
                if (delay != 0) yield return new WaitForSeconds(delay);         //Wait for the Delay     

                float elapsedTime = 0;

                Vector3 StartPos = item.localPosition;                                          //Store the Current Position Rotation and Scale
                Vector3 StartRot = item.localEulerAngles;
                Vector3 StartScale = item.localScale;

                var TargetPos = StartPos + Position;
                var TargetRot = StartRot + (Rotation);
                var TargetScale = Vector3.Scale(StartScale, Scale);

                while (true)
                {
                    float resultPos = PosCurve.Evaluate(elapsedTime / time);               //Evaluation of the Pos curve
                    float resultRot = RotCurve.Evaluate(elapsedTime / time);               //Evaluation of the Rot curve
                    float resultSca = ScaleCurve.Evaluate(elapsedTime /time);               //Evaluation of the Scale curve



                    if (UsePosition)  item.localPosition = Vector3.LerpUnclamped(StartPos, TargetPos, resultPos);
                    if (UseRotation)  item.transform.localEulerAngles= Vector3.LerpUnclamped(StartRot, TargetRot, resultRot);
                    if (UseScale)    item.transform.localScale = Vector3.LerpUnclamped(StartScale, TargetScale, resultSca);

                    elapsedTime += Time.deltaTime;
                    elapsedTime %= time;
                  
                    yield return null;
                }
            }
            yield return null;
            Stop(item);
        }
    } 
}