using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
  // GPU instanced lightning bolt render for a specific art style item.
  [RequireComponent(typeof(AudioSource))]
  public class LightningRenderer : BaseSpriteInstancedRenderer
  {
    static List<LightningSpawnArea> m_SpawnAreas = new List<LightningSpawnArea>();

    float m_LightningProbability;
    float m_NextSpawnTime;
    SkyProfile m_SkyProfile;
    LightningArtItem m_Style;
    float m_TimeOfDay;
    AudioSource m_AudioSource;
    float m_LightningIntensity;
    float m_ThunderSoundDelay;
    float m_SpawnCoolDown;

    // Frequency in time that we'll evaluate lighting probability at, so we're frame rate independant.
    const float k_ProbabiltyCheckInterval = .5f;

    private void Start()
    {
      // Verify GPU instancing is supported.
      if (SystemInfo.supportsInstancing == false) {
        Debug.LogError("Can't render lightning since GPU instancing is not supported on this platform.");
        enabled = false;
        return;
      }

      m_AudioSource = GetComponent<AudioSource>();
    }

    protected override Bounds CalculateMeshBounds()
    {
      return new Bounds(Vector3.zero, new Vector3(500, 500, 500));
    }

    // Allocate a new data item to track a sprite mesh rendering.
    protected override BaseSpriteItemData CreateSpriteItemData()
    {
      return new BaseSpriteItemData();
    }

    // Hook to let client prepare and check if we should render before trying.
    protected override bool IsRenderingEnabled()
    {
      if (m_SkyProfile == null || m_SkyProfile.IsFeatureEnabled(ProfileFeatureKeys.LightningFeature) == false) {
        return false;
      }

      if (m_SpawnAreas.Count == 0) {
        return false;
      }

      return true;
    }

    // Select where the next sprite will be rendered at.
    protected override void CalculateSpriteTRS(BaseSpriteItemData data, out Vector3 spritePosition, out Quaternion spriteRotation, out Vector3 spriteScale)
    {
      LightningSpawnArea area = GetRandomLightningSpawnArea();
      
      float boltScale = CalculateLightningBoltScaleForArea(area);
      spriteScale = new Vector3(boltScale, boltScale, boltScale);

      spritePosition = GetRandomWorldPositionInsideSpawnArea(area);
      
      // Always face the sprite textures towards the main camera.
      if (Camera.main == null) {
        Debug.LogError("Can't billboard lightning to viewer since there is no main camera tagged.");
        spriteRotation = area.transform.rotation;
      } else {
        spriteRotation = Quaternion.LookRotation(spritePosition - Camera.main.transform.position, Vector3.up);
      }
    }

    // Configure a new sprite item data object properties, (could be new or recycled).
    protected override void ConfigureSpriteItemData(BaseSpriteItemData data)
    {
      // A new bolt is getting prepared, so let's schedule the sound effect.
      if (m_SkyProfile.IsFeatureEnabled(ProfileFeatureKeys.ThunderFeature)) {
        Invoke("PlayThunderBoltSound", m_ThunderSoundDelay);
      }
    }

    // Setup any per-instance data you need to pass.
    protected override void PrepareDataArraysForRendering(int instanceId, BaseSpriteItemData data)
    {
      // No custom properties.
    }

    protected override void PopulatePropertyBlockForRendering(ref MaterialPropertyBlock propertyBlock)
    {
      propertyBlock.SetFloat("_Intensity", m_LightningIntensity);
    }

    protected override int GetNextSpawnCount()
    {
      // Cool down from last spawn.
      if (m_NextSpawnTime > Time.time) {
        return 0;
      }

      m_NextSpawnTime = Time.time + k_ProbabiltyCheckInterval;

      if (Random.value < m_LightningProbability) {
        m_NextSpawnTime += m_SpawnCoolDown;
        return 1;
      }

      return 0;
    }

    public void UpdateForTimeOfDay(SkyProfile skyProfile, float timeOfDay, LightningArtItem artItem)
    {
      m_SkyProfile = skyProfile;
      m_TimeOfDay = timeOfDay;
      m_Style = artItem;

      if (m_SkyProfile == null) {
        Debug.LogError("Assigned null sky profile!");
        return;
      }

      if (m_Style == null) {
        Debug.LogError("Can't render lightning without an art item");
        return;
      }

      SyncDataFromSkyProfile();
    }

    void SyncDataFromSkyProfile()
    {
      m_LightningProbability = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.LightningProbabilityKey, m_TimeOfDay);
      m_LightningIntensity = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.LightningIntensityKey, m_TimeOfDay);
      m_SpawnCoolDown = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.LightningStrikeCoolDown, m_TimeOfDay);
      m_ThunderSoundDelay = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.ThunderSoundDelayKey, m_TimeOfDay);

      // Scale the probability for this art style.
      m_LightningProbability *= m_Style.strikeProbability;
      m_LightningIntensity *= m_Style.intensity;

      m_SpriteSheetLayout.columns = m_Style.columns;
      m_SpriteSheetLayout.rows = m_Style.rows;
      m_SpriteSheetLayout.frameCount = m_Style.totalFrames;
      m_SpriteSheetLayout.frameRate = m_Style.animateSpeed;

      m_TintColor = m_Style.tintColor * m_SkyProfile.GetColorPropertyValue(ProfilePropertyKeys.LightningTintColorKey, m_TimeOfDay);
      renderMaterial = m_Style.material;
      
      modelMesh = m_Style.mesh;
    }

    private LightningSpawnArea GetRandomLightningSpawnArea()
    {
      if (m_SpawnAreas.Count == 0) {
        return null;
      }

      int randomSpawnIndex = Mathf.RoundToInt(Random.Range(0, m_SpawnAreas.Count)) % m_SpawnAreas.Count;
      return m_SpawnAreas[randomSpawnIndex];
    }

    private void PlayThunderBoltSound()
    {
      if (m_Style.thunderSound != null) {
        m_AudioSource.volume = m_SkyProfile.GetNumberPropertyValue(ProfilePropertyKeys.ThunderSoundVolumeKey, m_TimeOfDay);
        m_AudioSource.PlayOneShot(m_Style.thunderSound);
      }
    }

    public static void AddSpawnArea(LightningSpawnArea area)
    {
      if (m_SpawnAreas.Contains(area) == false) {
        m_SpawnAreas.Add(area);
      }
    }

    public static void RemoveSpawnArea(LightningSpawnArea area)
    {
      if (m_SpawnAreas.Contains(area)) {
        m_SpawnAreas.Remove(area);
      }
    }

    Vector3 GetRandomWorldPositionInsideSpawnArea(LightningSpawnArea area)
    {
      float xPos = Random.Range(-area.lightningArea.x, area.lightningArea.x) / 2.0f;
      float zPos = Random.Range(-area.lightningArea.z, area.lightningArea.z) / 2.0f;

      float yPos = 0;
      if (m_Style.alignment == LightningArtItem.Alignment.TopAlign) {
        yPos = (area.lightningArea.y / 2.0f) - (m_Style.size / 2.0f);
      }

      return area.transform.TransformPoint(new Vector3(xPos, yPos, zPos));
    }

    float CalculateLightningBoltScaleForArea(LightningSpawnArea area)
    {
      if (m_Style.alignment == LightningArtItem.Alignment.ScaleToFit) {
        return area.lightningArea.y / 2.0f;
      } else {
        return m_Style.size;
      }
    }
  }
}

