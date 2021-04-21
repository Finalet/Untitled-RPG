using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {

        #region Position

        /// <summary> [Tail Calculations - TC] Smoothing position for _tc_bone changing _tc_smoothPos variable in iteration towards _tc_targetPos </summary>
        Vector3 TailCalculations_SmoothPosition(Vector3 from, Vector3 to, TailSegment bone)
        {
            if (SmoothingStyle == EAnimationStyle.Accelerating)
                return TailCalculations_SmoothPositionSmoothDamp(from, to, ref bone.VelocityHelper, bone.PositionSpeed);
            else if (SmoothingStyle == EAnimationStyle.Quick)
                return TailCalculations_SmoothPositionLerp(from, to, bone.PositionSpeed);
            else
                return TailCalculations_SmoothPositionLinear(from, to, bone.PositionSpeed );
        }


        /// <summary> Smoothing position for _tc_bone with lerp divide logics changing _tc_smoothPos variable in iteration towards _tc_targetPos </summary>
        Vector3 TailCalculations_SmoothPositionLerp(Vector3 from, Vector3 to, float speed)
        {
            return Vector3.LerpUnclamped(from, to, secPeriodDelta * speed);
        }

        /// <summary> Smoothing position for _tc_bone with smooth damp method changing _tc_smoothPos variable in iteration towards _tc_targetPos </summary>
        Vector3 TailCalculations_SmoothPositionSmoothDamp(Vector3 from, Vector3 to, ref Vector3 velo, float speed)
        {
            return Vector3.SmoothDamp(from, to, ref velo, Mathf.LerpUnclamped(0.08f, 0.0001f, Mathf.Sqrt(Mathf.Sqrt(speed))), Mathf.Infinity, rateDelta);
        }

        /// <summary> Smoothing position for _tc_bone linearly changing _tc_smoothPos variable in iteration towards _tc_targetPos </summary>
        Vector3 TailCalculations_SmoothPositionLinear(Vector3 from, Vector3 to, float speed)
        {
            return Vector3.MoveTowards(from, to, deltaForLerps * speed * 45f);
        }

        #endregion


        #region Rotation



        /// <summary> [Tail Calculations - TC] Smoothing rotation for _tc_bone changing _tc_smoothRot variable in iteration towards _tc_targetRot </summary>
        Quaternion TailCalculations_SmoothRotation(Quaternion from, Quaternion to, TailSegment bone)
        {
            if (SmoothingStyle == EAnimationStyle.Accelerating)
                return TailCalculations_SmoothRotationSmoothDamp(from, to, ref bone.QVelocityHelper, bone.RotationSpeed);
            else
            if (SmoothingStyle == EAnimationStyle.Quick)
                return TailCalculations_SmoothRotationLerp(from, to, bone.RotationSpeed);
            else
                return TailCalculations_SmoothRotationLinear(from, to, bone.RotationSpeed);
        }

        /// <summary> Smoothing rotation for _tc_bone with lerp divide logics changing _tc_smoothRot variable in iteration towards _tc_targetRot </summary>
        Quaternion TailCalculations_SmoothRotationLerp(Quaternion from, Quaternion to, float speed)
        {
            return Quaternion.LerpUnclamped(from, to, secPeriodDelta * speed);
        }

        /// <summary> Smoothing rotation for _tc_bone with smooth damp method changing _tc_smoothRot variable in iteration towards _tc_targetRot </summary>
        Quaternion TailCalculations_SmoothRotationSmoothDamp(Quaternion from, Quaternion to, ref Quaternion velo, float speed)
        {
            return FEngineering.SmoothDampRotation(from, to, ref velo, Mathf.LerpUnclamped(0.25f, 0.0001f, Mathf.Sqrt(Mathf.Sqrt(speed))), rateDelta);
        }

        /// <summary> Smoothing rotation for _tc_bone linearly changing _tc_smoothRot variable in iteration towards _tc_targetRot </summary>
        Quaternion TailCalculations_SmoothRotationLinear(Quaternion from, Quaternion to, float speed)
        {
            return (Quaternion.RotateTowards(from, to, speed * deltaForLerps * 1600f));
        }


        #endregion

    }
}