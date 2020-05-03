using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HorizonBasedAmbientOcclusion.Universal
{
    public class CameraNormalsRendererFeature : ScriptableRendererFeature
    {
        private class CameraNormalsRenderPass : ScriptableRenderPass
        {
            private Material material { get; set; }
            private RenderTargetIdentifier source { get; set; }
            private RenderTextureDescriptor sourceDesc { get; set; }
            private RenderTargetIdentifier renderTarget { get; set; }
            private RenderTextureDescriptor renderTargetDesc { get; set; }

            private bool isRenderTargetDirty
            {
                get
                {
                    if (m_RenderTexture == null || m_PreviousWidth != sourceDesc.width || m_PreviousHeight != sourceDesc.height)
                    {
                        m_PreviousWidth = sourceDesc.width;
                        m_PreviousHeight = sourceDesc.height;
                        return true;
                    }
                    return false;
                }
            }

            private ShaderTagId m_ShaderTagId;
            private FilteringSettings m_FilteringSettings;
            private RenderStateBlock m_RenderStateBlock;
            private RenderTexture m_RenderTexture;
            private int m_PreviousWidth;
            private int m_PreviousHeight;

            public void Setup(Shader shader, ScriptableRenderer renderer, RenderPassEvent renderPassEvent)
            {
                if (material == null) material = CoreUtils.CreateEngineMaterial(shader);
                source = renderer.cameraColorTarget;

                // Configures where the render pass should be injected.
                this.renderPassEvent = renderPassEvent;

                m_ShaderTagId = new ShaderTagId("UniversalForward");
                m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask: -1);
                m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            }

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                cameraTextureDescriptor.msaaSamples = 1;
                cameraTextureDescriptor.depthBufferBits = 0;
                sourceDesc = cameraTextureDescriptor;

                if (isRenderTargetDirty)
                {
                    renderTargetDesc = GetStereoCompatibleDescriptor(sourceDesc.width, sourceDesc.height, format: RenderTextureFormat.ARGB2101010, depthBufferBits: 16, readWrite: RenderTextureReadWrite.Linear);

                    if (m_RenderTexture != null) m_RenderTexture.Release();

                    m_RenderTexture = new RenderTexture(renderTargetDesc);
                    m_RenderTexture.Create();

                    renderTarget = new RenderTargetIdentifier(m_RenderTexture);
                }
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null)
                {
                    Debug.LogError("Material has not been correctly initialized...");
                    return;
                }

                var cmd = CommandBufferPool.Get("CameraNormals");

                var sourceTexId = Shader.PropertyToID("_SourceTex");
                cmd.GetTemporaryRT(sourceTexId, sourceDesc);
                Blit(cmd, source, sourceTexId);

                CoreUtils.SetRenderTarget(cmd, renderTarget);
                CoreUtils.ClearRenderTarget(cmd, ClearFlag.All, new Color(0.5f, 0.5f, 1f));
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var drawingSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                drawingSettings.overrideMaterial = material;
                drawingSettings.overrideMaterialPassIndex = 0;

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);

                Blit(cmd, sourceTexId, source);
                cmd.ReleaseTemporaryRT(sourceTexId);

                cmd.SetGlobalTexture(Shader.PropertyToID("_CameraNormalsTexture"), renderTarget);

                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd) { }

            public void Cleanup()
            {
                if (m_RenderTexture != null)
                {
                    m_RenderTexture.Release();
                    m_RenderTexture = null;
                }

                CoreUtils.Destroy(material);
            }

            private RenderTextureDescriptor GetStereoCompatibleDescriptor(int width, int height, RenderTextureFormat format = RenderTextureFormat.Default, int depthBufferBits = 0, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default)
            {
                // Inherit the VR setup from the camera descriptor
                var desc = sourceDesc;
                desc.depthBufferBits = depthBufferBits;
                desc.msaaSamples = 1;
                desc.width = width;
                desc.height = height;
                desc.colorFormat = format;

                if (readWrite == RenderTextureReadWrite.sRGB)
                    desc.sRGB = true;
                else if (readWrite == RenderTextureReadWrite.Linear)
                    desc.sRGB = false;
                else if (readWrite == RenderTextureReadWrite.Default)
                    desc.sRGB = QualitySettings.activeColorSpace != ColorSpace.Gamma;

                return desc;
            }
        }

        [SerializeField, HideInInspector]
        private Shader shader;
        private CameraNormalsRenderPass m_RenderPass;

        void OnDisable()
        {
            if (m_RenderPass != null)
                m_RenderPass.Cleanup();
        }

        public override void Create()
        {
            name = "CameraNormals";

            m_RenderPass = new CameraNormalsRenderPass();

            shader = Shader.Find("Hidden/Universal Render Pipeline/CameraNormals");
            if (shader == null)
            {
                Debug.LogWarning("Shader was not found. Please ensure it compiles correctly");
                return;
            }
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_RenderPass.Setup(shader, renderer, RenderPassEvent.AfterRenderingOpaques);

            renderer.EnqueuePass(m_RenderPass);
        }
    }
}
