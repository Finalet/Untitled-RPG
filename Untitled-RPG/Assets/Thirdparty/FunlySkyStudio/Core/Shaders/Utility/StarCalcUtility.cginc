#ifndef STAR_CALC_UTILITY
#define STAR_CALC_UTILITY

// Copyright(c) 2018 Funly LLC
//
// Author: Jason Ederle
// Description: Helper functions for generating stars.
// Contact: jason@funly.io

#include "UnityCG.cginc"
#include "SkyMathUtilities.cginc"

float RangedRandom(float2 randomSeed, float minValue, float maxValue)
{
  float dist = maxValue - minValue;
  float percent = rand(randomSeed);

  return minValue + (dist * percent);
}

float3 RandomUnitSpherePoint(float2 randomSeed)
{
  float z = RangedRandom(randomSeed * .81239f, -1.0f, 1.0f);
  float a = RangedRandom(randomSeed * .12303f, 0.0f, UNITY_TWO_PI);

  float r = sqrt(1.0f - z * z);

  float x = r * cos(a);
  float y = r * sin(a);

  return normalize(float3(x, y, z));
}

float DistanceSquared(float3 a, float3 b)
{
  float dx = a.x - b.x;
  float dy = a.y - b.y;
  float dz = a.z - b.z;

  return dx * dx + dy * dy + dz * dz;
}

float IsInRange(float value, float vMin, float vMax)
{
  float minValid = step(vMin, value);
  float maxValid = !step(vMax - .00001f, value);

  return minValid * maxValid;
}

float IsInsideBox(float2 origin, float2 checkCoord, float texelWidth)
{
  float xValid = IsInRange(checkCoord.x, origin.x, origin.x + texelWidth);
  float yValid = IsInRange(checkCoord.y, origin.y, origin.y + texelWidth);

  return xValid * yValid;
}

float IsSamePixel(float2 pixel1, float2 pixel2, float texelWidth)
{
  float2 pixel1Count = floor(pixel1 / texelWidth);
  float2 pixel2Count = floor(pixel2 / texelWidth);

  if ((int)pixel1Count.x == (int)pixel2Count.x &&
      (int)pixel1Count.y == (int)pixel2Count.y)
  {
    return 1;
  }
  else
  {
    return 0;
  }
}

#endif
