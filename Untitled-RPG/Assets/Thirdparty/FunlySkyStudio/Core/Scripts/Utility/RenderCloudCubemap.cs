using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio {
  [RequireComponent(typeof(Camera))]
	public class RenderCloudCubemap : MonoBehaviour {
    public enum CubemapTextureFormat
    {
      RGBColor,
      RGBAColor,
      RGBALit
    };

    public const string kDefaultFilenamePrefix = "CloudCubemap";

    [Tooltip("Filename of the final output cubemap asset. It will be written to the same directory as the current scene.")]
    public string filenamePrefix = kDefaultFilenamePrefix;

    [Tooltip("Resolution of each face of the cubemap.")]
    public int faceWidth = 1024;

    [Tooltip("Format for the exported cubemap. RGBColor (Additive texture), RGBAColor (Color with alpha channel), RGBANormal (Normal lighting data encoded).")]
    public CubemapTextureFormat textureFormat = CubemapTextureFormat.RGBALit;
    public bool exportFaces;

  }
}

