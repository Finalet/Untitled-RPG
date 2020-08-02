//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;

namespace JBooth.MicroSplat
{
#if __MICROSPLAT__



   [InitializeOnLoad]
   public class MicroSplatStreams : FeatureDescriptor
   {

      [MenuItem("Window/MicroSplat/Create Stream Emitter")]
      static void AddEmitter()
      {
         GameObject go = new GameObject("MicroSplat Emitter");
         go.transform.localScale = new Vector3(10, 10, 10);
         go.AddComponent<StreamEmitter>();
      }

      [MenuItem("Window/MicroSplat/Create Stream Collider")]
      static void AddCollider()
      {
         GameObject go = new GameObject("MicroSplat Collider");
         go.transform.localScale = new Vector3(5, 5, 5);
         go.AddComponent<StreamCollider>();
      }


      const string sDefine = "__MICROSPLAT_STREAMS__";
      static MicroSplatStreams()
      {
         MicroSplatDefines.InitDefine(sDefine);
      }
      [PostProcessSceneAttribute(0)]
      public static void OnPostprocessScene()
      {
         MicroSplatDefines.InitDefine(sDefine);
      }

      public override string ModuleName()
      {
         return "Streams";
      }

      public enum DefineFeature
      {
         _WETNESS,
         _PUDDLES,
         _STREAMS,
         _LAVA,
#if __MICROSPLAT_TEXTURECLUSTERS__
         _LAVASTOCHASTIC,
#endif
         _PERTEXPOROSITY,
         _PERTEXFOAM,
         _DYNAMICFLOWS,
         _WETNESSMASKSNOW,
         _HEIGHTWETNESS,
         _RAINDROPS,
         _GLOBALWETNESS,
         _GLOBALPUDDLES,
         _GLOBALRAIN,
         _GLOBALSTREAMS,
         _STREAMHEIGHTFILTER,
         _LAVAHEIGHTFILTER,
         _SPECULARFADE,
         kNumFeatures,
      }

      public enum RainMode
      {
         None = 0,
         StreamsOnly,
         StreamsAndWetness,
      }


      static TextAsset wetnessProps;
      static TextAsset puddleProps;
      static TextAsset streamProps;
      static TextAsset lavaProps;
      static TextAsset rainProps;
      static TextAsset funcShared;
      static TextAsset cbufferShared;

      public bool wetness;
      public bool puddles;
      public bool streams;
      public bool lava;
      public bool perTexPorosity;
      public bool perTexFoam;
      public bool dynamicFlows;
      public bool wetnessMaskSnow;
      public bool heightWetness;
      public bool rainDrops;
      public bool globalWetness;
      public bool globalRain;
      public bool globalStreams;
      public bool globalPuddles;
      public bool heightDampStreams;
      public bool heightDampLava;
      public bool specularFade;
#if __MICROSPLAT_TEXTURECLUSTERS__
      public bool lavaStochastic;
#endif



