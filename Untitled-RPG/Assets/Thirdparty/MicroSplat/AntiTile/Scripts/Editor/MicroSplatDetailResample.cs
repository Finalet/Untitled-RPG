//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System;

namespace JBooth.MicroSplat
{
#if __MICROSPLAT__
   [InitializeOnLoad]
   public class MicroSplatDetailResample : FeatureDescriptor
   {
      const string sSnowDefine = "__MICROSPLAT_DETAILRESAMPLE__";
      static MicroSplatDetailResample()
      {
         MicroSplatDefines.InitDefine(sSnowDefine);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene()
      { 
         MicroSplatDefines.InitDefine(sSnowDefine);
      }

      public override string ModuleName()
      {
         return "Anti-Tiling";
      }

      public enum DefineFeature
      {
         _DETAILNOISE,
         _DISTANCENOISE,
         _DISTANCERESAMPLE,
         _DISTANCERESAMPLENORMAL,
         _DISTANCERESAMPLENOALBEDO,
         _DISTANCERESAMPLENOFADE,
         _DISTANCERESAMPLENOISE,
         _DISTANCERESAMPLEALBEDOBLENDOVERLAY,
         _DISTANCERESAMPLEALBEDOBLENDLIGHTERCOLOR,
         _DISTANCERESAMPLEMAXLAYER3,
         _DISTANCERESAMPLEMAXLAYER4,
         _PERTEXDETAILNOISESTRENGTH, 
         _PERTEXDISTANCENOISESTRENGTH,
         _PERTEXDISTANCERESAMPLESTRENGTH,
         _PERTEXDISTANCERESAMPLEUVSCALE,
         _NORMALNOISE,
         _NORMALNOISE2,
         _NORMALNOISE3,
         _PERTEXNORMALNOISESTRENGTH,
         _NOISEHEIGHT,
         _NOISEHEIGHTFBM,
         _PERTEXNOISEHEIGHTFREQ,
         _PERTEXNOISEHEIGHTAMP,
         _ANTITILEARRAYNORMAL,
         _ANTITILEARRAYDISTANCE,
         _ANTITILEARRAYDETAIL,
         _ANTITILEPERTEX,
         _RESAMPLECLUSTERS,
         _DISTANCERESAMPLEHEIGHTBLEND,
         _ANTITILETRIPLANAR,
         kNumFeatures,
      }

      static TextAsset properties_detail_noise;
      static TextAsset properties_distance_noise;
      static TextAsset properties_distance_resample;
      static TextAsset properties_normal_noise;
      static TextAsset properties_antitilearray;
      static TextAsset func_resamplers;
      static TextAsset func_antiTile;
      static TextAsset cbuffer_resamplers;
      static TextAsset cbuffer_antiTile;

      public enum NoiseType
      {
         None,
         Texture,
         Procedural
      }

      public enum DistanceResampleMode
      {
         None = 0,
         Albedo,
         Normals,
         Both,
         
      }

      public enum DistanceResampleFade
      {
         CrossFade,
         Constant,
         Noise
      }

      public enum ConstantBlendMode
      {
         Linear,
         Overlay,
         LighterColor
      }


      public bool detailNoise;
      public bool distanceNoise = false;
      public DistanceResampleMode distanceResample = DistanceResampleMode.None;
      public DistanceResampleFade distanceResampleFade = DistanceResampleFade.CrossFade;
      public ConstantBlendMode distanceResampleBlendMode = ConstantBlendMode.Linear;
      public bool perTexDetailNoiseStrength;
      public bool perTexDistanceNoiseStrength;
      public bool perTexDistanceResampleStrength;
      public bool perTexDistanceResampleUVScale;

      public NoiseType noiseHeight = NoiseType.None;
      public bool perTexNoiseHeightFreq;
      public bool perTexNoiseHeightAmp;

      public bool perTexNormalNoiseStrength;
      public bool perTexAntiTile;
      public bool distanceResampleHeightBlend;
#if __MICROSPLAT_TRIPLANAR__
      public bool antiTileTriplanar = false;
#endif
#if __MICROSPLAT_TEXTURECLUSTERS__
      public bool resampleClusters;
#endif

      public MicroSplatBaseFeatures.PerformanceMode maxLayer = MicroSplatBaseFeatures.PerformanceMode.Fastest;

      public enum AntiTileOptions
      {
         NoiseNormal = 1,
         DetailNoise = 2,
         DistanceNoise = 4,
      }
      public AntiTileOptions antiTileOptions = 0;

      public enum NormalNoiseChannels
      {
         Off,
         One,
         Two,
         Three
      }

      public NormalNoiseChannels noiseChannelCount = NormalNoiseChannels.One;

      GUIContent CDetailNoiseTex = new GUIContent("Noise", "A mostly greyscale linear texture with the center around 0.5");
      GUIContent CDistanceNoiseTex = new GUIContent("Noise", "A mostly greyscale linear texture with the center around 0.5");
      GUIContent CNormalNoiseTex = new GUIContent("Normal", "Normal map to blend with base map");
      GUIContent CAntiTileArray = new GUIContent("Anti Tile Array", "Texture Array for applying a noise normal, detail noise, and distance noise to each texture independently");
      GUIContent CDistanceResampleHeightBlend = new GUIContent("Resample Height Blend", "Use height map blend with distance resampling");
      GUIContent CDistanceResampleFade = new GUIContent("Resample Fade", "Cross fade, constant, or noise based blend between layers");
      GUIContent CDistanceResampleBlendMode = new GUIContent ("Resample Blend Mode", "In linear mode, albedo will be replace with distance albedo. Overlay and Lighter Color blend both textures together, but will change the overall lightness of the texture, but Lighter Color tends to be the closest to the original");

      GUIContent CNoiseHeight = new GUIContent ("Noise Height", "Apply a noise texture to the height map to break up repeating patterns from height map data");
#if __MICROSPLAT_TEXTURECLUSTERS__
      GUIContent CResampleClusters = new GUIContent("Resample Clusters", "Resample full texture clusters");
#endif
#if __MICROSPLAT_TRIPLANAR__
      GUIContent CTriplanarControls = new GUIContent("Triplanar Noises", "Triplanar map normal noise, distance noise, and detail noise");
#endif

      // Can we template these somehow?
      static Dictionary<DefineFeature, string> sFeatureNames = new Dictionary<DefineFeature, string>();
      public static string GetFeatureName(DefineFeature feature)
      {
         string ret;
         if (sFeatureNames.TryGetValue(feature, out ret))
         {
            return ret;
         }
         string fn = System.Enum.GetName(typeof(DefineFeature), feature);
         sFeatureNames[feature] = fn;
         return fn;
      }

      public static bool HasFeature(string[] keywords, DefineFeature feature)
      {
         string f = GetFeatureName(feature);
         for (int i = 0; i < keywords.Length; ++i)
         {
            if (keywords[i] == f)
               return true;
         }
         return false;
      }

      static GUIContent CDetailNoise = new GUIContent("Detail Noise", "Apply Noise up close, so textures never get blurry");
      static GUIContent CDistanceNoise = new GUIContent("Distance Noise", "Apply Noise far away, useful for creating detail in the distance");
      static GUIContent CDistanceResample = new GUIContent("Distance Resample", "Resample the Splat textures in the distance at a different UV scale and blend them in to prevent tiling artifacts. Can be done to just the albedo (fast), or full PBR spec (Full), or just normals");
      static GUIContent CNormalNoise = new GUIContent("Normal Noise", "Blend up to 3 normal textures over the terrain, giving it added shape and form, while breaking up tiling. Control the amount applied with a per-texture property");
      static GUIContent CAntiTileOptions = new GUIContent("Anti Tile Array Features", "Turn on/off anti-tile options which use a texture array to give each texture it's own noise normal, detail, and distance noise");
      static GUIContent CPerformanceMode = new GUIContent ("Performance Mode", "Lets you specify how many texture layers are used when distance resampling, 2 == fastest, 3 == balanced, 4 = best quality");

      public override string GetVersion()
      {
         return "3.4";
      }

      public override void DrawFeatureGUI (MicroSplatKeywords keywords)
      {
         detailNoise = EditorGUILayout.Toggle (CDetailNoise, detailNoise);
         distanceNoise = EditorGUILayout.Toggle (CDistanceNoise, distanceNoise);
         noiseChannelCount = (NormalNoiseChannels)EditorGUILayout.EnumPopup (CNormalNoise, noiseChannelCount);
         noiseHeight = (NoiseType)EditorGUILayout.EnumPopup (CNoiseHeight, noiseHeight);
#if UNITY_2017_3_OR_NEWER
         antiTileOptions = (AntiTileOptions)EditorGUILayout.EnumFlagsField(CAntiTileOptions, antiTileOptions);
#else
         antiTileOptions = (AntiTileOptions)EditorGUILayout.EnumMaskPopup (CAntiTileOptions, antiTileOptions);
#endif

#if __MICROSPLAT_TRIPLANAR__
         if (detailNoise || distanceNoise || noiseChannelCount != NormalNoiseChannels.Off || antiTileOptions != 0 || noiseHeight != NoiseType.None)
         {
            EditorGUI.indentLevel++;
            antiTileTriplanar = EditorGUILayout.Toggle(CTriplanarControls, antiTileTriplanar);
            EditorGUI.indentLevel--;
         }
#endif
         // people keep putting both of these on and not packing an antitile array and asking for support..
         if ((detailNoise || distanceNoise || noiseChannelCount != NormalNoiseChannels.Off) && antiTileOptions != 0)
         {
            EditorGUILayout.HelpBox ("Anti Tile Array and anti-tiling features are both on, have you read the docs?", MessageType.Info);
         }


         distanceResample = (DistanceResampleMode)EditorGUILayout.EnumPopup(CDistanceResample, distanceResample);
#if __MICROSPLAT_TEXTURECLUSTERS__
         if (distanceResample != DistanceResampleMode.None)
         {
            EditorGUI.indentLevel++;
            resampleClusters = EditorGUILayout.Toggle(CResampleClusters, resampleClusters);
            maxLayer = (MicroSplatBaseFeatures.PerformanceMode)EditorGUILayout.EnumPopup (CPerformanceMode, maxLayer);
            EditorGUI.indentLevel--;
         }
#endif

         if (distanceResample != DistanceResampleMode.None)
         {
            EditorGUI.indentLevel++;
            distanceResampleHeightBlend = EditorGUILayout.Toggle(CDistanceResampleHeightBlend, distanceResampleHeightBlend);
            distanceResampleFade = (DistanceResampleFade)EditorGUILayout.EnumPopup(CDistanceResampleFade, distanceResampleFade);
            if (distanceResampleFade == DistanceResampleFade.Constant || distanceResampleFade == DistanceResampleFade.Noise)
            {
               EditorGUI.indentLevel++;
               distanceResampleBlendMode = (ConstantBlendMode)EditorGUILayout.EnumPopup (CDistanceResampleBlendMode, distanceResampleBlendMode);
               EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
         }
      }

      static GUIContent CHeightNoiseTex = new GUIContent ("Height Noise Texture", "Noise texture used to modify height maps");

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (detailNoise)
         {
            if (MicroSplatUtilities.DrawRollup("Detail Noise"))
            {
               if (mat.HasProperty("_DetailNoise"))
               {
                  var texProp = shaderGUI.FindProp("_DetailNoise", props);
                  MicroSplatUtilities.WarnLinear(texProp.textureValue as Texture2D);
                  materialEditor.TexturePropertySingleLine(CDetailNoiseTex, texProp);
                  MicroSplatUtilities.EnforceDefaultTexture(texProp, "microsplat_def_detail_noise");

                  Vector4 scaleStr = shaderGUI.FindProp("_DetailNoiseScaleStrengthFade", props).vectorValue;
                  Vector4 newScaleStr = scaleStr;
                  newScaleStr.x = EditorGUILayout.FloatField("Scale", scaleStr.x);
                  newScaleStr.y = EditorGUILayout.FloatField("Strength", scaleStr.y);
                  newScaleStr.z = EditorGUILayout.FloatField("Fade Distance", scaleStr.z);
                  if (newScaleStr != scaleStr)
                  {
                     shaderGUI.FindProp("_DetailNoiseScaleStrengthFade", props).vectorValue = newScaleStr;
                  }

               }
            }
         }

         if (distanceNoise)
         {
            if (MicroSplatUtilities.DrawRollup("Distance Noise"))
            {
               if (mat.HasProperty("_DistanceNoise"))
               {
                  var texProp = shaderGUI.FindProp("_DistanceNoise", props);
                  MicroSplatUtilities.WarnLinear(texProp.textureValue as Texture2D);
                  materialEditor.TexturePropertySingleLine(CDistanceNoiseTex, texProp);
                  MicroSplatUtilities.EnforceDefaultTexture(texProp, "microsplat_def_detail_noise");

                  Vector4 scaleStr = shaderGUI.FindProp("_DistanceNoiseScaleStrengthFade", props).vectorValue;
                  Vector4 newScaleStr = scaleStr;
                  newScaleStr.x = EditorGUILayout.FloatField("Scale", scaleStr.x);
                  newScaleStr.y = EditorGUILayout.FloatField("Strength", scaleStr.y);
                  newScaleStr.z = EditorGUILayout.FloatField("Fade Start", scaleStr.z);
                  newScaleStr.w = EditorGUILayout.FloatField("Fade End", scaleStr.w);
                  if (newScaleStr != scaleStr)
                  {
                     shaderGUI.FindProp("_DistanceNoiseScaleStrengthFade", props).vectorValue = newScaleStr;
                  }

               }
            }
         }
         if (noiseHeight != NoiseType.None)
         {
            if (MicroSplatUtilities.DrawRollup ("Height Noise"))
            {
               if (mat.HasProperty("_NoiseHeightData"))
               {
                  if (mat.HasProperty ("_NoiseHeight"))
                  {
                     var texProp = shaderGUI.FindProp ("_NoiseHeight", props);
                     MicroSplatUtilities.WarnLinear (texProp.textureValue as Texture2D);
                     materialEditor.TexturePropertySingleLine (CHeightNoiseTex, texProp);
                     MicroSplatUtilities.EnforceDefaultTexture (texProp, "microsplat_def_perlin4");
                  }

                  EditorGUI.BeginChangeCheck ();
                  Vector4 data = mat.GetVector ("_NoiseHeightData");
                  data.x = EditorGUILayout.FloatField ("Frequency", data.x);
                  data.y = EditorGUILayout.Slider ("Amplitude", data.y, 0, 2);
                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector ("_NoiseHeightData", data);
                     EditorUtility.SetDirty (mat);
                  }

               }
            }
         }

         if (distanceResample != DistanceResampleMode.None && mat.HasProperty("_ResampleDistanceParams"))
         {
            if (MicroSplatUtilities.DrawRollup("Distance Resample"))
            {
               if (distanceResampleFade == DistanceResampleFade.CrossFade && mat.HasProperty("_ResampleDistanceParams"))
               {
                  EditorGUI.BeginChangeCheck();
                  Vector4 vec = mat.GetVector("_ResampleDistanceParams");
                  vec.x = EditorGUILayout.FloatField("Resample UV Scale", vec.x);

                  Vector2 xy = EditorGUILayout.Vector2Field("Resample Begin/End", new Vector2(vec.y, vec.z));
                  if (EditorGUI.EndChangeCheck())
                  {
                     vec.y = xy.x;
                     vec.z = xy.y;
                     mat.SetVector("_ResampleDistanceParams", vec);
                     EditorUtility.SetDirty(mat);
                  }
               }
               else
               {
                  EditorGUI.BeginChangeCheck();
                  Vector4 vec = mat.GetVector("_ResampleDistanceParams");
                  vec.x = EditorGUILayout.FloatField("Resample UV Scale", vec.x);

                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector("_ResampleDistanceParams", vec);
                     EditorUtility.SetDirty(mat);
                  }
               }
               if (mat.HasProperty ("_DistanceResampleAlbedoStrength"))
               {
                  EditorGUI.BeginChangeCheck ();
                  float f = mat.GetFloat ("_DistanceResampleAlbedoStrength");
                  f = EditorGUILayout.Slider ("Resample Albedo Strength", f, 0.1f, 1.0f);

                  if (EditorGUI.EndChangeCheck ())
                  {
                     mat.SetFloat ("_DistanceResampleAlbedoStrength", f);
                     EditorUtility.SetDirty (mat);
                  }

               }
               if (mat.HasProperty("_DistanceResampleNormalStrength"))
               {
                  EditorGUI.BeginChangeCheck ();
                  float f = mat.GetFloat ("_DistanceResampleNormalStrength");
                  f = EditorGUILayout.Slider ("Resample Normal Strength", f, 0.1f, 1.5f);

                  if (EditorGUI.EndChangeCheck ())
                  {
                     mat.SetFloat ("_DistanceResampleNormalStrength", f);
                     EditorUtility.SetDirty (mat);
                  }

               }


               if (distanceResampleFade == DistanceResampleFade.Noise)
               {
                  var prop = shaderGUI.FindProp("_DistanceResampleNoiseParams", props);
                  Vector2 vals = new Vector2(prop.vectorValue.x, prop.vectorValue.y);
                  EditorGUI.BeginChangeCheck();
                  vals = EditorGUILayout.Vector2Field("Noise Freq and Amp", vals);
                  if (EditorGUI.EndChangeCheck())
                  {
                     prop.vectorValue = new Vector4(vals.x, vals.y, 0, 0);
                  }

               }
            }
         }
         if (noiseChannelCount != NormalNoiseChannels.Off && mat.HasProperty("_NormalNoiseScaleStrength"))
         {
            if (MicroSplatUtilities.DrawRollup("Normal Noise"))
            {
               {
                  var texProp = shaderGUI.FindProp("_NormalNoise", props);
                  materialEditor.TexturePropertySingleLine(CNormalNoiseTex, texProp);
                  MicroSplatUtilities.EnforceDefaultTexture(texProp, "microsplat_def_detail_normal_01");

                  Vector4 scaleStr = shaderGUI.FindProp("_NormalNoiseScaleStrength", props).vectorValue;
                  Vector4 newScaleStr = scaleStr;
                  newScaleStr.x = EditorGUILayout.FloatField("Scale", scaleStr.x);
                  newScaleStr.y = EditorGUILayout.FloatField("Strength", scaleStr.y);
                  if (newScaleStr != scaleStr)
                  {
                     shaderGUI.FindProp("_NormalNoiseScaleStrength", props).vectorValue = newScaleStr;
                  }
               }
               if (noiseChannelCount == NormalNoiseChannels.Two || noiseChannelCount == NormalNoiseChannels.Three)
               {
                  if (mat.HasProperty("_NormalNoiseScaleStrength2"))
                  {
                     var texProp = shaderGUI.FindProp("_NormalNoise2", props);
                     materialEditor.TexturePropertySingleLine(CNormalNoiseTex, texProp);
                     MicroSplatUtilities.EnforceDefaultTexture(texProp, "microsplat_def_detail_normal_02");

                     Vector4 scaleStr = shaderGUI.FindProp("_NormalNoiseScaleStrength2", props).vectorValue;
                     Vector4 newScaleStr = scaleStr;
                     newScaleStr.x = EditorGUILayout.FloatField("Scale", scaleStr.x);
                     newScaleStr.y = EditorGUILayout.FloatField("Strength", scaleStr.y);
                     if (newScaleStr != scaleStr)
                     {
                        shaderGUI.FindProp("_NormalNoiseScaleStrength2", props).vectorValue = newScaleStr;
                     }
                  }
               }
               if (noiseChannelCount == NormalNoiseChannels.Three)
               {
                  if (mat.HasProperty("_NormalNoiseScaleStrength3"))
                  {
                     var texProp = shaderGUI.FindProp("_NormalNoise3", props);
                     materialEditor.TexturePropertySingleLine(CNormalNoiseTex, texProp);
                     MicroSplatUtilities.EnforceDefaultTexture(texProp, "microsplat_def_detail_normal_03");

                     Vector4 scaleStr = shaderGUI.FindProp("_NormalNoiseScaleStrength3", props).vectorValue;
                     Vector4 newScaleStr = scaleStr;
                     newScaleStr.x = EditorGUILayout.FloatField("Scale", scaleStr.x);
                     newScaleStr.y = EditorGUILayout.FloatField("Strength", scaleStr.y);
                     if (newScaleStr != scaleStr)
                     {
                        shaderGUI.FindProp("_NormalNoiseScaleStrength3", props).vectorValue = newScaleStr;
                     }
                  }
               }
            }
         }
         if (antiTileOptions != 0)
         {
            if (MicroSplatUtilities.DrawRollup("Anti Tile Array") && mat.HasProperty("_AntiTileArray"))
            {
               var array = shaderGUI.FindProp("_AntiTileArray", props);
               materialEditor.TexturePropertySingleLine(CAntiTileArray, array);

               if (array.textureValue == null)
               {
                  EditorGUILayout.HelpBox ("No Anti Tile Array found, you need to turn this on in the Texture Array Config, pack textures for the antitiling feature, and assign the generated array here.", MessageType.Error);
               }

               if (mat.HasProperty("_AntiTileNormalNoiseScaleStr") && (((int)antiTileOptions & (int)AntiTileOptions.NoiseNormal) != 0))
               {
                  Vector4 scaleStr = shaderGUI.FindProp("_AntiTileNormalNoiseScaleStr", props).vectorValue;
                  Vector4 newScaleStr = scaleStr;
                  newScaleStr.x = EditorGUILayout.FloatField("Normal Scale", scaleStr.x);
                  newScaleStr.y = EditorGUILayout.FloatField("Normal Strength", scaleStr.y);
                  if (newScaleStr != scaleStr)
                  {
                     shaderGUI.FindProp("_AntiTileNormalNoiseScaleStr", props).vectorValue = newScaleStr;
                  }
                  EditorGUILayout.Space();
               }
               if (mat.HasProperty("_AntiTileDetailNoiseScaleFadeStr") && (((int)antiTileOptions & (int)AntiTileOptions.DetailNoise) != 0))
               {
                  Vector4 scaleStr = shaderGUI.FindProp("_AntiTileDetailNoiseScaleFadeStr", props).vectorValue;
                  Vector4 newScaleStr = scaleStr;
                  newScaleStr.x = EditorGUILayout.FloatField("Detail Scale", scaleStr.x);
                  newScaleStr.y = EditorGUILayout.FloatField("Detail Fade Distance", scaleStr.y);
                  newScaleStr.z = EditorGUILayout.FloatField("Detail Strength", scaleStr.z);
                  if (newScaleStr != scaleStr)
                  {
                     shaderGUI.FindProp("_AntiTileDetailNoiseScaleFadeStr", props).vectorValue = newScaleStr;
                  }
                  EditorGUILayout.Space();
               }
               if (mat.HasProperty("_AntiTileDistanceNoiseScaleFadeStr") && (((int)antiTileOptions & (int)AntiTileOptions.DistanceNoise) != 0))
               {
                  Vector4 scaleStr = shaderGUI.FindProp("_AntiTileDistanceNoiseScaleFadeStr", props).vectorValue;
                  Vector4 newScaleStr = scaleStr;
                  newScaleStr.x = EditorGUILayout.FloatField("Distance Scale", scaleStr.x);
                  newScaleStr.y = EditorGUILayout.FloatField("Distance Fade Start", scaleStr.y);
                  newScaleStr.z = EditorGUILayout.FloatField("Distance Fade End", scaleStr.z);
                  newScaleStr.w = EditorGUILayout.FloatField("Distance Strength", scaleStr.w);
                  if (newScaleStr != scaleStr)
                  {
                     shaderGUI.FindProp("_AntiTileDistanceNoiseScaleFadeStr", props).vectorValue = newScaleStr;
                  }
               }

            }

         }
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (detailNoise)
         {
            features.Add(GetFeatureName(DefineFeature._DETAILNOISE));
            if (perTexDetailNoiseStrength)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXDETAILNOISESTRENGTH));
            }
         }
         if (distanceNoise)
         {
            features.Add(GetFeatureName(DefineFeature._DISTANCENOISE));
            if (perTexDistanceNoiseStrength)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXDISTANCENOISESTRENGTH));
            }
         }
         if (noiseHeight != NoiseType.None)
         {
            features.Add (GetFeatureName (DefineFeature._NOISEHEIGHT));
            if (noiseHeight == NoiseType.Procedural)
            {
               features.Add (GetFeatureName (DefineFeature._NOISEHEIGHTFBM));
            }
         }
         if (perTexNoiseHeightFreq)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXNOISEHEIGHTFREQ));
         }
         if (perTexNoiseHeightAmp)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXNOISEHEIGHTAMP));
         }
         if (distanceResample != DistanceResampleMode.None)
         {
            features.Add(GetFeatureName(DefineFeature._DISTANCERESAMPLE));
            if (distanceResample == DistanceResampleMode.Both || distanceResample == DistanceResampleMode.Normals)
            {
               features.Add(GetFeatureName(DefineFeature._DISTANCERESAMPLENORMAL));
            }
            if (distanceResample == DistanceResampleMode.Normals)
            {
               features.Add (GetFeatureName (DefineFeature._DISTANCERESAMPLENOALBEDO));
            }
            if (perTexDistanceResampleStrength)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXDISTANCERESAMPLESTRENGTH));
            }
