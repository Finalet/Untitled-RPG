// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#if CREST_HDRP

namespace Crest
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.HighDefinition;

    public class UnderwaterEffectPassHDRP : CustomPass
    {
        const string k_Name = "Underwater Effect";
        const string k_ShaderPath = "Hidden/Crest/Underwater/Underwater Effect HDRP";

        PropertyWrapperMaterial _underwaterEffectMaterial;
        RTHandle _colorTexture;
        bool _firstRender = true;
        UnderwaterRenderer.UnderwaterSphericalHarmonicsData _sphericalHarmonicsData = new UnderwaterRenderer.UnderwaterSphericalHarmonicsData();

        static GameObject s_GameObject;
        static UnderwaterRenderer s_UnderwaterRenderer;

        public static void Enable(UnderwaterRenderer underwaterRenderer)
        {
            CustomPassHelpers.CreateOrUpdate<UnderwaterEffectPassHDRP>(ref s_GameObject, k_Name, CustomPassInjectionPoint.BeforePostProcess);
            s_UnderwaterRenderer = underwaterRenderer;
        }

        public static void Disable()
        {
            // It should be safe to rely on this reference for this reference to fail.
            if (s_GameObject != null)
            {
                s_GameObject.SetActive(false);
            }
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            if (_underwaterEffectMaterial?.material == null)
            {
                _underwaterEffectMaterial = new PropertyWrapperMaterial(k_ShaderPath);
            }

            // TODO: Use a temporary RT if possible.
            _colorTexture = RTHandles.Alloc
            (
                Vector2.one,
                TextureXR.slices,
                dimension: TextureXR.dimension,
                colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.B10G11R11_UFloatPack32,
                useDynamicScale: true,
                name: "Crest Camera Color Texture"
            );
        }

        protected override void Cleanup()
        {
            if (_underwaterEffectMaterial?.material != null)
            {
                CoreUtils.Destroy(_underwaterEffectMaterial.material);
            }

            _colorTexture.Release();
        }

        protected override void Execute(CustomPassContext context)
        {
            if (!s_UnderwaterRenderer.IsActive)
            {
                return;
            }

            var camera = context.hdCamera.camera;

            // Custom passes execute for every camera. We only support one camera for now.
            if (!ReferenceEquals(camera, OceanRenderer.Instance.ViewCamera) || camera.cameraType != CameraType.Game)
            {
                return;
            }

            XRHelpers.Update(camera);
            XRHelpers.UpdatePassIndex(ref UnderwaterRenderer.s_xrPassIndex);

            // Dynamic resolution will cause the horizon gap to widen. For extreme values, the gap can be several pixels
            // thick. It can be disabled as it will require thorough testing across different hardware to get it 100%
            // right. This doesn't appear to be an issue for the built-in renderer.
            var horizonSafetyMarginMultiplier = s_UnderwaterRenderer._horizonSafetyMarginMultiplier;
            if (s_UnderwaterRenderer._useHorizonSafetyMarginMultiplier &&
                s_UnderwaterRenderer._scaleSafetyMarginWithDynamicResolution &&
                DynamicResolutionHandler.instance.DynamicResolutionEnabled())
            {
                // 100 is a magic number from testing. Works well with default horizonSafetyMarginMultiplier of 0.01.
                horizonSafetyMarginMultiplier = Mathf.Lerp(horizonSafetyMarginMultiplier * 100,
                    horizonSafetyMarginMultiplier, DynamicResolutionHandler.instance.GetCurrentScale());
            }

            UnderwaterRenderer.UpdatePostProcessMaterial(
                camera,
                _underwaterEffectMaterial,
                _sphericalHarmonicsData,
                s_UnderwaterRenderer._meniscus,
                _firstRender || s_UnderwaterRenderer._copyOceanMaterialParamsEachFrame,
                s_UnderwaterRenderer._debug._viewOceanMask,
                // horizonSafetyMarginMultiplier is added to the horizon, so no-op is zero.
                s_UnderwaterRenderer._useHorizonSafetyMarginMultiplier ? horizonSafetyMarginMultiplier : 0f,
                // farPlaneMultiplier is multiplied to the far plane, so no-op is one.
                s_UnderwaterRenderer._useHorizonSafetyMarginMultiplier ? 1f : s_UnderwaterRenderer._farPlaneMultiplier,
                s_UnderwaterRenderer._filterOceanData,
                UnderwaterRenderer.s_xrPassIndex
            );

            HDUtils.BlitCameraTexture(context.cmd, context.cameraColorBuffer, _colorTexture);
            context.propertyBlock.SetTexture(UnderwaterRenderer.sp_CrestCameraColorTexture, _colorTexture);

            CoreUtils.SetRenderTarget(context.cmd, context.cameraColorBuffer, ClearFlag.None);
            // Other method signatures can break XR SPI. Shader pass 1 is custom pass.
            CoreUtils.DrawFullScreen(context.cmd, _underwaterEffectMaterial.material, properties: context.propertyBlock, shaderPassId: 1);

            _firstRender = false;
        }
    }
}

#endif // CREST_HDRP
