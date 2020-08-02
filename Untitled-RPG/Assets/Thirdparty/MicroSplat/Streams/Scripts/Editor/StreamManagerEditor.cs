//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;

#if __MICROSPLAT__ && __MICROSPLAT_STREAMS__
[CustomEditor (typeof(StreamManager))]
public class StreamManagerEditor : Editor 
{
   static GUIContent CStrength = new GUIContent("Strength", "How much matter leaks into neighboring areas");
   static GUIContent CEvaporation = new GUIContent("Evaporation", "How much matter disolves over time");
   static GUIContent CSpeed = new GUIContent("Speed", "Speed of simulation. Note, due to the nature of the simulation, changing speed will change the simulation results");
   static GUIContent CResistance = new GUIContent("Resistance", "Higher resistance will keep the material from spreading out as much");
   static GUIContent CWetnessEvap = new GUIContent("Wetness Evaporation", "Controls evaporation of wetness after the water leaves the area");
   static GUIContent CBurnEvap = new GUIContent("Burn Evaporation", "How long the ground stays charred after the lava leaves");


   public override void OnInspectorGUI()
   {
      StreamManager sm = target as StreamManager;

      MicroSplatObject mso = sm.GetComponent<MicroSplatObject>();


      if (mso == null)
      {
         EditorGUILayout.HelpBox("Must be on a MicroSplatObject (terrain, mesh terrain, etc)", MessageType.Error);
         return;
      }
      if (mso.keywordSO == null)
      {
         EditorGUILayout.HelpBox("MicroSplat Terrain is missing keywords", MessageType.Error);
         return;
      }
      if (!mso.keywordSO.IsKeywordEnabled("_DYNAMICFLOWS"))
      {
         EditorGUILayout.HelpBox("Must have Dynamic Flows ON in your terrain material", MessageType.Error);
         return;
      }

    
      if (MicroSplatUtilities.DrawRollup("Streams", true, false))
      {
         sm.strength.x = EditorGUILayout.Slider(CStrength, sm.strength.x, 0, 1);
         sm.evaporation.x = EditorGUILayout.Slider(CEvaporation, sm.evaporation.x, 0, 0.5f);
         sm.speed.x = EditorGUILayout.Slider(CSpeed, sm.speed.x, 0, 10);
         sm.resistance.x = EditorGUILayout.Slider(CResistance, sm.resistance.x, 0.005f, 0.75f);
         sm.wetnessEvaporation = EditorGUILayout.Slider(CWetnessEvap, sm.wetnessEvaporation, 0.0f, 0.5f);
         if (Application.isPlaying)
         {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("Save State"))
            {
               SaveStream(sm);
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
         }
      }

      if (MicroSplatUtilities.DrawRollup("Lava", true, false))
      {
         sm.strength.y = EditorGUILayout.Slider(CStrength, sm.strength.y, 0, 1);
         sm.evaporation.y = EditorGUILayout.Slider(CEvaporation, sm.evaporation.y, 0, 0.5f);
         sm.speed.y = EditorGUILayout.Slider(CSpeed, sm.speed.y, 0, 10);
         sm.resistance.y = EditorGUILayout.Slider(CResistance, sm.resistance.y, 0.005f, 0.75f);
         sm.burnEvaporation = EditorGUILayout.Slider(CBurnEvap, sm.burnEvaporation, 0.0f, 0.5f);
         if (Application.isPlaying)
         {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("Save State"))
            {
               SaveLava(sm);
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
         }
      }
      if (sm.updateBuffer != null)
      {
         if (sm.updateBuffer.GetCurrent () != null)
         {
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("Dynamic Buffer");
            int mem = sm.updateBuffer.width * sm.updateBuffer.height * 2;
            mem /= 128;
            EditorGUILayout.LabelField ("Buffer Memory: " + mem.ToString () + "kb");
            EditorGUILayout.EndHorizontal ();
            GUILayout.Box (sm.updateBuffer.GetCurrent(), GUILayout.Width (256), GUILayout.Height (256));
         }
      }
   }


   void SaveStream(StreamManager sm)
   {
      MicroSplatTerrain mt = sm.gameObject.GetComponent<MicroSplatTerrain>();
      var streamTex = mt.streamTexture;
      var buffer = sm.updateBuffer.GetCurrent();
      if (mt == null || streamTex == null || buffer == null || buffer.width != streamTex.width || buffer.height != streamTex.height)
      {
         return;
      }
      RenderTexture.active = buffer;
      Texture2D tex = new Texture2D(buffer.width, buffer.height, TextureFormat.ARGB32, false, true);
      tex.ReadPixels(new Rect(0, 0, buffer.width, buffer.height), 0, 0);
      tex.Apply();
      RenderTexture.active = null;
      var srcClrs = tex.GetPixels();
      var trgClrs = streamTex.GetPixels();
      for (int i = 0; i < srcClrs.Length; ++i)
      {
         Color t = trgClrs[i];
         Color d = srcClrs[i];
         t.b = d.b;
         trgClrs[i] = t;
      }
      streamTex.SetPixels(trgClrs);
      streamTex.Apply();

      DestroyImmediate(tex);
      var path = AssetDatabase.GetAssetPath(streamTex);
      path = path.Replace("\\", "/");
      path = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "/" + path;
      var bytes = streamTex.EncodeToPNG();
      System.IO.File.WriteAllBytes(path, bytes);
      AssetDatabase.Refresh();
   }

   void SaveLava(StreamManager sm)
   {
      MicroSplatTerrain mt = sm.gameObject.GetComponent<MicroSplatTerrain>();
      var streamTex = mt.streamTexture;
      var buffer = sm.updateBuffer.GetCurrent ();
      if (mt == null || streamTex == null || buffer == null || buffer.width != streamTex.width || buffer.height != streamTex.height)
      {
         return;
      }
      RenderTexture.active = buffer;
      Texture2D tex = new Texture2D(buffer.width, buffer.height, TextureFormat.ARGB32, false, true);
      tex.ReadPixels(new Rect(0, 0, buffer.width, buffer.height), 0, 0);
      tex.Apply();
      RenderTexture.active = null;
      var srcClrs = tex.GetPixels();
      var trgClrs = streamTex.GetPixels();
      for (int i = 0; i < srcClrs.Length; ++i)
      {
         Color t = trgClrs[i];
         Color d = srcClrs[i];
         t.a = d.a;
         trgClrs[i] = t;
      }
      streamTex.SetPixels(trgClrs);
      streamTex.Apply();
      DestroyImmediate(tex);
      var path = AssetDatabase.GetAssetPath(streamTex);
      path = path.Replace("\\", "/");
      path = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "/" + path;
      var bytes = streamTex.EncodeToPNG();
      System.IO.File.WriteAllBytes(path, bytes);
      AssetDatabase.Refresh();
   }

}
#endif