      GUIContent CWetness = new GUIContent("Wetness", "Paintable Wetness");
      GUIContent CPuddles = new GUIContent("Puddles", "Paintable Puddles");
      GUIContent CStreams = new GUIContent("Streams", "Paintable Streams");
      GUIContent CLava = new GUIContent("Lava", "Paintable Lava");
      GUIContent CDynamicFlows = new GUIContent("Dynamic Flows", "Allow for dynamic flows of water and lava over the terrain from emitter objects");
      GUIContent CPerTexPorosity = new GUIContent("Porosity", "Porosity of the surface; higher porosities will absorb more water, darkening albedo and causing more reflection");
      GUIContent CPerTexFoam = new GUIContent("Foam", "Control amount of water foam over each texture");
      GUIContent CMaxStream = new GUIContent("Max Stream", "Global adjustment for stream strength");
      GUIContent CWetnessMaskSnow = new GUIContent("Wetness Masks Snow", "When enabled, snow will be removed from areas which are not wet. This can be used with raycast wetness to remove snow from areas under coverage");
      GUIContent CHeightWetness = new GUIContent("Wetness Height", "Enable wetness based on terrain height, useful for shorelines");
      GUIContent CRainDrops = new GUIContent("Rain Drops", "Enables rain effects on water areas");
      GUIContent CHeightDampStreams = new GUIContent("Stream Height Filter", "Adjust stream strength based on height. Filter below sea level, for instance");
      GUIContent CHeightDampLava = new GUIContent("Lava Height Filter", "Adjust stream strength based on height. Filter below sea level, for instance");
      GUIContent CSpecularFade = new GUIContent("Specular Fade", "Fade specular response below a certain height");
      GUIContent CEmissionMult = new GUIContent("Emission Multiplier", "Increase emission value of lava");
#if __MICROSPLAT_TEXTURECLUSTERS__
      GUIContent CLavaStochastic = new GUIContent("Stochastic Sample Lava", "Use Stochastic Sampling on lava to break up tiling");
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

      public override string GetVersion()
      {
         return "3.4";
      }

      public override void DrawFeatureGUI(MicroSplatKeywords keywords)
      {
         wetness = EditorGUILayout.Toggle(CWetness, wetness);
         if (wetness)
         {
            EditorGUI.indentLevel++;
            heightWetness = EditorGUILayout.Toggle(CHeightWetness, heightWetness);
            EditorGUI.indentLevel--;
         }
         puddles = EditorGUILayout.Toggle(CPuddles, puddles);
         if (puddles)
         {
            EditorGUI.indentLevel++;
            rainDrops = EditorGUILayout.Toggle(CRainDrops, rainDrops);
            EditorGUI.indentLevel--;
         }
         streams = EditorGUILayout.Toggle(CStreams, streams);
         if (streams)
         {
            EditorGUI.indentLevel++;
            heightDampStreams = EditorGUILayout.Toggle(CHeightDampStreams, heightDampStreams);
            EditorGUI.indentLevel--;
         }
         lava = EditorGUILayout.Toggle(CLava, lava);
         if (lava)
         {
            EditorGUI.indentLevel++;
            heightDampLava = EditorGUILayout.Toggle(CHeightDampLava, heightDampLava);
#if __MICROSPLAT_TEXTURECLUSTERS__
            lavaStochastic = EditorGUILayout.Toggle (CLavaStochastic, lavaStochastic);
#endif
            EditorGUI.indentLevel--;
         }



         if ((streams || lava) && !keywords.IsKeywordEnabled("_MICROMESH"))
         {
            dynamicFlows = EditorGUILayout.Toggle(CDynamicFlows, dynamicFlows);
         }
         else
         {
            dynamicFlows = false;
         }





         if (keywords.IsKeywordEnabled("_SNOW"))
         {
            wetnessMaskSnow = EditorGUILayout.Toggle(CWetnessMaskSnow, wetnessMaskSnow);
         }
         else
         {
            wetnessMaskSnow = false;
         }

         specularFade = EditorGUILayout.Toggle(CSpecularFade, specularFade);

      }

      //static GUIContent CControl = new GUIContent("Control Texture", "Texture used to store control data");
      static GUIContent CPuddleBlend = new GUIContent("Puddle Blend", "Blend sharpness for puddles");
      static GUIContent CPuddleMax = new GUIContent("Puddle Max", "Maximum amount of puddles allowed in the scene, useful to ramp pre-painted puddles in and out with rain");


