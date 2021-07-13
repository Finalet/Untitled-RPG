using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace Funly.SkyStudio {
    public class RenderingConfig {
        public enum DetectedRenderingConfig {
            Unsupported,
            Builtin,
            URP
        }


        public static DetectedRenderingConfig Detect() {
            RenderPipelineAsset pipeline = GraphicsSettings.renderPipelineAsset;

            if (pipeline == null) {
                return DetectedRenderingConfig.Builtin;
            }

            string assetPipelineClass = pipeline.GetType().Name;

            Debug.Log($"Detected asset pipeline class name: {assetPipelineClass}");

            if (assetPipelineClass.Contains("HDRenderPipelineAsset")) {
                Debug.Log("Sky Studio does not support the HDRP pipeline. Please use the Built-In or Universal Rendering Pipeline (URP).");
                return DetectedRenderingConfig.Unsupported;
            }

            return DetectedRenderingConfig.URP;
        }
    }
    
}

