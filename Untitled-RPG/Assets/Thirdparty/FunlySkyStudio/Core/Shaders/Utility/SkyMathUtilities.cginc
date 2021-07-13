#ifndef SKY_MATH_UTILITIES
#define SKY_MATH_UTILITIES

// Copyright(c) 2018 Funly LLC
//
// Author: Jason Ederle
// Description: Math utility functions for working with skyboxes.
// Contact: jason@funly.io

#include "UnityCG.cginc"

float rand(float2 co) {
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float AngleBetweenNormalizedVectors(float3 v1, float3 v2) {
    return acos(dot(v1, v2));
}

float Atan2Positive(float y, float x) {
  float angle = atan2(y, x);

  // This is the same as: angle = (angle > 0) ? angle : UNITY_PI + (UNITY_PI + angle)
  float isPositive = step(0, angle);
  float posAngle = angle * isPositive;
  float negAngle = (UNITY_PI + (UNITY_PI + angle)) * !isPositive;

  return posAngle + negAngle;
}

float2 DirectionToSphericalCoordinate(float3 direction) {
    float3 normDirection = normalize(direction);

    float horizontalRotation = Atan2Positive(normDirection.z, normDirection.x);
    float verticalRotation = 0;

    float angleToUp = AngleBetweenNormalizedVectors(direction, float3(0.0f, 1.0f, 0.0f));
    
    bool angleGreater = step(angleToUp, UNITY_HALF_PI);
    bool angleLesser = !angleGreater;

    float greaterValue = (-1.0f * (angleToUp - UNITY_HALF_PI)) * angleGreater;
    float lesserValue = (UNITY_HALF_PI - angleToUp) * angleLesser;

    verticalRotation = greaterValue + lesserValue;

    // This is the logic for the previous few lines.
    // if (angleToUp <= UNITY_HALF_PI) {
    //     verticalRotation = UNITY_HALF_PI - angleToUp;
    // }
    // else {
    //     verticalRotation = -1.0f * (angleToUp - UNITY_HALF_PI);
    // }

    return float2(horizontalRotation, verticalRotation);
}

float3 SphericalCoordinateToDirection(float2 spherePoint) {
    // Find the y coordinate and radius.
    float x = cos(spherePoint.y);
    float y = sin(spherePoint.y);

    float radius = x;
    float z = 0.0f;

    x = radius * cos(spherePoint.x);
    z = radius * sin(spherePoint.x);

    return normalize(float3(x, y, z));
}

inline float2 Rotate2d(float2 p, float angle) {
    return mul(float2x2(cos(angle), -sin(angle),
        sin(angle), cos(angle)),
        p);
}

float3 RotateAroundXAxis(float3 p, float angle) {
    float2 rotation = Rotate2d(p.zy, angle);
    return float3(p.x, rotation.y, rotation.x);
}

float3 RotateAroundYAxis(float3 p, float angle) {
    float2 rotation = Rotate2d(p.xz, angle);
    return float3(rotation.x, p.y, rotation.y);
}

float3 RotatePoint(float3 p, float xAxisRotation, float yAxisRotation) {
    float3 rotated = RotateAroundYAxis(p, yAxisRotation);
    return RotateAroundXAxis(rotated, xAxisRotation);
}

float AngleToReachTarget(float2 spot, float targetAngle) {
    float angle = Atan2Positive(spot.y, spot.x);
    return (UNITY_TWO_PI - angle) + targetAngle;
}

// Convert a 2D UV percent into spherical rotations.
float2 ConvertUVToSphericalCoordinate(float2 uv) {
    return float2(
      lerp(0.0f, UNITY_TWO_PI, uv.x),
      lerp(-UNITY_HALF_PI, UNITY_HALF_PI, uv.y));
}

// Convert spherical rotations into a 2D UV percent.
float2 ConvertSphericalCoordateToUV(float2 sphereCoord) {
    return float2(
            sphereCoord.x / UNITY_TWO_PI,
            (sphereCoord.y + UNITY_HALF_PI) / UNITY_PI
        );
}

float2 ConvertSphericalCoordinateToPercentage(float2 sphereCoord) {
    return float2(
        sphereCoord.x / UNITY_TWO_PI,
        (sphereCoord.y + UNITY_HALF_PI) / UNITY_PI
    );
}

float2 ConvertPercentToSphericalCoordinate(float2 percents) {
    return float2(
        lerp(0, UNITY_TWO_PI, percents.x),
        lerp(-UNITY_HALF_PI, UNITY_HALF_PI, percents.y)
    );
}

float2 ConvertPointToPercents(float3 starPoint) {
    float2 sphericalCoord = DirectionToSphericalCoordinate(starPoint);
    return ConvertSphericalCoordinateToPercentage(sphericalCoord);
}

float3 ConvertPercentToPoint(float2 percents) {
    float2 sphericalCoord = ConvertPercentToSphericalCoordinate(percents);
    return SphericalCoordinateToDirection(sphericalCoord);
}

bool IsSamePoint(float3 p1, float3 p2) {
    float xDelta = p1.x - p2.x;
    float yDelta = p1.y - p2.y;
    float zDelta = p1.z - p2.z;

    float nearZero = .01f;

    return xDelta < nearZero && yDelta < nearZero && zDelta < nearZero;
}

float2 GetPixelCeneteredUV(float2 uv, int imageSize) {
    float cellSize = 1.0f / (float)imageSize;
    float halfCellSize = cellSize / 2.0f;

    int xUnits = uv.x / cellSize;
    int yUnits = uv.y / cellSize;

    return float2(xUnits * cellSize + halfCellSize,
                  yUnits * cellSize + halfCellSize);
}

#endif
