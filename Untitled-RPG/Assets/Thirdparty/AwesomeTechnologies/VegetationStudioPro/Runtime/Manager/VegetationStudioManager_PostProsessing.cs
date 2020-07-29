using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Collections;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace AwesomeTechnologies.VegetationStudio
{
#if UNITY_POST_PROCESSING_STACK_V2
    [System.Serializable]
    public class PostProcessProfileInfo
    {
        public bool Enabled = true;
        public PostProcessProfile PostProcessProfile;
        public BiomeType BiomeType = BiomeType.Biome1;
        public float BlendDistance = 0;
        public float Weight = 1;
        public float VolumeHeight = 20;
        public float Priority; 
    }    

    public partial class VegetationStudioManager
    {



        public void RefreshPostProcessVolumes()
        {
            BiomeMaskArea[] biomeMaskAreas = Object.FindObjectsOfType<BiomeMaskArea>();

            for (int i = 0; i <= biomeMaskAreas.Length - 1; i++)
            {
                PostProcessProfileInfo postProcessProfileInfo = Instance_GetPostProcessProfileInfo(biomeMaskAreas[i].BiomeType);
                biomeMaskAreas[i].RefreshPostProcessVolume(postProcessProfileInfo, PostProcessingLayer);
            }           
        }

        public PostProcessProfileInfo Instance_GetPostProcessProfileInfo(BiomeType biomeType)
        {
            for (int i = 0; i <= PostProcessProfileInfoList.Count - 1; i++)
            {
                if (PostProcessProfileInfoList[i].BiomeType == biomeType)
                {
                    return PostProcessProfileInfoList[i];
                }
            }

            return null;
        }



        public static LayerMask GetPostProcessingLayer()
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                return Instance.PostProcessingLayer;
            }
            return 0;
        }

        public static PostProcessProfileInfo GetPostProcessProfileInfo(BiomeType biomeType)
        {
            if (!Instance) FindInstance();
            if (Instance)
            {
                return Instance.Instance_GetPostProcessProfileInfo(biomeType);
            }
            return null;
        }

        public void AddPostProcessProfile(PostProcessProfile postProcessProfile)
        {
            PostProcessProfileInfo postProcessProfileInfo =
                new PostProcessProfileInfo {PostProcessProfile = postProcessProfile};
            PostProcessProfileInfoList.Add(postProcessProfileInfo);
            RefreshPostProcessVolumes();
        }

        public void RemovePostProcessProfile(int index)
        {
            PostProcessProfileInfoList.RemoveAt(index);
            RefreshPostProcessVolumes();
        }
    }
#endif
}