#if __MICROSPLAT_TEXTURECLUSTERS__
            if (resampleClusters)
            {
               features.Add(GetFeatureName(DefineFeature._RESAMPLECLUSTERS));
            }
#endif
            if (distanceResampleHeightBlend)
            {
               features.Add(GetFeatureName(DefineFeature._DISTANCERESAMPLEHEIGHTBLEND));
            }
            if (distanceResampleFade == DistanceResampleFade.Constant)
            {
               features.Add(GetFeatureName(DefineFeature._DISTANCERESAMPLENOFADE));
            }
            else if (distanceResampleFade == DistanceResampleFade.Noise)
            {
               features.Add(GetFeatureName(DefineFeature._DISTANCERESAMPLENOISE));
            }
            if (perTexDistanceResampleUVScale)
            {
               features.Add(GetFeatureName (DefineFeature._PERTEXDISTANCERESAMPLEUVSCALE));
            }
            if (distanceResampleBlendMode == ConstantBlendMode.Overlay)
            {
               features.Add (GetFeatureName (DefineFeature._DISTANCERESAMPLEALBEDOBLENDOVERLAY));
            }
            else if (distanceResampleBlendMode == ConstantBlendMode.LighterColor)
            {
               features.Add (GetFeatureName (DefineFeature._DISTANCERESAMPLEALBEDOBLENDLIGHTERCOLOR));
            }
            if (maxLayer == MicroSplatBaseFeatures.PerformanceMode.Balanced)
            {
               features.Add (GetFeatureName (DefineFeature._DISTANCERESAMPLEMAXLAYER3));
            }
            else if (maxLayer == MicroSplatBaseFeatures.PerformanceMode.BestQuality)
            {
               features.Add (GetFeatureName (DefineFeature._DISTANCERESAMPLEMAXLAYER4));
            }
         }
         if (noiseChannelCount != NormalNoiseChannels.Off)
         {
            features.Add(GetFeatureName(DefineFeature._NORMALNOISE));
            if (perTexNormalNoiseStrength)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXNORMALNOISESTRENGTH));
            }

            if (noiseChannelCount == NormalNoiseChannels.Two || noiseChannelCount == NormalNoiseChannels.Three)
            {
               features.Add(GetFeatureName(DefineFeature._NORMALNOISE2));
            }

            if (noiseChannelCount == NormalNoiseChannels.Three)
            {
               features.Add(GetFeatureName(DefineFeature._NORMALNOISE3));
            }

         }
         if (((int)antiTileOptions & (int)AntiTileOptions.NoiseNormal) != 0)
         {
            features.Add(GetFeatureName(DefineFeature._ANTITILEARRAYNORMAL));
         }
         if (((int)antiTileOptions & (int)AntiTileOptions.DetailNoise) != 0)
         {
            features.Add(GetFeatureName(DefineFeature._ANTITILEARRAYDETAIL));
         }
         if (((int)antiTileOptions & (int)AntiTileOptions.DistanceNoise) != 0)
         {
            features.Add(GetFeatureName(DefineFeature._ANTITILEARRAYDISTANCE));
         }
         if (perTexAntiTile)
         {
            features.Add(GetFeatureName(DefineFeature._ANTITILEPERTEX));
         }