      static GUIContent CLavaDiffuse = new GUIContent("Lava Texture", "Normal (RG), Height (B), Hardening(A)");
      static GUIContent CLavaBlend = new GUIContent("Blend Width", "Blend border for lava");
      static GUIContent CLavaMax = new GUIContent("Max", "Maximum amount of lava allowed.");
      static GUIContent CLavaSpeed = new GUIContent("Speed", "Speed of lava flow");
      static GUIContent CLavaIntensity = new GUIContent("Intensity", "Intensity of Lava flow");
      static GUIContent CLavaDistSize = new GUIContent("Distortion Size", "Size of distortion waves");
      static GUIContent CLavaDistRate = new GUIContent("Distortion Rate", "Rate of distortion motion");
      static GUIContent CLavaDistScale = new GUIContent("Distortion Scale", "UV scale of distortion");
      static GUIContent CLavaColor = new GUIContent("Color", "Central color for lava");
      static GUIContent CLavaEdgeColor = new GUIContent("Edge Color", "Color for glow around edges");
      static GUIContent CLavaHighlightColor = new GUIContent("Highlight Color", "Color for highlights on lava");
      static GUIContent CLavaUVScale = new GUIContent("UV Scale", "Scale of lava texture");
      static GUIContent CLavaDarkening = new GUIContent("Drying", "Controls amount of drying lava to appear in flow");
      static GUIContent CLavaDisplacementScale = new GUIContent("Displacement Scale", "How thick lava is when displacing via tessellation");

      static GUIContent CStreamFoamTex = new GUIContent("Foam Texture", "Normal map in RG, Foam noise in B");
      static GUIContent CStreamNormalBlend = new GUIContent("Normal Blend", "Controls how dampened the lighting normals are below the surface of the water");
      static GUIContent CStreamBlend = new GUIContent("Edge Blend", "Controls the sharpness of the blend around stream edges");
      static GUIContent CStreamFoamStr = new GUIContent("Foam Strength", "How much foam to add");

      static GUIContent CStreamDepthDampen = new GUIContent("Depth Calm", "Causes deeper parts of the stream to have calmer water");
      static GUIContent CStreamUVScale = new GUIContent("UV Scale", "UV Scale for normal/foam texture");
      static GUIContent CStreamRefractionStrength = new GUIContent("Refraction Strength", "How much should the surface under the stream be distorted by the water");
      static GUIContent CStreamFlowSpeed = new GUIContent("Flow Speed", "Overall speed of flow mapping");
      static GUIContent CStreamFlowCycle = new GUIContent("Flow Cycle", "How long it takes the flow map to loop");


      static GUIContent CHeightWetnessHeight = new GUIContent("Height", "Height at which terrain should be wet below");
      static GUIContent CHeightWetnessContrast = new GUIContent("Contrast", "Blend width for height based wetness");
      static GUIContent CHeightWetnessFrequency = new GUIContent("Frequency", "Frequency at which wetness height is varied");
      static GUIContent CHeightWetnessAmplitude = new GUIContent("Amplitude", "Amplitude of wetness height motion");

      static GUIContent CMinMaxWetness = new GUIContent("Wetness Range", "Minimum/Maximum wetness for the entire scene");
      static GUIContent CRainIntensity = new GUIContent("Intensity", "How strong the rain drop effect is");
      static GUIContent CRainUVScales = new GUIContent("Drop Size", "How big the effect is");

      static GUIContent CHeightDamp = new GUIContent("Height Filtering", "World Height to start effect, be full effect, end full effect, and fade out effect");
      static GUIContent CSpecularHeights = new GUIContent("Specular Fade Heights", "Height range to ramp in specular");


#if __MICROSPLAT_TEXTURECLUSTERS__
      static GUIContent CLavaStochasticContrast = new GUIContent ("Stochastic Contrast", "Contrast for stochastic sampling of lava");
      static GUIContent ClavaStochasticSize = new GUIContent ("Stochastic Size", "Size of stochastic areas");
#endif

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (specularFade)
         {
            if (MicroSplatUtilities.DrawRollup("Specular Height Fade") && mat.HasProperty("_SpecularFades"))
            {
               Vector4 sf = mat.GetVector("_SpecularFades");
               EditorGUI.BeginChangeCheck();
               Vector2 vals = new Vector2(sf.x, sf.y);
               vals = EditorGUILayout.Vector2Field(CSpecularHeights, vals);
               sf.x = vals.x;
               sf.y = vals.y;
               if (EditorGUI.EndChangeCheck())
               {
                  mat.SetVector("_SpecularFades", sf);
                  EditorUtility.SetDirty(mat);
               }
            }
         }

