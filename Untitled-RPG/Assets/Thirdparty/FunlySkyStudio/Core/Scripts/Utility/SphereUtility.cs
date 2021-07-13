using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Helper functions for working with points and angles on spheres.
  public abstract class SphereUtility : System.Object
  {
    private const float k_HalfPI = Mathf.PI / 2.0f;

    // Convert a world direction into a spherical coordinates in radians.
    public static Vector2 DirectionToSphericalCoordinate(Vector3 direction)
    {
      Vector3 normDirection = direction.normalized;

      float horizontalRotation = Atan2Positive(normDirection.z, normDirection.x);
      float verticalRotation = 0;

      float angleToUp = Vector3.Angle(direction, Vector3.up) * Mathf.Deg2Rad;

      if (angleToUp <= k_HalfPI) {
        verticalRotation = k_HalfPI - angleToUp;
      } else {
        verticalRotation = -1 * (angleToUp - k_HalfPI);
      }

      return new Vector2(horizontalRotation, verticalRotation);
    }

    // We could have used AngleAxis however we do it this way to exercise the same logic in the shader.
    public static Vector3 SphericalCoordinateToDirection(Vector2 coord)
    {
      // Find the y coordinate and radius.
      float x = Mathf.Cos(coord.y);
      float y = Mathf.Sin(coord.y);
    
      float radius = x;
      float z = 0.0f;

      x = radius * Mathf.Cos(coord.x);
      z = radius * Mathf.Sin(coord.x);

      return new Vector3(x, y, z);
    }

    public static float RadiusAtHeight(float yPos)
    {
      return Mathf.Abs(Mathf.Cos(Mathf.Asin(yPos)));
    }

    public static Vector3 SphericalToPoint(float yPosition, float radAngle)
    {
      float radius = RadiusAtHeight(yPosition);
      Vector3 p = new Vector3(
        radius * Mathf.Cos(radAngle),
        yPosition,
        radius * Mathf.Sin(radAngle));

      return p;
    }

    public static float RadAngleToPercent(float radAngle)
    {
      return radAngle / (2 * Mathf.PI);
    }

    public static float PercentToRadAngle(float percent)
    {
      return percent * (2 * Mathf.PI);
    }

    // Y value is assumed to be -1/1 value range (radius).
    public static float HeightToPercent(float yValue)
    {
      return yValue / 2.0f + .5f;
    }

    public static float PercentToHeight(float hPercent)
    {
      return Mathf.Lerp(-1.0f, 1.0f, hPercent);
    }

    public static float AngleToReachTarget(Vector2 point, float targetAngle)
    {
      // Always move in a positive angle to the target.
      float angle = Atan2Positive(point.y, point.x);
      return ((Mathf.PI * 2) - angle) + targetAngle;
    }

    // Returns angle in radians.
    public static float Atan2Positive(float y, float x)
    {
      float angle = Mathf.Atan2(y, x);
      if (angle < 0) {
        angle = Mathf.PI + (Mathf.PI + angle);
      }
      return angle;
    }

    public static Vector3 RotateAroundXAxis(Vector3 point, float angle)
    {
      Vector2 rotation = Rotate2d(new Vector2(point.z, point.y), angle);
      return new Vector3(point.x, rotation.y, rotation.x);
    }

    public static Vector3 RotateAroundYAxis(Vector3 point, float angle)
    {
      Vector2 rotation = Rotate2d(new Vector2(point.x, point.z), angle);
      return new Vector3(rotation.x, point.y, rotation.y);
    }

    public static Vector3 RotatePoint(Vector3 point, float xAxisRotation, float yAxisRotation)
    {
      Vector3 rotated = RotateAroundYAxis(point, yAxisRotation);
      rotated = RotateAroundXAxis(rotated, xAxisRotation);
      return rotated;
    }

    public static Vector2 Rotate2d(Vector2 pos, float angle)
    {
      Vector4 matrix = new Vector4(
        Mathf.Cos(angle), -Mathf.Sin(angle),
        Mathf.Sin(angle), Mathf.Cos(angle));

      return Matrix2x2Mult(matrix, pos);
    }

    // Simulate a matrix multiplication against a vector.
    public static Vector2 Matrix2x2Mult(Vector4 matrix, Vector2 pos)
    {
      return new Vector2(
        (matrix[0] * pos[0]) + (matrix[1] * pos[1]),
        (matrix[2] * pos[0]) + (matrix[3] * pos[1])
        );
    }

    public static void CalculateStarRotation(Vector3 star, out float xRotationAngle, out float yRotationAngle)
    {
      Vector3 starPos = new Vector3(star.x, star.y, star.z);

      yRotationAngle = AngleToReachTarget(
        new Vector2(starPos.x, starPos.z),
        90.0f * Mathf.Deg2Rad);

      starPos = RotateAroundYAxis(starPos, yRotationAngle);

      xRotationAngle = AngleToReachTarget(
        new Vector3(starPos.z, starPos.y),
        0.0f * Mathf.Deg2Rad);
    }

    // Convert a 2D UV percent into spherical rotations.
    public static Vector2 ConvertUVToSphericalCoordinate(Vector2 uv) {
      const float halfPi = Mathf.PI / 2.0f;
      return new Vector2(
        Mathf.Lerp(0.0f, Mathf.PI * 2.0f, uv.x),
        Mathf.Lerp(-halfPi, halfPi, uv.y));
    }

    // Convert spherical rotations into a 2D UV percent.
    public static Vector2 ConvertSphericalCoordateToUV(Vector2 sphereCoord) {
      const float halfPi = Mathf.PI / 2.0f;
      return new Vector2(
        sphereCoord.x / (Mathf.PI * 2.0f),
        (sphereCoord.y + halfPi) / Mathf.PI);
    }
  }
}