#if __MICROSPLAT_TRIPLANAR__
         if (noiseHeight != NoiseType.None || detailNoise || distanceNoise || noiseChannelCount != NormalNoiseChannels.Off || antiTileOptions != 0) 
         {
            if (antiTileTriplanar)
            {
               features.Add(GetFeatureName(DefineFeature._ANTITILETRIPLANAR));
            }
         }
#endif
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
         detailNoise = HasFeature(keywords, DefineFeature._DETAILNOISE);
         distanceNoise = HasFeature(keywords, DefineFeature._DISTANCENOISE);
         noiseHeight = NoiseType.None;
         if (HasFeature (keywords, DefineFeature._NOISEHEIGHT))
         {
            noiseHeight = NoiseType.Texture;
            if (HasFeature(keywords, DefineFeature._NOISEHEIGHTFBM))
            {
               noiseHeight = NoiseType.Procedural;
            }
         }
         perTexNoiseHeightFreq = HasFeature (keywords, DefineFeature._PERTEXNOISEHEIGHTFREQ);
         perTexNoiseHeightAmp = HasFeature (keywords, DefineFeature._PERTEXNOISEHEIGHTAMP);

         distanceResample = DistanceResampleMode.None;
         if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLE))
         {
            distanceResample = DistanceResampleMode.Albedo;
            if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLENORMAL))
            {
               distanceResample = DistanceResampleMode.Both;
            }
            if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLENOALBEDO))
            {
               distanceResample = DistanceResampleMode.Normals;
            }
            distanceResampleHeightBlend = HasFeature(keywords, DefineFeature._DISTANCERESAMPLEHEIGHTBLEND);

            distanceResampleFade = DistanceResampleFade.CrossFade;
            if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLENOFADE))
            {
               distanceResampleFade = DistanceResampleFade.Constant;
            }
            else if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLENOISE))
            {
               distanceResampleFade = DistanceResampleFade.Noise;
            }

            distanceResampleBlendMode = ConstantBlendMode.Linear;
            if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLEALBEDOBLENDOVERLAY))
            {
               distanceResampleBlendMode = ConstantBlendMode.Overlay;
            }
            else if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLEALBEDOBLENDLIGHTERCOLOR))
            {
               distanceResampleBlendMode = ConstantBlendMode.LighterColor;
            }
            maxLayer = MicroSplatBaseFeatures.PerformanceMode.Fastest;
            if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLEMAXLAYER3))
            {
               maxLayer = MicroSplatBaseFeatures.PerformanceMode.Balanced;
            }
            else if (HasFeature(keywords, DefineFeature._DISTANCERESAMPLEMAXLAYER4))
            {
               maxLayer = MicroSplatBaseFeatures.PerformanceMode.BestQuality;
            }

         }
         noiseChannelCount = NormalNoiseChannels.Off;
         if (HasFeature(keywords, DefineFeature._NORMALNOISE))
         {
            noiseChannelCount = NormalNoiseChannels.One;
         }
         if (HasFeature(keywords, DefineFeature._NORMALNOISE2))
         {
            noiseChannelCount = NormalNoiseChannels.Two;
         }
         if (HasFeature(keywords, DefineFeature._NORMALNOISE3))
         {
            noiseChannelCount = NormalNoiseChannels.Three;
         }

         perTexDetailNoiseStrength = HasFeature(keywords, DefineFeature._PERTEXDETAILNOISESTRENGTH);
         perTexDistanceNoiseStrength = HasFeature(keywords, DefineFeature._PERTEXDISTANCENOISESTRENGTH);
         perTexDistanceResampleStrength = HasFeature(keywords, DefineFeature._PERTEXDISTANCERESAMPLESTRENGTH);
         perTexNormalNoiseStrength = HasFeature(keywords, DefineFeature._PERTEXNORMALNOISESTRENGTH);
         perTexAntiTile = HasFeature(keywords, DefineFeature._ANTITILEPERTEX);
         perTexDistanceResampleUVScale = (HasFeature (keywords, DefineFeature._PERTEXDISTANCERESAMPLEUVSCALE));