         if (wetness || puddles || streams || lava)
         {
            if (MicroSplatUtilities.DrawRollup("Streams & Lava"))
            {
               if (mat.HasProperty("_GlobalPorosity") && !keywords.IsKeywordEnabled("_PERTEXPOROSITY"))
               {
                  materialEditor.ShaderProperty(shaderGUI.FindProp("_GlobalPorosity", props), "Default Porosity");
               }


               if (wetness && mat.HasProperty("_WetnessParams"))
               {
                  Vector4 wet = mat.GetVector("_WetnessParams");
                  EditorGUI.BeginChangeCheck();

                  EditorGUILayout.BeginHorizontal();
                  var oldEnabled = GUI.enabled;
                  if (globalWetness)
                     GUI.enabled = false;
                  EditorGUILayout.MinMaxSlider(CMinMaxWetness, ref wet.x, ref wet.y, 0, 1);
                  GUI.enabled = oldEnabled;
                  globalWetness = DrawGlobalToggle(GetFeatureName(DefineFeature._GLOBALWETNESS), keywords);
                  EditorGUILayout.EndHorizontal();

                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector("_WetnessParams", wet);
                     EditorUtility.SetDirty(mat);
                  }
               }
               if (heightWetness && mat.HasProperty("_HeightWetness"))
               {
                  Vector4 wet = mat.GetVector("_HeightWetness");
                  EditorGUI.BeginChangeCheck();
                  wet.x = EditorGUILayout.FloatField(CHeightWetnessHeight, wet.x);
                  wet.y = EditorGUILayout.FloatField(CHeightWetnessContrast, wet.y);
                  wet.z = EditorGUILayout.FloatField(CHeightWetnessFrequency, wet.z);
                  wet.w = EditorGUILayout.FloatField(CHeightWetnessAmplitude, wet.w);
                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector("_HeightWetness", wet);
                     EditorUtility.SetDirty(mat);
                  }
               }


               // PUDDLES
               if (puddles && mat.HasProperty("_PuddleParams") && MicroSplatUtilities.DrawRollup("Puddles", true, true))
               {
                  Vector4 pudV = mat.GetVector("_PuddleParams");
                  EditorGUI.BeginChangeCheck();
                  pudV.x = EditorGUILayout.Slider(CPuddleBlend, pudV.x, 0.01f, 60.0f);

                  EditorGUILayout.BeginHorizontal();
                  var oldEnabled = GUI.enabled;
                  if (globalPuddles)
                     GUI.enabled = false;
                  pudV.y = EditorGUILayout.Slider(CPuddleMax, pudV.y, 0.01f, 1.0f);
                  GUI.enabled = oldEnabled;
                  globalPuddles = DrawGlobalToggle(GetFeatureName(DefineFeature._GLOBALPUDDLES), keywords);
                  EditorGUILayout.EndHorizontal();

                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector("_PuddleParams", pudV);
                     EditorUtility.SetDirty(mat);
                  }

               }

               if (rainDrops && mat.HasProperty("_RainDropTexture") && MicroSplatUtilities.DrawRollup("Rain Drops", true, true))
               {
                  MicroSplatUtilities.EnforceDefaultTexture(shaderGUI.FindProp("_RainDropTexture", props), "microsplat_def_raindrops");
                  EditorGUI.BeginChangeCheck();
                  var rainParams = shaderGUI.FindProp("_RainIntensityScale", props);

                  Vector4 v = rainParams.vectorValue;


                  EditorGUILayout.BeginHorizontal();
                  var oldEnabled = GUI.enabled;
                  if (globalRain)
                     GUI.enabled = false;
                  v.x = EditorGUILayout.Slider(CRainIntensity, v.x, 0, 1);
                  GUI.enabled = oldEnabled;
                  globalRain = DrawGlobalToggle(GetFeatureName(DefineFeature._GLOBALRAIN), keywords);
                  EditorGUILayout.EndHorizontal();

                  v.y = EditorGUILayout.FloatField(CRainUVScales, v.y);

                  if (EditorGUI.EndChangeCheck())
                  {
                     rainParams.vectorValue = v;
                  }
               }


