//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if __MICROSPLAT__
public class StreamManager : MonoBehaviour
{
   MicroSplatObject msObject;

   bool onBuffer0 = true;
   

   // Thank you unity, for your continued dedication to making things incompatible. Custom Render textures work on Standard pipeline,
   // but don't. So I have to detect the SRP and use different systems for SRP/Non-SRP code.

   public class UpdateBuffer
   {
      public virtual void Init (int w, int h) { }
      public virtual void Disable () { }
      public virtual void BlitA() { }
      public virtual void BlitB() { }
      public virtual RenderTexture GetCurrent () { return null; }

      
      public Material updateMat;
      public int width;
      public int height;
   }

   public class SRPBuffers : UpdateBuffer
   {
      public CustomRenderTexture buffer0;
      public CustomRenderTexture buffer1;
      public CustomRenderTexture currentBuffer;

      public override void BlitA ()
      {
         updateMat.SetTexture("_MainTex", buffer0);
         buffer1.Update();
         currentBuffer = buffer1;
      }
      public override void BlitB ()
      {
         updateMat.SetTexture("_MainTex", buffer1);
         buffer0.Update();
         currentBuffer = buffer0;
      }

      public override RenderTexture GetCurrent () { return currentBuffer; }

      public override void Init(int w, int h)
      {
         width = w;
         height = h;
         updateMat = new Material (Shader.Find ("Hidden/MicroSplat/StreamUpdateSRP"));
         buffer0 = new CustomRenderTexture (w, h, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
         buffer1 = new CustomRenderTexture (w, h, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
         buffer0.initializationMode = CustomRenderTextureUpdateMode.OnDemand;
         buffer1.initializationMode = CustomRenderTextureUpdateMode.OnDemand;
         buffer0.updateMode = CustomRenderTextureUpdateMode.OnDemand;
         buffer1.updateMode = CustomRenderTextureUpdateMode.OnDemand;
         buffer0.initializationSource = CustomRenderTextureInitializationSource.TextureAndColor;
         buffer1.initializationSource = CustomRenderTextureInitializationSource.TextureAndColor;
         buffer0.initializationTexture = Texture2D.blackTexture;
         buffer1.initializationTexture = Texture2D.blackTexture;
         buffer0.depth = 0;
         buffer1.depth = 0;
         buffer0.material = updateMat;
         buffer1.material = updateMat;
         buffer0.Create ();
         buffer1.Create ();
         buffer0.Initialize ();
         buffer1.Initialize ();
      }

      public override void Disable()
      {
         buffer0.Release ();
         buffer1.Release ();
         DestroyImmediate (buffer0);
         DestroyImmediate (buffer1);

         DestroyImmediate (updateMat);
         buffer0 = null;
         buffer1 = null;
         updateMat = null;
      }
   }

   public class StandardBuffers : UpdateBuffer
   {
      public RenderTexture buffer0;
      public RenderTexture buffer1;
      public RenderTexture currentBuffer;

      public override void BlitA ()
      {
         Graphics.Blit (buffer0, buffer1, updateMat);
         currentBuffer = buffer1;
      }
      public override void BlitB ()
      {
         Graphics.Blit (buffer1, buffer0, updateMat);
         currentBuffer = buffer0;
      }

      public override RenderTexture GetCurrent () { return currentBuffer; }

      public override void Init(int w, int h)
      {
         width = w;
         height = h;
         updateMat = new Material (Shader.Find ("Hidden/MicroSplat/StreamUpdate"));
         buffer0 = new RenderTexture (w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
         buffer1 = new RenderTexture (w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
         Graphics.Blit (Texture2D.blackTexture, buffer0);
         Graphics.Blit (Texture2D.blackTexture, buffer1);
      }

      public override void Disable ()
      {
         buffer0.Release ();
         buffer1.Release ();
         DestroyImmediate (buffer0);
         DestroyImmediate (buffer1);

         DestroyImmediate (updateMat);
         buffer0 = null;
         buffer1 = null;
         updateMat = null;
      }
   }

   public UpdateBuffer updateBuffer;


   Vector4[] spawnBuffer = new Vector4[64];
   Vector4[] colliderBuffer = new Vector4[64];

   Texture2D terrainDesc;


   // props
   public Vector2 evaporation = new Vector2(0.01f, 0.01f);
   public Vector2 strength = new Vector2(1.0f, 1.0f);
   public Vector2 speed = new Vector2(1, 1);
   public Vector2 resistance = new Vector2(0.1f, 0.1f);
   public float wetnessEvaporation = 0.01f;
   public float burnEvaporation = 0.01f;

   List<StreamEmitter> emitters = new List<StreamEmitter>(16);
   List<StreamCollider> colliders = new List<StreamCollider>(16);

   static Vector2 WorldToTerrain(MicroSplatObject ter, Vector3 point, int width, int height)
   {
      Bounds b = ter.GetBounds();
      point = ter.transform.worldToLocalMatrix.MultiplyPoint(point);
      float x = (point.x / b.size.x) * width;
      float z = (point.z / b.size.z) * height;
      return new Vector2(x, z);
   }

   public void Register(StreamEmitter e)
   {
      emitters.Add(e);
   }

   public void Unregister(StreamEmitter e)
   {
      emitters.Remove(e);
   }

   public void Register(StreamCollider e)
   {
      colliders.Add(e);
   }

   public void Unregister(StreamCollider e)
   {
      colliders.Remove(e);
   }

   void Awake()
   {
      msObject = GetComponent<MicroSplatObject>();
   }

   void OnEnable()
   {
      terrainDesc = msObject.terrainDesc;
      if (terrainDesc == null)
      {
         Debug.LogError ("Terrain Descriptor does not exist on the terrain, please create it from the MicroSplatTerrain component");
      }
      int w = terrainDesc.width;
      int h = terrainDesc.height;

      if (msObject.keywordSO == null)
      {
         Debug.LogError ("Terrain does not have keywords");
         return;
      }
      if (msObject.keywordSO.IsKeywordEnabled("_MSRENDERLOOP_UNITYLD") || msObject.keywordSO.IsKeywordEnabled("_MSRENDERLOOP_UNITYHD"))
      {
         updateBuffer = new SRPBuffers ();
      }
      else
      {
         updateBuffer = new StandardBuffers ();
      }

      updateBuffer.Init (w, h);
   }

   void OnDisable()
   {
      onBuffer0 = false;
      updateBuffer.Disable ();
      updateBuffer = null;
   }


   double timeSinceWetnessEvap = 0;
   double timeSinceBurnEvap = 0;
   double timeSinceEvapX = 0;
   double timeSinceEvapY = 0;

   Vector2 evapAmount = new Vector2(0, 0);

   void Update()
   {
      if (msObject == null)
      {
         return;
      }
      int emitterCount = emitters.Count;
      if (emitterCount > 64)
      {
         emitterCount = 64;
      }

      int colliderCount = colliders.Count;
      if (colliderCount > 64)
      {
         colliderCount = 64;
      }

      int usedEmitters = 0;
      for (int i = 0; i < emitters.Count; ++i)
      {
         var e = emitters[i];
         Vector2 ter = WorldToTerrain(msObject, e.transform.position, updateBuffer.width, updateBuffer.height);
         if (ter.x >= 0 && ter.x < updateBuffer.width && ter.y >= 0 && ter.y < updateBuffer.height)
         {
            Vector3 pos = e.transform.position + Vector3.left * e.transform.lossyScale.x;
            Vector2 endPoint = WorldToTerrain(msObject, pos, updateBuffer.width, updateBuffer.height);
            float d = Vector2.Distance(ter, endPoint);
            if (d < 1)
               d = 1;
            d *= e.strength;

            Vector4 data = new Vector4(ter.x, ter.y, 0, 0);
            if (e.emitterType == StreamEmitter.EmitterType.Water)
            {
               data.z = d;
            }
            else
            {
               data.w = d;
            }
            spawnBuffer[usedEmitters] = data;
            usedEmitters++;
         }
      }

      int usedColliders = 0;
      for (int i = 0; i < colliders.Count; ++i)
      {
         var c = colliders[i];
         Vector2 ter = WorldToTerrain(msObject, c.transform.position, updateBuffer.width, updateBuffer.height);

         if (ter.x >= 0 && ter.x < updateBuffer.width && ter.y >= 0 && ter.y < updateBuffer.height)
         {
            Vector3 pos = c.transform.position + Vector3.left * c.transform.lossyScale.x;
            Vector2 endPoint = WorldToTerrain(msObject, pos, updateBuffer.width, updateBuffer.height);
            float d = Vector2.Distance(ter, endPoint);

            Vector4 data = new Vector4(ter.x, ter.y, 0, 0);
            if (c.colliderType != StreamCollider.ColliderType.Lava)
            {
               data.z = d;
            }
            if (c.colliderType != StreamCollider.ColliderType.Water)
            {
               data.w = d;
            }


            colliderBuffer[usedColliders] = data;
            usedColliders++;
         }
      }

      updateBuffer.updateMat.SetVectorArray("_Positions", spawnBuffer);
      updateBuffer.updateMat.SetVectorArray("_Colliders", colliderBuffer);
      updateBuffer.updateMat.SetInt("_PositionsCount", usedEmitters);
      updateBuffer.updateMat.SetInt("_CollidersCount", usedColliders);
      updateBuffer.updateMat.SetVector("_SpawnStrength", strength);

      updateBuffer.updateMat.SetTexture("_TerrainDesc", terrainDesc);
      updateBuffer.updateMat.SetFloat("_DeltaTime", Time.smoothDeltaTime);
      updateBuffer.updateMat.SetVector("_Speed", speed);
      updateBuffer.updateMat.SetVector("_Resistance", resistance);


      if (onBuffer0)
      {
         if (evaporation.x > 0)
         {
            float evapDelay = (1.0f / evaporation.x) / 255.0f;
            if (timeSinceEvapX > evapDelay)
            {
               timeSinceEvapX = 0;
               evapAmount.x = 0.004f;
            }
            else
            {
               evapAmount.x = 0;
            }
         }
         if (evaporation.y > 0)
         {
            float evapDelay = (1.0f / evaporation.y) / 255.0f;
            if (timeSinceEvapY > evapDelay)
            {
               timeSinceEvapY = 0;
               evapAmount.y = 0.004f;
            }
            else
            {
               evapAmount.y = 0;
            }
         }
         updateBuffer.updateMat.SetVector("_Evaporation", evapAmount);

         if (wetnessEvaporation > 0)
         {
            float wetnessDelay = (1.0f / wetnessEvaporation) / 255.0f;
            if (timeSinceWetnessEvap > wetnessDelay)
            {
               updateBuffer.updateMat.SetFloat("_WetnessEvaporation", 0.004f);
               timeSinceWetnessEvap = 0;
            }
            else
            {
               updateBuffer.updateMat.SetFloat("_WetnessEvaporation", 0);
            }
         }

         if (burnEvaporation > 0)
         {
            float burnDelay = (1.0f * burnEvaporation) / 255.0f;
            if (timeSinceBurnEvap > burnDelay)
            {
               updateBuffer.updateMat.SetFloat("_BurnEvaporation", 0.004f);
               timeSinceBurnEvap = 0;
            }
            else
            {
               updateBuffer.updateMat.SetFloat("_BurnEvaporation", 0);
            }
         }

         updateBuffer.BlitA ();

      }
      else
      {
         // only spawn, evaporate on first pass
         updateBuffer.updateMat.SetInt("_PositionsCount", 0);
         updateBuffer.updateMat.SetVector("_Evaporation", Vector2.zero);
         updateBuffer.updateMat.SetFloat("_WetnessEvaporation", 0);
         updateBuffer.updateMat.SetFloat("_BurnEvaporation", 0);
         updateBuffer.BlitB ();
      }
      onBuffer0 = !onBuffer0;

      float dt = Time.deltaTime;
      timeSinceEvapX += dt;
      timeSinceEvapY += dt;
      timeSinceWetnessEvap += dt;
      timeSinceBurnEvap += dt;

      msObject.matInstance.SetTexture("_DynamicStreamControl", updateBuffer.GetCurrent());

   }
}

#endif