#if __MICROSPLAT_TEXTURECLUSTERS__
         resampleClusters = HasFeature(keywords, DefineFeature._RESAMPLECLUSTERS);
#endif

         antiTileOptions = 0;
         if (HasFeature(keywords, DefineFeature._ANTITILEARRAYNORMAL))
         {
            antiTileOptions = (AntiTileOptions)((int)antiTileOptions | ((int)AntiTileOptions.NoiseNormal));
         }
         if (HasFeature(keywords, DefineFeature._ANTITILEARRAYDETAIL))
         {
            antiTileOptions = (AntiTileOptions)((int)antiTileOptions | ((int)AntiTileOptions.DetailNoise));
         }
         if (HasFeature(keywords, DefineFeature._ANTITILEARRAYDISTANCE))
         {
            antiTileOptions = (AntiTileOptions)((int)antiTileOptions | ((int)AntiTileOptions.DistanceNoise));
         }
         if (antiTileOptions == 0)
         {
            perTexAntiTile = false;
         }
#if __MICROSPLAT_TRIPLANAR__
         if (noiseHeight != NoiseType.None || detailNoise || distanceNoise || noiseChannelCount != NormalNoiseChannels.Off)
         {
            antiTileTriplanar = HasFeature(keywords, DefineFeature._ANTITILETRIPLANAR);
         }
