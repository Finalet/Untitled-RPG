// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#if CREST_URP

namespace Crest
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    internal class UnderwaterEffectPassURP : ScriptableRenderPass
    {
        const string SHADER_UNDERWATER_EFFECT = "Hidden/Crest/Underwater/Underwater Effect URP";
        static readonly int sp_TemporaryRT = Shader.PropertyToID("_TemporaryRT");

        readonly PropertyWrapperMaterial _underwaterEffectMaterial;
        RenderTargetIdentifier _sourceIdentifierRT;
        bool _firstRender = true;

        static UnderwaterEffectPassURP s_instance;
        UnderwaterRenderer _underwaterRenderer;

        public UnderwaterEffectPassURP()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            _underwaterEffectMaterial = new PropertyWrapperMaterial(SHADER_UNDERWATER_EFFECT);
        }

        ~UnderwaterEffectPassURP()
        {
            CoreUtils.Destroy(_underwaterEffectMaterial.material);
        }

        public static void Enable(UnderwaterRenderer underwaterRenderer)
        {
            if (s_instance == null)
            {
                s_instance = new UnderwaterEffectPassURP();
            }

            s_instance._underwaterRenderer = underwaterRenderer;

            RenderPipelineManager.beginCameraRendering -= EnqueuePass;
            RenderPipelineManager.beginCameraRendering += EnqueuePass;
        }

        public static void Disable()
        {
            RenderPipelineManager.beginCameraRendering -= EnqueuePass;
        }

        static void EnqueuePass(ScriptableRenderContext context, Camera camera)
        {
            if (!s_instance._underwaterRenderer.IsActive)
            {
                return;
            }

            // Only support main camera for now.
            if (!ReferenceEquals(OceanRenderer.Instance.ViewCamera, camera))
            {
                return;
            }

            // Only support game cameras for now.
            if (camera.cameraType != CameraType.Game)
            {
                return;
            }

            if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData))
            {
                cameraData.scriptableRenderer.EnqueuePass(s_instance);
            }
        }

        // Called before Configure.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _sourceIdentifierRT = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(sp_TemporaryRT, renderingData.cameraData.cameraTargetDescriptor);
        }

        // Called before Execute.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(_sourceIdentifierRT);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(sp_TemporaryRT);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

            XRHelpers.Update(camera);
            XRHelpers.UpdatePassIndex(ref UnderwaterRenderer.s_xrPassIndex);

            // Ensure legacy underwater fog is disabled.
            if (_firstRender)
            {
                OceanRenderer.Instance.OceanMaterial.DisableKeyword("_OLD_UNDERWATER");
            }

            CommandBuffer commandBuffer = CommandBufferPool.Get("Underwater Effect");

            UnderwaterRenderer.UpdatePostProcessMaterial(
                camera,
                _underwaterEffectMaterial,
                _underwaterRenderer._sphericalHarmonicsData,
                _underwaterRenderer._meniscus,
                _firstRender || _underwaterRenderer._copyOceanMaterialParamsEachFrame,
                _underwaterRenderer._debug._viewOceanMask,
                // horizonSafetyMarginMultiplier is added to the horizon, so no-op is zero.
                _underwaterRenderer._useHorizonSafetyMarginMultiplier ? _underwaterRenderer._horizonSafetyMarginMultiplier : 0f,
                // farPlaneMultiplier is multiplied to the far plane, so no-op is one.
                _underwaterRenderer._useHorizonSafetyMarginMultiplier ? 1f : _underwaterRenderer._farPlaneMultiplier,
                _underwaterRenderer._filterOceanData,
                UnderwaterRenderer.s_xrPassIndex
            );

            // We cannot read and write using the same texture so use a temporary texture as an intermediary.
            Blit(commandBuffer, _sourceIdentifierRT, sp_TemporaryRT, _underwaterEffectMaterial.material);
            Blit(commandBuffer, sp_TemporaryRT, _sourceIdentifierRT);

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);

            _firstRender = false;
        }
    }
}

#endif // CREST_URP
