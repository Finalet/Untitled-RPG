using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // Base abstract class for effeciently rendering GPU instanced meshes.
  public abstract class BaseSpriteInstancedRenderer : MonoBehaviour
  {
    // Absolute maximum of arrays, any higher and GPUs might not support it.
    public const int kArrayMaxSprites = 1000;

    // User configured maximum to stay below (must be less than kMaxSprites.
    public int maxSprites { get; protected set; }

    [Tooltip("Mesh used to render the instances onto. If empty, a quad will be used.")]
    public Mesh modelMesh;

    [Tooltip("Sky Studio sprite sheet animated shader material.")]
    public Material renderMaterial;

    protected Queue<BaseSpriteItemData> m_Available = new Queue<BaseSpriteItemData>();
    protected HashSet<BaseSpriteItemData> m_Active = new HashSet<BaseSpriteItemData>();

    MaterialPropertyBlock m_PropertyBlock;

    // Fields for the gpu instancing arguments.
    Matrix4x4[] m_ModelMatrices = new Matrix4x4[kArrayMaxSprites];
    float[] m_StartTimes = new float[kArrayMaxSprites];
    float[] m_EndTimes = new float[kArrayMaxSprites];
  
    // Sprite sheet info.
    protected SpriteSheetData m_SpriteSheetLayout = new SpriteSheetData();
    protected Texture m_SpriteTexture;
    protected Color m_TintColor = Color.white;
    protected Camera m_ViewerCamera { get; set; }
    protected Mesh m_DefaltModelMesh;

    void Start()
    {
      // Verify GPU instancing is supported.
      if (SystemInfo.supportsInstancing == false) {
        Debug.LogError("Can't render since GPU instancing isn't supported on this device");
        enabled = false;
        return;
      }

      m_ViewerCamera = Camera.main;
    }
    
    // Bounds so the instanced rendering isn't culled.
    protected abstract Bounds CalculateMeshBounds();

    // Allocate a new data item to track a sprite mesh rendering.
    protected abstract BaseSpriteItemData CreateSpriteItemData();

    // Hook to let client prepare and check if we should render before trying.
    protected abstract bool IsRenderingEnabled();

    // Ask the subclass how many new instances we should create this frame.
    protected abstract int GetNextSpawnCount();

    // Select where the next sprite will be rendered at.
    protected abstract void CalculateSpriteTRS(BaseSpriteItemData data, out Vector3 spritePosition, out Quaternion spriteRotation, out Vector3 spriteScale);

    // Configure a new sprite item data object properties, (could be new or recycled).
    protected abstract void ConfigureSpriteItemData(BaseSpriteItemData data);

    // Setup any per-instance data you need to pass.
    protected abstract void PrepareDataArraysForRendering(int instanceId, BaseSpriteItemData data);

    protected abstract void PopulatePropertyBlockForRendering(ref MaterialPropertyBlock propertyBlock);

    // Get or create a instance data field for a particle.
    BaseSpriteItemData DequeueNextSpriteItemData()
    {
      BaseSpriteItemData data = null;

      if (m_Available.Count == 0) {
        data = CreateSpriteItemData();
      } else {
        data = m_Available.Dequeue();
      }

      m_Active.Add(data);

      return data;
    }

    void ReturnSpriteItemData(BaseSpriteItemData splash)
    {
      splash.Reset();
      m_Active.Remove(splash);
      m_Available.Enqueue(splash);
    }

    protected virtual void LateUpdate()
    {
      m_ViewerCamera = Camera.main;

      if (IsRenderingEnabled() == false) {
        return;
      }

      GenerateNewSprites();
      AdvanceAllSprites();
      RenderAllSprites();
    }

    void GenerateNewSprites()
    {
      int spawnCount = GetNextSpawnCount();

      Vector3 spritePosition;
      Vector3 spriteScale;
      Quaternion spriteRotation;

      for (int i = 0; i < spawnCount; i++) {
        // Dequeue a sprite item data, and configure it.
        BaseSpriteItemData data = DequeueNextSpriteItemData();
        data.spriteSheetData = m_SpriteSheetLayout;
        
        ConfigureSpriteItemData(data);
        CalculateSpriteTRS(data, out spritePosition, out spriteRotation, out spriteScale);

        data.SetTRSMatrix(spritePosition, spriteRotation, spriteScale);
        data.Start();
      }
    }

    void AdvanceAllSprites()
    {
      // FIXME - Lets not make a copy, just make returning safe.
      HashSet<BaseSpriteItemData> dataCopy = new HashSet<BaseSpriteItemData>(m_Active);
      foreach (BaseSpriteItemData data in dataCopy) {
        data.Continue();

        if (data.state == BaseSpriteItemData.SpriteState.Complete) {
          ReturnSpriteItemData(data);
        }
      }
    }

    void RenderAllSprites()
    {
      if (m_Active.Count == 0) {
        return;
      }
      
      if (renderMaterial == null) {
        Debug.LogError("Can't render sprite without a material.");
        return;
      }
      
      if (m_PropertyBlock == null) {
        m_PropertyBlock = new MaterialPropertyBlock();
      }
      
      int renderCount = 0;
      foreach (BaseSpriteItemData data in m_Active) {
        if (renderCount >= kArrayMaxSprites) {
          Debug.LogError("Can't render any more sprites...");
          break;
        }
      
        if (data.state != BaseSpriteItemData.SpriteState.Animating) {
          continue;
        }
      
        if (data.startTime > Time.time) {
          continue;
        }
      
        m_ModelMatrices[renderCount] = data.modelMatrix;
        m_StartTimes[renderCount] = data.startTime;
        m_EndTimes[renderCount] = data.endTime;
      
        PrepareDataArraysForRendering(renderCount, data);
      
        renderCount++;
      }
      
      if (renderCount == 0) {
        return;
      }
      
      m_PropertyBlock.Clear();

      // Sprite sheet timing.
      m_PropertyBlock.SetFloatArray("_StartTime", m_StartTimes);
      m_PropertyBlock.SetFloatArray("_EndTime", m_EndTimes);
      
      // For some reason using the property block fails for the sprite texture.
      // Feels like a Unity bug possibly. Instead we assign that texture
      // at the time of creating the renderMaterial to work around the issue.
      // Using renderMaterial.SetTexture() leaks a material every frame.

      // Sprite sheet layout.
      m_PropertyBlock.SetFloat("_SpriteColumnCount", m_SpriteSheetLayout.columns);
      m_PropertyBlock.SetFloat("_SpriteRowCount", m_SpriteSheetLayout.rows);
      m_PropertyBlock.SetFloat("_SpriteItemCount", m_SpriteSheetLayout.frameCount);
      m_PropertyBlock.SetFloat("_AnimationSpeed", m_SpriteSheetLayout.frameRate);
      m_PropertyBlock.SetVector("_TintColor", m_TintColor);
      
      PopulatePropertyBlockForRendering(ref m_PropertyBlock); 
      
      Mesh mesh = GetMesh();
      mesh.bounds = CalculateMeshBounds();
      
      Graphics.DrawMeshInstanced(
        mesh, 0, renderMaterial, m_ModelMatrices, renderCount, m_PropertyBlock,
        UnityEngine.Rendering.ShadowCastingMode.Off, false, LayerMask.NameToLayer("TransparentFX"));
    }

    protected Mesh GetMesh() {
      if (modelMesh) {
        return modelMesh;
      }

      if (m_DefaltModelMesh) {
        return m_DefaltModelMesh;
      }

      m_DefaltModelMesh = GenerateMesh();

      return m_DefaltModelMesh;
    }

    protected virtual Mesh GenerateMesh()
    {
      Mesh m = new Mesh();

      Vector3[] verts = new Vector3[4];
      verts[0] = new Vector3(-1, -1, 0);
      verts[1] = new Vector3(-1, 1, 0);
      verts[2] = new Vector3(1, 1, 0);
      verts[3] = new Vector3(1, -1, 0);

      Vector2[] uvs = new Vector2[4];
      uvs[0] = new Vector2(0, 0);
      uvs[1] = new Vector2(0, 1);
      uvs[2] = new Vector2(1, 1);
      uvs[3] = new Vector2(1, 0);

      int[] triangles = new int[6];
      triangles[0] = 0;
      triangles[1] = 1;
      triangles[2] = 2;
      triangles[3] = 0;
      triangles[4] = 2;
      triangles[5] = 3;

      m.vertices = verts;
      m.uv = uvs;
      m.triangles = triangles;
      m.bounds = new Bounds(Vector3.zero, new Vector3(500.0f, 500.0f, 500.0f));

      return m;
    }
  }
}
