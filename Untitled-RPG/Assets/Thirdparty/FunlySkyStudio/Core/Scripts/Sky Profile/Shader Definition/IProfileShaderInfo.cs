using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  public interface IProfileDefinition
  {
    // Name of shader as defined in the shader file.
    string shaderName { get; }

    // Keywords used in this shader.
    ProfileFeatureSection[] features { get; }

    // List of sections and properties supported by this shader.
    ProfileGroupSection[] groups { get; }

    // Get a feature definition.
    ProfileFeatureDefinition GetFeatureDefinition(string featureKey);
  }
}