               // Streams
               if (streams && mat.HasProperty("_StreamBlend") && MicroSplatUtilities.DrawRollup("Streams", true, true))
               {
                  materialEditor.TexturePropertySingleLine(CStreamFoamTex, shaderGUI.FindProp("_StreamNormal", props));
                  MicroSplatUtilities.EnforceDefaultTexture(shaderGUI.FindProp("_StreamNormal", props), "microsplat_def_streamfoam");
                  materialEditor.ShaderProperty(shaderGUI.FindProp("_StreamBlend", props), CStreamBlend);

                  EditorGUILayout.BeginHorizontal();
                  var oldEnabled = GUI.enabled;
                  if (globalStreams)
                     GUI.enabled = false;
                  materialEditor.ShaderProperty(shaderGUI.FindProp("_StreamMax", props), CMaxStream);
                  GUI.enabled = oldEnabled;
                  globalStreams = DrawGlobalToggle(GetFeatureName(DefineFeature._GLOBALSTREAMS), keywords);
                  EditorGUILayout.EndHorizontal();

                  Vector4 pudNF = mat.GetVector("_StreamNormalFoam");
                  EditorGUI.BeginChangeCheck();
                  pudNF.x = EditorGUILayout.Slider(CStreamNormalBlend, pudNF.x, 0, 1.0f);
                  pudNF.y = EditorGUILayout.Slider(CStreamFoamStr, pudNF.y, 0.0f, 35.0f);

                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector("_StreamNormalFoam", pudNF);
                     EditorUtility.SetDirty(mat);
                  }

                  if (mat.HasProperty("_StreamFlowParams"))
                  {
                     EditorGUI.BeginChangeCheck();
                     Vector4 pudp = mat.GetVector("_StreamFlowParams");
                     Vector4 pudUV = mat.GetVector("_StreamUVScales");
                     Vector2 puv = new Vector2(pudUV.x, pudUV.y);
                     puv = EditorGUILayout.Vector2Field(CStreamUVScale, puv);
                     pudp.x = EditorGUILayout.Slider(CStreamRefractionStrength, pudp.x, 0.0f, 0.5f);
                     pudp.w = EditorGUILayout.Slider(CStreamDepthDampen, pudp.w, 0.0f, 1.0f);
                     pudp.y = EditorGUILayout.FloatField(CStreamFlowSpeed, pudp.y);
                     pudp.z = EditorGUILayout.FloatField(CStreamFlowCycle, pudp.z);

                     if (EditorGUI.EndChangeCheck())
                     {
                        mat.SetVector("_StreamFlowParams", pudp);
                        mat.SetVector("_StreamUVScales", new Vector4(puv.x, puv.y, 0, 0));
                        EditorUtility.SetDirty(mat);
                     }
                  }

