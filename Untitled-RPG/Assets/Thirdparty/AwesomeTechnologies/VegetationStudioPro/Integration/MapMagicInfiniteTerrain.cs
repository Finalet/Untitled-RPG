using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.External.MapMagicInterface
{
    public class MapMagicInfiniteTerrain : MonoBehaviour
    {     
        // ReSharper disable once UnusedMember.Local
        void OnEnable()
        {
#if MAPMAGIC
            MapMagic.MapMagic.OnApplyCompleted += OnGenerateCompleted;
#endif
        }

        private void OnDisable()
        {
#if MAPMAGIC
            MapMagic.MapMagic.OnApplyCompleted -= OnGenerateCompleted;
#endif
        }

        // ReSharper disable once UnusedMember.Local
        void OnGenerateCompleted(Terrain terrain)
        {
            UnityTerrain unityTerrain = terrain.gameObject.GetComponent<UnityTerrain>();
            if (!unityTerrain)
            {
                unityTerrain = terrain.gameObject.AddComponent<UnityTerrain>();
            }            
            unityTerrain.TerrainPosition = terrain.transform.position;
            unityTerrain.AutoAddToVegegetationSystem = true;
            unityTerrain.AddTerrainToVegetationSystem();
        }     
    }
}
