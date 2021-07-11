using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;

public class SkyboxCloudsFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public RenderTargetIdentifier source;

        private Material cloudPassMaterial;
        private RenderTargetHandle tempRenderTargetRenderClouds;
        private RenderTargetHandle tempRenderTargetMerge;

        public CustomRenderPass(Material cloudPassMaterial)
        {
            this.cloudPassMaterial = cloudPassMaterial;
            tempRenderTargetRenderClouds.Init("_TemporaryColorTextureCloudPass");
            tempRenderTargetMerge.Init("_TemporaryMergePass");
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Profiler.BeginSample("Clouds Execution");
            CommandBuffer commandBuffer = CommandBufferPool.Get("CustomBlitRenderPass");
            RenderTextureDescriptor cloudDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cloudDescriptor.colorFormat = RenderTextureFormat.ARGBFloat;
            cloudDescriptor.height = (int)(cloudDescriptor.height * 0.5f);
            cloudDescriptor.width = (int)(cloudDescriptor.width * 0.5f); 
            commandBuffer.GetTemporaryRT(tempRenderTargetRenderClouds.id, cloudDescriptor);

            // Blits "tempRendertargetHandleVolumetricClouds.Identifier()" to "_CLOUDBLIT" Texture, accessible from the blurMaterial shader.
            Blit(commandBuffer, source, tempRenderTargetRenderClouds.Identifier(), cloudPassMaterial);
            commandBuffer.SetGlobalTexture("_CLOUDBLIT", tempRenderTargetRenderClouds.Identifier());

            RenderTextureDescriptor rtDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            Material mergeMat = new Material(Shader.Find("Shader Graphs/Volumetric Clouds Merge Pass"));
            commandBuffer.GetTemporaryRT(tempRenderTargetMerge.id, rtDescriptor);
            Blit(commandBuffer, source, tempRenderTargetMerge.Identifier(), mergeMat);

            // blit to screen
            Blit(commandBuffer, tempRenderTargetMerge.Identifier(), source);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            CommandBufferPool.Release(commandBuffer);
            Profiler.EndSample();
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material cloudPass = null;
    }
    public Settings settings = new Settings();
    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(settings.cloudPass);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}