                  if (heightDampStreams && mat.HasProperty("_StreamFades"))
                  {
                     materialEditor.ShaderProperty(shaderGUI.FindProp("_StreamFades", props), CHeightDamp);
                  }
               }



               if (lava && mat.HasProperty("_LavaParams2") && MicroSplatUtilities.DrawRollup("Lava", true, true))
               {
                  materialEditor.TexturePropertySingleLine(CLavaDiffuse, shaderGUI.FindProp("_LavaDiffuse", props));
                  MicroSplatUtilities.EnforceDefaultTexture(shaderGUI.FindProp("_LavaDiffuse", props), "microsplat_def_lava_01");
                  EditorGUI.BeginChangeCheck();
                  Vector4 lavaUVs = mat.GetVector("_LavaUVScale");
                  Vector2 luv = new Vector2(lavaUVs.x, lavaUVs.y);
                  luv = EditorGUILayout.Vector2Field(CLavaUVScale, luv);
                  lavaUVs.x = luv.x;
                  lavaUVs.y = luv.y;
                  Vector4 lavaParams = mat.GetVector("_LavaParams");


                  lavaParams.x = EditorGUILayout.Slider(CLavaBlend, lavaParams.x, 2.0f, 40.0f);
                  lavaParams.y = EditorGUILayout.Slider(CLavaMax, lavaParams.y, 0.0f, 1.0f);
                  lavaParams.z = EditorGUILayout.FloatField(CLavaSpeed, lavaParams.z);
                  lavaParams.w = EditorGUILayout.FloatField(CLavaIntensity, lavaParams.w);

                  Vector4 lavaParams2 = mat.GetVector("_LavaParams2");
                  lavaParams2.w = EditorGUILayout.Slider(CLavaDarkening, lavaParams2.w, 0.0f, 6.0f);
                  lavaParams2.x = EditorGUILayout.Slider(CLavaDistSize, lavaParams2.x, 0.0f, 0.3f);
                  lavaParams2.y = EditorGUILayout.Slider(CLavaDistRate, lavaParams2.y, 0.0f, 0.08f);
                  lavaParams2.z = EditorGUILayout.Slider(CLavaDistScale, lavaParams2.z, 0.02f, 1.0f);

                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector("_LavaParams", lavaParams);
                     mat.SetVector("_LavaParams2", lavaParams2);
                     mat.SetVector("_LavaUVScale", lavaUVs);
                     EditorUtility.SetDirty(mat);
                  }
                  materialEditor.ShaderProperty(shaderGUI.FindProp("_LavaColorLow", props), CLavaColor);
                  materialEditor.ShaderProperty(shaderGUI.FindProp("_LavaColorHighlight", props), CLavaHighlightColor);
                  materialEditor.ShaderProperty(shaderGUI.FindProp("_LavaEdgeColor", props), CLavaEdgeColor);
                  if (mat.HasProperty ("_LavaEmissiveMult"))
                  {
                     materialEditor.ShaderProperty (shaderGUI.FindProp ("_LavaEmissiveMult", props), CEmissionMult);
                  }

                  if (keywords.IsKeywordEnabled("_TESSDISTANCE") && mat.HasProperty("_LavaDislacementScale"))
                  {
                     materialEditor.ShaderProperty(shaderGUI.FindProp("_LavaDislacementScale", props), CLavaDisplacementScale);
                  }

                  if (heightDampLava && mat.HasProperty("_LavaFades"))
                  {
                     materialEditor.ShaderProperty(shaderGUI.FindProp("_LavaFades", props), CHeightDamp);
                  }
#if __MICROSPLAT_TEXTURECLUSTERS__
                  if (lavaStochastic && mat.HasProperty("_LavaStochasticContrast"))
                  {
                     materialEditor.ShaderProperty (shaderGUI.FindProp ("_LavaStochasticContrast", props), CLavaStochasticContrast);
                     materialEditor.ShaderProperty (shaderGUI.FindProp ("_LavaStochasticSize", props), ClavaStochasticSize);
                  }
