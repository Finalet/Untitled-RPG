using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // A sphere poitn represents a position on the skybox (actually a sky sphere).
  // The positions are represented by a horizontal rotation, and a vertical rotation in radians.
  [Serializable]
  public class SpherePoint
  {
    public float horizontalRotation;
    public float verticalRotation;

    public const float MinHorizontalRotation = -Mathf.PI;
    public const float MaxHorizontalRotation = Mathf.PI;
    public const float MinVerticalRotation = -Mathf.PI / 2.0f;
    public const float MaxVerticalRotation = Mathf.PI / 2.0f;

    public SpherePoint(float horizontalRotation, float verticalRotation)
    {
      this.horizontalRotation = horizontalRotation;
      this.verticalRotation = verticalRotation;
    }

    public SpherePoint(Vector3 worldDirection)
    {
      Vector2 coords = SphereUtility.DirectionToSphericalCoordinate(worldDirection);
      horizontalRotation = coords.x;
      verticalRotation = coords.y;
    }

    public void SetFromWorldDirection(Vector3 worldDirection)
    {
      Vector2 coords = SphereUtility.DirectionToSphericalCoordinate(worldDirection);
      horizontalRotation = coords.x;
      verticalRotation = coords.y;
    }

    public Vector3 GetWorldDirection()
    {
      return SphereUtility.SphericalCoordinateToDirection(new Vector2(horizontalRotation, verticalRotation));
    }
  }
}

