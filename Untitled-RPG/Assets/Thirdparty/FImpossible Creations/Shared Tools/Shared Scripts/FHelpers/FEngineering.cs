using UnityEngine;

/// <summary>
/// FM: Class which contains many helpful methods which operates on Vectors and Quaternions or some other floating point maths
/// </summary>
public static class FEngineering
{


    #region Rotations and directions


    public static bool VIsZero(Vector3 vec)
    {
        if (vec.sqrMagnitude == 0f) return true; return false;
        //if (vec.x != 0f) return false; if (vec.y != 0f) return false; if (vec.z != 0f) return false; return true;
    }

    public static bool VIsSame(Vector3 vec1, Vector3 vec2)
    {
        if (vec1.x != vec2.x) return false; if (vec1.y != vec2.y) return false; if (vec1.z != vec2.z) return false; return true;
    }


    public static Vector3 TransformVector(Quaternion parentRot, Vector3 parentLossyScale, Vector3 childLocalPos)
    {
        return parentRot * Vector3.Scale(childLocalPos, parentLossyScale);
    }

    /// <summary> Same like transform vector but without scaling but also supporting negative scale </summary>
    public static Vector3 TransformInDirection(Quaternion childRotation, Vector3 parentLossyScale, Vector3 childLocalPos)
    {
        return childRotation * Vector3.Scale(childLocalPos, new Vector3(parentLossyScale.x > 0 ? 1 : -1, parentLossyScale.y > 0 ? 1 : -1, parentLossyScale.y > 0 ? 1 : -1));
    }

    public static Vector3 InverseTransformVector(Quaternion tRotation, Vector3 tLossyScale, Vector3 worldPos)
    {
        worldPos = Quaternion.Inverse(tRotation) * worldPos;
        return new Vector3(worldPos.x / tLossyScale.x, worldPos.y / tLossyScale.y, worldPos.z / tLossyScale.z);
    }


    /// <summary> Instance for 2D Axis limit calculations </summary>
    private static Plane axis2DProjection;

    /// <summary>
    /// Calculating offset (currentPos -= Axis2DLimit...) to prevent object from moving in provided axis
    /// </summary>
    /// <param name="axis">1 is X   2 is Y   3 is Z</param>
    public static Vector3 VAxis2DLimit(Transform parent, Vector3 parentPos, Vector3 childPos, int axis = 3)
    {
        if (axis == 3)  // Z is depth
            axis2DProjection.SetNormalAndPosition(parent.forward, parentPos);
        else
        if (axis == 2)   // Y
            axis2DProjection.SetNormalAndPosition(parent.up, parentPos);
        else             // X is depth
            axis2DProjection.SetNormalAndPosition(parent.right, parentPos);

        return axis2DProjection.normal * axis2DProjection.GetDistanceToPoint(childPos);
    }

    #endregion


    #region Just Rotations related

    /// <summary>
    /// Locating world rotation in local space of parent transform
    /// </summary>
    public static Quaternion QToLocal(Quaternion parentRotation, Quaternion worldRotation)
    {
        return Quaternion.Inverse(parentRotation) * worldRotation;
    }

    /// <summary>
    /// Locating local rotation of child local space to world
    /// </summary>
    public static Quaternion QToWorld(Quaternion parentRotation, Quaternion localRotation)
    {
        return parentRotation * localRotation;
    }

    /// <summary>
    /// Offsetting rotation of child transform with defined axis orientation
    /// </summary>
    public static Quaternion QRotateChild(Quaternion offset, Quaternion parentRot, Quaternion childLocalRot)
    {
        return (offset * parentRot) * childLocalRot;
    }

    public static Quaternion ClampRotation(Vector3 current, Vector3 bounds)
    {
        WrapVector(current);

        if (current.x < -bounds.x) current.x = -bounds.x; else if (current.x > bounds.x) current.x = bounds.x;
        if (current.y < -bounds.y) current.y = -bounds.y; else if (current.y > bounds.y) current.y = bounds.y;
        if (current.z < -bounds.z) current.z = -bounds.z; else if (current.z > bounds.z) current.z = bounds.z;

        return Quaternion.Euler(current);
    }


    /// <summary>
    /// For use with rigidbody.angularVelocity (Remember to set "rigidbody.maxAngularVelocity" higher)
    /// </summary>
    /// <param name="deltaRotation"> Create with [TargetRotation] * Quaternion.Inverse([CurrentRotation]) </param>
    /// <returns> Multiply this value by rotation speed parameter like QToAngularVelocity(deltaRot) * RotationSpeed </returns>
    public static Vector3 QToAngularVelocity(Quaternion deltaRotation)
    {
        float angle; Vector3 axis;
        deltaRotation.ToAngleAxis(out angle, out axis);
        return axis * (angle * Mathf.Deg2Rad);
    }


    public static bool QIsZero(Quaternion rot)
    {
        if (rot.x != 0f) return false; if (rot.y != 0f) return false; if (rot.z != 0f) return false; return true;
    }

    public static bool QIsSame(Quaternion rot1, Quaternion rot2)
    {
        if (rot1.x != rot2.x) return false; if (rot1.y != rot2.y) return false; if (rot1.z != rot2.z) return false; if (rot1.w != rot2.w) return false; return true;
    }