#endif
               }
            }
         }
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();

         if (specularFade)
         {
            features.Add(GetFeatureName(DefineFeature._SPECULARFADE));
         }

         if (wetness)
         {
            features.Add(GetFeatureName(DefineFeature._WETNESS));
            if (heightWetness)
            {
               features.Add(GetFeatureName(DefineFeature._HEIGHTWETNESS));
            }

         }
         if (puddles)
         {
            features.Add(GetFeatureName(DefineFeature._PUDDLES));
         }
         if (streams)
         {
            features.Add(GetFeatureName(DefineFeature._STREAMS));
            if (heightDampStreams)
            {
               features.Add(GetFeatureName(DefineFeature._STREAMHEIGHTFILTER));
            }
         }
         if (lava)
         {
            features.Add(GetFeatureName(DefineFeature._LAVA));
            if (heightDampLava)
            {
               features.Add(GetFeatureName(DefineFeature._LAVAHEIGHTFILTER));
            }
#if __MICROSPLAT_TEXTURECLUSTERS__
            if (lavaStochastic)
            {
               features.Add (GetFeatureName (DefineFeature._LAVASTOCHASTIC));
            }
#endif
         }
         if (perTexFoam)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXFOAM));
         }
         if (perTexPorosity)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXPOROSITY));
         }
         if (dynamicFlows)
         {
            features.Add(GetFeatureName(DefineFeature._DYNAMICFLOWS));
         }
         if (wetnessMaskSnow)
         {
            features.Add(GetFeatureName(DefineFeature._WETNESSMASKSNOW));
         }

         if (puddles && rainDrops)
         {
            features.Add(GetFeatureName(DefineFeature._RAINDROPS));
         }

         if (globalWetness && wetness)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALWETNESS));
         }
         if (globalStreams && streams)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALSTREAMS));
         }
         if (globalPuddles && puddles)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALPUDDLES));
         }
         if (globalRain && puddles)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALRAIN));
         }

         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
         wetness = HasFeature(keywords, DefineFeature._WETNESS);
         heightWetness = wetness && HasFeature(keywords, DefineFeature._HEIGHTWETNESS);
         puddles = HasFeature(keywords, DefineFeature._PUDDLES);
         streams = HasFeature(keywords, DefineFeature._STREAMS);
         lava = HasFeature(keywords, DefineFeature._LAVA);
         dynamicFlows = HasFeature(keywords, DefineFeature._DYNAMICFLOWS);
         perTexFoam = HasFeature(keywords, DefineFeature._PERTEXFOAM);
         perTexPorosity = HasFeature(keywords, DefineFeature._PERTEXPOROSITY);
         wetnessMaskSnow = HasFeature(keywords, DefineFeature._WETNESSMASKSNOW);
         rainDrops = HasFeature(keywords, DefineFeature._RAINDROPS);
         globalWetness = HasFeature(keywords, DefineFeature._GLOBALWETNESS);
         globalStreams = HasFeature(keywords, DefineFeature._GLOBALSTREAMS);
         globalPuddles = HasFeature(keywords, DefineFeature._GLOBALPUDDLES);
         globalRain = HasFeature(keywords, DefineFeature._GLOBALRAIN);
         heightDampStreams = HasFeature(keywords, DefineFeature._STREAMHEIGHTFILTER);
         heightDampLava = HasFeature(keywords, DefineFeature._LAVAHEIGHTFILTER);
         specularFade = HasFeature(keywords, DefineFeature._SPECULARFADE);
#if __MICROSPLAT_TEXTURECLUSTERS__
         lavaStochastic = HasFeature (keywords, DefineFeature._LAVASTOCHASTIC);
