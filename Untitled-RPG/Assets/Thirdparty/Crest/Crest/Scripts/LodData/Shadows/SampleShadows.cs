// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Crest
{
    public class SampleShadows : ScriptableRendererFeature
    {
        public static bool Created { get; private set; }

        SampleShadowsPass renderObjectsPass;

        public override void Create()
        {
            renderObjectsPass = new SampleShadowsPass(RenderPassEvent.AfterRenderingSkybox);

            Created = true;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(renderObjectsPass);
        }
    }

    public class SampleShadowsPass : ScriptableRenderPass
    {
        public SampleShadowsPass(RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (OceanRenderer.Instance == null || OceanRenderer.Instance._lodDataShadow == null) return;

            // Only sample shadows for the main camera.
            if (!ReferenceEquals(OceanRenderer.Instance.Viewpoint, renderingData.cameraData.camera.transform))
            {
                return;
            }

            if (context == null)
                throw new System.ArgumentNullException("context");

            if (renderingData.lightData.mainLightIndex == -1)
                return;

            var cmd = OceanRenderer.Instance._lodDataShadow.BufCopyShadowMap;
            if (cmd == null) return;

            var camera = renderingData.cameraData.camera;

            // Target is not multi-eye so stop mult-eye rendering for this command buffer. Breaks registered shadow
            // inputs without this.
            if (camera.stereoEnabled)
            {
                context.StopMultiEye(camera);
            }

            context.ExecuteCommandBuffer(cmd);

            if (camera.stereoEnabled)
            {
                context.StartMultiEye(camera);
            }
            else
            {
                // Restore matrices otherwise remaining render will have incorrect matrices. Each pass is responsible
                // for restoring matrices if required.
                cmd.Clear();
                cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                context.ExecuteCommandBuffer(cmd);
            }
        }
    }
}
