using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Funly.SkyStudio
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(UniversalAdditionalCameraData))]
    public class URPWeatherDepth : MonoBehaviour
    {
        public RenderTexture renderTexture;
        private Camera m_Camera;
        private UniversalAdditionalCameraData m_CameraData;
        
        private void Start()
        {
            m_Camera = GetComponent<Camera>();
            m_CameraData = GetComponent<UniversalAdditionalCameraData>();
        }
        
        private void Update()
        {
            // FIXME - This assumes our renderer is at index 1 in the pipeline. This is very fragile for custom pipelines.
            m_CameraData.SetRenderer(1);
            
            Shader.SetGlobalTexture("_OverheadDepthTex", renderTexture);
            Shader.SetGlobalVector("_OverheadDepthPosition", m_Camera.transform.position);
            Shader.SetGlobalFloat("_OverheadDepthNearClip", m_Camera.nearClipPlane);
            Shader.SetGlobalFloat("_OverheadDepthFarClip", m_Camera.farClipPlane);
        }
    }
}