#endif
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_wetness.txt"))
            {
               wetnessProps = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_puddles.txt"))
            {
               puddleProps = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_streams.txt"))
            {
               streamProps = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_lava.txt"))
            {
               lavaProps = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_stream_shared.txt"))
            {
               funcShared = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_cbuffer_stream_shared.txt"))
            {
               cbufferShared = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_raindrops.txt"))
            {
               rainProps = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (wetness || puddles || streams || lava)
         {
#if __MICROSPLAT_TRAX__
            sb.AppendLine ("      _TraxFXThresholds(\"Trax FX Thresholds\", Vector) = (0,0,0,0)");
#endif
            sb.AppendLine("      _StreamControl(\"Stream Control\", 2D) = \"black\" {}");
            if (dynamicFlows)
            {
               sb.AppendLine("      _DynamicStreamControl(\"Stream Control\", 2D) = \"black\" {}");
            }
         }

         if (specularFade)
         {
            sb.AppendLine("      _SpecularFades(\"Specular Heights\", Vector) = (0, 100, 0, 0)");
         }

         if (wetness || puddles || streams)
         {
            sb.AppendLine("      _GlobalPorosity(\"Porosity\", Range(0.0, 1.0)) = 0.4");
         }

         if (wetness)
         {
            sb.AppendLine(wetnessProps.text);
            if (heightWetness)
            {
               sb.Append("      _HeightWetness(\"Height Wetness Params\", Vector) = (1, 0.1, 1, 1)");
            }
         }
         if (puddles)
         {
            sb.AppendLine(puddleProps.text);
         }
         if (streams)
         {
            sb.AppendLine(streamProps.text);
            if (heightDampStreams)
            {
               sb.AppendLine("      _StreamFades(\"Stream Height Fades\", Vector) = (0, 10, 1000, 1500)");
            }
         }
         if (lava)
         {
            sb.AppendLine(lavaProps.text);
            if (heightDampLava)
            {
               sb.AppendLine("      _LavaFades(\"Lava Height Fades\", Vector) = (0, 10, 1000, 1500)");
            }

#if __MICROSPLAT_TEXTURECLUSTERS__
            if (lavaStochastic)
            {
               sb.AppendLine ("      _LavaStochasticContrast(\"Stochastic Contrast\", Range(0.001, 0.999)) = 0.5");
               sb.AppendLine ("      _LavaStochasticSize(\"Stochastic Size\", Range(0.5, 1.5)) = 1");
            }
#endif
         }

         if (puddles && rainDrops)
         {
            sb.AppendLine(rainProps.text);
         }
      }

      public override void WritePerMaterialCBuffer (string[] features, System.Text.StringBuilder sb)
      {
         if (specularFade)
         { 
            sb.AppendLine("      float2 _SpecularFades;");
         }

         if (wetness || puddles || streams || lava)
         {
            sb.AppendLine(cbufferShared.text);

#if __MICROSPLAT_TRAX__
            sb.AppendLine ("      half4 _TraxFXThresholds;");
#endif
         }
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {
         if (wetness || puddles || streams || lava)
         {
            sb.AppendLine(funcShared.text);
         }
      }

      public override void DrawPerTextureGUI(int index, MicroSplatKeywords keywords, Material mat, MicroSplatPropData propData)
      {
         if (wetness || streams)
         {
            perTexPorosity = DrawPerTexFloatSlider(index, 3, GetFeatureName(DefineFeature._PERTEXPOROSITY), 
               keywords, propData, Channel.B, CPerTexPorosity, 0.0f, 1.0f);
         }

         if (streams)
         {
            perTexFoam = DrawPerTexFloatSlider(index, 3, GetFeatureName(DefineFeature._PERTEXFOAM), 
               keywords, propData, Channel.A, CPerTexFoam, 0.0f, 2.0f);

         }
      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (wetness || puddles || streams || lava)
         {
            textureSampleCount++;
         }
         if (streams)
         {
            textureSampleCount += 4;

            if (System.Array.Exists(features, e => e == "_MAX2LAYER"))
            {
               arraySampleCount += 2;
            }
            else if (System.Array.Exists(features, e => e == "_MAX3LAYER"))
            {
               arraySampleCount += 3;
            }
            else
            {
               arraySampleCount += 4;
            }
         }
         if (lava)
         {
            textureSampleCount += 4;

#if __MICROSPLAT_TEXTURECLUSTERS__
            if (lavaStochastic)
            {
               textureSampleCount += 8;
            }
#endif
         }
         if (rainDrops)
         {
            textureSampleCount += 4;
         }
      }

   }   


   #endif
}