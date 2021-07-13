using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // This class manages the API for setting values on the sky shader.
  public class SkyMaterialController : System.Object
  {
    [SerializeField]
    private Material _skyboxMaterial;
    public Material SkyboxMaterial
    {
      get { return _skyboxMaterial; }
      set
      {
        _skyboxMaterial = value;
        RenderSettings.skybox = _skyboxMaterial;
      }
    }

    [SerializeField]
    private Color _skyColor = ColorHelper.ColorWithHex(0x2C2260);
    public Color SkyColor
    {
      get { return _skyColor; }
      set {
        _skyColor = value;
        SkyboxMaterial.SetColor("_GradientSkyUpperColor", _skyColor);
      }
    }

    [SerializeField]
    private Color _skyMiddleColor = Color.white;
    public Color SkyMiddleColor
    {
      get { return _skyMiddleColor; }
      set
      {
        _skyMiddleColor = value;
        SkyboxMaterial.SetColor("_GradientSkyMiddleColor", _skyMiddleColor);  
      }
    }

    [SerializeField]
    private Color _horizonColor = ColorHelper.ColorWithHex(0xE3C882);
    public Color HorizonColor
    {
      get { return _horizonColor; }
      set {
        _horizonColor = value;
        SkyboxMaterial.SetColor("_GradientSkyLowerColor", _horizonColor);
      }
    }

    [SerializeField, Range(-1, 1)]
    private float _gradientFadeBegin = 0.0f;
    public float GradientFadeBegin
    {
      get { return _gradientFadeBegin; }
      set {
        _gradientFadeBegin = value;
        ApplyGradientValuesOnMaterial();
      }
    }

    [SerializeField, Range(0, 2)]
    private float _gradientFadeLength = 1.0f;
    public float GradientFadeLength
    {
      get { return _gradientFadeLength; }
      set {
        _gradientFadeLength = value;
        ApplyGradientValuesOnMaterial();
      }
    }

    [SerializeField, Range(0, 1)]
    private float _skyMiddlePosition = .5f;
    public float SkyMiddlePosition
    {
      get { return _skyMiddlePosition; }
      set
      {
        _skyMiddlePosition = value;
        SkyboxMaterial.SetFloat("_GradientFadeMiddlePosition", _skyMiddlePosition);
      }
    }

    [SerializeField]
    private Cubemap _backgroundCubemap;
    public Cubemap BackgroundCubemap
    {
      get { return _backgroundCubemap; }
      set {
        _backgroundCubemap = value;
        SkyboxMaterial.SetTexture("_MainTex", _backgroundCubemap);
      }
    }

    [SerializeField, Range(-1, 1)]
    private float _starFadeBegin = .067f;
    public float StarFadeBegin
    {
      get { return _starFadeBegin; }
      set {
        _starFadeBegin = value;
        ApplyStarFadeValuesOnMaterial();
      }
    }

    [SerializeField, Range(0, 2)]
    private float _starFadeLength = .36f;
    public float StarFadeLength
    {
      get { return _starFadeLength; }
      set {
        _starFadeLength = value;
        ApplyStarFadeValuesOnMaterial();
      }
    }

    [SerializeField, Range(0, 1)]
    private float _horizonDistanceScale = .7f;
    public float HorizonDistanceScale
    {
      get { return _horizonDistanceScale; }
      set {
        _horizonDistanceScale = value;
        SkyboxMaterial.SetFloat("_HorizonScaleFactor", _horizonDistanceScale);
      }
    }


    // Stars Basic.
    [SerializeField]
    private Texture _starBasicCubemap;
    public Texture StarBasicCubemap {
      get { return _starBasicCubemap; }
      set {
        _starBasicCubemap = value;
        SkyboxMaterial.SetTexture("_StarBasicCubemap", _starBasicCubemap);
      }
    }

    [SerializeField]
    private float _starBasicTwinkleSpeed;
    public float StarBasicTwinkleSpeed {
      get { return _starBasicTwinkleSpeed; }
      set {
        _starBasicTwinkleSpeed = value;
        SkyboxMaterial.SetFloat("_StarBasicTwinkleSpeed", _starBasicTwinkleSpeed);
      }
    }

    [SerializeField]
    private float _starBasicTwinkleAmount;
    public float StarBasicTwinkleAmount {
      get { return _starBasicTwinkleAmount; }
      set {
        _starBasicTwinkleAmount = value;
        SkyboxMaterial.SetFloat("_StarBasicTwinkleAmount", _starBasicTwinkleAmount);
      }
    }

    [SerializeField]
    private float _starBasicOpacity;
    public float StarBasicOpacity {
      get { return _starBasicOpacity; }
      set {
        _starBasicOpacity = value;
        SkyboxMaterial.SetFloat("_StarBasicOpacity", _starBasicOpacity);
      }
    }
    
    [SerializeField]
    private Color _starBasicTintColor;
    public Color StarBasicTintColor {
      get { return _starBasicTintColor; }
      set {
        _starBasicTintColor = value;
        SkyboxMaterial.SetColor("_StarBasicTintColor", _starBasicTintColor);
      }
    }

    [SerializeField]
    private float _starBasicExponent;
    public float StarBasicExponent {
      get { return _starBasicExponent; }
      set {
        _starBasicExponent = value;
        SkyboxMaterial.SetFloat("_StarBasicExponent", _starBasicExponent);
      }
    }

    [SerializeField]
    private float _starBasicIntensity;
    public float StarBasicIntensity {
      get { return _starBasicIntensity; }
      set {
        _starBasicIntensity = value;
        SkyboxMaterial.SetFloat("_StarBasicHDRBoost", _starBasicIntensity);
      }
    }
    
    // Star layer 1.
    [SerializeField]
    private Texture _starLayer1Texture;
    public Texture StarLayer1Texture
    {
      get { return _starLayer1Texture; }
      set {
        _starLayer1Texture = value;
        SkyboxMaterial.SetTexture("_StarLayer1Tex", _starLayer1Texture);
      }
    }

    [SerializeField]
    private Texture2D _starLayer1DataTexture;
    public Texture2D StarLayer1DataTexture
    {
      get { return _starLayer1DataTexture; }
      set {
        _starLayer1DataTexture = value;
        SkyboxMaterial.SetTexture("_StarLayer1DataTex", value);
      }
    }

    [SerializeField]
    private Color _starLayer1Color;
    public Color StarLayer1Color
    {
      get { return _starLayer1Color; }
      set {
        _starLayer1Color = value;
        SkyboxMaterial.SetColor("_StarLayer1Color", _starLayer1Color);
      }
    }

    [SerializeField, Range(0, .1f)]
    private float _starLayer1MaxRadius = .007f;
    public float StarLayer1MaxRadius
    {
      get { return _starLayer1MaxRadius; }
      set {
        _starLayer1MaxRadius = value;
        SkyboxMaterial.SetFloat("_StarLayer1MaxRadius", _starLayer1MaxRadius);
      }
    }

    [SerializeField, Range(0, 1)]
    private float _starLayer1TwinkleAmount = .7f;
    public float StarLayer1TwinkleAmount
    {
      get { return _starLayer1TwinkleAmount; }
      set {
        _starLayer1TwinkleAmount = value;
        SkyboxMaterial.SetFloat("_StarLayer1TwinkleAmount", _starLayer1TwinkleAmount);
      }
    }

    [SerializeField, Range(0, 10)]
    private float _starLayer1TwinkleSpeed = .7f;
    public float StarLayer1TwinkleSpeed
    {
      get { return _starLayer1TwinkleSpeed; }
      set {
        _starLayer1TwinkleSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer1TwinkleSpeed", _starLayer1TwinkleSpeed);
      }
    }

    [SerializeField, Range(0, 10)]
    private float _starLayer1RotationSpeed = .7f;
    public float StarLayer1RotationSpeed
    {
      get { return _starLayer1RotationSpeed; }
      set {
        _starLayer1RotationSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer1RotationSpeed", _starLayer1RotationSpeed);
      }
    }

    [SerializeField, Range(0.0001f, .9999f)]
    private float _starLayer1EdgeFeathering = .2f;
    public float StarLayer1EdgeFeathering
    {
      get { return _starLayer1EdgeFeathering; }
      set {
        _starLayer1EdgeFeathering = value;
        SkyboxMaterial.SetFloat("_StarLayer1EdgeFade", _starLayer1EdgeFeathering);
      }
    }

    [SerializeField, Range(1, 10)]
    private float _starLayer1BloomFilterBoost;
    public float StarLayer1BloomFilterBoost
    {
      get { return _starLayer1BloomFilterBoost; }
      set {
        _starLayer1BloomFilterBoost = value;
        SkyboxMaterial.SetFloat("_StarLayer1HDRBoost", _starLayer1BloomFilterBoost);
      }
    }

    [SerializeField]
    private Vector4 _starLayer1SpriteDimensions = Vector4.zero;
    public void SetStarLayer1SpriteDimensions(int columns, int rows)
    {
      _starLayer1SpriteDimensions.x = columns;
      _starLayer1SpriteDimensions.y = rows;

      SkyboxMaterial.SetVector("_StarLayer1SpriteDimensions", _starLayer1SpriteDimensions);
    }

    public Vector2 GetStarLayer1SpriteDimensions()
    {
      return new Vector2(_starLayer1SpriteDimensions.x, _starLayer1SpriteDimensions.y);
    }

    [SerializeField]
    private int _starLayer1SpriteItemCount = 1;
    public int StarLayer1SpriteItemCount
    {
      get { return _starLayer1SpriteItemCount; }
      set {
        _starLayer1SpriteItemCount = value;
        SkyboxMaterial.SetInt("_StarLayer1SpriteItemCount", _starLayer1SpriteItemCount);
      }
    }

    [SerializeField, Range(0.0f, 1.0f)]
    private float _starLayer1SpriteAnimationSpeed = 1.0f;
    public float StarLayer1SpriteAnimationSpeed
    {
      get { return _starLayer1SpriteAnimationSpeed; }
      set {
        _starLayer1SpriteAnimationSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer1SpriteAnimationSpeed", _starLayer1SpriteAnimationSpeed);
      }
    }

    // Star layer 2.
    [SerializeField]
    private Texture _starLayer2Texture;
    public Texture StarLayer2Texture
    {
      get { return _starLayer2Texture; }
      set {
        _starLayer2Texture = value;
        SkyboxMaterial.SetTexture("_StarLayer2Tex", _starLayer2Texture);
      }
    }

    [SerializeField]
    private Texture2D _starLayer2DataTexture;
    public Texture2D StarLayer2DataTexture
    {
      get { return _starLayer2DataTexture; }
      set {
        _starLayer2DataTexture = value;
        SkyboxMaterial.SetTexture("_StarLayer2DataTex", value);
      }
    }

    [SerializeField]
    private Color _starLayer2Color;
    public Color StarLayer2Color
    {
      get { return _starLayer2Color; }
      set {
        _starLayer2Color = value;
        SkyboxMaterial.SetColor("_StarLayer2Color", _starLayer2Color);
      }
    }

    [SerializeField, Range(0, .1f)]
    private float _starLayer2MaxRadius = .007f;
    public float StarLayer2MaxRadius
    {
      get { return _starLayer2MaxRadius; }
      set {
        _starLayer2MaxRadius = value;
        SkyboxMaterial.SetFloat("_StarLayer2MaxRadius", _starLayer2MaxRadius);
      }
    }

    [SerializeField, Range(0, 1)]
    private float _starLayer2TwinkleAmount = .7f;
    public float StarLayer2TwinkleAmount
    {
      get { return _starLayer2TwinkleAmount; }
      set {
        _starLayer2TwinkleAmount = value;
        SkyboxMaterial.SetFloat("_StarLayer2TwinkleAmount", _starLayer2TwinkleAmount);
      }
    }

    [SerializeField, Range(0, 10)]
    private float _starLayer2TwinkleSpeed = .7f;
    public float StarLayer2TwinkleSpeed
    {
      get { return _starLayer2TwinkleSpeed; }
      set {
        _starLayer2TwinkleSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer2TwinkleSpeed", _starLayer2TwinkleSpeed);
      }
    }

    [SerializeField, Range(0, 10)]
    private float _starLayer2RotationSpeed = .7f;
    public float StarLayer2RotationSpeed
    {
      get { return _starLayer2RotationSpeed; }
      set {
        _starLayer2RotationSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer2RotationSpeed", _starLayer2RotationSpeed);
      }
    }

    [SerializeField, Range(0.0001f, .9999f)]
    private float _starLayer2EdgeFeathering = .2f;
    public float StarLayer2EdgeFeathering
    {
      get { return _starLayer2EdgeFeathering; }
      set {
        _starLayer2EdgeFeathering = value;
        SkyboxMaterial.SetFloat("_StarLayer2EdgeFade", _starLayer2EdgeFeathering);
      }
    }

    [SerializeField, Range(1, 10)]
    private float _starLayer2BloomFilterBoost;
    public float StarLayer2BloomFilterBoost
    {
      get { return _starLayer2BloomFilterBoost; }
      set {
        _starLayer2BloomFilterBoost = value;
        SkyboxMaterial.SetFloat("_StarLayer2HDRBoost", _starLayer2BloomFilterBoost);
      }
    }

    [SerializeField]
    private Vector4 _starLayer2SpriteDimensions = Vector4.zero;
    public void SetStarLayer2SpriteDimensions(int columns, int rows)
    {
      _starLayer2SpriteDimensions.x = columns;
      _starLayer2SpriteDimensions.y = rows;

      SkyboxMaterial.SetVector("_StarLayer2SpriteDimensions", _starLayer2SpriteDimensions);
    }

    public Vector2 GetStarLayer2SpriteDimensions()
    {
      return new Vector2(_starLayer2SpriteDimensions.x, _starLayer2SpriteDimensions.y);
    }

    [SerializeField]
    private int _starLayer2SpriteItemCount = 1;
    public int StarLayer2SpriteItemCount
    {
      get { return _starLayer2SpriteItemCount; }
      set {
        _starLayer2SpriteItemCount = value;
        SkyboxMaterial.SetInt("_StarLayer2SpriteItemCount", _starLayer2SpriteItemCount);
      }
    }

    [SerializeField, Range(0.0f, 1.0f)]
    private float _starLayer2SpriteAnimationSpeed = 1.0f;
    public float StarLayer2SpriteAnimationSpeed
    {
      get { return _starLayer2SpriteAnimationSpeed; }
      set {
        _starLayer2SpriteAnimationSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer2SpriteAnimationSpeed", _starLayer2SpriteAnimationSpeed);
      }
    }

    // Star layer 3.
    [SerializeField]
    private Texture _starLayer3Texture;
    public Texture StarLayer3Texture
    {
      get { return _starLayer3Texture; }
      set {
        _starLayer3Texture = value;
        SkyboxMaterial.SetTexture("_StarLayer3Tex", _starLayer3Texture);
      }
    }

    [SerializeField]
    private Texture2D _starLayer3DataTexture;
    public Texture2D StarLayer3DataTexture
    {
      get { return _starLayer3DataTexture; }
      set {
        _starLayer3DataTexture = value;
        SkyboxMaterial.SetTexture("_StarLayer3DataTex", value);
      }
    }

    [SerializeField]
    private Color _starLayer3Color;
    public Color StarLayer3Color
    {
      get { return _starLayer3Color; }
      set {
        _starLayer3Color = value;
        SkyboxMaterial.SetColor("_StarLayer3Color", _starLayer3Color);
      }
    }

    [SerializeField, Range(0, .1f)]
    private float _starLayer3MaxRadius = .007f;
    public float StarLayer3MaxRadius
    {
      get { return _starLayer3MaxRadius; }
      set {
        _starLayer3MaxRadius = value;
        SkyboxMaterial.SetFloat("_StarLayer3MaxRadius", _starLayer3MaxRadius);
      }
    }

    [SerializeField, Range(0, 1)]
    private float _starLayer3TwinkleAmount = .7f;
    public float StarLayer3TwinkleAmount
    {
      get { return _starLayer3TwinkleAmount; }
      set {
        _starLayer3TwinkleAmount = value;
        SkyboxMaterial.SetFloat("_StarLayer3TwinkleAmount", _starLayer3TwinkleAmount);
      }
    }

    [SerializeField, Range(0, 10)]
    private float _starLayer3TwinkleSpeed = .7f;
    public float StarLayer3TwinkleSpeed
    {
      get { return _starLayer3TwinkleSpeed; }
      set {
        _starLayer3TwinkleSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer3TwinkleSpeed", _starLayer3TwinkleSpeed);
      }
    }

    [SerializeField, Range(0, 10)]
    private float _starLayer3RotationSpeed = .7f;
    public float StarLayer3RotationSpeed
    {
      get { return _starLayer3RotationSpeed; }
      set {
        _starLayer3RotationSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer3RotationSpeed", _starLayer3RotationSpeed);
      }
    }

    [SerializeField, Range(0.0001f, .9999f)]
    private float _starLayer3EdgeFeathering = .2f;
    public float StarLayer3EdgeFeathering
    {
      get { return _starLayer3EdgeFeathering; }
      set {
        _starLayer3EdgeFeathering = value;
        SkyboxMaterial.SetFloat("_StarLayer3EdgeFade", _starLayer3EdgeFeathering);
      }
    }

    [SerializeField, Range(1, 10)]
    private float _starLayer3BloomFilterBoost;
    public float StarLayer3BloomFilterBoost
    {
      get { return _starLayer3BloomFilterBoost; }
      set {
        _starLayer3BloomFilterBoost = value;
        SkyboxMaterial.SetFloat("_StarLayer3HDRBoost", _starLayer3BloomFilterBoost);
      }
    }

    [SerializeField]
    private Vector4 _starLayer3SpriteDimensions = Vector4.zero;
    public void SetStarLayer3SpriteDimensions(int columns, int rows)
    {
      _starLayer3SpriteDimensions.x = columns;
      _starLayer3SpriteDimensions.y = rows;

      SkyboxMaterial.SetVector("_StarLayer3SpriteDimensions", _starLayer3SpriteDimensions);
    }

    public Vector2 GetStarLayer3SpriteDimensions()
    {
      return new Vector2(_starLayer3SpriteDimensions.x, _starLayer3SpriteDimensions.y);
    }

    [SerializeField]
    private int _starLayer3SpriteItemCount = 1;
    public int StarLayer3SpriteItemCount
    {
      get { return _starLayer3SpriteItemCount; }
      set {
        _starLayer3SpriteItemCount = value;
        SkyboxMaterial.SetInt("_StarLayer3SpriteItemCount", _starLayer3SpriteItemCount);
      }
    }

    [SerializeField, Range(0.0f, 1.0f)]
    private float _starLayer3SpriteAnimationSpeed = 1.0f;
    public float StarLayer3SpriteAnimationSpeed
    {
      get { return _starLayer3SpriteAnimationSpeed; }
      set {
        _starLayer3SpriteAnimationSpeed = value;
        SkyboxMaterial.SetFloat("_StarLayer3SpriteAnimationSpeed", _starLayer3SpriteAnimationSpeed);
      }
    }

    // Moon
    [SerializeField]
    private Texture _moonTexture;
    public Texture MoonTexture
    {
      get { return _moonTexture; }
      set {
        _moonTexture = value;
        SkyboxMaterial.SetTexture("_MoonTex", _moonTexture);
      }
    }

    [SerializeField]
    private float _moonRotationSpeed = 0;
    public float MoonRotationSpeed
    {
      get { return _moonRotationSpeed; }
      set
      {
        _moonRotationSpeed = value;
        SkyboxMaterial.SetFloat("_MoonRotationSpeed", _moonRotationSpeed);
      }
    }

    [SerializeField]
    private Color _moonColor = Color.white;
    public Color MoonColor
    {
      get { return _moonColor; }
      set {
        _moonColor = value;
        SkyboxMaterial.SetColor("_MoonColor", _moonColor);
      }
    }

    [SerializeField]
    private Vector3 _moonDirection = Vector3.right;
    public Vector3 MoonDirection
    {
      get { return _moonDirection; }
      set {
        _moonDirection = value.normalized;
        SkyboxMaterial.SetVector("_MoonPosition", _moonDirection);
      }
    }

    [SerializeField]
    private Matrix4x4 _moonWorldToLocalMatrix = Matrix4x4.identity;
    public Matrix4x4 MoonWorldToLocalMatrix {
      get { return _moonWorldToLocalMatrix; }
      set {
        _moonWorldToLocalMatrix = value;
        SkyboxMaterial.SetMatrix("_MoonWorldToLocalMat", _moonWorldToLocalMatrix);
      }
    }

    [SerializeField, Range(0, 1)]
    private float _moonSize = .1f;
    public float MoonSize
    {
      get { return _moonSize; }
      set {
        _moonSize = value;
        SkyboxMaterial.SetFloat("_MoonRadius", _moonSize);
      }
    }

    [SerializeField, Range(0.0001f, .9999f)]
    private float _moonEdgeFeathering = 0.085f;
    public float MoonEdgeFeathering
    {
      get { return _moonEdgeFeathering; }
      set {
        _moonEdgeFeathering = value;
        SkyboxMaterial.SetFloat("_MoonEdgeFade", _moonEdgeFeathering);
      }
    }

    [SerializeField, Range(1, 10)]
    private float _moonBloomFilterBoost = 1.0f;
    public float MoonBloomFilterBoost
    {
      get { return _moonBloomFilterBoost; }
      set {
        _moonBloomFilterBoost = value;
        SkyboxMaterial.SetFloat("_MoonHDRBoost", _moonBloomFilterBoost);
      }
    }

    [SerializeField]
    private Vector4 _moonSpriteDimensions = Vector4.zero;
    public void SetMoonSpriteDimensions(int columns, int rows)
    {
      _moonSpriteDimensions.x = columns;
      _moonSpriteDimensions.y = rows;

      SkyboxMaterial.SetVector("_MoonSpriteDimensions", _moonSpriteDimensions);
    }

    public Vector2 GetMoonSpriteDimensions()
    {
      return new Vector2(_moonSpriteDimensions.x, _moonSpriteDimensions.y);
    }
   
    [SerializeField]
    private int _moonSpriteItemCount = 1;
    public int MoonSpriteItemCount
    {
      get { return _moonSpriteItemCount; }
      set {
        _moonSpriteItemCount = value;
        SkyboxMaterial.SetInt("_MoonSpriteItemCount", _moonSpriteItemCount);
      }
    }

    [SerializeField, Range(0.0f, 1.0f)]
    private float _moonSpriteAnimationSpeed = 1.0f;
    public float MoonSpriteAnimationSpeed
    {
      get { return _moonSpriteAnimationSpeed; }
      set {
        _moonSpriteAnimationSpeed = value;
        SkyboxMaterial.SetFloat("_MoonSpriteAnimationSpeed", _moonSpriteAnimationSpeed);
      }
    }

    // Sun
    [SerializeField]
    private Texture _sunTexture;
    public Texture SunTexture
    {
      get { return _sunTexture; }
      set {
        _sunTexture = value;
        SkyboxMaterial.SetTexture("_SunTex", _sunTexture);
      }
    }

    [SerializeField]
    private Color _sunColor = Color.white;
    public Color SunColor
    {
      get { return _sunColor; }
      set {
        _sunColor = value;
        SkyboxMaterial.SetColor("_SunColor", _sunColor);
      }
    }

    [SerializeField]
    private float _sunRotationSpeed = 0;
    public float SunRotationSpeed
    {
      get { return _sunRotationSpeed; }
      set {
        _sunRotationSpeed = value;
        SkyboxMaterial.SetFloat("_SunRotationSpeed", _sunRotationSpeed);
      }
    }

    [SerializeField]
    private Vector3 _sunDirection = Vector3.right;
    public Vector3 SunDirection
    {
      get { return _sunDirection; }
      set {
        _sunDirection = value.normalized;
        SkyboxMaterial.SetVector("_SunPosition", _sunDirection);
      }
    }

    [SerializeField]
    private Matrix4x4 _sunWorldToLocalMatrix = Matrix4x4.identity;
    public Matrix4x4 SunWorldToLocalMatrix {
      get { return _sunWorldToLocalMatrix; }
      set {
        _sunWorldToLocalMatrix = value;
        SkyboxMaterial.SetMatrix("_SunWorldToLocalMat", _sunWorldToLocalMatrix);
      }
    }

    [SerializeField, Range(0, 1)]
    private float _sunSize = .1f;
    public float SunSize
    {
      get { return _sunSize; }
      set {
        _sunSize = value;
        SkyboxMaterial.SetFloat("_SunRadius", _sunSize);
      }
    }

    [SerializeField, Range(0.0001f, .9999f)]
    private float _sunEdgeFeathering = 0.085f;
    public float SunEdgeFeathering
    {
      get { return _sunEdgeFeathering; }
      set {
        _sunEdgeFeathering = value;
        SkyboxMaterial.SetFloat("_SunEdgeFade", _sunEdgeFeathering);
      }
    }

    [SerializeField, Range(1, 10)]
    private float _sunBloomFilterBoost = 1.0f;
    public float SunBloomFilterBoost
    {
      get { return _sunBloomFilterBoost; }
      set {
        _sunBloomFilterBoost = value;
        SkyboxMaterial.SetFloat("_SunHDRBoost", _sunBloomFilterBoost);
      }
    }

    [SerializeField]
    private Vector4 _sunSpriteDimensions = Vector4.zero;
    public void SetSunSpriteDimensions(int columns, int rows)
    {
      _sunSpriteDimensions.x = columns;
      _sunSpriteDimensions.y = rows;

      SkyboxMaterial.SetVector("_SunSpriteDimensions", _sunSpriteDimensions);
    }

    public Vector2 GetSunSpriteDimensions()
    {
      return new Vector2(_sunSpriteDimensions.x, _sunSpriteDimensions.y);
    }

    [SerializeField]
    private int _sunSpriteItemCount = 1;
    public int SunSpriteItemCount
    {
      get { return _sunSpriteItemCount; }
      set {
        _sunSpriteItemCount = value;
        SkyboxMaterial.SetInt("_SunSpriteItemCount", _sunSpriteItemCount);
      }
    }

    [SerializeField, Range(0.0f, 1.0f)]
    private float _sunSpriteAnimationSpeed = 1.0f;
    public float SunSpriteAnimationSpeed
    {
      get { return _sunSpriteAnimationSpeed; }
      set {
        _sunSpriteAnimationSpeed = value;
        SkyboxMaterial.SetFloat("_SunSpriteAnimationSpeed", _sunSpriteAnimationSpeed);
      }
    }

    // Clouds.
    [SerializeField, Range(-1, 1)]
    private float _cloudBegin = .2f;
    public float CloudBegin
    {
      get { return _cloudBegin; }
      set
      {
        _cloudBegin = value;
        SkyboxMaterial.SetFloat("_CloudBegin", _cloudBegin);
      }
    }

    private float _cloudTextureTiling;
    public float CloudTextureTiling
    {
      get { return _cloudTextureTiling; }
      set
      {
        _cloudTextureTiling = value;
        SkyboxMaterial.SetFloat("_CloudTextureTiling", _cloudTextureTiling);
      }
    }
    [SerializeField]
    private Color _cloudColor = Color.white;
    public Color CloudColor
    {
      get { return _cloudColor; }
      set
      {
        _cloudColor = value;
        SkyboxMaterial.SetColor("_CloudColor", _cloudColor);
      }
    }

    [SerializeField]
    private Texture _cloudTexture = null;
    public Texture CloudTexture
    {
      get
      {
        return _cloudTexture != null ? _cloudTexture : Texture2D.blackTexture;
      }
      set
      {
        _cloudTexture = value;
        SkyboxMaterial.SetTexture("_CloudNoiseTexture", _cloudTexture);
      }
    }

    [SerializeField]
    private Texture _artCloudCustomTexture = null;
    public Texture ArtCloudCustomTexture
    {
      get {
        return _artCloudCustomTexture != null ? _artCloudCustomTexture : Texture2D.blackTexture;
      }
      set {
        _artCloudCustomTexture = value;
        SkyboxMaterial.SetTexture("_ArtCloudCustomTexture", _artCloudCustomTexture);
      }
    }

    [SerializeField]
    private float _cloudDensity = 0;
    public float CloudDensity
    {
      get { return _cloudDensity; }
      set
      {
        _cloudDensity = value;
        SkyboxMaterial.SetFloat("_CloudDensity", _cloudDensity);
      }
    }

    [SerializeField]
    private float _cloudSpeed = 0;
    public float CloudSpeed
    {
      get { return _cloudSpeed; }
      set {
        _cloudSpeed = value;
        SkyboxMaterial.SetFloat("_CloudSpeed", _cloudSpeed);
      }
    }

    [SerializeField]
    private float _cloudDirection = 0;
    public float CloudDirection
    {
      get { return _cloudDirection; }
      set
      {
        _cloudDirection = value;
        SkyboxMaterial.SetFloat("_CloudDirection", _cloudDirection);
      }
    }

    [SerializeField]
    private float _cloudHeight = 0;
    public float CloudHeight
    {
      get { return _cloudHeight; }
      set {
        _cloudHeight = value;
        SkyboxMaterial.SetFloat("_CloudHeight", _cloudHeight);
      }
    }

    [SerializeField]
    private Color _cloudColor1 = Color.white;
    public Color CloudColor1
    {
      get { return _cloudColor1; }
      set
      {
        _cloudColor1 = value;
        SkyboxMaterial.SetColor("_CloudColor1", _cloudColor1);
      }
    }

    [SerializeField]
    private Color _cloudColor2 = Color.white;
    public Color CloudColor2
    {
      get { return _cloudColor2; }
      set {
        _cloudColor2 = value;
        SkyboxMaterial.SetColor("_CloudColor2", _cloudColor2);
      }
    }

    [SerializeField]
    private float _cloudFadePosition = 0;
    public float CloudFadePosition
    {
      get { return _cloudFadePosition; }
      set {
        _cloudFadePosition = value;
        SkyboxMaterial.SetFloat("_CloudFadePosition", _cloudFadePosition);
      }
    }

    [SerializeField]
    private float _cloudFadeAmount = .5f;
    public float CloudFadeAmount
    {
      get { return _cloudFadeAmount; }
      set {
        _cloudFadeAmount = value;
        SkyboxMaterial.SetFloat("_CloudFadeAmount", _cloudFadeAmount);
      }
    }


    // Clouds Cubemap.
    [SerializeField]
    private Texture _cloudCubemap;
    public Texture CloudCubemap
    {
      get { return _cloudCubemap; }
      set {
        _cloudCubemap = value;
        SkyboxMaterial.SetTexture("_CloudCubemapTexture", _cloudCubemap);
      }
    }

    [SerializeField]
    private float _cloudCubemapRotationSpeed;
    public float CloudCubemapRotationSpeed
    {
      get { return _cloudCubemapRotationSpeed; }
      set {
        _cloudCubemapRotationSpeed = value;
        SkyboxMaterial.SetFloat("_CloudCubemapRotationSpeed", _cloudCubemapRotationSpeed);
      }
    }

    [SerializeField]
    private Texture _cloudCubemapDoubleLayerCustomTexture;
    public Texture CloudCubemapDoubleLayerCustomTexture 
    {
      get { return _cloudCubemapDoubleLayerCustomTexture; }
      set {
        _cloudCubemapDoubleLayerCustomTexture = value;
        SkyboxMaterial.SetTexture("_CloudCubemapDoubleTexture", _cloudCubemapDoubleLayerCustomTexture);
      }
    }

    [SerializeField]
    private float _cloudCubemapDoubleLayerRotationSpeed;
    public float CloudCubemapDoubleLayerRotationSpeed
    {
      get { return _cloudCubemapDoubleLayerRotationSpeed; }
      set {
        _cloudCubemapDoubleLayerRotationSpeed = value;
        SkyboxMaterial.SetFloat("_CloudCubemapDoubleLayerRotationSpeed", _cloudCubemapDoubleLayerRotationSpeed);
      }
    }

    [SerializeField]
    private float _cloudCubemapDoubleLayerHeight;
    public float CloudCubemapDoubleLayerHeight
    {
      get { return _cloudCubemapDoubleLayerHeight; }
      set {
        _cloudCubemapDoubleLayerHeight = value;
        SkyboxMaterial.SetFloat("_CloudCubemapDoubleLayerHeight", _cloudCubemapDoubleLayerHeight);
      }
    }

    [SerializeField]
    private Color _cloudCubemapDoubleLayerTintColor = Color.white;
    public Color CloudCubemapDoubleLayerTintColor
    {
      get { return _cloudCubemapDoubleLayerTintColor; }
      set {
        _cloudCubemapDoubleLayerTintColor = value;
        SkyboxMaterial.SetColor("_CloudCubemapDoubleLayerTintColor", _cloudCubemapDoubleLayerTintColor);
      }
    }

    [SerializeField]
    private Color _cloudCubemapTintColor = Color.white;
    public Color CloudCubemapTintColor
    {
      get { return _cloudCubemapTintColor; }
      set {
        _cloudCubemapTintColor = value;
        SkyboxMaterial.SetColor("_CloudCubemapTintColor", _cloudCubemapTintColor);
      }
    }

    [SerializeField]
    private float _cloudCubemapHeight;
    public float CloudCubemapHeight
    {
      get { return _cloudCubemapHeight; }
      set {
        _cloudCubemapHeight = value;
        SkyboxMaterial.SetFloat("_CloudCubemapHeight", _cloudCubemapHeight);
      }
    }

    // Clouds Normal Cubemap.
    [SerializeField]
    private Texture _cloudCubemapNormalTexture;
    public Texture CloudCubemapNormalTexture
    {
      get { return _cloudCubemap; }
      set {
        _cloudCubemapNormalTexture = value;
        SkyboxMaterial.SetTexture("_CloudCubemapNormalTexture", _cloudCubemapNormalTexture);
      }
    }

    [SerializeField]
    private Color _cloudCubemapNormalLitColor = Color.white;
    public Color CloudCubemapNormalLitColor
    {
      get { return _cloudCubemapNormalLitColor; }
      set {
        _cloudCubemapNormalLitColor = value;
        SkyboxMaterial.SetColor("_CloudCubemapNormalLitColor", _cloudCubemapNormalLitColor);
      }
    }

    [SerializeField]
    private Color _cloudCubemapNormalShadowColor = Color.gray;
    public Color CloudCubemapNormalShadowColor
    {
      get { return _cloudCubemapNormalShadowColor; }
      set {
        _cloudCubemapNormalShadowColor = value;
        SkyboxMaterial.SetColor("_CloudCubemapNormalShadowColor", _cloudCubemapNormalShadowColor);
      }
    }

    [SerializeField]
    private float _cloudCubemapNormalRotationSpeed;
    public float CloudCubemapNormalRotationSpeed
    {
      get { return _cloudCubemapNormalRotationSpeed; }
      set {
        _cloudCubemapNormalRotationSpeed = value;
        SkyboxMaterial.SetFloat("_CloudCubemapNormalRotationSpeed", _cloudCubemapNormalRotationSpeed);
      }
    }

    [SerializeField]
    private float _cloudCubemapNormalHeight;
    public float CloudCubemapNormalHeight
    {
      get { return _cloudCubemapNormalHeight; }
      set {
        _cloudCubemapNormalHeight = value;
        SkyboxMaterial.SetFloat("_CloudCubemapNormalHeight", _cloudCubemapNormalHeight);
      }
    }

    [SerializeField]
    private float _cloudCubemapNormalAmbientItensity;
    public float CloudCubemapNormalAmbientIntensity
    {
      get { return _cloudCubemapNormalAmbientItensity; }
      set {
        _cloudCubemapNormalAmbientItensity = value;
        SkyboxMaterial.SetFloat("_CloudCubemapNormalAmbientIntensity", _cloudCubemapNormalAmbientItensity);
      }
    }
    
    [SerializeField]
    private Texture _cloudCubemapNormalDoubleLayerCustomTexture;
    public Texture CloudCubemapNormalDoubleLayerCustomTexture 
    {
      get { return _cloudCubemapNormalDoubleLayerCustomTexture; }
      set {
        _cloudCubemapNormalDoubleLayerCustomTexture = value;
        SkyboxMaterial.SetTexture("_CloudCubemapNormalDoubleTexture", _cloudCubemapNormalDoubleLayerCustomTexture);
      }
    }

    [SerializeField]
    private float _cloudCubemapNormalDoubleLayerRotationSpeed;
    public float CloudCubemapNormalDoubleLayerRotationSpeed
    {
      get { return _cloudCubemapNormalDoubleLayerRotationSpeed; }
      set {
        _cloudCubemapNormalDoubleLayerRotationSpeed = value;
        SkyboxMaterial.SetFloat("_CloudCubemapNormalDoubleLayerRotationSpeed", _cloudCubemapNormalDoubleLayerRotationSpeed);
      }
    }

    [SerializeField]
    private float _cloudCubemapNormalDoubleLayerHeight;
    public float CloudCubemapNormalDoubleLayerHeight
    {
      get { return _cloudCubemapDoubleLayerHeight; }
      set {
        _cloudCubemapNormalDoubleLayerHeight = value;
        SkyboxMaterial.SetFloat("_CloudCubemapNormalDoubleLayerHeight", _cloudCubemapNormalDoubleLayerHeight);
      }
    }

    [SerializeField]
    private Color _cloudCubemapNormalDoubleLayerLitColor = Color.white;
    public Color CloudCubemapNormalDoubleLayerLitColor
    {
      get { return _cloudCubemapNormalDoubleLayerLitColor; }
      set {
        _cloudCubemapNormalDoubleLayerLitColor = value;
        SkyboxMaterial.SetColor("_CloudCubemapNormalDoubleLitColor", _cloudCubemapNormalDoubleLayerLitColor);
      }
    }

    [SerializeField]
    private Color _cloudCubemapNormalDoubleLayerShadowColor = Color.gray;
    public Color CloudCubemapNormalDoubleLayerShadowColor
    {
      get { return _cloudCubemapNormalDoubleLayerShadowColor; }
      set {
        _cloudCubemapNormalDoubleLayerShadowColor = value;
        SkyboxMaterial.SetColor("_CloudCubemapNormalDoubleShadowColor", _cloudCubemapNormalDoubleLayerShadowColor);
      }
    }

    // Direction that points to the light source.
    [SerializeField]
    private Vector3 _cloudCubemapNormalLightDirection = new Vector3(0, 1, 0);
    public Vector3 CloudCubemapNormalLightDirection
    {
      get { return _cloudCubemapNormalLightDirection; }
      set {
        _cloudCubemapNormalLightDirection = value;
        SkyboxMaterial.SetVector("_CloudCubemapNormalToLight", _cloudCubemapNormalLightDirection);
      }
    }

    // Fog.
    [SerializeField]
    private Color _fogColor = Color.white;
    public Color FogColor
    {
      get { return _fogColor; }
      set {
        _fogColor = value;
        SkyboxMaterial.SetColor("_HorizonFogColor", _fogColor);
      }
    }

    [SerializeField]
    private float _fogDensity = .12f;
    public float FogDensity
    {
      get { return _fogDensity; }
      set {
        _fogDensity = value;
        SkyboxMaterial.SetFloat("_HorizonFogDensity", _fogDensity);
      }
    }

    [SerializeField]
    private float _fogHeight = .12f;
    public float FogHeight
    {
      get { return _fogHeight; }
      set {
        _fogHeight = value;
        SkyboxMaterial.SetFloat("_HorizonFogLength", _fogHeight);
      }
    }

    private void ApplyGradientValuesOnMaterial()
    {
      float gradientFadeEnd = Mathf.Clamp(_gradientFadeBegin + _gradientFadeLength, -1.0f, 1.0f);
      SkyboxMaterial.SetFloat("_GradientFadeBegin", _gradientFadeBegin);
      SkyboxMaterial.SetFloat("_GradientFadeEnd", gradientFadeEnd);
    }

    private void ApplyStarFadeValuesOnMaterial()
    {
      float starFadeEnd = Mathf.Clamp(_starFadeBegin + _starFadeLength, -1.0f, 1.0f);
      SkyboxMaterial.SetFloat("_StarFadeBegin", _starFadeBegin);
      SkyboxMaterial.SetFloat("_StarFadeEnd", starFadeEnd);
    }
  }
}