    /// <summary> Wrapping angle (clamping in +- 360) </summary>
    public static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180) return angle - 360;
        return angle;
    }

    public static Vector3 WrapVector(Vector3 angles)
    { return new Vector3(WrapAngle(angles.x), WrapAngle(angles.y), WrapAngle(angles.z)); }

    /// <summary> Unwrapping angle </summary>
    public static float UnwrapAngle(float angle)
    {
        if (angle >= 0) return angle;
        angle = -angle % 360;
        return 360 - angle;
    }

    public static Vector3 UnwrapVector(Vector3 angles)
    { return new Vector3(UnwrapAngle(angles.x), UnwrapAngle(angles.y), UnwrapAngle(angles.z)); }


    #endregion


    #region Animation Related


    public static Quaternion SmoothDampRotation(Quaternion current, Quaternion target, ref Quaternion velocityRef, float duration, float delta)
    {
        return SmoothDampRotation(current, target, ref velocityRef, duration, Mathf.Infinity, delta);
    }

    public static Quaternion SmoothDampRotation(Quaternion current, Quaternion target, ref Quaternion velocityRef, float duration, float maxSpeed, float delta)
    {
        float dot = Quaternion.Dot(current, target);
        float sign = dot > 0f ? 1f : -1f;
        target.x *= sign;
        target.y *= sign;
        target.z *= sign;
        target.w *= sign;

        Vector4 smoothVal = new Vector4(
            Mathf.SmoothDamp(current.x, target.x, ref velocityRef.x, duration, maxSpeed, delta),
            Mathf.SmoothDamp(current.y, target.y, ref velocityRef.y, duration, maxSpeed, delta),
            Mathf.SmoothDamp(current.z, target.z, ref velocityRef.z, duration, maxSpeed, delta),
            Mathf.SmoothDamp(current.w, target.w, ref velocityRef.w, duration, maxSpeed, delta)).normalized;

        Vector4 correction = Vector4.Project(new Vector4(velocityRef.x, velocityRef.y, velocityRef.z, velocityRef.w), smoothVal);
        velocityRef.x -= correction.x;
        velocityRef.y -= correction.y;
        velocityRef.z -= correction.z;
        velocityRef.w -= correction.w;

        return new Quaternion(smoothVal.x, smoothVal.y, smoothVal.z, smoothVal.w);
    }


    #endregion



    #region Helper Maths

    /// <summary>
    /// Inverse Lerp without clamping
    /// </summary>
    public static float InverseLerp(float from, float to, float value)
    {
        if (to != from) // Prevent from dividing by zero
            return Mathf.Clamp((value - from) / (to - from), -1f, 1f);

        return 0;
    }


    public static float HyperCurve(float value)
    {
        return -(1f / (3.2f * value - 4)) - 0.25f;
    }


    #endregion



    #region Matrixes


    /// <summary>
    /// Extracting position from Matrix
    /// </summary>
    public static Vector3 PosFromMatrix(Matrix4x4 m)
    {
        return m.GetColumn(3);
    }

    /// <summary>
    /// Extracting rotation from Matrix
    /// </summary>
    public static Quaternion RotFromMatrix(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }

    /// <summary>
    /// Extracting scale from Matrix
    /// </summary>
    public static Vector3 ScaleFromMatrix(Matrix4x4 m)
    {
        return new Vector3
        (
            m.GetColumn(0).magnitude,
            m.GetColumn(1).magnitude,
            m.GetColumn(2).magnitude
        );
    }


    #endregion



    #region Physical Materials Stuff

    public static PhysicMaterial PMSliding 
    {
        get
        {
            if (_slidingMat) return _slidingMat;
            else
            {
                _slidingMat = new PhysicMaterial("Slide");
                _slidingMat.frictionCombine = PhysicMaterialCombine.Minimum;
                _slidingMat.dynamicFriction = 0f;
                _slidingMat.staticFriction = 0f;
                return _slidingMat;
            }
        }
    }

    private static PhysicMaterial _slidingMat;
    public static PhysicMaterial PMFrict 
    {
        get
        {
            if (_frictMat) return _frictMat;
            else
            {
                _frictMat = new PhysicMaterial("Friction");
                _frictMat.frictionCombine = PhysicMaterialCombine.Maximum;
                _frictMat.dynamicFriction = 1f;
                _frictMat.staticFriction = 1f;
                return _frictMat;
            }
        }
    }

    private static PhysicMaterial _frictMat;


    public static PhysicsMaterial2D PMSliding2D 
    {
        get
        {
            if (_slidingMat2D) return _slidingMat2D;
            else
            {
                _slidingMat2D = new PhysicsMaterial2D("Slide2D");
                _slidingMat2D.friction = 0f;
                return _slidingMat2D;
            }
        }
    }

    private static PhysicsMaterial2D _slidingMat2D;

    public static PhysicsMaterial2D PMFrict2D 
    { 
        get
        {
            if (_frictMat2D) return _frictMat2D;
            else
            {
                _frictMat2D = new PhysicsMaterial2D("Friction2D");
                _frictMat2D.friction = 5f;
                return _frictMat2D;
            }
        } 
    }

    private static PhysicsMaterial2D _frictMat2D;

    #endregion

}