#endif

      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_detail_noise.txt"))
            {
               properties_detail_noise = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_distance_noise.txt"))
            {
               properties_distance_noise = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_distanceresample.txt"))
            {
               properties_distance_resample = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_resamplenoise.txt"))
            {
               func_resamplers = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_normal_noise.txt"))
            {
               properties_normal_noise = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_antitilearray.txt"))
            {
               properties_antitilearray = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_antitilearray.txt"))
            {
               func_antiTile = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith ("microsplat_cbuffer_antitilearray.txt"))
            {
               cbuffer_antiTile = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_cbuffer_resamplenoise.txt"))
            {
               cbuffer_resamplers = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
         }
      }

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (HasFeature(features, DefineFeature._DETAILNOISE))
         {
            sb.Append(properties_detail_noise.text);
         }
         if (HasFeature(features, DefineFeature._DISTANCENOISE))
         {
            sb.Append(properties_distance_noise.text);
         }
         if (noiseHeight != NoiseType.None)
         {
            if (noiseHeight == NoiseType.Texture)
            {
               sb.AppendLine ("      _NoiseHeight(\"Noise Texture\", 2D) = \"grey\" {}");
            }
            sb.AppendLine ("      _NoiseHeightData(\"Noise Height Data\", Vector) = (1, 0.15, 0, 0)");
         }
         if (HasFeature(features, DefineFeature._DISTANCERESAMPLE))
         {
            sb.Append(properties_distance_resample.text);
            if (HasFeature(features, DefineFeature._DISTANCERESAMPLENOISE))
            {
               sb.AppendLine("      _DistanceResampleNoiseParams(\"Distance Noise Params\", Vector) = (0.5, 0.5, 0, 0)");
            }
           
            if (distanceResample == DistanceResampleMode.Albedo || distanceResample == DistanceResampleMode.Both)
            {
               sb.AppendLine ("     _DistanceResampleAlbedoStrength(\"Resampled Albedo Strength\", Range(0.1, 1.0)) = 1");
            }
            if (distanceResample == DistanceResampleMode.Normals || distanceResample == DistanceResampleMode.Both)
            {
               sb.AppendLine ("     _DistanceResampleNormalStrength(\"Resampled Normal Strength\", Range(0.1, 1.3)) = 1");
            }
            
         }
         if (HasFeature(features, DefineFeature._NORMALNOISE))
         {
            sb.Append(properties_normal_noise.text);
            if (HasFeature(features, DefineFeature._NORMALNOISE2))
            {
               sb.AppendLine("      [NoScaleOffset]_NormalNoise2(\"Normal Noise 2\", 2D) = \"bump\" {}");
               sb.AppendLine("      _NormalNoiseScaleStrength2(\"Normal Scale 2\", Vector) = (8, 0.5, 0, 0)");
            }
            if (HasFeature(features, DefineFeature._NORMALNOISE3))
            {
               sb.AppendLine("      [NoScaleOffset]_NormalNoise3(\"Normal Noise 3\", 2D) = \"bump\" {}");
               sb.AppendLine("      _NormalNoiseScaleStrength3(\"Normal Scale 3\", Vector) = (8, 0.5, 0, 0)");
            }
             
         }
         if (HasFeature(features, DefineFeature._ANTITILEARRAYDETAIL) ||
             HasFeature(features, DefineFeature._ANTITILEARRAYDISTANCE) ||
             HasFeature(features, DefineFeature._ANTITILEARRAYNORMAL))
         {
            sb.AppendLine(properties_antitilearray.text);
         }
      }

      public override void WritePerMaterialCBuffer (string[] features, System.Text.StringBuilder sb)
      {
         if (noiseHeight != NoiseType.None || detailNoise || distanceNoise || distanceResample != DistanceResampleMode.None || noiseChannelCount != NormalNoiseChannels.Off )
         {
            sb.AppendLine(cbuffer_resamplers.text);
            if (distanceResample == DistanceResampleMode.Normals || distanceResample == DistanceResampleMode.Both)
            {
               sb.AppendLine ("      half _DistanceResampleNormalStrength;");
            }
            if (distanceResample == DistanceResampleMode.Albedo || distanceResample == DistanceResampleMode.Both)
            {
               sb.AppendLine ("     half _DistanceResampleAlbedoStrength;");
            }
         }
         if (antiTileOptions != 0)
         {
            sb.AppendLine(cbuffer_antiTile.text);
         }
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {
         if (noiseHeight != NoiseType.None || detailNoise || distanceNoise || distanceResample != DistanceResampleMode.None || noiseChannelCount != NormalNoiseChannels.Off || antiTileOptions != 0)
         {
            sb.AppendLine(func_resamplers.text);
         }
         if (antiTileOptions != 0)
         {
            sb.AppendLine(func_antiTile.text);
         }
      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         bool trip = false;
#if __MICROSPLAT_TRIPLANAR__
         trip = antiTileTriplanar;
#endif

         if (detailNoise)
         {
            textureSampleCount += trip ? 3 : 1;
         }
         if (distanceNoise)
         {
            textureSampleCount += trip ? 3 : 1;
         }
         if (noiseHeight == NoiseType.Texture)
         {
            textureSampleCount += 4;
         }
         if (distanceResample != DistanceResampleMode.None)
         {
            int samps = 2;
            if (maxLayer == MicroSplatBaseFeatures.PerformanceMode.Balanced)
            {
               samps = 3;
            }
            else if (maxLayer == MicroSplatBaseFeatures.PerformanceMode.BestQuality)
            {
               samps = 4;
            }
            if (distanceResample == DistanceResampleMode.Both)
            {
               samps *= 2;
            }
            
            if (System.Array.Exists(features, e => e == "_TRIPLANAR"))
            {
               samps *= 3;
            }
#if __MICROSPLAT_TEXTURECLUSTERS__
            if (resampleClusters)
            {
               if (System.Array.Exists (features, e => e == "_STOCHASTIC") ||
                  System.Array.Exists (features, e => e == "_CLUSTER3"))
               {
                  samps *= 3;
               }
               else if (System.Array.Exists (features, e => e == "_CLUSTER2"))
               {
                  samps *= 2;
               }
            }
#endif
            arraySampleCount += samps;
         }
         textureSampleCount += (int)noiseChannelCount;
         if (antiTileOptions != 0)
         {
            if (((int)antiTileOptions & (int)AntiTileOptions.NoiseNormal) != 0)
            {
               arraySampleCount += 4;
            }
            if (((int)antiTileOptions & (int)AntiTileOptions.DetailNoise) != 0)
            {
               arraySampleCount += 4;
            }
            if (((int)antiTileOptions & (int)AntiTileOptions.DistanceNoise) != 0)
            {
               arraySampleCount += 4;
            }
         }
      }

      static GUIContent CPerTexDetailNoiseStr = new GUIContent("Detail Noise", "Amount of detail noise to apply to this texture");
      static GUIContent CPerTexDistanceNoiseStr = new GUIContent("Distance Noise", "Amount of distance noise to apply to this texture");
      static GUIContent CPerTexDistanceResample = new GUIContent("Distance Resample", "Amount of distance resampling to blend in to this texture");
      static GUIContent CPerTexDisatnceResampleUVScale = new GUIContent ("Distance Resample UV scale", "per texture UV scales for distance resampling");
      static GUIContent CPerTexNormalNoise = new GUIContent("Normal Noise", "Amount of Normal noise to apply for this texture");
      static GUIContent CPerTexNormalNoise2 = new GUIContent("Second Normal Noise", "Amount of Normal noise to apply for this texture");
      static GUIContent CPerTexNormalNoise3 = new GUIContent("Third Normal Noise", "Amount of Normal noise to apply for this texture");
      static GUIContent CPerTexAntiTileNormal = new GUIContent("AntiTile Array Normal", "Noise Normal strength for anti-tile array");
      static GUIContent CPerTexAntiTileDetail = new GUIContent("AntiTile Array Detail", "Detail Noise strength for anti-tile array");
      static GUIContent CPerTexAntiTileDistance = new GUIContent("AntiTile Array Distance", "Distance Noise strength for anti-tile array");

      static GUIContent CPerTexNoiseHeightFreq = new GUIContent ("Noise Height Frequency", "Frequency of noise applied to height map (uv scale)");
      static GUIContent CPerTexNoiseHeightAmp = new GUIContent ("Noise Height Amplitude", "Amplitude of noise applied to height maps");

      public override void DrawPerTextureGUI(int index, MicroSplatKeywords keywords, Material mat, MicroSplatPropData propData)
      {
         InitPropData(4, propData, new Color(1.0f, 1, 1, 1));
         InitPropData(14, propData, new Color(1, 1, 1, 1));

         if (detailNoise)
         {
            perTexDetailNoiseStrength = DrawPerTexFloatSlider(index, 4, GetFeatureName(DefineFeature._PERTEXDETAILNOISESTRENGTH),
               keywords, propData, Channel.R, CPerTexDetailNoiseStr, 0, 3);

         }
         if (distanceNoise)
         {
            perTexDistanceNoiseStrength = DrawPerTexFloatSlider(index, 4, GetFeatureName(DefineFeature._PERTEXDISTANCENOISESTRENGTH),
               keywords, propData, Channel.G, CPerTexDistanceNoiseStr, 0, 3);
         }
         if (noiseHeight != NoiseType.None)
         {
            perTexNoiseHeightFreq = DrawPerTexFloatSlider (index, 22, GetFeatureName (DefineFeature._PERTEXNOISEHEIGHTFREQ),
               keywords, propData, Channel.R, CPerTexNoiseHeightFreq, -30, 30);
            perTexNoiseHeightAmp = DrawPerTexFloatSlider (index, 22, GetFeatureName (DefineFeature._PERTEXNOISEHEIGHTAMP),
               keywords, propData, Channel.G, CPerTexNoiseHeightAmp, -1, 1);
         }

         if (distanceResample != DistanceResampleMode.None)
         {
            perTexDistanceResampleStrength = DrawPerTexFloatSlider(index, 4, GetFeatureName(DefineFeature._PERTEXDISTANCERESAMPLESTRENGTH),
               keywords, propData, Channel.B, CPerTexDistanceResample, 0, 3);
            perTexDistanceResampleUVScale = DrawPerTexFloatSlider(index, 13, GetFeatureName(DefineFeature._PERTEXDISTANCERESAMPLEUVSCALE),
               keywords, propData, Channel.A, CPerTexDisatnceResampleUVScale, 0.1f, 10);
         }
         if (noiseChannelCount != NormalNoiseChannels.Off)
         {
            perTexNormalNoiseStrength = DrawPerTexFloatSlider(index, 7, GetFeatureName(DefineFeature._PERTEXNORMALNOISESTRENGTH), keywords, propData, Channel.R, CPerTexNormalNoise, 0, 2);
         }
         if (noiseChannelCount == NormalNoiseChannels.Two || noiseChannelCount == NormalNoiseChannels.Three)
         {
            DrawPerTexFloatSliderNoToggle(index, 7, GetFeatureName(DefineFeature._PERTEXNORMALNOISESTRENGTH), keywords, propData, Channel.G, CPerTexNormalNoise2, 0, 2);
         }
         if (noiseChannelCount == NormalNoiseChannels.Three)
         {
            DrawPerTexFloatSliderNoToggle(index, 7, GetFeatureName(DefineFeature._PERTEXNORMALNOISESTRENGTH), keywords, propData, Channel.B, CPerTexNormalNoise3, 0, 2);
         }

         if (antiTileOptions != 0)
         {
            bool controlDrawn = false;
            if (((int)antiTileOptions & (int)AntiTileOptions.NoiseNormal) != 0)
            {
               perTexAntiTile = DrawPerTexFloatSlider(index, 14, GetFeatureName(DefineFeature._ANTITILEPERTEX), keywords, propData, Channel.R, CPerTexAntiTileNormal, 0, 2);
               controlDrawn = true;
            }
            if (((int)antiTileOptions & (int)AntiTileOptions.DetailNoise) != 0)
            {
               if (controlDrawn)
               {
                  DrawPerTexFloatSliderNoToggle(index, 14, GetFeatureName(DefineFeature._ANTITILEPERTEX), keywords, propData, Channel.G, CPerTexAntiTileDetail, 0, 2);
               }
               else
               {
                  perTexAntiTile = DrawPerTexFloatSlider(index, 14, GetFeatureName(DefineFeature._ANTITILEPERTEX), keywords, propData, Channel.G, CPerTexAntiTileDetail, 0, 2);
                  controlDrawn = true;
               }
            }
            if (((int)antiTileOptions & (int)AntiTileOptions.DistanceNoise) != 0)
            {
               if (controlDrawn)
               {
                  DrawPerTexFloatSliderNoToggle(index, 14, GetFeatureName(DefineFeature._ANTITILEPERTEX), keywords, propData, Channel.B, CPerTexAntiTileDistance, 0, 2);
               }
               else
               {
                  perTexAntiTile = DrawPerTexFloatSlider(index, 14, GetFeatureName(DefineFeature._ANTITILEPERTEX), keywords, propData, Channel.B, CPerTexAntiTileDistance, 0, 2);
               }
            }
         }

      }
   }   

#endif
   }
